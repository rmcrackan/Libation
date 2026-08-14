using DataLayer;
using LibationUiBase.ProcessQueue;

namespace LibationUiBase.Tests;

[TestClass]
public class BackupRequestTests
{
	private static LibraryBook LibraryBook(
		string asin,
		LiberatedStatus bookStatus = LiberatedStatus.NotLiberated,
		ContentType contentType = ContentType.Product,
		bool absentFromLastScan = false)
	{
		var contributor = Contributor.GetEmpty();
		var book = new Book(
			new AudibleProductId(asin),
			asin,
			null,
			null,
			1,
			contentType,
			[contributor],
			[contributor],
			"us");

		book.UserDefinedItem.BookStatus = bookStatus;

		return new LibraryBook(book, new DateTime(2026, 8, 10), "account") { AbsentFromLastScan = absentFromLastScan };
	}

	[TestMethod]
	public void books_that_need_downloading_are_queueable()
	{
		var request = BackupRequest.Create([
			LibraryBook("NEW"),
			LibraryBook("DONE", LiberatedStatus.Liberated)]);

		CollectionAssert.AreEqual(new[] { "NEW" }, request.Queueable.Select(lb => lb.Book.AudibleProductId).ToList());
		Assert.AreEqual(2, request.RequestedCount);
		Assert.AreEqual(1, request.SkippedCount);
	}

	[TestMethod]
	public void liberated_books_are_skipped_as_already_downloaded()
	{
		var request = BackupRequest.Create([
			LibraryBook("DONE1", LiberatedStatus.Liberated),
			LibraryBook("DONE2", LiberatedStatus.Liberated)]);

		Assert.AreEqual(0, request.Queueable.Length);
		Assert.AreEqual(2, request.SkippedByReason[BackupSkipReason.AlreadyDownloaded]);
	}

	[TestMethod]
	public void errored_books_are_reported_apart_from_downloaded_ones()
	{
		var request = BackupRequest.Create([LibraryBook("BAD", LiberatedStatus.Error)]);

		Assert.AreEqual(1, request.SkippedByReason[BackupSkipReason.PreviousError]);
	}

	[TestMethod]
	public void absent_from_last_scan_outranks_the_download_status()
	{
		//Downloadable is false while a book is absent, so its NotLiberated status cannot be acted on
		var request = BackupRequest.Create([LibraryBook("GONE", absentFromLastScan: true)]);

		Assert.AreEqual(0, request.Queueable.Length);
		Assert.AreEqual(1, request.SkippedByReason[BackupSkipReason.AbsentFromLastScan]);
	}

	[TestMethod]
	public void series_parents_have_no_audio_of_their_own()
	{
		var request = BackupRequest.Create([LibraryBook("SHOW", contentType: ContentType.Parent)]);

		Assert.AreEqual(1, request.SkippedByReason[BackupSkipReason.NoAudioOfItsOwn]);
	}

	[TestMethod]
	public void empty_request_says_nothing_needs_downloading()
	{
		var request = BackupRequest.Create([]);

		Assert.AreEqual(0, request.RequestedCount);
		Assert.AreEqual(0, request.SkippedCount);
		StringAssert.Contains(request.BuildNothingQueuedBody(), "no titles that need downloading");
		Assert.AreEqual("none", request.BuildSkippedLogSummary());
	}

	[TestMethod]
	public void nothing_queued_body_counts_every_reason()
	{
		var request = BackupRequest.Create([
			LibraryBook("DONE1", LiberatedStatus.Liberated),
			LibraryBook("DONE2", LiberatedStatus.Liberated),
			LibraryBook("DONE3", LiberatedStatus.Liberated),
			LibraryBook("BAD", LiberatedStatus.Error),
			LibraryBook("GONE", absentFromLastScan: true)]);

		var body = request.BuildNothingQueuedBody();

		StringAssert.Contains(body, "None of the 5 titles could be queued for download.");
		StringAssert.Contains(body, "Already downloaded: 3");
		StringAssert.Contains(body, "Previously failed to download: 1");
		StringAssert.Contains(body, "Absent from your last library scan: 1");
		//the guidance follows the count so the numbers stay scannable
		StringAssert.Contains(body, "1  (run Scan, or `libationcli scan`, then try again)");
	}

	[TestMethod]
	public void log_summary_is_a_compact_breakdown()
	{
		var request = BackupRequest.Create([
			LibraryBook("DONE1", LiberatedStatus.Liberated),
			LibraryBook("DONE2", LiberatedStatus.Liberated),
			LibraryBook("GONE", absentFromLastScan: true)]);

		Assert.AreEqual("already downloaded: 2, absent from last scan: 1", request.BuildSkippedLogSummary());
	}
}
