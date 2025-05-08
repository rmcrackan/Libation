using Dinah.Core;
using Dinah.Core.Net.Http;
using Dinah.Core.StepRunner;
using FileManager;
using System;
using System.IO;
using System.Threading.Tasks;

namespace AaxDecrypter
{
	public enum OutputFormat { M4b, Mp3 }

	public abstract class AudiobookDownloadBase
	{
		public event EventHandler<string> RetrievedTitle;
		public event EventHandler<string> RetrievedAuthors;
		public event EventHandler<string> RetrievedNarrators;
		public event EventHandler<byte[]> RetrievedCoverArt;
		public event EventHandler<DownloadProgress> DecryptProgressUpdate;
		public event EventHandler<TimeSpan> DecryptTimeRemaining;
		public event EventHandler<string> FileCreated;

		public bool IsCanceled { get; protected set; }
		protected AsyncStepSequence AsyncSteps { get; } = new();
		protected string OutputFileName { get; }
		public IDownloadOptions DownloadOptions { get; }
		protected NetworkFileStream InputFileStream => nfsPersister.NetworkFileStream;
		protected virtual long InputFilePosition => InputFileStream.Position;
		private bool downloadFinished;

		private readonly NetworkFileStreamPersister nfsPersister;
		private readonly DownloadProgress zeroProgress;
		private readonly string jsonDownloadState;
		private readonly string tempFilePath;

		protected AudiobookDownloadBase(string outFileName, string cacheDirectory, IDownloadOptions dlOptions)
		{
			OutputFileName = ArgumentValidator.EnsureNotNullOrWhiteSpace(outFileName, nameof(outFileName));

			var outDir = Path.GetDirectoryName(OutputFileName);
			if (!Directory.Exists(outDir))
				Directory.CreateDirectory(outDir);

			if (!Directory.Exists(cacheDirectory))
				Directory.CreateDirectory(cacheDirectory);

			jsonDownloadState = Path.Combine(cacheDirectory, Path.GetFileName(Path.ChangeExtension(OutputFileName, ".json")));
			tempFilePath = Path.ChangeExtension(jsonDownloadState, ".aaxc");

			DownloadOptions = ArgumentValidator.EnsureNotNull(dlOptions, nameof(dlOptions));
			DownloadOptions.DownloadSpeedChanged += (_, speed) => InputFileStream.SpeedLimit = speed;

			// delete file after validation is complete
			FileUtility.SaferDelete(OutputFileName);

			nfsPersister = OpenNetworkFileStream();

			zeroProgress = new DownloadProgress
			{
				BytesReceived = 0,
				ProgressPercentage = 0,
				TotalBytesToReceive = 0
			};

			OnDecryptProgressUpdate(zeroProgress);
		}

		public async Task<bool> RunAsync()
		{
			await InputFileStream.BeginDownloadingAsync();
			var progressTask = Task.Run(reportProgress);

			AsyncSteps[$"Cleanup"] = CleanupAsync;
			(bool success, var elapsed) = await AsyncSteps.RunAsync();

			await progressTask;

			var speedup = DownloadOptions.RuntimeLength / elapsed;
			Serilog.Log.Information($"Speedup is {speedup:F0}x realtime.");

			return success;

			async Task reportProgress()
			{
				AverageSpeed averageSpeed = new();

				while (
					InputFileStream.CanRead
					&& InputFileStream.Length > InputFilePosition
					&& !InputFileStream.IsCancelled
					&& !downloadFinished)
				{
					averageSpeed.AddPosition(InputFilePosition);

					var estSecsRemaining = (InputFileStream.Length - InputFilePosition) / averageSpeed.Average;

					if (double.IsNormal(estSecsRemaining))
						OnDecryptTimeRemaining(TimeSpan.FromSeconds(estSecsRemaining));

					var progressPercent = 100d * InputFilePosition / InputFileStream.Length;

					OnDecryptProgressUpdate(
						new DownloadProgress
						{
							ProgressPercentage = progressPercent,
							BytesReceived = InputFilePosition,
							TotalBytesToReceive = InputFileStream.Length
						});

					await Task.Delay(200);
				}

				OnDecryptTimeRemaining(TimeSpan.Zero);
				OnDecryptProgressUpdate(zeroProgress);
			}
		}

		public abstract Task CancelAsync();
		protected abstract Task<bool> Step_DownloadAndDecryptAudiobookAsync();

		public virtual void SetCoverArt(byte[] coverArt) { }

		protected void OnRetrievedTitle(string title)
			=> RetrievedTitle?.Invoke(this, title);
		protected void OnRetrievedAuthors(string authors)
			=> RetrievedAuthors?.Invoke(this, authors);
		protected void OnRetrievedNarrators(string narrators)
			=> RetrievedNarrators?.Invoke(this, narrators);
		protected void OnRetrievedCoverArt(byte[] coverArt)
			=> RetrievedCoverArt?.Invoke(this, coverArt);
		protected void OnDecryptProgressUpdate(DownloadProgress downloadProgress)
			=> DecryptProgressUpdate?.Invoke(this, downloadProgress);
		protected void OnDecryptTimeRemaining(TimeSpan timeRemaining)
			=> DecryptTimeRemaining?.Invoke(this, timeRemaining);
		protected void OnFileCreated(string path)
			=> FileCreated?.Invoke(this, path);

		protected virtual void FinalizeDownload()
		{
			nfsPersister?.Dispose();
			downloadFinished = true;
		}

		protected async Task<bool> Step_DownloadClipsBookmarksAsync()
		{
			if (!IsCanceled && DownloadOptions.DownloadClipsBookmarks)
			{
				var recordsFile = await DownloadOptions.SaveClipsAndBookmarksAsync(OutputFileName);

				if (File.Exists(recordsFile))
					OnFileCreated(recordsFile);
			}
			return !IsCanceled;
		}

		protected async Task<bool> Step_CreateCueAsync()
		{
			if (!DownloadOptions.CreateCueSheet) return !IsCanceled;

			// not a critical step. its failure should not prevent future steps from running
			try
			{
				var path = Path.ChangeExtension(OutputFileName, ".cue");
				await File.WriteAllTextAsync(path, Cue.CreateContents(Path.GetFileName(OutputFileName), DownloadOptions.ChapterInfo));
				OnFileCreated(path);
			}
			catch (Exception ex)
			{
				Serilog.Log.Logger.Error(ex, $"{nameof(Step_CreateCueAsync)} Failed");
			}
			return !IsCanceled;
		}

		private async Task<bool> CleanupAsync()
		{
			if (IsCanceled) return false;

			FileUtility.SaferDelete(jsonDownloadState);

			if (!string.IsNullOrEmpty(DownloadOptions.AudibleKey) &&
				DownloadOptions.RetainEncryptedFile &&
				DownloadOptions.InputType is AAXClean.FileType fileType)
			{
				//Write aax decryption key
				string keyPath = Path.ChangeExtension(tempFilePath, ".key");
				FileUtility.SaferDelete(keyPath);
				string aaxPath;

				if (fileType is AAXClean.FileType.Aax)
				{
					await File.WriteAllTextAsync(keyPath, $"ActivationBytes={DownloadOptions.AudibleKey}");
					aaxPath = Path.ChangeExtension(tempFilePath, ".aax");
				}
				else if (fileType is AAXClean.FileType.Aaxc)
				{
					await File.WriteAllTextAsync(keyPath, $"Key={DownloadOptions.AudibleKey}{Environment.NewLine}IV={DownloadOptions.AudibleIV}");
					aaxPath = Path.ChangeExtension(tempFilePath, ".aaxc");
				}
				else if (fileType is AAXClean.FileType.Dash)
				{
					await File.WriteAllTextAsync(keyPath, $"KeyId={DownloadOptions.AudibleKey}{Environment.NewLine}Key={DownloadOptions.AudibleIV}");
					aaxPath = Path.ChangeExtension(tempFilePath, ".dash");
				}
				else
					throw new InvalidOperationException($"Unknown file type: {fileType}");

				if (tempFilePath != aaxPath)
					FileUtility.SaferMove(tempFilePath, aaxPath);

				OnFileCreated(aaxPath);
				OnFileCreated(keyPath);
			}
			else
				FileUtility.SaferDelete(tempFilePath);

			return !IsCanceled;
		}

		private NetworkFileStreamPersister OpenNetworkFileStream()
		{
			NetworkFileStreamPersister nfsp = default;
			try
			{
				if (!File.Exists(jsonDownloadState))
					return nfsp = newNetworkFilePersister();

				nfsp = new NetworkFileStreamPersister(jsonDownloadState);
				// The download url expires after 1 hour.
				// The new url points to the same file.
				nfsp.NetworkFileStream.SetUriForSameFile(new Uri(DownloadOptions.DownloadUrl));
				return nfsp;
			}
			catch
			{
				nfsp?.Target?.Dispose();
				FileUtility.SaferDelete(jsonDownloadState);
				FileUtility.SaferDelete(tempFilePath);
				return nfsp = newNetworkFilePersister();
			}
			finally
			{
				nfsp.NetworkFileStream.RequestHeaders["User-Agent"] = DownloadOptions.UserAgent;
				nfsp.NetworkFileStream.SpeedLimit = DownloadOptions.DownloadSpeedBps;
			}

			NetworkFileStreamPersister newNetworkFilePersister()
			{
				var networkFileStream = new NetworkFileStream(tempFilePath, new Uri(DownloadOptions.DownloadUrl), 0, new() { { "User-Agent", DownloadOptions.UserAgent } });
				return new NetworkFileStreamPersister(networkFileStream, jsonDownloadState);
			}
		}
	}
}
