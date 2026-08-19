using ApplicationServices;
using DataLayer;
using Dinah.Core;
using Dinah.Core.ErrorHandling;
using Dinah.Core.Net.Http;
using FileManager;
using LibationFileManager;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace FileLiberator;

public class DownloadPdf : Processable, IProcessable<DownloadPdf>, ILicensedDownload
{
	public override string Name => "Download Pdf";

	/// <summary>
	/// A supplement is delivered through a content license, the same request the audiobook download makes, so
	/// a refusal of one is a refusal of the other and belongs in the record that keeps a scheduled run from
	/// asking again.
	/// </summary>
	protected override bool RecordsAttemptFailures => true;

	/// <inheritdoc/>
	public DownloadOptions.LicenseInfo? LicenseInfo { get; set; }

	/// <inheritdoc/>
	public DownloadOptions.LicenseInfo? ObtainedLicense { get; private set; }

	/// <summary>
	/// A title Audible has a supplement for, whose supplement is not downloaded and was not written off. The
	/// status check matches <see cref="DataLayer.LibraryBookQueries.NeedsPdfDownload"/>, and treats
	/// <see cref="LiberatedStatus.Error"/> as "don't retry" exactly as the audiobook download does through
	/// <c>AudioExists</c>.
	/// </summary>
	public override bool Validate(LibraryBook libraryBook)
		=> !string.IsNullOrWhiteSpace(getdownloadUrl(libraryBook))
		&& libraryBook.Book.UserDefinedItem.PdfStatus is LiberatedStatus.NotLiberated;

	public override async Task<StatusHandler> ProcessAsync(LibraryBook libraryBook)
	{
		OnBegin(libraryBook);
		string? createdDirectory = null;
		ObtainedLicense = null;

		try
		{
			// A license this run already holds is used as it is. Requesting another would repeat the request
			// the audiobook download just made, which is what filled the log in issue #1973.
			var carriedLicense = LicenseInfo;
			var license = carriedLicense ?? await requestLicenseAsync(libraryBook);
			ObtainedLicense = license;

			if (license.PdfUrl is not string downloadUrl)
				return await noSupplementAvailableAsync(libraryBook);

			var proposedDownloadFilePath = GetProposedDownloadFilePath(libraryBook);
			createdDirectory = createDirectoryFor(proposedDownloadFilePath);

			var result = await downloadAndVerifyAsync(libraryBook, downloadUrl, proposedDownloadFilePath);

			// Audible's download links are signed and expire, and a carried license was granted before an
			// audiobook download that may have taken hours. One fresh license is worth trying before the
			// title is reported as failed; a license requested moments ago would only fail the same way.
			if (!result.IsSuccess && carriedLicense is not null)
			{
				Serilog.Log.Logger.Information(
					"Requesting a new license to fetch the supplement of {libraryBook}: the one from its audiobook download did not work.",
					libraryBook.LogFriendly());

				license = await requestLicenseAsync(libraryBook);
				ObtainedLicense = license;

				if (license.PdfUrl is not string freshDownloadUrl)
					return await noSupplementAvailableAsync(libraryBook);

				result = await downloadAndVerifyAsync(libraryBook, freshDownloadUrl, proposedDownloadFilePath);
			}

			await libraryBook.UpdatePdfStatusAsync(result.IsSuccess ? LiberatedStatus.Liberated : LiberatedStatus.NotLiberated);

			return result;
		}
		// Refusals are left to reach ProcessSingleAsync, which records them for every step alike. Everything
		// else is still swallowed here, which is what stopped a missing PDF from taking the app down with it.
		catch (Exception ex) when (DownloadFailureClassifier.Classify(ex) is null)
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

	private async Task<DownloadOptions.LicenseInfo> requestLicenseAsync(LibraryBook libraryBook)
	{
		var api = await libraryBook.GetApiAsync();
		return await DownloadOptions.GetSupplementLicenseAsync(api, libraryBook, Configuration);
	}

	/// <summary>
	/// Audible granted a license and it carried no supplement link, which is Audible saying this title has no
	/// PDF to give. Written off the way the audiobook download writes off a title it should not attempt again:
	/// <see cref="LiberatedStatus.Error"/> means "don't retry" for a supplement exactly as it does for audio,
	/// and the same controls - a forced or named <c>liberate</c> run, Set PDF Not Downloaded - set it back.
	/// <para>
	/// Reported as a completed step and not as a failure. Nothing went wrong and nothing is left to attempt, and
	/// a failure here would put the queue's Abort / Retry / Ignore question to the user about a book whose audio
	/// is fine.
	/// </para>
	/// </summary>
	private async Task<StatusHandler> noSupplementAvailableAsync(LibraryBook libraryBook)
	{
		const string explanation = "Audible has no PDF for this title, although the library listing says it has one. "
			+ "Its PDF will not be asked for again; set the PDF status back to Not Downloaded to try once more.";

		Serilog.Log.Logger.Information(
			"Audible granted a license for {libraryBook} that carried no supplement link, so it has no PDF to download. {explanation}",
			libraryBook.LogFriendly(),
			explanation);

		OnStatusUpdate(explanation);
		await libraryBook.UpdatePdfStatusAsync(LiberatedStatus.Error);

		return new StatusHandler();
	}

	/// <summary>
	/// Fetches the supplement and keeps it only if what came back is one. A transport failure is reported as a
	/// failed status rather than thrown, so that a license worth retrying still gets retried.
	/// </summary>
	private async Task<StatusHandler> downloadAndVerifyAsync(LibraryBook libraryBook, string downloadUrl, string proposedDownloadFilePath)
	{
		string actualDownloadedFilePath;
		try
		{
			actualDownloadedFilePath = await downloadPdfAsync(downloadUrl, proposedDownloadFilePath);
		}
		catch (Exception ex)
		{
			Serilog.Log.Logger.Error(ex, "Error downloading PDF");
			return new StatusHandler { $"Error downloading PDF. See log for details. Error summary: {ex.Message}" };
		}

		var result = verifyDownload(actualDownloadedFilePath);

		if (result.IsSuccess)
		{
			OnFileCreated(libraryBook, actualDownloadedFilePath);
			SetFileTime(libraryBook, actualDownloadedFilePath);
			if (Path.GetDirectoryName(actualDownloadedFilePath) is string outputDir)
				SetDirectoryTime(libraryBook, outputDir);
		}
		else
		{
			// Keeping it would leave a file in the library that is not the supplement, under a name that
			// says it is, and would stop the folder cleanup in ProcessAsync from running.
			FileUtility.SaferDelete(actualDownloadedFilePath);
		}

		return result;
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

	private async Task<string> downloadPdfAsync(string downloadUrl, string proposedDownloadFilePath)
	{
		var progress = new Progress<DownloadProgress>(OnStreamingProgressChanged);

		var client = new HttpClient();

		var actualDownloadedFilePath = await client.DownloadFileAsync(downloadUrl, proposedDownloadFilePath, progress);

		OnStatusUpdate(actualDownloadedFilePath);
		return actualDownloadedFilePath;
	}

	/// <summary>
	/// That the file exists says only that the server sent a body, and an Audible error is a 200 with a JSON
	/// body like any other response. Dinah's downloader renames by Content-Disposition, so such a body lands
	/// in the book's folder under whatever Audible called it and the title is recorded as having its PDF.
	/// <para>
	/// A supplement is whatever its URL says it is, usually a PDF and occasionally an archive, so only a file
	/// named .pdf is held to the PDF header. Everything else is checked for the one thing no supplement of
	/// any kind starts with: the opening character of a JSON or markup document.
	/// </para>
	/// </summary>
	internal static StatusHandler verifyDownload(string actualDownloadedFilePath)
	{
		if (!File.Exists(actualDownloadedFilePath))
			return new StatusHandler { "Downloaded PDF cannot be found" };

		var firstBytes = readFirstBytes(actualDownloadedFilePath);

		if (firstBytes.Length == 0)
			return new StatusHandler { "Downloaded PDF is empty" };

		if (Path.GetExtension(actualDownloadedFilePath).EqualsInsensitive(".pdf")
			&& !firstBytes.AsSpan().StartsWith("%PDF"u8))
			return new StatusHandler { "What Audible returned is not a PDF. Nothing was saved; the PDF is still marked as not downloaded." };

		if (firstBytes[0] is (byte)'{' or (byte)'[' or (byte)'<')
			return new StatusHandler { "Audible returned a document instead of the supplement. Nothing was saved; the PDF is still marked as not downloaded." };

		return new StatusHandler();
	}

	/// <summary>Enough to recognise a file signature, and past a byte-order mark if the body carries one.</summary>
	private static byte[] readFirstBytes(string path)
	{
		var buffer = new byte[8];

		using var stream = File.OpenRead(path);
		var read = buffer[..stream.ReadAtLeast(buffer, buffer.Length, throwOnEndOfStream: false)];

		return read.AsSpan().StartsWith("\uFEFF"u8) ? read[3..] : read;
	}

	public static DownloadPdf Create(Configuration config) => new() { Configuration = config };
	private DownloadPdf() { }
}
