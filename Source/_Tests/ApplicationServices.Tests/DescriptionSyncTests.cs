using AudibleApi.Common;
using DataLayer;
using DtoImporterService;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ApplicationServices.Tests;

/// <summary>
/// What a scan does to a book's stored description. Descriptions used to be stored as the HTML Audible
/// sends and were never revisited, so every book imported before issue #2002 was fixed is still holding
/// markup that only a re-scan can clear out.
/// </summary>
[TestClass]
public class DescriptionSyncTests
{
	private static Book Book(string description)
		=> new(
			new AudibleProductId("B0DESCRIPT"),
			"Described Title",
			"",
			description,
			600,
			DataLayer.ContentType.Product,
			[new Contributor("Test Author")],
			[new Contributor("Test Narrator")],
			"us");

	private static Item Scanned(string? publisherSummary)
		=> new() { Asin = "B0DESCRIPT", PublisherSummary = publisherSummary };

	[TestMethod]
	public void A_description_stored_as_html_is_rewritten_as_plain_text()
	{
		var book = Book("<p>First.</p> <p>Second.</p>");

		BookImporter.syncDescription(Scanned("<p>First.</p> <p>Second.</p>"), book);

		Assert.AreEqual("First.\nSecond.", book.Description);
	}

	[TestMethod]
	public void A_description_that_has_changed_replaces_the_old_one()
	{
		var book = Book("The old blurb.");

		BookImporter.syncDescription(Scanned("<p>The new blurb.</p>"), book);

		Assert.AreEqual("The new blurb.", book.Description);
	}

	[TestMethod]
	public void A_scan_with_no_summary_leaves_the_stored_description_alone()
	{
		// Episodes come from the catalog, where a missing summary can just as easily mean the response
		// group was not asked for. Blanking a description on that basis would lose it for good.
		var book = Book("The blurb an earlier scan found.");

		BookImporter.syncDescription(Scanned(null), book);

		Assert.AreEqual("The blurb an earlier scan found.", book.Description);
	}

	[TestMethod]
	public void A_scan_with_an_empty_summary_leaves_the_stored_description_alone()
	{
		var book = Book("The blurb an earlier scan found.");

		BookImporter.syncDescription(Scanned("   "), book);

		Assert.AreEqual("The blurb an earlier scan found.", book.Description);
	}
}
