using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using DataLayer;
using Dinah.Core.ErrorHandling;
using Dinah.Core.Net.Http;
using FileManager;

namespace FileLiberator
{
	public class DownloadPdf : DownloadableBase
	{
		public override bool Validate(LibraryBook libraryBook)
			=> !string.IsNullOrWhiteSpace(getdownloadUrl(libraryBook))
			&& !AudibleFileStorage.PDF.Exists(libraryBook.Book.AudibleProductId);

		public override async Task<StatusHandler> ProcessItemAsync(LibraryBook libraryBook)
		{
			var proposedDownloadFilePath = getProposedDownloadFilePath(libraryBook);
			await downloadPdfAsync(libraryBook, proposedDownloadFilePath);
			return verifyDownload(libraryBook);
		}

		private static StatusHandler verifyDownload(LibraryBook libraryBook)
			=> !AudibleFileStorage.PDF.Exists(libraryBook.Book.AudibleProductId)
			? new StatusHandler { "Downloaded PDF cannot be found" }
			: new StatusHandler();

		private static string getProposedDownloadFilePath(LibraryBook libraryBook)
		{
			// if audio file exists, get it's dir. else return base Book dir
			var existingPath = Path.GetDirectoryName(AudibleFileStorage.Audio.GetPath(libraryBook.Book.AudibleProductId));
			var file = getdownloadUrl(libraryBook);

			if (existingPath != null)
				return Path.Combine(existingPath, Path.GetFileName(file));

			var full = FileUtility.GetValidFilename(
				AudibleFileStorage.PDF.StorageDirectory,
				libraryBook.Book.Title,
				Path.GetExtension(file),
				libraryBook.Book.AudibleProductId);
			return full;
		}

		private async Task downloadPdfAsync(LibraryBook libraryBook, string proposedDownloadFilePath)
		{
			var api = await GetApiAsync(libraryBook);
			var downloadUrl = await api.GetPdfDownloadLinkAsync(libraryBook.Book.AudibleProductId);

			var client = new HttpClient();
			var actualDownloadedFilePath = await PerformDownloadAsync(
				proposedDownloadFilePath,
				(p) => client.DownloadFileAsync(downloadUrl, proposedDownloadFilePath, p));
		}

		private static string getdownloadUrl(LibraryBook libraryBook)
			=> libraryBook?.Book?.Supplements?.FirstOrDefault()?.Url;
	}
}
