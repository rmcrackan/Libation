using Dinah.Core;
using Dinah.Core.IO;
using Dinah.Core.Net.Http;
using Dinah.Core.StepRunner;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AaxDecrypter
{
	public enum OutputFormat { M4b, Mp3 }

	public abstract class AudioDownloadBase
	{
		public event EventHandler<string> RetrievedTitle;
		public event EventHandler<string> RetrievedAuthors;
		public event EventHandler<string> RetrievedNarrators;
		public event EventHandler<byte[]> RetrievedCoverArt;
		public event EventHandler<DownloadProgress> DecryptProgressUpdate;
		public event EventHandler<TimeSpan> DecryptTimeRemaining;

		public string AppName { get; set; }

		protected bool isCanceled { get; set; }
		protected string outputFileName { get; }
		protected string cacheDir { get; }
		protected DownloadLicense downloadLicense { get; }
		protected NetworkFileStream InputFileStream => (nfsPersister ??= OpenNetworkFileStream()).NetworkFileStream;


		protected abstract StepSequence steps { get; }
		private NetworkFileStreamPersister nfsPersister;

		private string jsonDownloadState => Path.Combine(cacheDir, Path.GetFileNameWithoutExtension(outputFileName) + ".json");
		private string tempFile => PathLib.ReplaceExtension(jsonDownloadState, ".tmp");

		public AudioDownloadBase(string outFileName, string cacheDirectory, DownloadLicense dlLic)
		{
			AppName = GetType().Name;

			ArgumentValidator.EnsureNotNullOrWhiteSpace(outFileName, nameof(outFileName));
			outputFileName = outFileName;

			var outDir = Path.GetDirectoryName(outputFileName);
			if (!Directory.Exists(outDir))
				throw new ArgumentNullException(nameof(outDir), "Directory does not exist");
			if (File.Exists(outputFileName))
				File.Delete(outputFileName);

			if (!Directory.Exists(cacheDirectory))
				throw new ArgumentNullException(nameof(cacheDirectory), "Directory does not exist");
			cacheDir = cacheDirectory;

			downloadLicense = ArgumentValidator.EnsureNotNull(dlLic, nameof(dlLic));
		}

		public abstract void Cancel();
		protected abstract int GetSpeedup(TimeSpan elapsed);
		protected abstract bool Step2_DownloadAudiobook();
		protected abstract bool Step1_GetMetadata();

		public virtual void SetCoverArt(byte[] coverArt)
		{
			if (coverArt is null) return;

			OnRetrievedCoverArt(coverArt);
		}


		public bool Run()
		{
			var (IsSuccess, Elapsed) = steps.Run();

			if (!IsSuccess)
			{
				Console.WriteLine("WARNING-Conversion failed");
				return false;
			}

			Serilog.Log.Logger.Information($"Speedup is {GetSpeedup(Elapsed)}x realtime.");
			return true;
		}		

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

		protected void CloseInputFileStream()
		{
			nfsPersister?.NetworkFileStream?.Close();
			nfsPersister?.Dispose();
		}

		protected bool Step3_CreateCue()
		{
			// not a critical step. its failure should not prevent future steps from running
			try
			{
				File.WriteAllText(PathLib.ReplaceExtension(outputFileName, ".cue"), Cue.CreateContents(Path.GetFileName(outputFileName), downloadLicense.ChapterInfo));
			}
			catch (Exception ex)
			{
				Serilog.Log.Logger.Error(ex, $"{nameof(Step3_CreateCue)}. FAILED");
			}
			return !isCanceled;
		}

		protected bool Step4_Cleanup()
		{
			FileExt.SafeDelete(jsonDownloadState);
			FileExt.SafeDelete(tempFile);
			return !isCanceled;
		}

		private NetworkFileStreamPersister OpenNetworkFileStream()
		{
			NetworkFileStreamPersister nfsp;

			if (File.Exists(jsonDownloadState))
			{
				try
				{
					nfsp = new NetworkFileStreamPersister(jsonDownloadState);
					//If More than ~1 hour has elapsed since getting the download url, it will expire.
					//The new url will be to the same file.
					nfsp.NetworkFileStream.SetUriForSameFile(new Uri(downloadLicense.DownloadUrl));
				}
				catch
				{
					FileExt.SafeDelete(jsonDownloadState);
					FileExt.SafeDelete(tempFile);
					nfsp = NewNetworkFilePersister();
				}
			}
			else
			{
				nfsp = NewNetworkFilePersister();
			}
			return nfsp;
		}

		private NetworkFileStreamPersister NewNetworkFilePersister()
		{
			var headers = new System.Net.WebHeaderCollection
			{
				{ "User-Agent", downloadLicense.UserAgent }
			};

			var networkFileStream = new NetworkFileStream(tempFile, new Uri(downloadLicense.DownloadUrl), 0, headers);
			return new NetworkFileStreamPersister(networkFileStream, jsonDownloadState);
		}
	}
}
