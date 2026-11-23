using DataLayer;
using LibationUiBase;

namespace LibationUiBase.Tests;

[TestClass]
public class SearchExamplesTests
{
	private static LibraryBook Lib(string asin, string title, string author, string narrator = "Some Narrator", int minutes = 300)
	{
		var book = new Book(new AudibleProductId(asin), title, null, null, minutes, ContentType.Product,
			[new Contributor(author)], [new Contributor(narrator)], "us");
		return new LibraryBook(book, new DateTime(2026, 1, 1), "account");
	}

	private static List<LibraryBook> Library() =>
	[
		Lib("B000000001", "The First Book", "Ann Author"),
		Lib("B000000002", "The Second Book", "Ann Author"),
		Lib("B000000003", "Third Thing", "Bob Writer"),
	];

	[TestMethod]
	public void AuthorExample_PicksMostFrequentAuthor()
	{
		var examples = SearchExamples.Build(Library(), _ => 1);
		Assert.AreEqual("author:\"Ann Author\"", examples[0].Query);
		Assert.AreEqual("Books by Ann Author", examples[0].Description);
	}

	[TestMethod]
	public void ZeroCountCandidates_AreDropped()
	{
		var examples = SearchExamples.Build(Library(), q => q == "author:\"Ann Author\"" ? 0 : 1);
		Assert.IsFalse(examples.Any(e => e.Query == "author:\"Ann Author\""));
		Assert.IsTrue(examples.Count >= 3);
	}

	[TestMethod]
	public void ThrowingCandidates_AreDropped()
	{
		var examples = SearchExamples.Build(Library(), q => q.StartsWith("author:") ? throw new InvalidOperationException() : 1);
		Assert.IsFalse(examples.Any(e => e.Query.StartsWith("author:")));
	}

	[TestMethod]
	public void EmptyLibrary_ReturnsEmpty()
		=> Assert.AreEqual(0, SearchExamples.Build([], _ => 1).Count);

	[TestMethod]
	public void FewerThanThreeSurvivors_ReturnsEmpty()
		=> Assert.AreEqual(0, SearchExamples.Build(Library(), q => q.StartsWith("id:") ? 1 : 0).Count);

	[TestMethod]
	public void QuotesAndBackslashesInNames_AreStripped()
	{
		var library = new List<LibraryBook> { Lib("B000000001", "A Title", "Ann \"The Pen\" Au\\thor") };
		var examples = SearchExamples.Build(library, _ => 1);
		Assert.AreEqual("author:\"Ann The Pen Author\"", examples[0].Query);
	}
}
