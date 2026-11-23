using System;
using System.IO;
using AssertionHelper;
using DataLayer;
using LibationSearchEngine;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Directory = System.IO.Directory;

namespace SearchEngineTests;

[TestClass]
public class IndexSchemaTests
{
	private string indexDirectory = null!;
	private SearchEngine engine = null!;

	[TestInitialize]
	public void Initialize()
	{
		indexDirectory = Path.Combine(Path.GetTempPath(), "LibationSearchEngineTests", Guid.NewGuid().ToString("N"));
		Directory.CreateDirectory(indexDirectory);
		engine = new SearchEngine(indexDirectory);
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
		}
	}

	private static LibraryBook book()
	{
		var contributor = Contributor.GetEmpty();
		var b = new Book(new AudibleProductId("B0SCHEMA001"), "Schema", null, null, 1, ContentType.Product, [contributor], [contributor], "us");
		return new LibraryBook(b, new DateTime(2026, 8, 15), "account");
	}

	[TestMethod]
	public void NoIndexIsNotCurrent()
		=> engine.IsIndexSchemaCurrent().Should().BeFalse();

	[TestMethod]
	public void FreshIndexIsCurrent()
	{
		engine.CreateNewIndex([book()]);
		engine.IsIndexSchemaCurrent().Should().BeTrue();
	}

	[TestMethod]
	public void DeletedSchemaFileIsNotCurrent()
	{
		engine.CreateNewIndex([book()]);
		File.Delete(Path.Combine(indexDirectory, "schema.txt"));
		engine.IsIndexSchemaCurrent().Should().BeFalse();
	}

	[TestMethod]
	public void AlteredSchemaFileIsNotCurrent()
	{
		engine.CreateNewIndex([book()]);
		File.WriteAllText(Path.Combine(indexDirectory, "schema.txt"), "old rules");
		engine.IsIndexSchemaCurrent().Should().BeFalse();
	}
}
