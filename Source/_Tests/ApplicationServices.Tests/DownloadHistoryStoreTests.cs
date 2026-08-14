using ApplicationServices;
using AssertionHelper;
using LibationFileManager;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;

namespace DownloadHistoryStoreTests;

/// <summary>
/// Exercises the download history against a real SQLite database in a temp directory, which also proves the
/// new migration applies to a fresh database and that the store's queries work on the shipping provider.
/// </summary>
[TestClass]
[DoNotParallelize]
public class DownloadHistoryStoreTests
{
	private string tempLibationFiles = string.Empty;

	[TestInitialize]
	public void Initialize()
	{
		tempLibationFiles = Path.Combine(Path.GetTempPath(), $"libation-download-history-tests-{Guid.NewGuid():N}");
		Directory.CreateDirectory(tempLibationFiles);

		// A fresh Configuration resolves LibationFiles from this variable, so the database lands in the temp dir.
		Environment.SetEnvironmentVariable(LibationFiles.LIBATION_FILES_DIR, tempLibationFiles);
		Configuration.CreateMockInstance();
	}

	[TestCleanup]
	public void Cleanup()
	{
		Configuration.RestoreSingletonInstance();
		Environment.SetEnvironmentVariable(LibationFiles.LIBATION_FILES_DIR, null);

		try
		{
			Directory.Delete(tempLibationFiles, recursive: true);
		}
		catch (IOException)
		{
			// A leftover temp directory is not worth failing a test over.
		}
	}

	[TestMethod]
	public void Round_trips_a_recorded_download_through_a_real_database()
	{
		var completedAt = DateTimeOffset.Now.AddHours(-2);

		DownloadHistoryStore.Record("B004V3LWLM", isAudiblePlus: true, bytes: 123_456_789, completedAt: completedAt);

		var entries = DownloadHistoryStore.GetCurrentWindow(DateTimeOffset.Now);

		entries.Count.Should().Be(1);
		entries[0].AudibleProductId.Should().Be("B004V3LWLM");
		entries[0].IsAudiblePlus.Should().BeTrue();
		entries[0].Bytes.Should().Be(123_456_789);
		// Round-tripped through UTC ticks, so this is exact rather than approximate.
		Assert.AreEqual(completedAt.UtcTicks, entries[0].CompletedAt.UtcTicks);
	}

	[TestMethod]
	public void Creates_the_database_file_and_applies_the_new_migration()
	{
		DownloadHistoryStore.Record("ASIN1", isAudiblePlus: false, bytes: 1);

		File.Exists(Path.Combine(tempLibationFiles, "LibationContext.db")).Should().BeTrue();
		DownloadHistoryStore.GetSince(DateTimeOffset.Now.AddMinutes(-1)).Count.Should().Be(1);
	}

	[TestMethod]
	public void GetSince_excludes_downloads_before_the_cutoff()
	{
		var now = DateTimeOffset.Now;
		DownloadHistoryStore.Record("OLD", isAudiblePlus: true, bytes: 1, completedAt: now.AddHours(-23));
		DownloadHistoryStore.Record("NEW", isAudiblePlus: true, bytes: 1, completedAt: now.AddHours(-1));

		var lastTwoHours = DownloadHistoryStore.GetSince(now.AddHours(-2));

		lastTwoHours.Count.Should().Be(1);
		lastTwoHours[0].AudibleProductId.Should().Be("NEW");

		// Both are inside the rolling window the limit uses.
		DownloadHistoryStore.GetCurrentWindow(now).Count.Should().Be(2);
	}

	[TestMethod]
	public void Recording_prunes_rows_older_than_the_retention_period()
	{
		var now = DateTimeOffset.Now;
		DownloadHistoryStore.Record("ANCIENT", isAudiblePlus: true, bytes: 1, completedAt: now.AddDays(-4));
		DownloadHistoryStore.Record("YESTERDAY", isAudiblePlus: true, bytes: 1, completedAt: now.AddHours(-20));

		var asins = DownloadHistoryStore.GetSince(DateTimeOffset.MinValue).Select(e => e.AudibleProductId).ToList();

		asins.Contains("ANCIENT").Should().BeFalse();
		asins.Contains("YESTERDAY").Should().BeTrue();
	}

	[TestMethod]
	public void Entries_are_returned_oldest_first()
	{
		var now = DateTimeOffset.Now;
		DownloadHistoryStore.Record("SECOND", isAudiblePlus: true, bytes: 1, completedAt: now.AddHours(-2));
		DownloadHistoryStore.Record("FIRST", isAudiblePlus: true, bytes: 1, completedAt: now.AddHours(-5));

		var entries = DownloadHistoryStore.GetCurrentWindow(now);

		entries.Select(e => e.AudibleProductId).Should().BeEquivalentTo(["FIRST", "SECOND"]);
	}

	[TestMethod]
	public void A_broken_database_cannot_fail_a_finished_download()
	{
		// Recording runs after a download has already succeeded, so a database problem must not throw.
		Configuration.Instance.PostgresqlConnectionString
			= "Host=127.0.0.1;Port=1;Database=nope;Username=nobody;Password=nothing;Timeout=1;Command Timeout=1";

		DownloadHistoryStore.Record("ASIN1", isAudiblePlus: true, bytes: 1);

		// And a failed read reports an empty window rather than blocking downloads.
		DownloadHistoryStore.GetCurrentWindow(DateTimeOffset.Now).Count.Should().Be(0);
	}
}
