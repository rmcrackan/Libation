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
/// Stephen King's <i>11/22/63</i>, whose title is nothing but digits and punctuation. It sits either side of the
/// line the number handling draws: the analyzer keeps a date like this as one token, so it is text and must not
/// be zero-padded, while <c>title:11*</c> is a bare number in a text field and must not be padded either.
/// <para>
/// Audible lists it with slashes; a hyphenated spelling turns up in filenames and in some catalogues, so both
/// are covered. Every query here is also checked against books whose <em>length</em> is 11, 22 or 63 minutes,
/// so nothing can pass by matching a number field instead of the title.
/// </para>
/// </summary>
[TestClass]
public class PunctuatedNumericTitleTests
{
	private const string HYPHEN = "B0KINGHYPH1";
	private const string SLASH = "B0KINGSLSH1";
	private const string ELEVEN_MIN = "B0DECOY11MN";
	private const string TWENTYTWO_MIN = "B0DECOY22MN";
	private const string SIXTYTHREE_MIN = "B0DECOY63MN";
	private const string SHINING = "B0OTHERBOOK";

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

	private static LibraryBook book(string asin, string title, int lengthInMinutes)
	{
		var b = new Book(new AudibleProductId(asin), title, null, null, lengthInMinutes, ContentType.Product,
			[new Contributor("Stephen King")], [new Contributor("Craig Wasson")], "us");
		return new LibraryBook(b, new DateTime(2026, 8, 15), "account");
	}

	private static readonly List<LibraryBook> library =
	[
		book(HYPHEN, "11-22-63", lengthInMinutes: 1849),
		book(SLASH, "11/22/63", lengthInMinutes: 1849),
		book(ELEVEN_MIN, "The Eleven Minute Book", lengthInMinutes: 11),
		book(TWENTYTWO_MIN, "The Twenty Two Minute Book", lengthInMinutes: 22),
		book(SIXTYTHREE_MIN, "The Sixty Three Minute Book", lengthInMinutes: 63),
		book(SHINING, "The Shining", lengthInMinutes: 950)
	];

	private string[] search(string query)
		=> [.. new SearchEngine(indexDirectory).Search(query).Docs.Select(d => d.ProductId)];

	/// <summary>
	/// Typing the title finds the book. The hyphens must not be read as Lucene's exclusion operator either,
	/// which would turn this into "11, but not 22 or 63" and return most of the library.
	/// </summary>
	[TestMethod]
	[DataRow("11-22-63", HYPHEN)]
	[DataRow("11/22/63", SLASH)]
	public void the_title_finds_the_book(string query, string expected)
		=> search(query).Should().BeEquivalentTo([expected]);

	/// <summary>Naming the field, and quoting, reach it the same way.</summary>
	[TestMethod]
	[DataRow("title:11-22-63", HYPHEN)]
	[DataRow("title:\"11-22-63\"", HYPHEN)]
	[DataRow("\"11-22-63\"", HYPHEN)]
	[DataRow("title:11/22/63", SLASH)]
	[DataRow("title:\"11/22/63\"", SLASH)]
	public void the_title_is_reachable_through_the_title_field(string query, string expected)
		=> search(query).Should().BeEquivalentTo([expected]);

	/// <summary>
	/// A wildcard reaches it, which is what a bare number in a text field could not do while every number in
	/// a query was zero-padded: <c>title:11*</c> used to go looking for <c>00000011.00*</c>.
	/// </summary>
	[TestMethod]
	public void a_wildcard_on_the_leading_number_reaches_both_spellings()
		=> search("title:11*").Should().BeEquivalentTo([HYPHEN, SLASH]);

	/// <summary>
	/// The date is one token, not three, so its parts are not separately searchable. Searching 11 finds the
	/// book that is eleven minutes long and nothing else. Worth pinning: it reads like a bug otherwise, and
	/// the fix for it would be to split the token, which would cost the exact-title match above.
	/// </summary>
	[TestMethod]
	public void a_single_part_of_the_date_is_not_a_search_for_the_book()
	{
		search("11").Should().BeEquivalentTo([ELEVEN_MIN]);
		search("22").Should().BeEquivalentTo([TWENTYTWO_MIN]);
		search("63").Should().BeEquivalentTo([SIXTYTHREE_MIN]);
	}

	/// <summary>And the number fields still answer for themselves, on a book whose title is a number too.</summary>
	[TestMethod]
	public void the_length_of_the_book_is_still_its_own_search()
	{
		search("1849").Should().BeEquivalentTo([HYPHEN, SLASH]);
		search("LengthInMinutes:1849").Should().BeEquivalentTo([HYPHEN, SLASH]);
		search("title:1849").Should().BeEquivalentTo([]);
		search("LengthInMinutes:[1800 TO 1900]").Should().BeEquivalentTo([HYPHEN, SLASH]);
	}
}
