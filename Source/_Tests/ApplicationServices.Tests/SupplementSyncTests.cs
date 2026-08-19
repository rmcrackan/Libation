using AudibleApi.Common;
using DataLayer;
using DtoImporterService;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace ApplicationServices.Tests;

/// <summary>
/// What a scan does to the record of a book's supplement. Reported in issue #1973: three titles Audible has no
/// PDF for were asked for on every run, because only a newly imported book ever had its supplement recorded and
/// nothing ever revisited it.
/// </summary>
[TestClass]
public class SupplementSyncTests
{
	private static Book Book(string? supplementUrl = null, LiberatedStatus? pdfStatus = null)
	{
		var book = new DataLayer.Book(
			new AudibleProductId("B0SUPPLMNT"),
			"Supplemented Title",
			"",
			"",
			600,
			DataLayer.ContentType.Product,
			[new Contributor("Test Author")],
			[new Contributor("Test Narrator")],
			"us");

		if (supplementUrl is not null)
			book.SetSupplementDownloadUrl(supplementUrl);

		if (pdfStatus is not null)
			book.UserDefinedItem.SetPdfStatus(pdfStatus);

		return book;
	}

	private static Item Scanned(string? pdfUrl = null, bool? isPdfUrlAvailable = null)
		=> new()
		{
			Asin = "B0SUPPLMNT",
			PdfUrl = pdfUrl is null ? null : new Uri(pdfUrl),
			IsPdfUrlAvailable = isPdfUrlAvailable
		};

	private static string? UrlOf(Book book) => book.Supplements.FirstOrDefault()?.Url;

	[TestMethod]
	public void A_book_that_has_gained_a_supplement_gets_one()
	{
		// Only new books used to reach this, so a title Audible added a PDF to after it was imported never got it.
		var book = Book();

		BookImporter.syncSupplement(Scanned("https://example.com/new.pdf"), book);

		Assert.AreEqual("https://example.com/new.pdf", UrlOf(book));
		Assert.AreEqual(LiberatedStatus.NotLiberated, book.UserDefinedItem.PdfStatus);
	}

	[TestMethod]
	public void A_url_that_has_changed_replaces_the_old_one()
	{
		// Audible reports one supplement per title, so the two must not end up side by side.
		var book = Book("https://example.com/old.pdf");

		BookImporter.syncSupplement(Scanned("https://example.com/new.pdf"), book);

		Assert.AreEqual(1, book.Supplements.Count());
		Assert.AreEqual("https://example.com/new.pdf", UrlOf(book));
	}

	[TestMethod]
	public void A_downloaded_pdf_stays_downloaded_when_its_url_changes()
	{
		var book = Book("https://example.com/old.pdf", LiberatedStatus.Liberated);

		BookImporter.syncSupplement(Scanned("https://example.com/new.pdf"), book);

		Assert.AreEqual(LiberatedStatus.Liberated, book.UserDefinedItem.PdfStatus);
	}

	[TestMethod]
	public void A_supplement_Audible_says_is_unavailable_is_dropped()
	{
		var book = Book("https://example.com/gone.pdf");

		BookImporter.syncSupplement(Scanned(isPdfUrlAvailable: false), book);

		Assert.IsFalse(book.HasPdf);
		// Null, not NotLiberated: it is what a book that never had a PDF looks like, and what stops the grid
		// showing a PDF marker and the counts expecting one.
		Assert.IsNull(book.UserDefinedItem.PdfStatus);
	}

	[TestMethod]
	public void A_pdf_already_downloaded_is_never_dropped()
	{
		// The file is on disk. Audible changing its mind about listing it does not delete it.
		var book = Book("https://example.com/have-it.pdf", LiberatedStatus.Liberated);

		BookImporter.syncSupplement(Scanned(isPdfUrlAvailable: false), book);

		Assert.IsTrue(book.HasPdf);
		Assert.AreEqual(LiberatedStatus.Liberated, book.UserDefinedItem.PdfStatus);
	}

	[TestMethod]
	public void Saying_nothing_about_a_supplement_changes_nothing()
	{
		// Episodes are imported from the catalog, which is never asked for pdf_url, so a missing url there means
		// "not asked" rather than "none exists". Dropping on that would strip every episode's supplement.
		var book = Book("https://example.com/keep.pdf");

		BookImporter.syncSupplement(Scanned(), book);

		Assert.AreEqual("https://example.com/keep.pdf", UrlOf(book));
		Assert.AreEqual(LiberatedStatus.NotLiberated, book.UserDefinedItem.PdfStatus);
	}

	[TestMethod]
	public void A_book_with_no_supplement_and_nothing_to_add_is_left_alone()
	{
		var book = Book();

		BookImporter.syncSupplement(Scanned(isPdfUrlAvailable: false), book);

		Assert.IsFalse(book.HasPdf);
		Assert.IsNull(book.UserDefinedItem.PdfStatus);
	}

	[TestMethod]
	public void The_same_url_twice_is_recorded_once()
	{
		// The duplicate guard used to compare the incoming url to itself.
		var book = Book("https://example.com/same.pdf");

		BookImporter.syncSupplement(Scanned("https://example.com/same.pdf"), book);
		BookImporter.syncSupplement(Scanned("https://example.com/SAME.pdf"), book);

		Assert.AreEqual(1, book.Supplements.Count());
		Assert.AreEqual("https://example.com/same.pdf", UrlOf(book));
	}
}
