using AssertionHelper;
using LibationSearchEngine;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using DataLayer;
using Directory = System.IO.Directory;

namespace SearchEngineTests;

/// <summary>
/// The filter box has to tell a query the user mistyped apart from an index Libation cannot read: one deserves
/// "bad filter string", the other deserves the recovery steps, and only the latter is worth rebuilding over.
/// </summary>
[TestClass]
public class QueryFailureShapeTests
{
	private string indexDirectory = null!;

	[TestInitialize]
	public void Initialize()
	{
		indexDirectory = Path.Combine(Path.GetTempPath(), "LibationSearchEngineTests", Guid.NewGuid().ToString("N"));
		Directory.CreateDirectory(indexDirectory);

		var contributor = Contributor.GetEmpty();
		var book = new Book(new AudibleProductId("B0TEST0001"), "Hound of the Baskervilles", null, null, 1, ContentType.Product, [contributor], [contributor], "us");
		new SearchEngine(indexDirectory).CreateNewIndex(new List<LibraryBook> { new(book, new DateTime(2026, 8, 15), "account") });
	}

	[TestCleanup]
	public void Cleanup()
	{
		try
		{
			if (Directory.Exists(indexDirectory))
				Directory.Delete(indexDirectory, recursive: true);
		}
		catch (IOException) { }
	}

	[TestMethod]
	[DataRow("title:[unclosed")]
	[DataRow("*")]
	[DataRow("AND OR")]
	[DataRow("(((")]
	[DataRow(@"title:""unbalanced")]
	public void a_malformed_query_does_not_look_like_an_unreachable_index(string searchString)
	{
		var engine = new SearchEngine(indexDirectory);

		Exception? thrown = null;
		try
		{
			engine.Search(searchString);
		}
		catch (Exception ex)
		{
			thrown = ex;
		}

		// some of these parse fine and simply match nothing, which is also acceptable
		if (thrown is null)
			return;

		// what must never happen is a parse failure being read as index trouble and triggering a rebuild
		(thrown is IOException).Should().BeFalse();
		SearchEngine.IsRecoverableCorruptIndexException(thrown).Should().BeFalse();
	}
}
