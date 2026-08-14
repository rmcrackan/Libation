using AssertionHelper;
using LibationFileManager;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LibationCli.Tests;

/// <summary>
/// The per-run limit options on <c>liberate</c>, and the decision to stop a run. Everything here is pure: the
/// history a run has produced is supplied directly, so no database or downloading is involved.
/// </summary>
[TestClass]
public class RunDownloadLimitTests
{
	private static readonly DateTimeOffset RunStart = new(2026, 8, 14, 19, 0, 0, TimeSpan.FromHours(-4));

	private const long ThreeHundredMB = 300L * 1024 * 1024;

	private static LiberateOptions? Parse(params string[] args)
	{
		using var error = new StringWriter();
		return Program.ParseInvocation(args, error).Result?.Value as LiberateOptions;
	}

	private static DownloadHistoryEntry Entry(string asin, long bytes = ThreeHundredMB)
		=> new(RunStart.AddMinutes(1), asin, IsAudiblePlus: true, bytes);

	#region parsing

	[TestMethod]
	[DataRow("--limit-books", 10, Configuration.DailyLimitUnit.Books)]
	[DataRow("--limit-mb", 500, Configuration.DailyLimitUnit.MB)]
	[DataRow("--limit-gb", 5, Configuration.DailyLimitUnit.GB)]
	public void Each_option_maps_to_its_own_unit(string option, int quantity, Configuration.DailyLimitUnit expectedUnit)
	{
		var options = Parse("liberate", option, quantity.ToString());
		Assert.IsNotNull(options);

		Assert.IsTrue(RunDownloadLimit.TryCreate(options.LimitBooks, options.LimitMB, options.LimitGB, options.PdfOnly, out var limit, out var error));
		Assert.IsNull(error);
		Assert.IsNotNull(limit);
		Assert.AreEqual(expectedUnit, limit.Value.Unit);
		limit.Value.Quantity.Should().Be(quantity);
	}

	[TestMethod]
	[DataRow("--limit-books", "10", "--limit-mb", "500")]
	[DataRow("--limit-books", "10", "--limit-gb", "5")]
	[DataRow("--limit-mb", "500", "--limit-gb", "5")]
	public void Two_limit_options_cannot_be_used_together(params string[] limitArgs)
	{
		using var error = new StringWriter();

		var outcome = Program.ParseInvocation(["liberate", .. limitArgs], error);

		Assert.AreEqual(ExitCode.ParseError, outcome.ExitCode);
		StringAssert.Contains(error.ToString(), "is not compatible with");
	}

	[TestMethod]
	public void A_limit_combines_with_the_other_liberate_options()
	{
		var options = Parse("liberate", "--force", "--limit-books", "3", "B017V4IM1G");

		Assert.IsNotNull(options);
		options.Force.Should().BeTrue();
		Assert.AreEqual(3, options.LimitBooks);
		CollectionAssert.AreEqual(new[] { "B017V4IM1G" }, options.Asins!.ToArray());
	}

	[TestMethod]
	public void No_limit_option_is_an_unlimited_run_rather_than_an_error()
	{
		var options = Parse("liberate");
		Assert.IsNotNull(options);

		Assert.IsTrue(RunDownloadLimit.TryCreate(options.LimitBooks, options.LimitMB, options.LimitGB, options.PdfOnly, out var limit, out var error));
		Assert.IsNull(limit);
		Assert.IsNull(error);
	}

	#endregion

	#region validation

	[TestMethod]
	[DataRow(0)]
	[DataRow(-7)]
	public void A_quantity_below_one_is_rejected(int quantity)
	{
		Assert.IsFalse(RunDownloadLimit.TryCreate(quantity, null, null, pdfOnly: false, out var limit, out var error));

		Assert.IsNull(limit);
		StringAssert.Contains(error!, "--limit-books");
		StringAssert.Contains(error!, "at least 1");
	}

	[TestMethod]
	public void The_rejection_names_the_option_the_user_actually_typed()
	{
		Assert.IsFalse(RunDownloadLimit.TryCreate(null, null, 0, pdfOnly: false, out _, out var error));

		StringAssert.Contains(error!, "--limit-gb");
	}

	[TestMethod]
	public void A_limit_is_rejected_with_pdf_only_rather_than_silently_never_stopping()
	{
		Assert.IsFalse(RunDownloadLimit.TryCreate(null, 500, null, pdfOnly: true, out var limit, out var error));

		Assert.IsNull(limit);
		StringAssert.Contains(error!, "--limit-mb");
		StringAssert.Contains(error!, "--pdf");
	}

	#endregion

	#region stopping a run

	[TestMethod]
	public void A_book_limit_stops_the_run_once_that_many_have_downloaded()
	{
		var history = new List<DownloadHistoryEntry>();
		var tracker = new RunLimitTracker(new(Configuration.DailyLimitUnit.Books, 2), RunStart, _ => history);

		tracker.TryStop(out _).Should().BeFalse();

		Download(tracker, history, "ASIN1");
		tracker.TryStop(out _).Should().BeFalse();

		Download(tracker, history, "ASIN2");
		tracker.TryStop(out var message).Should().BeTrue();

		StringAssert.Contains(message!, "2 book(s)");
		StringAssert.Contains(message!, "Downloaded 2 title(s)");
		StringAssert.Contains(message!, "tried on the next run");
	}

	[TestMethod]
	public void A_size_limit_stops_when_another_book_would_not_fit()
	{
		// 300 MB each against a 1 GB limit: a fourth would be assumed to need another 400 MB, over the limit.
		var history = new List<DownloadHistoryEntry>();
		var tracker = new RunLimitTracker(new(Configuration.DailyLimitUnit.GB, 1), RunStart, _ => history);

		Download(tracker, history, "ASIN1");
		Download(tracker, history, "ASIN2");
		tracker.TryStop(out _).Should().BeFalse();

		Download(tracker, history, "ASIN3");
		tracker.TryStop(out var message).Should().BeTrue();

		StringAssert.Contains(message!, "1 GB");
		StringAssert.Contains(message!, "900 MB");
		StringAssert.Contains(message!, "across 3 title(s)");
	}

	[TestMethod]
	public void One_download_is_allowed_even_when_the_limit_is_smaller_than_a_book()
	{
		var history = new List<DownloadHistoryEntry>();
		var tracker = new RunLimitTracker(new(Configuration.DailyLimitUnit.MB, 1), RunStart, _ => history);

		// Otherwise a run would end immediately and report a limit reached to someone who downloaded nothing.
		tracker.TryStop(out _).Should().BeFalse();

		Download(tracker, history, "ASIN1");
		tracker.TryStop(out _).Should().BeTrue();
	}

	[TestMethod]
	public void Downloads_this_run_did_not_perform_are_not_counted()
	{
		// A Libation window or a second container downloading at the same time writes to the same history.
		var history = new List<DownloadHistoryEntry>
		{
			Entry("SOMEONE_ELSE1"),
			Entry("SOMEONE_ELSE2"),
			Entry("SOMEONE_ELSE3")
		};
		var tracker = new RunLimitTracker(new(Configuration.DailyLimitUnit.Books, 2), RunStart, _ => history);

		Download(tracker, history, "ASIN1");

		tracker.TryStop(out _).Should().BeFalse();
	}

	[TestMethod]
	public void Titles_attempted_but_not_downloaded_are_not_counted()
	{
		// A title that failed, or that the daily limit skipped, writes no history row.
		var history = new List<DownloadHistoryEntry>();
		var tracker = new RunLimitTracker(new(Configuration.DailyLimitUnit.Books, 1), RunStart, _ => history);

		tracker.Attempting("FAILED1");
		tracker.Attempting("FAILED2");

		tracker.TryStop(out _).Should().BeFalse();
	}

	[TestMethod]
	public void Product_ids_are_matched_without_regard_to_case()
	{
		var history = new List<DownloadHistoryEntry> { Entry("b0test0001") };
		var tracker = new RunLimitTracker(new(Configuration.DailyLimitUnit.Books, 1), RunStart, _ => history);

		tracker.Attempting("B0TEST0001");

		tracker.TryStop(out _).Should().BeTrue();
	}

	[TestMethod]
	public void History_is_read_afresh_from_the_start_of_the_run()
	{
		// Re-reading is what lets the limit reflect what the last title actually weighed.
		var reads = new List<DateTimeOffset>();
		var tracker = new RunLimitTracker(
			new(Configuration.DailyLimitUnit.Books, 5),
			RunStart,
			since => { reads.Add(since); return []; });

		tracker.Attempting("ASIN1");
		tracker.TryStop(out _);
		tracker.TryStop(out _);

		CollectionAssert.AreEqual(new[] { RunStart, RunStart }, reads);
	}

	private static void Download(RunLimitTracker tracker, List<DownloadHistoryEntry> history, string asin)
	{
		tracker.Attempting(asin);
		history.Add(Entry(asin));
	}

	#endregion
}
