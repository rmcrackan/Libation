using Dinah.Core.Net.Http;
using FileManager;
using System;
using System.Threading.Tasks;

namespace AaxDecrypter
{
	public class UnencryptedAudiobookDownloader : AudiobookDownloadBase
	{
		public UnencryptedAudiobookDownloader(string outFileName, string cacheDirectory, IDownloadOptions dlLic)
			: base(outFileName, cacheDirectory, dlLic)
		{
			AsyncSteps.Name = "Download Unencrypted Audiobook";
			AsyncSteps["Step 1: Download Audiobook"] = Step_DownloadAndDecryptAudiobookAsync;
			AsyncSteps["Step 2: Download Clips and Bookmarks"] = Step_DownloadClipsBookmarksAsync;
			AsyncSteps["Step 3: Create Cue"] = Step_CreateCueAsync;
		}

		public override Task CancelAsync()
		{
			IsCanceled = true;
			FinalizeDownload();
			return Task.CompletedTask;
		}

		protected override async Task<bool> Step_DownloadAndDecryptAudiobookAsync()
		{
			DateTime startTime = DateTime.Now;

			// MUST put InputFileStream.Length first, because it starts background downloader.

			while (InputFileStream.Length > InputFileStream.WritePosition && !InputFileStream.IsCancelled)
			{
				var rate = InputFileStream.WritePosition / (DateTime.Now - startTime).TotalSeconds;

				var estTimeRemaining = (InputFileStream.Length - InputFileStream.WritePosition) / rate;

				if (double.IsNormal(estTimeRemaining))
					OnDecryptTimeRemaining(TimeSpan.FromSeconds(estTimeRemaining));

				var progressPercent = 100d * InputFileStream.WritePosition / InputFileStream.Length;

				OnDecryptProgressUpdate(
					new DownloadProgress
					{
						ProgressPercentage = progressPercent,
						BytesReceived = InputFileStream.WritePosition,
						TotalBytesToReceive = InputFileStream.Length
					});

				await Task.Delay(200);
			}

			if (IsCanceled)
				return false;
			else
			{
				FinalizeDownload();
				FileUtility.SaferMove(InputFileStream.SaveFilePath, OutputFileName);
				OnFileCreated(OutputFileName);
				return true;
			}
		}
	}
}
