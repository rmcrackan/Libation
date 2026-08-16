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
		string? createdDirectory = null;

		try
		{
			var proposedDownloadFilePath = GetProposedDownloadFilePath(libraryBook);
			createdDirectory = createDirectoryFor(proposedDownloadFilePath);
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
			removeIfLeftEmpty(createdDirectory);
			OnCompleted(libraryBook);
		}
	}

	/// <summary>The directory this run had to create, or null when it was already there.</summary>
	private static string? createDirectoryFor(string filePath)
	{
		if (Path.GetDirectoryName(filePath) is not string directory || Directory.Exists(directory))
			return null;

		Directory.CreateDirectory(directory);
		return directory;
	}

	/// <summary>
	/// A PDF-only download is the one case that has to make the book's folder before it has anything to put
	/// in it. Without this, every failed download would leave an empty folder in the library.
	/// </summary>
	private static void removeIfLeftEmpty(string? directory)
	{
		try
		{
			if (directory is not null && Directory.Exists(directory) && !Directory.EnumerateFileSystemEntries(directory).Any())
				Directory.Delete(directory);
		}
		catch (Exception ex)
		{
			Serilog.Log.Logger.Debug(ex, "Could not remove the empty folder left by a failed PDF download: {directory}", directory);
		}
	}

	/// <summary>
	/// Beside the book's audio files, in the folder the naming templates put that book in. The directory may
	/// not exist yet; see <see cref="createDirectoryFor"/>.
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
