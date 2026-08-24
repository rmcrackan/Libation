using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AssertionHelper;
using DataLayer;
using LibationSearchEngine;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Directory = System.IO.Directory;

namespace SearchEngineTests;

/// <summary>
/// A book the index does not hold is worse off than a book the index gets wrong. Filtering keeps the grid rows
/// whose product id the query returned, so a missing book cannot be found by any positive term, and every
/// negated term - which resolves to "every document in the index" - drops it from the grid instead. Reported as
/// issue #1989, where <c>Absent</c> found nothing while <c>-Absent</c> appeared to work perfectly.
/// </summary>
[TestClass]
public class IndexCompletenessTests
{
	private const string PRESENT = "B0PRESENT01";
	private const string ABSENT = "B0ABSENT001";
	private const string THIRD = "B0THIRD0001";

	private string indexDirectory = null!;

	[TestInitialize]
	public void Initialize()
	{
		indexDirectory = Path.Combine(Path.GetTempPath(), "LibationSearchEngineTests", Guid.NewGuid().ToString("N"));
		Directory.CreateDirectory(indexDirectory);
	}

	[TestCleanup]
	public void Cleanup()
	{
		try
		{
			if (Directory.Exists(indexDirectory))
				Directory.Delete(indexDirectory, recursive: true);
		}
		catch (IOException)
		{
			// Windows refuses to delete a file Lucene still holds open, and a leftover temp directory is
			// not worth failing a test over
		}
	}

	private static LibraryBook book(string asin, bool absentFromLastScan = false)
	{
		var contributor = Contributor.GetEmpty();
		var b = new Book(new AudibleProductId(asin), $"Title {asin}", null, null, 1, ContentType.Product, [contributor], [contributor], "us");
		return new LibraryBook(b, new DateTime(2026, 8, 15), "account") { AbsentFromLastScan = absentFromLastScan };
	}

	/// <summary>
	/// EF materializes entities through the private parameterless constructor, so a <see cref="LibraryBook"/>
	/// row whose <c>BookId</c> matches nothing in the Books table arrives with a null <see cref="LibraryBook.Book"/>.
	/// <see cref="DtoImporterService.LibraryBookImporter"/> says as much where it marks those rows absent.
	/// Every index rule reads through that property, so such a row throws while it is being indexed.
	/// </summary>
	private static LibraryBook bookThatCannotBeIndexed()
		=> (LibraryBook)Activator.CreateInstance(typeof(LibraryBook), nonPublic: true)!;

	private string[] search(string query)
		=> [.. new SearchEngine(indexDirectory).Search(query).Docs.Select(d => d.ProductId).Order()];

	[TestMethod]
	public void an_unindexable_book_does_not_cost_the_books_after_it_their_place()
	{
		List<LibraryBook> library = [book(PRESENT), bookThatCannotBeIndexed(), book(ABSENT, absentFromLastScan: true), book(THIRD)];

		var failures = new SearchEngine(indexDirectory).CreateNewIndex(library);

		failures.Should().Be(1);
		search("*:*").Should().BeEquivalentTo([PRESENT, ABSENT, THIRD]);
		search("Absent").Should().BeEquivalentTo([ABSENT]);
		search("-Absent").Should().BeEquivalentTo([PRESENT, THIRD]);
	}

	[TestMethod]
	public void a_library_that_indexes_cleanly_reports_no_failures()
		=> new SearchEngine(indexDirectory).CreateNewIndex([book(PRESENT), book(ABSENT)]).Should().Be(0);

	[TestMethod]
	public void the_indexed_book_count_is_minus_one_before_there_is_an_index()
		=> new SearchEngine(indexDirectory).GetIndexedBookCount().Should().Be(-1);

	[TestMethod]
	public void the_indexed_book_count_is_what_the_index_holds()
	{
		var engine = new SearchEngine(indexDirectory);
		engine.CreateNewIndex([book(PRESENT), book(ABSENT), book(THIRD)]);

		engine.GetIndexedBookCount().Should().Be(3);
	}

	/// <summary>
	/// The signature from issue #1989, and the reason a short index has to be repaired rather than tolerated:
	/// the two halves disagree, so the filter looks half-broken instead of looking like a stale index.
	/// </summary>
	[TestMethod]
	public void a_book_the_index_never_received_reads_as_a_half_working_filter()
	{
		// the absent book is in the library but never made it into the index
		new SearchEngine(indexDirectory).CreateNewIndex([book(PRESENT), book(THIRD)]);

		// nothing to show, even though the library has an absent book
		search("Absent").Should().BeEquivalentTo([]);

		// and the negation quietly leaves it out, which is indistinguishable from working
		search("-Absent").Should().BeEquivalentTo([PRESENT, THIRD]);

		// once the index holds the whole library, both halves agree
		new SearchEngine(indexDirectory).CreateNewIndex([book(PRESENT), book(ABSENT, absentFromLastScan: true), book(THIRD)]);

		search("Absent").Should().BeEquivalentTo([ABSENT]);
		search("-Absent").Should().BeEquivalentTo([PRESENT, THIRD]);
	}
}
