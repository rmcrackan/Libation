using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using ApplicationServices;
using DataLayer;
using Dinah.Core.ErrorHandling;
using Dinah.Core.Net.Http;
using FileManager;
using LibationFileManager;

namespace FileLiberator
{
	public class DownloadPdf : Processable
	{
		public override string Name => "Download Pdf";
		public override bool Validate(LibraryBook libraryBook)
			=> !string.IsNullOrWhiteSpace(getdownloadUrl(libraryBook))
			&& !libraryBook.Book.PDF_Exists();

		public override async Task<StatusHandler> ProcessAsync(LibraryBook libraryBook)
		{
			OnBegin(libraryBook);

            try
			{
				var proposedDownloadFilePath = getProposedDownloadFilePath(libraryBook);
				var actualDownloadedFilePath = await downloadPdfAsync(libraryBook, proposedDownloadFilePath);
				var result = verifyDownload(actualDownloadedFilePath);

				if (result.IsSuccess)
				{
					SetFileTime(libraryBook, actualDownloadedFilePath);
					SetDirectoryTime(libraryBook, Path.GetDirectoryName(actualDownloadedFilePath));
				}
				await libraryBook.UpdatePdfStatusAsync(result.IsSuccess ? LiberatedStatus.Liberated : LiberatedStatus.NotLiberated);

                return result;
            }
			catch (Exception ex)
            {
                Serilog.Log.Logger.Error(ex, "Error downloading PDF");

                var result = new StatusHandler();
                result.AddError($"Error downloading PDF. See log for details. Error summary: {ex.Message}");

                return result;
            }
			finally
			{
				OnCompleted(libraryBook);
            }
        }

		private static string getProposedDownloadFilePath(LibraryBook libraryBook)
		{
			var extension = Path.GetExtension(getdownloadUrl(libraryBook));

			// if audio file exists, get it's dir. else return base Book dir
			var existingPath = Path.GetDirectoryName(AudibleFileStorage.Audio.GetPath(libraryBook.Book.AudibleProductId));
			if (existingPath is not null)
				return AudibleFileStorage.Audio.GetCustomDirFilename(libraryBook, existingPath, extension);

			return AudibleFileStorage.Audio.GetBooksDirectoryFilename(libraryBook, extension);
		}

		private static string getdownloadUrl(LibraryBook libraryBook)
			=> libraryBook?.Book?.Supplements?.FirstOrDefault()?.Url;

		private async Task<string> downloadPdfAsync(LibraryBook libraryBook, string proposedDownloadFilePath)
		{
			var api = await libraryBook.GetApiAsync();
			var downloadUrl = await api.GetPdfDownloadLinkAsync(libraryBook.Book.AudibleProductId);

			var progress = new Progress<DownloadProgress>(OnStreamingProgressChanged);

			var client = new HttpClient();

			var actualDownloadedFilePath = await client.DownloadFileAsync(downloadUrl, proposedDownloadFilePath, progress);
			OnFileCreated(libraryBook, actualDownloadedFilePath);

			OnStatusUpdate(actualDownloadedFilePath);
			return actualDownloadedFilePath;
		}

		private static StatusHandler verifyDownload(string actualDownloadedFilePath)
			=> !File.Exists(actualDownloadedFilePath)
			? new StatusHandler { "Downloaded PDF cannot be found" }
			: new StatusHandler();
	}
}
