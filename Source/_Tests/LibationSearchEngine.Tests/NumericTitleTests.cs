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
/// Lucene 3 has no numeric type, so a number field is indexed zero-padded to make a range sort correctly:
/// 600 minutes is stored as "00000600.00". Every other field is analyzed and keeps the number as written, so
/// the novel "1984" is stored as "1984". Padding every number in a query regardless of the field it was being
/// compared against made a numeric title unfindable: <c>title:1984</c> searched titles for "00001984.00", and
/// a bare <c>1984</c> could only ever mean "some number field equals 1984".
/// </summary>
[TestClass]
public class NumericTitleTests
{
	private const string ORWELL = "B01984ORWEL";
	private const string PETERS = "B00000014XX";
	private const string CLARKE = "B02001SPACE";
	private const string LONG = "B0LONGBOOK1";
	private const string SHORT = "B0SHORTBOK1";
	private const string FOURTEEN_HOURS = "B0HOUND0001";

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
		var contributor = Contributor.GetEmpty();
		var b = new Book(new AudibleProductId(asin), title, null, null, lengthInMinutes, ContentType.Product, [contributor], [contributor], "us");
		return new LibraryBook(b, new DateTime(2026, 8, 15), "account");
	}

	/// <summary>
	/// Three novels whose titles are numbers, and three books that match those same numbers through a number
	/// field instead, so a test cannot pass by finding the right book for the wrong reason. Both
	/// <c>LengthInMinutes</c> and <c>Hours</c> are indexed, and both are covered.
	/// <para>
	/// The novels' own lengths avoid the collision on purpose. At 866 minutes "14" was found through its
	/// fourteen-hour running time and the test passed even against the padding bug, which is what
	/// <see cref="FOURTEEN_HOURS"/> is here to cover deliberately rather than by accident.
	/// </para>
	/// </summary>
	private static readonly List<LibraryBook> library =
	[
		book(ORWELL, "1984", lengthInMinutes: 675),
		book(PETERS, "14", lengthInMinutes: 780),
		book(CLARKE, "2001: A Space Odyssey", lengthInMinutes: 397),
		book(LONG, "Sign of the Four", lengthInMinutes: 1984),
		book(SHORT, "A Study in Scarlet", lengthInMinutes: 14),
		book(FOURTEEN_HOURS, "The Hound of the Baskervilles", lengthInMinutes: 14 * 60)
	];

	private string[] search(string query)
		=> [.. new SearchEngine(indexDirectory).Search(query).Docs.Select(d => d.ProductId)];

	/// <summary>Typing a title into the search box finds the book, even when the title is a number.</summary>
	[TestMethod]
	[DataRow("1984", ORWELL)]
	[DataRow("14", PETERS)]
	[DataRow("2001", CLARKE)]
	public void a_bare_number_finds_the_novel_named_after_it(string query, string expected)
		=> CollectionAssert.Contains(search(query), expected);

	/// <summary>
	/// Nothing a bare number used to find is given up for it. The number fields were all it could match
	/// before, and it still matches every one of them: the default field holds both spellings of a number,
	/// so searching for both is a union rather than a choice between them.
	/// </summary>
	[TestMethod]
	public void a_bare_number_adds_the_title_to_what_it_already_found()
	{
		//14 hours and 14 minutes both still match, and now so does the novel
		search("14").Should().BeEquivalentTo([PETERS, SHORT, FOURTEEN_HOURS]);
		search("1984").Should().BeEquivalentTo([ORWELL, LONG]);
	}

	/// <summary>Naming the field says which of them is wanted, and no answer is padded wrongly.</summary>
	[TestMethod]
	public void naming_the_field_picks_one_of_them()
	{
		search("title:1984").Should().BeEquivalentTo([ORWELL]);
		search("LengthInMinutes:1984").Should().BeEquivalentTo([LONG]);
		search("title:14").Should().BeEquivalentTo([PETERS]);
		search("LengthInMinutes:14").Should().BeEquivalentTo([SHORT]);
		search("Hours:14").Should().BeEquivalentTo([FOURTEEN_HOURS]);
	}

	/// <summary>A numeric title inside a phrase or beside other words is text too.</summary>
	[TestMethod]
	public void a_number_in_a_phrase_is_text()
	{
		search("title:\"2001 a space odyssey\"").Should().BeEquivalentTo([CLARKE]);
		search("\"2001 a space odyssey\"").Should().BeEquivalentTo([CLARKE]);
		search("title:2001 AND title:odyssey").Should().BeEquivalentTo([CLARKE]);
	}

	/// <summary>The padding exists for ranges, which still sort. This is what must not regress.</summary>
	[TestMethod]
	public void number_ranges_still_sort()
	{
		search("LengthInMinutes:[600 TO 900]").Should().BeEquivalentTo([ORWELL, PETERS, FOURTEEN_HOURS]);
		search("LengthInMinutes:[1 to 400]").Should().BeEquivalentTo([CLARKE, SHORT]);
		search("Hours:[10 TO 20]").Should().BeEquivalentTo([ORWELL, PETERS, FOURTEEN_HOURS]);
	}

	/// <summary>A bare number combines with the rest of the syntax without dragging its expansion along.</summary>
	[TestMethod]
	public void a_bare_number_combines_with_other_terms()
	{
		search("1984 AND title:sign").Should().BeEquivalentTo([LONG]);
		search("-1984").Should().BeEquivalentTo([PETERS, CLARKE, SHORT, FOURTEEN_HOURS]);
	}
}
