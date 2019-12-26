using System;
using System.IO;
using System.Threading.Tasks;
using DataLayer;
using Dinah.Core;
using Dinah.Core.ErrorHandling;
using FileManager;

namespace FileLiberator
{
    /// <summary>
    /// Download DRM book
    /// 
    /// Processes:
    /// Download: download aax file: the DRM encrypted audiobook
    /// Decrypt: remove DRM encryption from audiobook. Store final book
    /// Backup: perform all steps (downloaded, decrypt) still needed to get final book
    /// </summary>
    public class DownloadBook : DownloadableBase
    {
        public override bool Validate(LibraryBook libraryBook)
            => !AudibleFileStorage.Audio.Exists(libraryBook.Book.AudibleProductId)
            && !AudibleFileStorage.AAX.Exists(libraryBook.Book.AudibleProductId);

		public override async Task<StatusHandler> ProcessItemAsync(LibraryBook libraryBook)
		{
			var tempAaxFilename = getDownloadPath(libraryBook);
			var actualFilePath = await downloadBookAsync(libraryBook, tempAaxFilename);
			moveBook(libraryBook, actualFilePath);
			return verifyDownload(libraryBook);
		}

		private static string getDownloadPath(LibraryBook libraryBook)
			=> FileUtility.GetValidFilename(
				AudibleFileStorage.DownloadsInProgress,
				libraryBook.Book.Title,
				"aax",
				libraryBook.Book.AudibleProductId);

		private async Task<string> downloadBookAsync(LibraryBook libraryBook, string tempAaxFilename)
		{
			var api = await AudibleApi.EzApiCreator.GetApiAsync(AudibleApiStorage.IdentityTokensFile);

			var actualFilePath = await PerformDownloadAsync(
				tempAaxFilename,
				(p) => api.DownloadAaxWorkaroundAsync(libraryBook.Book.AudibleProductId, tempAaxFilename, p));

			System.Threading.Thread.Sleep(100);
			// if bad file download, a 0-33 byte file will be created
			// if service unavailable, a 52 byte string will be saved as file
			if (new FileInfo(actualFilePath).Length < 100)
			{
				var contents = File.ReadAllText(actualFilePath);
				File.Delete(actualFilePath);

				var unavailable = "Content Delivery Companion Service is not available.";
				if (contents.StartsWithInsensitive(unavailable))
					throw new Exception(unavailable);
				throw new Exception("Error downloading file");
			}

			return actualFilePath;
		}

		private void moveBook(LibraryBook libraryBook, string actualFilePath)
		{
			var newAaxFilename = FileUtility.GetValidFilename(
				AudibleFileStorage.DownloadsFinal,
				libraryBook.Book.Title,
				"aax",
				libraryBook.Book.AudibleProductId);
			File.Move(actualFilePath, newAaxFilename);
			Invoke_StatusUpdate($"Successfully downloaded. Moved to: {newAaxFilename}");
		}

		private static StatusHandler verifyDownload(LibraryBook libraryBook)
			=> !AudibleFileStorage.AAX.Exists(libraryBook.Book.AudibleProductId)
			? new StatusHandler { "Downloaded AAX file cannot be found" }
			: new StatusHandler();
	}
}
