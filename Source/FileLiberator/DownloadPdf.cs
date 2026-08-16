using ApplicationServices;
using DataLayer;
using Dinah.Core.ErrorHandling;
using Dinah.Core.Net.Http;
using LibationFileManager;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace FileLiberator;

public class DownloadPdf : Processable, IProcessable<DownloadPdf>
{
	public override string Name => "Download Pdf";
	public override bool Validate(LibraryBook libraryBook)
		=> !string.IsNullOrWhiteSpace(getdownloadUrl(libraryBook))
		&& !libraryBook.Book.PdfExists;

	public override async Task<StatusHandler> ProcessAsync(LibraryBook libraryBook)
	{
		OnBegin(libraryBook);

		try
		{
			var proposedDownloadFilePath = GetProposedDownloadFilePath(libraryBook);
			var actualDownloadedFilePath = await downloadPdfAsync(libraryBook, proposedDownloadFilePath);
			var result = verifyDownload(actualDownloadedFilePath);

			if (result.IsSuccess)
			{
				SetFileTime(libraryBook, actualDownloadedFilePath);
				if (Path.GetDirectoryName(actualDownloadedFilePath) is string outputDir)
					SetDirectoryTime(libraryBook, outputDir);
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

	/// <summary>
	/// Beside the book's audio files, in the folder the naming templates put that book in.
	/// <para>
	/// The audio file is looked up first so a PDF joins the files already on disk even if they were named by
	/// an older template or moved by hand. That lookup matches on the product id appearing in the path, so it
	/// finds nothing for a library whose folder and file templates omit <c>&lt;id&gt;</c>, and nothing for a
	/// book marked downloaded whose files are not on this machine. Falling back to the folder template rather
	/// than to the Books directory itself keeps those PDFs with their book instead of loose in the library
	/// root, where they also risk colliding with each other.
	/// </para>
	/// </summary>
	internal string GetProposedDownloadFilePath(LibraryBook libraryBook)
	{
		var extension = Path.GetExtension(getdownloadUrl(libraryBook)) ?? ".pdf";

		var destinationDir
			= Path.GetDirectoryName(AudibleFileStorage.Audio.GetPath(libraryBook.Book.AudibleProductId))
			?? AudibleFileStorage.Audio.GetDestinationDirectory(libraryBook, Configuration);

		// Nothing else creates it on the PDF-only path, where the book has no folder yet.
		Directory.CreateDirectory(destinationDir);

		return AudibleFileStorage.Audio.GetCustomDirFilename(libraryBook, destinationDir, extension);
	}

	private static string? getdownloadUrl(LibraryBook libraryBook)
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

	public static DownloadPdf Create(Configuration config) => new() { Configuration = config };
	private DownloadPdf() { }
}
