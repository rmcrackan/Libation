using DataLayer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace ApplicationServices.Tests;

/// <summary>
/// Setting download statuses from the files on disk. Only audio used to be looked for, so a library rebuilt
/// from the files already on disk, such as a scheduled container that starts each run with a new database,
/// downloaded every PDF again on every liberate.
/// </summary>
[TestClass]
public class BulkSetDownloadStatusTests
{
	private static LibraryBook Book(LiberatedStatus bookStatus, LiberatedStatus? pdfStatus)
		=> MockLibraryBook.CreateBook(bookStatus: bookStatus, pdfStatus: pdfStatus);

	private static string[] Discover(LibraryBook libraryBook, bool setDownloaded, bool setPending, bool audioOnDisk, bool pdfOnDisk)
	{
		var bulkSetStatus = new BulkSetDownloadStatus([libraryBook], setDownloaded, setPending, _ => audioOnDisk, _ => pdfOnDisk);
		bulkSetStatus.Discover();
		return [.. bulkSetStatus.Messages];
	}

	[TestMethod]
	public void A_new_library_has_its_audio_and_its_pdf_marked_downloaded()
	{
		var messages = Discover(Book(LiberatedStatus.NotLiberated, LiberatedStatus.NotLiberated), setDownloaded: true, setPending: false, audioOnDisk: true, pdfOnDisk: true);

		CollectionAssert.AreEqual(new[] { "1 book to 'Downloaded'", "1 PDF to 'Downloaded'" }, messages);
	}

	[TestMethod]
	public void A_pdf_on_disk_is_marked_downloaded_when_the_audio_already_is()
	{
		var messages = Discover(Book(LiberatedStatus.Liberated, LiberatedStatus.NotLiberated), setDownloaded: true, setPending: false, audioOnDisk: true, pdfOnDisk: true);

		CollectionAssert.AreEqual(new[] { "1 PDF to 'Downloaded'" }, messages);
	}

	[TestMethod]
	public void A_pdf_not_on_disk_is_left_for_liberate_to_fetch()
	{
		var messages = Discover(Book(LiberatedStatus.NotLiberated, LiberatedStatus.NotLiberated), setDownloaded: true, setPending: false, audioOnDisk: true, pdfOnDisk: false);

		CollectionAssert.AreEqual(new[] { "1 book to 'Downloaded'" }, messages);
	}

	[TestMethod]
	public void A_book_without_a_supplement_is_given_no_pdf_status()
	{
		var messages = Discover(Book(LiberatedStatus.Liberated, null), setDownloaded: true, setPending: true, audioOnDisk: true, pdfOnDisk: true);

		Assert.AreEqual(0, messages.Length);
	}

	[TestMethod]
	public void A_downloaded_pdf_missing_from_disk_is_set_pending()
	{
		var messages = Discover(Book(LiberatedStatus.Liberated, LiberatedStatus.Liberated), setDownloaded: false, setPending: true, audioOnDisk: true, pdfOnDisk: false);

		CollectionAssert.AreEqual(new[] { "1 PDF to 'Download Pending'" }, messages);
	}

	[TestMethod]
	public void A_pdf_audible_has_none_of_is_not_set_pending()
	{
		// Error is what a PDF is set to when Audible grants a license with no supplement in it. Setting it
		// pending because no file is on disk would have every liberate ask for it again (issue #1973).
		var messages = Discover(Book(LiberatedStatus.Liberated, LiberatedStatus.Error), setDownloaded: false, setPending: true, audioOnDisk: true, pdfOnDisk: false);

		Assert.AreEqual(0, messages.Length);
	}

	[TestMethod]
	public void A_pdf_on_disk_is_marked_downloaded_even_after_an_error()
	{
		var messages = Discover(Book(LiberatedStatus.Liberated, LiberatedStatus.Error), setDownloaded: true, setPending: false, audioOnDisk: true, pdfOnDisk: true);

		CollectionAssert.AreEqual(new[] { "1 PDF to 'Downloaded'" }, messages);
	}
}
