using FileManager;
using System;
using System.Threading.Tasks;

namespace AaxDecrypter
{
	public class UnencryptedAudiobookDownloader : AudiobookDownloadBase
	{
		protected override long InputFilePosition => InputFileStream.WritePosition;

		public UnencryptedAudiobookDownloader(string outFileName, string cacheDirectory, IDownloadOptions dlLic)
			: base(outFileName, cacheDirectory, dlLic)
		{
			AsyncSteps.Name = "Download Unencrypted Audiobook";
			AsyncSteps["Step 1: Download Audiobook"] = Step_DownloadAndDecryptAudiobookAsync;
			AsyncSteps["Step 2: Download Clips and Bookmarks"] = Step_DownloadClipsBookmarksAsync;
			AsyncSteps["Step 3: Create Cue"] = Step_CreateCueAsync;
		}

		protected override async Task<bool> Step_DownloadAndDecryptAudiobookAsync()
		{
			await InputFileStream.DownloadTask;

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
