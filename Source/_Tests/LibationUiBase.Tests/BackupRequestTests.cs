using ApplicationServices;
using DataLayer;
using LibationUiBase.ProcessQueue;

namespace LibationUiBase.Tests;

[TestClass]
public class BackupRequestTests
{
	private static LibraryBook LibraryBook(
		string asin,
		LiberatedStatus bookStatus = LiberatedStatus.NotLiberated,
		bool absentFromLastScan = false)
	{
		var contributor = Contributor.GetEmpty();
		var book = new Book(
			new AudibleProductId(asin),
			asin,
			null,
			null,
			1,
			ContentType.Product,
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
		Assert.AreEqual(2, request.Skipped(BackupRequest.SkipReason.AlreadyDownloaded));
	}

	[TestMethod]
	public void errored_books_are_reported_apart_from_downloaded_ones()
	{
		var request = BackupRequest.Create([LibraryBook("BAD", LiberatedStatus.Error)]);

		Assert.AreEqual(1, request.Skipped(BackupRequest.SkipReason.PreviousError));
	}

	[TestMethod]
	public void absent_from_last_scan_outranks_the_download_status()
	{
		//Downloadable is false while a book is absent, so its NotLiberated status cannot be acted on
		var request = BackupRequest.Create([LibraryBook("GONE", absentFromLastScan: true)]);

		Assert.AreEqual(0, request.Queueable.Length);
		Assert.AreEqual(1, request.Skipped(BackupRequest.SkipReason.AbsentFromLastScan));
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

		Assert.AreEqual("already downloaded: 2, absent from your last library scan: 1", request.BuildSkippedLogSummary());
	}

	private static DownloadDeferrals Deferring(
		LibraryBook libraryBook,
		DownloadFailureKind kind = DownloadFailureKind.LicenseDenied,
		int hoursUntilRetry = 20)
		=> DownloadDeferrals.Create([
			new DeferredDownload(
				libraryBook.Account,
				libraryBook.Book.AudibleProductId,
				kind,
				ConsecutiveFailures: 1,
				LastFailedAt: DateTimeOffset.Now,
				RetryAfter: DateTimeOffset.Now.AddHours(hoursUntilRetry),
				Reason: "Ownership: not owned")]);

	[TestMethod]
	public void a_title_being_waited_on_is_not_queued()
	{
		var waiting = LibraryBook("REFUSED");

		var request = BackupRequest.Create([waiting, LibraryBook("NEW")], Deferring(waiting));

		CollectionAssert.AreEqual(new[] { "NEW" }, request.Queueable.Select(lb => lb.Book.AudibleProductId).ToList());
		Assert.AreEqual(1, request.Skipped(BackupRequest.SkipReason.WaitingToRetry));
		Assert.AreEqual(1, request.Deferred.Count);
	}

	[TestMethod]
	public void no_title_is_waited_on_when_no_deferrals_are_supplied()
	{
		// The default is what a request about specific titles passes: an explicit ask is always attempted.
		var waiting = LibraryBook("REFUSED");

		var request = BackupRequest.Create([waiting]);

		Assert.AreEqual(1, request.Queueable.Length);
		Assert.AreEqual(0, request.Deferred.Count);
	}

	[TestMethod]
	public void a_title_needing_only_its_pdf_is_waited_on_too()
	{
		// A PDF is fetched through the same license request as the audiobook, so queueing this would put back
		// to Audible the request it just refused. Libation used to wait only on titles needing their audio.
		var pdfOnly = MockLibraryBook
			.CreateBook(title: "PDFONLY", bookStatus: LiberatedStatus.Liberated)
			.WithPdfStatus(LiberatedStatus.NotLiberated);

		var request = BackupRequest.Create([pdfOnly], Deferring(pdfOnly));

		Assert.AreEqual(0, request.Queueable.Length);
		Assert.AreEqual(1, request.Skipped(BackupRequest.SkipReason.WaitingToRetry));
		Assert.AreEqual(1, request.Deferred.Count);
	}

	[TestMethod]
	public void an_already_downloaded_title_is_reported_as_such_rather_than_as_waiting()
	{
		var done = LibraryBook("DONE", LiberatedStatus.Liberated);

		var request = BackupRequest.Create([done], Deferring(done));

		Assert.AreEqual(1, request.Skipped(BackupRequest.SkipReason.AlreadyDownloaded));
		Assert.AreEqual(0, request.Skipped(BackupRequest.SkipReason.WaitingToRetry));
	}

	[TestMethod]
	public void nothing_queued_body_says_why_libation_is_waiting_and_for_how_long()
	{
		var waiting = LibraryBook("REFUSED");

		var request = BackupRequest.Create([waiting], Deferring(waiting));
		var body = request.BuildNothingQueuedBody();

		StringAssert.Contains(body, "Waiting before trying again after a recent failure: 1");
		StringAssert.Contains(body, "download the title on its own to try it now");
		StringAssert.Contains(body, "Audible denied a download license (1 title)");
		StringAssert.Contains(body, "Next attempt in about 20 hours");
	}

	[TestMethod]
	public void the_waiting_detail_groups_titles_by_reason()
	{
		var refused = LibraryBook("REFUSED");
		var preorder = LibraryBook("PREORDER");
		var now = DateTimeOffset.Now;

		var request = BackupRequest.Create(
			[refused, preorder],
			DownloadDeferrals.Create([
				new DeferredDownload(refused.Account, "REFUSED", DownloadFailureKind.LicenseDenied, 2, now, now.AddDays(3), null),
				new DeferredDownload(preorder.Account, "PREORDER", DownloadFailureKind.AssetUnavailable, 1, now, now.AddHours(6), null)]));

		var detail = request.BuildDeferredDetail(now);

		StringAssert.Contains(detail, "Audible denied a download license (1 title). Next attempt in about 3 days");
		StringAssert.Contains(detail, "Audible has no downloadable audio yet (1 title). Next attempt in about 6 hours");
	}
}
