using ApplicationServices;
using DataLayer;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LibationUiBase.Tests;

[TestClass]
public class LibraryStatsTextTests
{
	private static string Status(int backedUp = 0, int downloadedOnly = 0, int noProgress = 0, int error = 0, int unavailable = 0, int pdfs = 0, int pdfsMissing = 0, int pdfsUnavailable = 0)
		=> LibraryStatsText.Of(new LibraryCommands.LibraryStats(backedUp, downloadedOnly, noProgress, error, unavailable, pdfs, pdfsMissing, pdfsUnavailable, System.Array.Empty<LibraryBook>()));

	[TestMethod]
	public void An_empty_library_says_to_import()
		=> Assert.AreEqual("No books. Begin by importing your library", Status());

	[TestMethod]
	public void A_fully_backed_up_library_counts_its_books()
	{
		Assert.AreEqual("All 1 book backed up", Status(backedUp: 1));
		Assert.AreEqual("All 3 books backed up", Status(backedUp: 3));
	}

	[TestMethod]
	public void Pending_books_list_every_count_and_only_the_problems_there_are()
	{
		Assert.AreEqual("BACKUPS: No progress: 2  In process: 1  Fully backed up: 5", Status(backedUp: 5, downloadedOnly: 1, noProgress: 2));
		Assert.AreEqual("BACKUPS: No progress: 2  In process: 1  Fully backed up: 5  Errors: 1  Unavailable: 4", Status(backedUp: 5, downloadedOnly: 1, noProgress: 2, error: 1, unavailable: 4));
	}

	[TestMethod]
	public void Pdfs_follow_the_books_after_a_bar()
	{
		Assert.AreEqual("All 3 books backed up  |  All 2 PDFs downloaded", Status(backedUp: 3, pdfs: 2));
		Assert.AreEqual("All 3 books backed up  |  PDFs: NOT d/l'ed: 3  Downloaded: 1  Unavailable: 2", Status(backedUp: 3, pdfs: 1, pdfsMissing: 3, pdfsUnavailable: 2));
	}
}
