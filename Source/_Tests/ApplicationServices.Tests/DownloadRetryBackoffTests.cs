using ApplicationServices;
using AssertionHelper;
using DataLayer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace DownloadRetryBackoffTests;

[TestClass]
public class DownloadRetryBackoffTests
{
	[TestMethod]
	[DataRow(DownloadFailureKind.LicenseDenied, 1, 24)]
	[DataRow(DownloadFailureKind.LicenseDenied, 2, 48)]
	[DataRow(DownloadFailureKind.LicenseDenied, 3, 96)]
	[DataRow(DownloadFailureKind.AssetUnavailable, 1, 6)]
	[DataRow(DownloadFailureKind.AssetUnavailable, 2, 12)]
	[DataRow(DownloadFailureKind.ServiceInterruption, 1, 1)]
	[DataRow(DownloadFailureKind.ServiceInterruption, 2, 2)]
	public void Wait_doubles_with_each_consecutive_failure(DownloadFailureKind kind, int consecutiveFailures, int expectedHours)
		=> DownloadRetryBackoff.GetWait(kind, consecutiveFailures).Should().Be(TimeSpan.FromHours(expectedHours));

	[TestMethod]
	[DataRow(DownloadFailureKind.LicenseDenied, 30)]
	[DataRow(DownloadFailureKind.AssetUnavailable, 7)]
	public void Wait_is_capped(DownloadFailureKind kind, int capDays)
	{
		// Deliberately absurd counts: the cap must hold rather than overflow.
		foreach (var failures in new[] { 20, 100, int.MaxValue })
			DownloadRetryBackoff.GetWait(kind, failures).Should().Be(TimeSpan.FromDays(capDays));
	}

	[TestMethod]
	public void A_possible_outage_is_never_waited_on_for_more_than_half_a_day()
		=> DownloadRetryBackoff.GetWait(DownloadFailureKind.ServiceInterruption, int.MaxValue)
			.Should().Be(TimeSpan.FromHours(12));

	[TestMethod]
	public void Every_kind_is_attempted_again_eventually()
	{
		// Nothing here may be permanent: Audible never distinguishes "never" from "not now".
		foreach (var kind in Enum.GetValues<DownloadFailureKind>())
			Assert.IsTrue(DownloadRetryBackoff.GetWait(kind, int.MaxValue) <= TimeSpan.FromDays(30), $"{kind} is waited on forever");
	}

	[TestMethod]
	public void A_first_failure_is_never_waited_on_for_less_than_an_hour()
	{
		// A shorter wait would leave an hourly cron re-requesting the same refused license every run.
		foreach (var kind in Enum.GetValues<DownloadFailureKind>())
			Assert.IsTrue(DownloadRetryBackoff.GetWait(kind, 1) >= TimeSpan.FromHours(1), $"{kind} is retried too soon");
	}

	[TestMethod]
	public void A_count_of_zero_or_less_is_treated_as_the_first_failure()
	{
		var first = DownloadRetryBackoff.GetWait(DownloadFailureKind.LicenseDenied, 1);

		DownloadRetryBackoff.GetWait(DownloadFailureKind.LicenseDenied, 0).Should().Be(first);
		DownloadRetryBackoff.GetWait(DownloadFailureKind.LicenseDenied, -5).Should().Be(first);
	}

	[TestMethod]
	public void RetryAfter_is_the_wait_added_to_when_the_attempt_failed()
	{
		var failedAt = new DateTimeOffset(2026, 8, 16, 3, 7, 0, TimeSpan.Zero);

		DownloadRetryBackoff.GetRetryAfter(DownloadFailureKind.LicenseDenied, 1, failedAt)
			.Should().Be(failedAt.AddDays(1));
	}
}
