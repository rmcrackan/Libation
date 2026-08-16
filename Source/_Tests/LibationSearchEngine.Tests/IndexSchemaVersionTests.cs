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
/// A Lucene document only holds the fields that existed when it was written, and nothing in Libation invalidated
/// the index when the field list changed. An index left over from an older version therefore answered a query
/// about a newly added field with silence, which reads as "no such books" rather than "ask again".
/// </summary>
[TestClass]
public class IndexSchemaVersionTests
{
	private string indexDirectory = null!;
	private string versionFile => Path.Combine(indexDirectory, "schema-version.txt");

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
			// a leftover temp directory is not worth failing a test over
		}
	}

	private static readonly List<LibraryBook> library = [libraryBook()];

	private static LibraryBook libraryBook()
	{
		var contributor = Contributor.GetEmpty();
		var b = new Book(new AudibleProductId("B0TEST0001"), "Sign of the Four", null, null, 1, ContentType.Product, [contributor], [contributor], "us");
		return new LibraryBook(b, new DateTime(2026, 8, 15), "account");
	}

	private SearchEngine engine() => new(indexDirectory);

	[TestMethod]
	public void an_index_that_was_never_built_is_outdated()
		=> engine().IsIndexSchemaOutdated().Should().BeTrue();

	[TestMethod]
	public void a_freshly_built_index_is_current()
	{
		engine().CreateNewIndex(library);

		engine().IsIndexSchemaOutdated().Should().BeFalse();
		File.ReadAllText(versionFile).Trim().Should().Be(SearchEngine.SchemaVersion.ToString());
	}

	/// <summary>How every index built before this marker existed looks.</summary>
	[TestMethod]
	public void an_index_with_no_recorded_version_is_outdated()
	{
		engine().CreateNewIndex(library);
		File.Delete(versionFile);

		engine().IsIndexSchemaOutdated().Should().BeTrue();
	}

	[TestMethod]
	[DataRow("0")]
	[DataRow("999")]
	[DataRow("not a version")]
	[DataRow("")]
	public void an_index_recorded_at_a_different_version_is_outdated(string recorded)
	{
		engine().CreateNewIndex(library);
		File.WriteAllText(versionFile, recorded);

		engine().IsIndexSchemaOutdated().Should().BeTrue();
	}

	/// <summary>Rebuilding is what clears the flag, so the check cannot fire on every search.</summary>
	[TestMethod]
	public void rebuilding_brings_an_outdated_index_up_to_date()
	{
		engine().CreateNewIndex(library);
		File.Delete(versionFile);

		engine().CreateNewIndex(library);

		engine().IsIndexSchemaOutdated().Should().BeFalse();
		engine().Search(SearchEngine.ALL_QUERY).Docs.Should().HaveCount(1);
	}
}
