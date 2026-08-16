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
/// Subtitle removal is all-or-nothing: <c>&lt;title short&gt;</c> stops at the first colon, so "Omnibus: Volume One"
/// and "Omnibus: Volume Two" both become "Omnibus". Finding the affected books used to be impossible -- the analyzer
/// throws punctuation away and Lucene reads a colon in a query as a field separator -- so the index flags the two
/// separate ways a name can lose part of its title.
/// </summary>
[TestClass]
public class SubtitleSearchFieldTests
{
	private const string PLAIN = "B0PLAIN0001";
	private const string SUBTITLE = "B0SUBTITL01";
	private const string COLON = "B0COLON0001";
	private const string BOTH = "B0BOTH00001";

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
			// Windows refuses to delete a file Lucene still holds open, and a leftover temp directory is not
			// worth failing a test over
		}
	}

	private static LibraryBook book(string asin, string title, string? subtitle)
	{
		var contributor = Contributor.GetEmpty();
		var b = new Book(new AudibleProductId(asin), title, subtitle, null, 1, ContentType.Product, [contributor], [contributor], "us");
		return new LibraryBook(b, new DateTime(2026, 8, 15), "account");
	}

	private static readonly List<LibraryBook> library =
	[
		book(PLAIN, "Sign of the Four", null),
		book(SUBTITLE, "A Book Series Omnibus", "Volume One"),
		book(COLON, "Star Trek: The Next Generation", null),
		book(BOTH, "Dune: Book One", "The Graphic Novel")
	];

	private string[] search(string query)
		=> [.. new SearchEngine(indexDirectory).Search(query).Docs.Select(d => d.ProductId)];

	/// <summary>Audible's own subtitle field, which every template except <c>&lt;title&gt;</c> leaves out.</summary>
	[TestMethod]
	[DataRow("HasSubtitle")]
	[DataRow("HasSubtitles")]
	[DataRow("hassubtitle")]
	public void books_with_an_audible_subtitle_are_found(string query)
		=> search(query).Should().BeEquivalentTo([SUBTITLE, BOTH]);

	/// <summary>The riskier case: the colon is inside Audible's title, so shortening cuts the title itself.</summary>
	[TestMethod]
	[DataRow("TitleHasColon")]
	[DataRow("ColonInTitle")]
	[DataRow("titlehascolon")]
	public void books_whose_title_contains_a_colon_are_found(string query)
		=> search(query).Should().BeEquivalentTo([COLON, BOTH]);

	[TestMethod]
	public void the_two_fields_are_independent()
	{
		search("HasSubtitle AND TitleHasColon").Should().BeEquivalentTo([BOTH]);
		search("HasSubtitle OR TitleHasColon").Should().BeEquivalentTo([SUBTITLE, COLON, BOTH]);
	}

	/// <summary>The complement is what makes this useful: everything shortening cannot damage.</summary>
	[TestMethod]
	public void books_a_short_title_leaves_alone_are_found_by_negation()
	{
		search("-HasSubtitle").Should().BeEquivalentTo([PLAIN, COLON]);
		search("-TitleHasColon").Should().BeEquivalentTo([PLAIN, SUBTITLE]);
		search("-HasSubtitle AND -TitleHasColon").Should().BeEquivalentTo([PLAIN]);
	}

	/// <summary>Bool fields combine with the rest of the syntax, which is the point: filter, then liberate.</summary>
	[TestMethod]
	public void the_fields_combine_with_other_search_terms()
		=> search("TitleHasColon AND title:dune").Should().BeEquivalentTo([BOTH]);

	/// <summary>A subtitle is still part of the title text, so searching for its words keeps working.</summary>
	[TestMethod]
	public void subtitle_text_remains_searchable()
		=> search("title:\"volume one\"").Should().BeEquivalentTo([SUBTITLE]);
}
