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
/// Lucene returns nothing for a query that only excludes, so an all-negative query needs a match-all clause
/// added to subtract from. Deciding that by every clause in the query tree rather than by the clauses of each
/// query in it meant a negation with anything inside it - a group, a second term - was read as mixed and left
/// the user with no results.
/// </summary>
[TestClass]
public class PureNegationTests
{
	private const string SIGN = "B0SIGN00001";
	private const string STUDY = "B0STUDY0001";
	private const string HOUND = "B0HOUND0001";

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

	private static LibraryBook book(string asin, string title)
	{
		var contributor = Contributor.GetEmpty();
		var b = new Book(new AudibleProductId(asin), title, null, null, 100, ContentType.Product, [contributor], [contributor], "us");
		return new LibraryBook(b, new DateTime(2026, 8, 15), "account");
	}

	private static readonly List<LibraryBook> library =
	[
		book(SIGN, "Doyle Sign of the Four"),
		book(STUDY, "Doyle A Study in Scarlet"),
		book(HOUND, "Doyle The Hound")
	];

	private string[] search(string query)
		=> [.. new SearchEngine(indexDirectory).Search(query).Docs.Select(d => d.ProductId)];

	/// <summary>The plain shapes, which already worked and must keep working.</summary>
	[TestMethod]
	public void a_single_negation_subtracts_from_the_whole_library()
	{
		search("-title:sign").Should().BeEquivalentTo([STUDY, HOUND]);
		search("-title:sign -title:study").Should().BeEquivalentTo([HOUND]);
		search("-(title:sign)").Should().BeEquivalentTo([STUDY, HOUND]);
	}

	/// <summary>Excluding a group of alternatives at once, which used to return nothing.</summary>
	[TestMethod]
	public void a_negated_group_subtracts_every_alternative_in_it()
		=> search("-(title:sign OR title:study)").Should().BeEquivalentTo([HOUND]);

	/// <summary>
	/// A parenthesized negation used as a subquery. Each group matches nothing on its own, so the query
	/// they make up matched nothing either, whichever way they were combined.
	/// </summary>
	[TestMethod]
	public void negations_combined_as_subqueries_each_get_their_own_match_all()
	{
		search("(-title:sign) AND (-title:study)").Should().BeEquivalentTo([HOUND]);
		search("(-title:sign) OR (-title:study)").Should().BeEquivalentTo([SIGN, STUDY, HOUND]);
	}

	/// <summary>
	/// A query that already has something positive in it must not get a match-all, or the negation would
	/// have the whole library to subtract from and the positive half would stop narrowing anything.
	/// </summary>
	[TestMethod]
	public void a_query_with_a_positive_clause_is_left_alone()
	{
		search("title:hound OR -title:sign").Should().BeEquivalentTo([HOUND]);
		search("title:doyle AND -(title:sign OR title:study)").Should().BeEquivalentTo([HOUND]);
		search("title:sign").Should().BeEquivalentTo([SIGN]);
	}
}
