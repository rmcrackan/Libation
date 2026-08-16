using ApplicationServices;
using AssertionHelper;
using DataLayer;
using LibationFileManager;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;

namespace SearchIndexUpdateGuardTests;

/// <summary>
/// A library change is committed to the database before the search index is updated, so a failure to update that
/// index must be reported rather than propagated. Letting it escape reported a successful scan as
/// "Error importing library" and starved the event's remaining subscribers.
/// </summary>
[TestClass]
[DoNotParallelize]
public class SearchIndexUpdateGuardTests
{
	private string tempLibationFiles = string.Empty;
	private readonly List<Exception> reportedFailures = [];

	[TestInitialize]
	public void Initialize()
	{
		tempLibationFiles = Path.Combine(Path.GetTempPath(), $"libation-search-index-guard-tests-{Guid.NewGuid():N}");
		Directory.CreateDirectory(tempLibationFiles);

		// A fresh Configuration resolves LibationFiles from this variable, so the index lands in the temp dir.
		Environment.SetEnvironmentVariable(LibationFiles.LIBATION_FILES_DIR, tempLibationFiles);
		Configuration.CreateMockInstance();

		SearchEngineCommands.UpdateFailed += recordFailure;
	}

	[TestCleanup]
	public void Cleanup()
	{
		SearchEngineCommands.UpdateFailed -= recordFailure;
		Configuration.RestoreSingletonInstance();
		Environment.SetEnvironmentVariable(LibationFiles.LIBATION_FILES_DIR, null);

		try
		{
			Directory.Delete(tempLibationFiles, recursive: true);
		}
		catch (IOException)
		{
			// a leftover temp directory is not worth failing a test over
		}
	}

	private void recordFailure(object? sender, Exception ex) => reportedFailures.Add(ex);

	/// <summary>Occupying the SearchEngine path with a file leaves the engine nowhere to build its index.</summary>
	private void blockTheIndexDirectory()
		=> File.WriteAllText(Path.Combine(tempLibationFiles, "SearchEngine"), "not a directory");

	private static List<LibraryBook> library()
	{
		var contributor = Contributor.GetEmpty();
		var book = new Book(new AudibleProductId("B0TEST0001"), "Hound of the Baskervilles", null, null, 1, ContentType.Product, [contributor], [contributor], "us");
		return [new LibraryBook(book, new DateTime(2026, 8, 15), "account")];
	}

	[TestMethod]
	public void a_library_size_change_survives_an_unusable_search_index()
	{
		blockTheIndexDirectory();

		SearchEngineCommands.OnLibrarySizeChanged(library());

		reportedFailures.Should().HaveCount(1);
	}

	[TestMethod]
	public void a_book_detail_change_survives_an_unusable_search_index()
	{
		blockTheIndexDirectory();

		SearchEngineCommands.OnBookUserDefinedItemCommitted(library());

		reportedFailures.Should().HaveCount(1);
	}

	[TestMethod]
	public void a_successful_update_reports_no_failure()
	{
		SearchEngineCommands.OnLibrarySizeChanged(library());

		reportedFailures.Should().HaveCount(0);
		Directory.Exists(Path.Combine(tempLibationFiles, "SearchEngine")).Should().BeTrue();
	}
}
