using AudibleApi;
using AudibleApi.Common;
using LibationFileManager;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace LibationCli.Tests;

[TestClass]
public class ContentLicenseDeniedCliSummaryTests
{
	[TestInitialize]
	public void Initialize() => Configuration.CreateMockInstance();

	[TestCleanup]
	public void Cleanup() => Configuration.RestoreSingletonInstance();

	private static ContentLicenseDeniedException Denied(params (string ValidationType, string RejectionReason, string Message)[] reasons)
		=> new(
			new Uri("https://api.audible.com/1.0/content/B005EGKBYK/licenserequest"),
			new ContentLicense
			{
				Asin = "B005EGKBYK",
				StatusCode = "Denied",
				LicenseDenialReasons = [.. Array.ConvertAll(reasons, r => new LicenseDenialReason
				{
					ValidationType = r.ValidationType,
					RejectionReason = r.RejectionReason,
					Message = r.Message
				})]
			});

	[TestMethod]
	public void A_throttled_denial_leads_with_throttling_guidance()
	{
		var ex = Denied(
			("Ownership", RejectionReason.CustomerThrottled, "Customer id [##############] being throttled"),
			("AYCL", RejectionReason.ContentEligibility, "Asin: [B005EGKBYK] is not eligible for AYCL"));

		var lines = ContentLicenseDeniedCliSummary.Lines(ex).ToList();

		StringAssert.Contains(lines[0], "throttled");
		Assert.IsTrue(lines.Any(l => l.Contains("24 to 48 hours")));
		Assert.IsTrue(lines.Any(l => l.Contains("login-external --account", StringComparison.Ordinal)));
		Assert.IsTrue(lines.Any(l => l.Contains("AccountsSettings.json", StringComparison.Ordinal)));
		Assert.IsFalse(lines.Any(l => l.Contains("Settings > Accounts")));
		Assert.IsFalse(lines.Any(l => l.Contains("not a Libation bug")));
		Assert.IsTrue(lines.Any(l => l.StartsWith("Ownership:", StringComparison.Ordinal)));
	}

	[TestMethod]
	[DataRow(AppScaffolding.VersionCheckOutcome.UpToDate, false)]
	[DataRow(AppScaffolding.VersionCheckOutcome.UpdateAvailable, true)]
	[DataRow(AppScaffolding.VersionCheckOutcome.UnableToDetermine, true)]
	public void Recovery_uses_passed_region_and_update_status(AppScaffolding.VersionCheckOutcome status, bool upgrade)
	{
		var ex = Denied(("Ownership", RejectionReason.CustomerThrottled, "throttled"));
		var body = string.Join("\n", ContentLicenseDeniedCliSummary.Lines(ex, Localization.Get("uk"), status));
		StringAssert.Contains(body, "https://www.amazon.co.uk/");
		StringAssert.Contains(body, "regional link is unverified");
		Assert.AreEqual(upgrade, body.Contains(AppScaffolding.LicenseRecoveryGuidance.ReleasesUrl));
		Assert.IsTrue(body.IndexOf("Deregister") < body.IndexOf("24 to 48 hours"));
	}

	[TestMethod]
	[DataRow(false)]
	[DataRow(true)]
	public void Additional_marketplace_guidance_is_conditional(bool hasAdditional)
	{
		var ex = Denied(("Ownership", RejectionReason.CustomerThrottled, "throttled"));
		var body = string.Join("\n", ContentLicenseDeniedCliSummary.Lines(ex, Localization.Get("uk"),
			AppScaffolding.VersionCheckOutcome.UpToDate, hasAdditional));
		Assert.AreEqual(hasAdditional, body.Contains("AdditionalLocaleNames"));
		Assert.AreEqual(hasAdditional, body.Contains("Also scans"));
	}

	[TestMethod]
	public void An_eligibility_denial_keeps_the_generic_opener()
	{
		var ex = Denied(("Ownership", RejectionReason.RequesterEligibility, "not owned"));

		var lines = ContentLicenseDeniedCliSummary.Lines(ex).ToList();

		StringAssert.Contains(lines[0], "download not allowed");
		Assert.IsFalse(lines[0].Contains("throttled", StringComparison.Ordinal));
	}
}
