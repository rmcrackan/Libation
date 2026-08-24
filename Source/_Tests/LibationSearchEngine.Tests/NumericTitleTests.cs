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
	/// Three novels whose titles are numbers, and two books whose lengths collide with those numbers, so a
	/// test cannot pass by finding the right book for the wrong reason.
	/// <para>
	/// The lengths are picked to avoid a second such coincidence. <c>Hours</c> is indexed as well as
	/// <c>LengthInMinutes</c>, so at 866 minutes "14" found the novel through its 14-hour running time and
	/// passed even against the padding bug. Nothing here is 14 or 1984 hours long.
	/// </para>
	/// </summary>
	private static readonly List<LibraryBook> library =
	[
		book(ORWELL, "1984", lengthInMinutes: 675),
		book(PETERS, "14", lengthInMinutes: 780),
		book(CLARKE, "2001: A Space Odyssey", lengthInMinutes: 397),
		book(LONG, "Sign of the Four", lengthInMinutes: 1984),
		book(SHORT, "A Study in Scarlet", lengthInMinutes: 14)
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
	/// And still finds the book whose length is that number, which is all it could find before. The default
	/// field holds both spellings, so this is a union rather than a choice between the two.
	/// </summary>
	[TestMethod]
	public void a_bare_number_still_finds_the_number_fields_too()
	{
		search("1984").Should().BeEquivalentTo([ORWELL, LONG]);
		search("14").Should().BeEquivalentTo([PETERS, SHORT]);
	}

	/// <summary>Naming the field says which of the two is wanted, and neither answer is padded wrongly.</summary>
	[TestMethod]
	public void naming_the_field_picks_one_of_the_two()
	{
		search("title:1984").Should().BeEquivalentTo([ORWELL]);
		search("LengthInMinutes:1984").Should().BeEquivalentTo([LONG]);
		search("title:14").Should().BeEquivalentTo([PETERS]);
		search("LengthInMinutes:14").Should().BeEquivalentTo([SHORT]);
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
		search("LengthInMinutes:[600 TO 900]").Should().BeEquivalentTo([ORWELL, PETERS]);
		search("LengthInMinutes:[1 to 400]").Should().BeEquivalentTo([CLARKE, SHORT]);
		search("Hours:[10 TO 20]").Should().BeEquivalentTo([ORWELL, PETERS]);
	}

	/// <summary>A bare number combines with the rest of the syntax without dragging its expansion along.</summary>
	[TestMethod]
	public void a_bare_number_combines_with_other_terms()
	{
		search("1984 AND title:sign").Should().BeEquivalentTo([LONG]);
		search("-1984").Should().BeEquivalentTo([PETERS, CLARKE, SHORT]);
	}
}
