using DataLayer;
using LibationFileManager;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace FileLiberator.Tests;

/// <summary>
/// Which titles each liberation step selects out of a library. Reported in issue #1947: the CLI had never
/// downloaded PDFs. The cause is here - the audiobook step selects on audio alone, so a title that needs
/// nothing but its PDF is not in the set a plain run iterates.
/// </summary>
[TestClass]
public class PdfBackFillSelectionTests
{
	private static LibraryBook Book(string title, LiberatedStatus bookStatus, LiberatedStatus? pdfStatus, bool hasSupplement = true)
	{
		var libraryBook = MockLibraryBook.CreateBook(title: title, subtitle: "", bookStatus: bookStatus);
		libraryBook.WithPdfStatus(pdfStatus ?? LiberatedStatus.NotLiberated);

		if (hasSupplement)
			libraryBook.Book.AddSupplementDownloadUrl("https://example.com/supplement.pdf");

		return libraryBook;
	}

	private static LibraryBook[] Library() =>
	[
		Book("Needs Audio And Pdf", LiberatedStatus.NotLiberated, LiberatedStatus.NotLiberated),
		Book("Needs Only Its Pdf", LiberatedStatus.Liberated, LiberatedStatus.NotLiberated),
		Book("Needs Nothing", LiberatedStatus.Liberated, LiberatedStatus.Liberated),
		Book("Has No Supplement", LiberatedStatus.Liberated, LiberatedStatus.NotLiberated, hasSupplement: false),
	];

	private static string[] Selected<T>(LibraryBook[] library) where T : Processable, IProcessable<T>
		=> [.. T.Create(Configuration.CreateMockInstance())
			.GetValidLibraryBooks(library)
			.Select(lb => lb.Book.Title)];

	[TestCleanup]
	public void Cleanup() => Configuration.RestoreSingletonInstance();

	[TestMethod]
	public void The_audiobook_step_passes_over_a_title_that_needs_only_its_pdf()
	{
		// Not a bug in itself: this step has nothing to do for such a title. The bug was that a plain
		// liberate run iterated only this set, so nothing else got a look at it either.
		CollectionAssert.AreEqual(new[] { "Needs Audio And Pdf" }, Selected<DownloadDecryptBook>(Library()));
	}

	[TestMethod]
	public void The_pdf_step_selects_every_title_missing_a_pdf_it_can_fetch()
	{
		CollectionAssert.AreEqual(
			new[] { "Needs Audio And Pdf", "Needs Only Its Pdf" },
			Selected<DownloadPdf>(Library()));
	}

	[TestMethod]
	public void A_title_with_no_supplement_is_never_selected_for_a_pdf()
		=> Assert.IsFalse(Selected<DownloadPdf>(Library()).Contains("Has No Supplement"));

	[TestMethod]
	public void A_title_whose_pdf_is_already_downloaded_is_never_selected()
		=> Assert.IsFalse(Selected<DownloadPdf>(Library()).Contains("Needs Nothing"));

	[TestMethod]
	public void Together_the_two_steps_cover_every_title_that_needs_anything()
	{
		var library = Library();
		var covered = Selected<DownloadDecryptBook>(library).Union(Selected<DownloadPdf>(library)).ToArray();

		CollectionAssert.AreEquivalent(new[] { "Needs Audio And Pdf", "Needs Only Its Pdf" }, covered);
	}
}
