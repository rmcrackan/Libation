using ApplicationServices;
using DataLayer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace DeferredDownloadUserMessageTests;

[TestClass]
public class DeferredDownloadUserMessageTests
{
	private static readonly DateTimeOffset Now = new(2026, 8, 16, 3, 7, 0, TimeSpan.Zero);

	private static DeferredDownload Deferred(DownloadFailureKind kind, TimeSpan untilRetry, string asin = "ASIN")
		=> new("account", asin, kind, ConsecutiveFailures: 1, LastFailedAt: Now, RetryAfter: Now + untilRetry, Reason: null);

	[TestMethod]
	public void Nothing_is_said_when_no_title_was_held_back()
		=> Assert.AreEqual(0, DeferredDownloadUserMessage.BuildCliSkippedLines([], Now).Count());

	[TestMethod]
	public void The_cli_summary_replaces_a_warning_per_title_with_a_count_per_reason()
	{
		var skipped = new[]
		{
			Deferred(DownloadFailureKind.LicenseDenied, TimeSpan.FromDays(3), "A"),
			Deferred(DownloadFailureKind.LicenseDenied, TimeSpan.FromDays(1), "B"),
			Deferred(DownloadFailureKind.AssetUnavailable, TimeSpan.FromHours(6), "C"),
		};

		var lines = DeferredDownloadUserMessage.BuildCliSkippedLines(skipped, Now).ToList();

		Assert.AreEqual("Skipped 3 titles that recently failed to download. Libation will try again by itself.", lines[0]);
		// The soonest of each group, so the summary says when something will actually happen.
		StringAssert.Contains(lines[1], "Audible denied a download license: 2 (next attempt in about 1 day");
		StringAssert.Contains(lines[2], "Audible has no downloadable audio yet: 1 (next attempt in about 6 hours)");
		StringAssert.Contains(lines[3], "libationcli liberate --force");
	}

	[TestMethod]
	public void One_title_is_counted_in_the_singular()
	{
		var lines = DeferredDownloadUserMessage.BuildCliSkippedLines([Deferred(DownloadFailureKind.LicenseDenied, TimeSpan.FromDays(1))], Now).ToList();

		StringAssert.Contains(lines[0], "Skipped 1 title that recently");
	}

	[TestMethod]
	public void The_log_breakdown_is_compact()
	{
		var breakdown = DeferredDownloadUserMessage.BuildLogBreakdown([
			Deferred(DownloadFailureKind.LicenseDenied, TimeSpan.FromDays(1), "A"),
			Deferred(DownloadFailureKind.LicenseDenied, TimeSpan.FromDays(1), "B"),
			Deferred(DownloadFailureKind.ServiceInterruption, TimeSpan.FromHours(1), "C")]);

		Assert.AreEqual("Audible denied a download license: 2, A possible Audible service interruption: 1", breakdown);
	}

	[TestMethod]
	public void The_log_breakdown_of_nothing_is_none()
		=> Assert.AreEqual("none", DeferredDownloadUserMessage.BuildLogBreakdown([]));

	[TestMethod]
	[DataRow(0, "on the next run")]
	[DataRow(-90, "on the next run")]
	[DataRow(1, "in about 1 minute")]
	[DataRow(45, "in about 45 minutes")]
	[DataRow(90, "in about 2 hours")]
	[DataRow(60 * 20, "in about 20 hours")]
	[DataRow(60 * 24, "in about 1 day")]
	[DataRow(60 * 24 * 3, "in about 3 days")]
	public void When_a_title_comes_back_is_described_without_needing_a_clock(int minutes, string expected)
		=> StringAssert.StartsWith(DeferredDownloadUserMessage.DescribeWhen(Now.AddMinutes(minutes), Now), expected);

	[TestMethod]
	public void A_wait_of_days_also_names_the_date()
		=> StringAssert.Contains(
			DeferredDownloadUserMessage.DescribeWhen(Now.AddDays(30), Now),
			Now.AddDays(30).ToLocalTime().ToString("d"));
}
