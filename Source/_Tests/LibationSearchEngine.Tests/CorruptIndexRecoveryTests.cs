using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AssertionHelper;
using DataLayer;
using LibationSearchEngine;
using Lucene.Net.Index;
using Lucene.Net.Store;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Directory = System.IO.Directory;

namespace SearchEngineTests;

/// <summary>
/// A damaged search index used to block library imports forever: Lucene 3 reports an unreadable segments file as a
/// plain <see cref="IOException"/>, which Libation mistook for a write.lock conflict and retried instead of repairing,
/// and passing create/overwrite to <see cref="IndexWriter"/> does not repair it either.
/// </summary>
[TestClass]
public class CorruptIndexRecoveryTests
{
	private string indexDirectory = null!;

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
			// Windows refuses to delete a file Lucene still holds open, and a leftover temp directory is not
			// worth failing a test over
		}
	}

	private static LibraryBook book(string asin, string title)
	{
		var contributor = Contributor.GetEmpty();
		var b = new Book(new AudibleProductId(asin), title, null, null, 1, ContentType.Product, [contributor], [contributor], "us");
		return new LibraryBook(b, new DateTime(2026, 8, 15), "account");
	}

	private static readonly List<LibraryBook> library
		= [book("B0TEST0001", "Hound of the Baskervilles"), book("B0TEST0002", "Sign of the Four")];

	private SearchEngine reIndex()
	{
		var engine = new SearchEngine(indexDirectory);
		engine.CreateNewIndex(library);
		return engine;
	}

	private void assertIndexIsUsable()
	{
		var engine = new SearchEngine(indexDirectory);
		engine.Search("Baskervilles").Docs.Select(d => d.ProductId).Should().BeEquivalentTo(["B0TEST0001"]);
		engine.Search(SearchEngine.ALL_QUERY).Docs.Should().HaveCount(2);
	}

	private string currentSegmentsFile()
		=> Directory.GetFiles(indexDirectory, "segments_*").OrderBy(f => f.Length).ThenBy(f => f).Last();

	/// <summary>Reproduces the state Lucene leaves behind when a commit is interrupted, eg. by a crash or power loss.</summary>
	private void truncateCurrentSegmentsFile(int keepBytes)
	{
		var file = currentSegmentsFile();
		File.WriteAllBytes(file, File.ReadAllBytes(file).Take(keepBytes).ToArray());
	}

	private static Exception exceptionFromOpeningWriter(string directory)
	{
		using var index = FSDirectory.Open(directory);
		using var analyzer = new Lucene.Net.Analysis.Standard.StandardAnalyzer(SearchEngine.Version);
		return Assert.ThrowsExactly<IOException>(() => new IndexWriter(index, analyzer, true, IndexWriter.MaxFieldLength.UNLIMITED).Dispose());
	}

	[TestMethod]
	public void full_reindex_builds_a_searchable_index()
	{
		reIndex();
		assertIndexIsUsable();
	}

	[TestMethod]
	public void full_reindex_repairs_a_zero_length_segments_file()
	{
		reIndex();
		truncateCurrentSegmentsFile(0);

		reIndex();
		assertIndexIsUsable();
	}

	[TestMethod]
	public void full_reindex_repairs_a_partially_written_segments_file()
	{
		reIndex();
		truncateCurrentSegmentsFile(File.ReadAllBytes(currentSegmentsFile()).Length / 2);

		reIndex();
		assertIndexIsUsable();
	}

	/// <summary>
	/// The nastiest variant: the current commit is intact, so the index looks fine, but IndexWriter's IndexFileDeleter
	/// reads every segments_* file it finds and one unreadable leftover poisons the whole directory.
	/// </summary>
	[TestMethod]
	public void full_reindex_repairs_a_stale_unreadable_segments_file_beside_a_valid_commit()
	{
		reIndex();
		File.WriteAllBytes(Path.Combine(indexDirectory, "segments_9"), []);

		reIndex();
		assertIndexIsUsable();
	}

	[TestMethod]
	public void full_reindex_repairs_a_checksum_mismatch()
	{
		reIndex();
		var file = currentSegmentsFile();
		var bytes = File.ReadAllBytes(file);
		bytes[^1] ^= 0xFF;
		File.WriteAllBytes(file, bytes);

		reIndex();
		assertIndexIsUsable();
	}

	/// <summary>Cloud sync (eg. OneDrive) leaves conflict copies whose names Lucene 3's segments parser rejects.</summary>
	[TestMethod]
	public void full_reindex_repairs_cloud_sync_debris()
	{
		reIndex();
		File.WriteAllText(Path.Combine(indexDirectory, "segments_2 (1)"), "conflict copy");

		reIndex();
		assertIndexIsUsable();
	}

	/// <summary>
	/// A garbled segments.gen sends Lucene looking for an absurd generation, and Lucene 3's base-36 filename
	/// formatter overruns its buffer on the way. Damage does not always announce itself as an IOException.
	/// </summary>
	[TestMethod]
	public void full_reindex_repairs_a_segments_gen_pointing_at_a_bogus_generation()
	{
		reIndex();
		var gen = Path.Combine(indexDirectory, "segments.gen");
		// format(int) then the generation as a long, written twice; corrupt the high bytes of both copies
		var bytes = File.ReadAllBytes(gen);
		bytes[7] = 0x63;
		bytes[15] = 0x63;
		File.WriteAllBytes(gen, bytes);

		reIndex();
		assertIndexIsUsable();
	}

	[TestMethod]
	public void search_repairs_a_zero_length_segments_file()
	{
		reIndex();
		truncateCurrentSegmentsFile(0);

		// the query path recovers via SearchEngineCommands, which keys off IsRecoverableCorruptIndexException
		var ex = Assert.ThrowsExactly<IOException>(() => new SearchEngine(indexDirectory).Search("Baskervilles"));
		SearchEngine.IsRecoverableCorruptIndexException(ex).Should().BeTrue();
	}

	[TestMethod]
	public void an_unreadable_segments_file_is_classified_as_recoverable_corruption()
	{
		reIndex();
		truncateCurrentSegmentsFile(0);

		var ex = exceptionFromOpeningWriter(indexDirectory);

		(ex is CorruptIndexException).Should().BeFalse();
		ex.Message.Should().Be("read past EOF");
		SearchEngine.IsRecoverableCorruptIndexException(ex).Should().BeTrue();
	}

	/// <summary>
	/// A held write.lock is transient and must keep being retried. It derives from <see cref="IOException"/>, so
	/// widening the corruption check to cover IOException must not swallow it and delete a healthy index.
	/// </summary>
	[TestMethod]
	public void a_write_lock_conflict_is_not_classified_as_corruption()
	{
		SearchEngine.IsRecoverableCorruptIndexException(new LockObtainFailedException("Lock obtain timed out")).Should().BeFalse();
		SearchEngine.IsRecoverableCorruptIndexException(new UnauthorizedAccessException()).Should().BeFalse();

		// Windows raises the sharing violation on the lock file before Lucene can turn it into a
		// LockObtainFailedException, so this arrives as a plain IOException. Mistaking it for corruption would
		// delete the index the other holder is using.
		SearchEngine.IsRecoverableCorruptIndexException(
			new IOException(@"The process cannot access the file 'C:\Users\me\Libation\SearchEngine\write.lock' because it is being used by another process."))
			.Should().BeFalse();
	}

	/// <summary>
	/// The risk in repairing anything that is not a lock conflict is over-reach, so a held write.lock has to keep
	/// being retried and must leave the index alone. The lock is held for the whole retry budget rather than
	/// released part way through, which would race with Lucene 3's own lock bookkeeping.
	/// </summary>
	[TestMethod]
	public void a_write_lock_conflict_is_retried_and_leaves_the_index_intact()
	{
		reIndex();
		var indexedBefore = indexFiles();

		using var index = FSDirectory.Open(indexDirectory);
		var writeLock = index.MakeLock(IndexWriter.WRITE_LOCK_NAME);
		writeLock.Obtain().Should().BeTrue();

		try
		{
			// Linux surfaces this as Lucene's LockObtainFailedException, Windows as a sharing violation on the
			// lock file. Either way it must not be read as corruption, and the index must survive.
			var ex = Assert.Throws<IOException>(() => reIndex());

			SearchEngine.IsRecoverableCorruptIndexException(ex).Should().BeFalse();
			indexFiles().Should().BeEquivalentTo(indexedBefore);
		}
		finally
		{
			// a competing NativeFSLock.Obtain can delete the lock file it failed to take, leaving Release nothing
			// to delete. Lucene 3's lock bookkeeping is not what this test is about.
			try { writeLock.Release(); }
			catch (Exception ex) when (ex is LockReleaseFailedException or IOException) { }
		}
	}

	private List<string> indexFiles()
		=> [.. Directory.GetFiles(indexDirectory)
			.Select(Path.GetFileName)
			.OfType<string>()
			.Where(f => f != IndexWriter.WRITE_LOCK_NAME)
			.OrderBy(f => f)];
}
