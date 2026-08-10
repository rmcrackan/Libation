using DataLayer;

namespace LibationUiBase.Tests;

[TestClass]
public class StandaloneBooksTests
{
	private static LibraryBook libraryBook(string asin, ContentType contentType, string? seriesAsin = null)
	{
		var contributor = Contributor.GetEmpty();
		var book = new Book(
			new AudibleProductId(asin),
			asin,
			null,
			null,
			1,
			contentType,
			[contributor],
			[contributor],
			"us");

		if (seriesAsin is not null)
			book.UpsertSeries(new Series(new AudibleSeriesId(seriesAsin), $"Series {seriesAsin}"), "1");

		return new LibraryBook(book, new DateTime(2026, 8, 10), "account");
	}

	[TestMethod]
	public void ordinary_books_are_standalone()
	{
		var product = libraryBook("BOOK", ContentType.Product);

		var standalone = new[] { product }.StandaloneBooks().ToList();

		CollectionAssert.AreEqual(new[] { "BOOK" }, standalone.Select(lb => lb.Book.AudibleProductId).ToList());
	}

	[TestMethod]
	public void episode_without_its_parent_is_standalone()
	{
		var orphan = libraryBook("EPISODE", ContentType.Episode, "SHOW");

		var standalone = new[] { orphan }.StandaloneBooks().ToList();

		CollectionAssert.AreEqual(new[] { "EPISODE" }, standalone.Select(lb => lb.Book.AudibleProductId).ToList());
	}

	[TestMethod]
	public void episode_with_its_parent_is_not_standalone()
	{
		var parent = libraryBook("SHOW", ContentType.Parent, "SHOW");
		var episode = libraryBook("EPISODE", ContentType.Episode, "SHOW");

		var standalone = new[] { parent, episode }.StandaloneBooks().ToList();

		Assert.AreEqual(0, standalone.Count);
	}

	[TestMethod]
	public void products_and_orphans_are_returned_but_parents_and_parented_episodes_are_not()
	{
		var product = libraryBook("BOOK", ContentType.Product);
		var parent = libraryBook("SHOW", ContentType.Parent, "SHOW");
		var child = libraryBook("CHILD", ContentType.Episode, "SHOW");
		var orphan = libraryBook("ORPHAN", ContentType.Episode, "MISSING_SHOW");

		var standalone = new[] { product, parent, child, orphan }.StandaloneBooks().ToList();

		CollectionAssert.AreEquivalent(
			new[] { "BOOK", "ORPHAN" },
			standalone.Select(lb => lb.Book.AudibleProductId).ToList());
	}
}
