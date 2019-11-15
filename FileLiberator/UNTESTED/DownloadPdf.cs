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
		public override async Task<bool> ValidateAsync(LibraryBook libraryBook)
		{
			if (string.IsNullOrWhiteSpace(getdownloadUrl(libraryBook)))
				return false;

			return !await AudibleFileStorage.PDF.ExistsAsync(libraryBook.Book.AudibleProductId);
		}

		private static string getdownloadUrl(LibraryBook libraryBook)
			=> libraryBook?.Book?.Supplements?.FirstOrDefault()?.Url;

		public override async Task<StatusHandler> ProcessItemAsync(LibraryBook libraryBook)
		{
			var proposedDownloadFilePath = await getProposedDownloadFilePathAsync(libraryBook);
			await downloadPdfAsync(libraryBook, proposedDownloadFilePath);
			return await verifyDownloadAsync(libraryBook);
		}

		private static async Task<string> getProposedDownloadFilePathAsync(LibraryBook libraryBook)
		{
			// if audio file exists, get it's dir. else return base Book dir
			var destinationDir =
				// this is safe b/c GetDirectoryName(null) == null
				Path.GetDirectoryName(await AudibleFileStorage.Audio.GetAsync(libraryBook.Book.AudibleProductId))
				?? AudibleFileStorage.PDF.StorageDirectory;

			return Path.Combine(destinationDir, Path.GetFileName(getdownloadUrl(libraryBook)));
		}

		private async Task downloadPdfAsync(LibraryBook libraryBook, string proposedDownloadFilePath)
		{
			var client = new HttpClient();
			var actualDownloadedFilePath = await PerformDownloadAsync(
				proposedDownloadFilePath,
				(p) => client.DownloadFileAsync(getdownloadUrl(libraryBook), proposedDownloadFilePath, p));
		}

		private static async Task<StatusHandler> verifyDownloadAsync(LibraryBook libraryBook)
			=> !await AudibleFileStorage.PDF.ExistsAsync(libraryBook.Book.AudibleProductId)
			? new StatusHandler { "Downloaded PDF cannot be found" }
			: new StatusHandler();
	}
}
