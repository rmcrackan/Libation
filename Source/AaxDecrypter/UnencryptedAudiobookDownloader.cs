using FileManager;
using System.Threading.Tasks;

namespace AaxDecrypter;

public class UnencryptedAudiobookDownloader : AudiobookDownloadBase
{
	protected override long InputFilePosition => InputFileStream.WritePosition;

	public UnencryptedAudiobookDownloader(string outDirectory, string cacheDirectory, IDownloadOptions dlLic)
		: base(outDirectory, cacheDirectory, dlLic)
	{
		AsyncSteps.Name = "Download Unencrypted Audiobook";
		AsyncSteps["Step 1: Download Audiobook"] = Step_DownloadAndDecryptAudiobookAsync;
		AsyncSteps["Step 2: Create Cue"] = Step_CreateCueAsync;
	}

	protected override async Task<bool> Step_DownloadAndDecryptAudiobookAsync()
	{
		await (InputFileStream.DownloadTask ?? Task.CompletedTask);

		if (IsCanceled)
			return false;
		else
		{
			FinalizeDownload();
			var tempFile = GetNewTempFilePath(DownloadOptions.OutputFormat.ToString());
			FileUtility.SaferMove(InputFileStream.SaveFilePath, tempFile.FilePath);
			OnTempFileCreated(tempFile);
			return true;
		}
	}
}
