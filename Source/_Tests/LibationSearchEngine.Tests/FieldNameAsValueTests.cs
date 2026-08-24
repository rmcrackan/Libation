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
/// Search field names are ordinary words - absent, podcast, plus, series, finished - and a book's own text is
/// full of them. Searching a field for one used to append the implied <c>:True</c> of a bool field to the value
/// instead of the field, so <c>title:absent</c> became <c>title:absent:True</c> and Lucene refused to parse it.
/// </summary>
[TestClass]
public class FieldNameAsValueTests
{
	private const string TITLED_ABSENT = "B0TITLED001";
	private const string REALLY_ABSENT = "B0MISSING01";
	private const string UNRELATED = "B0OTHER0001";

	private string indexDirectory = null!;

	[TestInitialize]
	public void Initialize()
	{
		indexDirectory = Path.Combine(Path.GetTempPath(), "LibationSearchEngineTests", Guid.NewGuid().ToString("N"));
		Directory.CreateDirectory(indexDirectory);
		new SearchEngine(indexDirectory).CreateNewIndex(library);
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

	private static LibraryBook book(string asin, string title, bool absentFromLastScan = false)
	{
		var contributor = Contributor.GetEmpty();
		var b = new Book(new AudibleProductId(asin), title, null, null, 1, ContentType.Product, [contributor], [contributor], "us");
		return new LibraryBook(b, new DateTime(2026, 8, 15), "account") { AbsentFromLastScan = absentFromLastScan };
	}

	private static readonly List<LibraryBook> library =
	[
		book(TITLED_ABSENT, "The Absent Friend"),
		book(REALLY_ABSENT, "Sign of the Four", absentFromLastScan: true),
		book(UNRELATED, "A Study in Scarlet")
	];

	private string[] search(string query)
		=> [.. new SearchEngine(indexDirectory).Search(query).Docs.Select(d => d.ProductId)];

	/// <summary>The word in a title, not the flag. These are different books, and the search now tells them apart.</summary>
	[TestMethod]
	[DataRow("title:absent")]
	[DataRow("Title:Absent")]
	[DataRow("title:\"absent friend\"")]
	public void a_field_can_be_searched_for_a_word_that_names_another_field(string query)
		=> search(query).Should().BeEquivalentTo([TITLED_ABSENT]);

	/// <summary>The flag still works as a bare keyword, and still ignores what the titles say.</summary>
	[TestMethod]
	public void the_bare_keyword_still_means_the_field()
		=> search("Absent").Should().BeEquivalentTo([REALLY_ABSENT]);

	/// <summary>Only the term right after the colon is a value. Anything later is a field keyword again.</summary>
	[TestMethod]
	public void the_next_term_is_a_field_keyword_again()
	{
		search("title:absent OR Absent").Should().BeEquivalentTo([TITLED_ABSENT, REALLY_ABSENT]);
		search("title:absent AND Absent").Should().BeEquivalentTo([]);
	}

	/// <summary>A bool field given something that is not a bool finds nothing rather than throwing.</summary>
	[TestMethod]
	public void a_bool_field_given_a_non_bool_value_finds_nothing()
		=> search("israted:absent").Should().BeEquivalentTo([]);
}
