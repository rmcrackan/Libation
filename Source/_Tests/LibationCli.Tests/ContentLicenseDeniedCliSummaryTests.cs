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
		StringAssert.Contains(lines[0], "24 to 48 hours");
		Assert.IsTrue(lines.Any(l => l.StartsWith("Ownership:", StringComparison.Ordinal)));
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
