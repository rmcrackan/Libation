using ApplicationServices;
using AssertionHelper;
using DataLayer;
using LibationFileManager;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DownloadAttemptFailureStoreTests;

/// <summary>
/// Exercises the record of refused downloads against a real SQLite database in a temp directory, which also
/// proves the new migration applies to a fresh database and that the store's queries work on the shipping
/// provider.
/// </summary>
[TestClass]
[DoNotParallelize]
public class DownloadAttemptFailureStoreTests
{
	private string tempLibationFiles = string.Empty;

	[TestInitialize]
	public void Initialize()
	{
		tempLibationFiles = Path.Combine(Path.GetTempPath(), $"libation-attempt-failure-tests-{Guid.NewGuid():N}");
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

	private static LibraryBook Book(string title = "Refused Title", string account = "someone@email.co")
		=> MockLibraryBook.CreateBook(title: title, account: account, bookStatus: LiberatedStatus.NotLiberated);

	[TestMethod]
	public void Round_trips_a_refusal_through_a_real_database()
	{
		var book = Book();
		var failedAt = DateTimeOffset.Now.AddMinutes(-5);

		DownloadAttemptFailureStore.Record(book, DownloadFailureKind.LicenseDenied, "Ownership: not owned", failedAt);

		var deferred = DownloadAttemptFailureStore.GetDeferred(DateTimeOffset.Now);

		deferred.Count.Should().Be(1);
		deferred[0].AudibleProductId.Should().Be(book.Book.AudibleProductId);
		deferred[0].Account.Should().Be(book.Account);
		Assert.AreEqual(DownloadFailureKind.LicenseDenied, deferred[0].Kind);
		deferred[0].ConsecutiveFailures.Should().Be(1);
		deferred[0].Reason.Should().Be("Ownership: not owned");
		Assert.AreEqual(failedAt.UtcTicks, deferred[0].LastFailedAt.UtcTicks);
		Assert.AreEqual(failedAt.AddDays(1).UtcTicks, deferred[0].RetryAfter.UtcTicks);

		File.Exists(Path.Combine(tempLibationFiles, "LibationContext.db")).Should().BeTrue();
	}

	[TestMethod]
	public void Repeated_refusals_of_the_same_kind_push_the_next_attempt_further_out()
	{
		var book = Book();
		var now = DateTimeOffset.Now;

		DownloadAttemptFailureStore.Record(book, DownloadFailureKind.LicenseDenied, "nope", now.AddDays(-8));
		DownloadAttemptFailureStore.Record(book, DownloadFailureKind.LicenseDenied, "nope", now.AddDays(-4));
		DownloadAttemptFailureStore.Record(book, DownloadFailureKind.LicenseDenied, "nope", now);

		var deferred = DownloadAttemptFailureStore.GetDeferred(now);

		// One row per title, not a history.
		deferred.Count.Should().Be(1);
		deferred[0].ConsecutiveFailures.Should().Be(3);
		Assert.AreEqual(now.AddDays(4).UtcTicks, deferred[0].RetryAfter.UtcTicks);
	}

	[TestMethod]
	public void A_refusal_for_a_different_reason_restarts_the_wait()
	{
		var book = Book();
		var now = DateTimeOffset.Now;

		DownloadAttemptFailureStore.Record(book, DownloadFailureKind.LicenseDenied, "nope", now.AddDays(-10));
		DownloadAttemptFailureStore.Record(book, DownloadFailureKind.LicenseDenied, "nope", now.AddDays(-5));
		// Audible now says something different, so the wait built up for the old reason no longer applies.
		DownloadAttemptFailureStore.Record(book, DownloadFailureKind.ServiceInterruption, "outage", now);

		var deferred = DownloadAttemptFailureStore.GetDeferred(now);

		Assert.AreEqual(DownloadFailureKind.ServiceInterruption, deferred[0].Kind);
		deferred[0].ConsecutiveFailures.Should().Be(1);
		Assert.AreEqual(now.AddHours(1).UtcTicks, deferred[0].RetryAfter.UtcTicks);
	}

	[TestMethod]
	public void The_same_title_on_two_accounts_is_tracked_separately()
	{
		// Refused on one account says nothing about whether another account can download it.
		var refused = MockLibraryBook.CreateBook(title: "Shared", account: "a@email.co");
		var allowed = MockLibraryBook.CreateBook(title: "Shared", account: "b@email.co");
		refused.Book.AudibleProductId.Should().Be(allowed.Book.AudibleProductId);

		DownloadAttemptFailureStore.Record(refused, DownloadFailureKind.LicenseDenied, "nope");

		var deferrals = DownloadDeferrals.Load(DateTimeOffset.Now);

		deferrals.IsDeferred(refused).Should().BeTrue();
		deferrals.IsDeferred(allowed).Should().BeFalse();
	}

	[TestMethod]
	public void A_title_is_ready_again_once_its_wait_has_elapsed()
	{
		var book = Book();
		var failedAt = DateTimeOffset.Now.AddDays(-2);

		// One day's wait, two days ago.
		DownloadAttemptFailureStore.Record(book, DownloadFailureKind.LicenseDenied, "nope", failedAt);

		DownloadAttemptFailureStore.GetDeferred(DateTimeOffset.Now).Count.Should().Be(0);
		DownloadAttemptFailureStore.Find(book, DateTimeOffset.Now).Should().BeNull();
		// The row stays so the next failure continues the count rather than restarting the schedule.
		DownloadAttemptFailureStore.Record(book, DownloadFailureKind.LicenseDenied, "nope");
		DownloadAttemptFailureStore.GetDeferred(DateTimeOffset.Now)[0].ConsecutiveFailures.Should().Be(2);
	}

	[TestMethod]
	public void Clear_forgets_a_title()
	{
		var book = Book();
		DownloadAttemptFailureStore.Record(book, DownloadFailureKind.LicenseDenied, "nope");

		DownloadAttemptFailureStore.Clear(book);

		DownloadAttemptFailureStore.GetDeferred(DateTimeOffset.Now).Count.Should().Be(0);
		// And the count starts over, so an explicit retry does not inherit a long wait.
		DownloadAttemptFailureStore.Record(book, DownloadFailureKind.LicenseDenied, "nope");
		DownloadAttemptFailureStore.GetDeferred(DateTimeOffset.Now)[0].ConsecutiveFailures.Should().Be(1);
	}

	[TestMethod]
	public void Clear_leaves_other_titles_alone()
	{
		var kept = Book("Kept");
		var cleared = Book("Cleared");
		DownloadAttemptFailureStore.Record(kept, DownloadFailureKind.LicenseDenied, "nope");
		DownloadAttemptFailureStore.Record(cleared, DownloadFailureKind.LicenseDenied, "nope");

		DownloadAttemptFailureStore.Clear(cleared);

		DownloadAttemptFailureStore.GetDeferred(DateTimeOffset.Now)
			.Select(d => d.AudibleProductId)
			.Should().BeEquivalentTo([kept.Book.AudibleProductId]);
	}

	[TestMethod]
	public void Clearing_a_title_that_was_never_recorded_is_harmless()
	{
		DownloadAttemptFailureStore.Clear(Book());
		DownloadAttemptFailureStore.Clear(null, null);

		DownloadAttemptFailureStore.GetDeferred(DateTimeOffset.Now).Count.Should().Be(0);
	}

	[TestMethod]
	public void Find_returns_only_the_named_title()
	{
		var deferred = Book("Deferred");
		var other = Book("Other");
		DownloadAttemptFailureStore.Record(deferred, DownloadFailureKind.AssetUnavailable, "preorder");

		Assert.AreEqual(DownloadFailureKind.AssetUnavailable, DownloadAttemptFailureStore.Find(deferred, DateTimeOffset.Now)!.Kind);
		DownloadAttemptFailureStore.Find(other, DateTimeOffset.Now).Should().BeNull();
	}

	[TestMethod]
	public void An_overlong_reason_is_stored_truncated()
	{
		var book = Book();
		DownloadAttemptFailureStore.Record(book, DownloadFailureKind.LicenseDenied, new string('x', 5000));

		DownloadAttemptFailureStore.GetDeferred(DateTimeOffset.Now)[0].Reason!.Length.Should().Be(400);
	}

	/// <summary>Puts a title in the library so the real update path can be exercised against it.</summary>
	private static LibraryBook InsertBook(string title, LiberatedStatus bookStatus = LiberatedStatus.Liberated)
	{
		var libraryBook = MockLibraryBook.CreateBook(title: title, bookStatus: bookStatus);

		using (var context = DbContexts.GetContext())
		{
			context.LibraryBooks.Add(new LibraryBook(libraryBook.Book, libraryBook.DateAdded, libraryBook.Account));
			context.SaveChanges();
		}

		return DbContexts.GetLibraryBook_Flat_NoTracking(libraryBook.Book.AudibleProductId)!;
	}

	[TestMethod]
	public async Task Changing_a_titles_download_status_clears_the_record()
	{
		// Setting a title to Not Downloaded is the user saying they want it tried again.
		var book = InsertBook("Refused Then Reset");
		DownloadAttemptFailureStore.Record(book, DownloadFailureKind.LicenseDenied, "nope");

		await book.UpdateBookStatusAsync(LiberatedStatus.NotLiberated);

		DownloadAttemptFailureStore.GetDeferred(DateTimeOffset.Now).Count.Should().Be(0);
	}

	[TestMethod]
	public async Task Editing_tags_does_not_clear_the_record()
	{
		// Otherwise any grid edit would quietly put a refused title back into the next scheduled run.
		var book = InsertBook("Refused Then Tagged", LiberatedStatus.NotLiberated);
		DownloadAttemptFailureStore.Record(book, DownloadFailureKind.LicenseDenied, "nope");

		await book.UpdateTagsAsync("favourite");

		DownloadAttemptFailureStore.GetDeferred(DateTimeOffset.Now).Count.Should().Be(1);
	}

	[TestMethod]
	public void A_broken_database_leaves_downloading_exactly_as_it_was()
	{
		// This is bookkeeping to make downloading quieter. It must never be the reason a download stops.
		Configuration.Instance.PostgresqlConnectionString
			= "Host=127.0.0.1;Port=1;Database=nope;Username=nobody;Password=nothing;Timeout=1;Command Timeout=1";

		var book = Book();

		DownloadAttemptFailureStore.Record(book, DownloadFailureKind.LicenseDenied, "nope");
		DownloadAttemptFailureStore.Clear(book);

		// A failed read defers nothing, so every title is attempted.
		DownloadAttemptFailureStore.GetDeferred(DateTimeOffset.Now).Count.Should().Be(0);
		DownloadAttemptFailureStore.Find(book, DateTimeOffset.Now).Should().BeNull();
	}
}
