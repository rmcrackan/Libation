using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Dinah.Core;
using Dinah.Core.Net.Http;
using FileManager;

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

		public bool IsCanceled { get; set; }
		public string TempFilePath { get; }

		protected string OutputFileName { get; private set; }
		protected IDownloadOptions DownloadOptions { get; }
		protected NetworkFileStream InputFileStream => (nfsPersister ??= OpenNetworkFileStream()).NetworkFileStream;

		// Don't give the property a 'set'. This should have to be an obvious choice; not accidental
		protected void SetOutputFileName(string newOutputFileName) => OutputFileName = newOutputFileName;

		private NetworkFileStreamPersister nfsPersister;

		private string jsonDownloadState { get; }

		protected AudiobookDownloadBase(string outFileName, string cacheDirectory, IDownloadOptions dlOptions)
		{
			OutputFileName = ArgumentValidator.EnsureNotNullOrWhiteSpace(outFileName, nameof(outFileName));

			var outDir = Path.GetDirectoryName(OutputFileName);
			if (!Directory.Exists(outDir))
				Directory.CreateDirectory(outDir);

			if (!Directory.Exists(cacheDirectory))
				Directory.CreateDirectory(cacheDirectory);

			jsonDownloadState = Path.Combine(cacheDirectory, Path.GetFileName(Path.ChangeExtension(OutputFileName, ".json")));
			TempFilePath = Path.ChangeExtension(jsonDownloadState, ".aaxc");

			DownloadOptions = ArgumentValidator.EnsureNotNull(dlOptions, nameof(dlOptions));

			// delete file after validation is complete
			FileUtility.SaferDelete(OutputFileName);
		}

		public abstract Task CancelAsync();

		public virtual void SetCoverArt(byte[] coverArt)
		{
			if (coverArt is not null)
				OnRetrievedCoverArt(coverArt);
		}

		public abstract Task<bool> RunAsync();

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

		protected void CloseInputFileStream()
		{
			nfsPersister?.NetworkFileStream?.Close();
			nfsPersister?.Dispose();
		}

		protected bool Step_CreateCue()
		{
			if (!DownloadOptions.CreateCueSheet) return true;

			// not a critical step. its failure should not prevent future steps from running
			try
			{
				var path = Path.ChangeExtension(OutputFileName, ".cue");
				path = FileUtility.GetValidFilename(path, DownloadOptions.ReplacementCharacters);
				File.WriteAllText(path, Cue.CreateContents(Path.GetFileName(OutputFileName), DownloadOptions.ChapterInfo));
				OnFileCreated(path);
			}
			catch (Exception ex)
			{
				Serilog.Log.Logger.Error(ex, $"{nameof(Step_CreateCue)}. FAILED");
			}
			return !IsCanceled;
		}

		protected bool Step_Cleanup()
		{
			bool success = !IsCanceled;
			if (success)
			{
				FileUtility.SaferDelete(jsonDownloadState);

				if (DownloadOptions.AudibleKey is not null &&
					DownloadOptions.AudibleIV is not null &&
					DownloadOptions.RetainEncryptedFile)
				{
					string aaxPath = Path.ChangeExtension(TempFilePath, ".aax");
					FileUtility.SaferMove(TempFilePath, aaxPath);

					//Write aax decryption key
					string keyPath = Path.ChangeExtension(aaxPath, ".key");
					FileUtility.SaferDelete(keyPath);
					File.WriteAllText(keyPath, $"Key={DownloadOptions.AudibleKey}\r\nIV={DownloadOptions.AudibleIV}");

					OnFileCreated(aaxPath);
					OnFileCreated(keyPath);
				}
				else
					FileUtility.SaferDelete(TempFilePath);
			}

			return success;
		}

		private NetworkFileStreamPersister OpenNetworkFileStream()
		{
			if (!File.Exists(jsonDownloadState))
				return NewNetworkFilePersister();

			try
			{
				var nfsp = new NetworkFileStreamPersister(jsonDownloadState);
				// If More than ~1 hour has elapsed since getting the download url, it will expire.
				// The new url will be to the same file.
				nfsp.NetworkFileStream.SetUriForSameFile(new Uri(DownloadOptions.DownloadUrl));
				return nfsp;
			}
			catch
			{
				FileUtility.SaferDelete(jsonDownloadState);
				FileUtility.SaferDelete(TempFilePath);
				return NewNetworkFilePersister();
			}
		}

		private NetworkFileStreamPersister NewNetworkFilePersister()
		{
			var networkFileStream = new NetworkFileStream(TempFilePath, new Uri(DownloadOptions.DownloadUrl), 0, new() { { "User-Agent", DownloadOptions.UserAgent } });
			return new NetworkFileStreamPersister(networkFileStream, jsonDownloadState);
		}
	}
}
