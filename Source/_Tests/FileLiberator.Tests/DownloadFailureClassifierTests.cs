using AudibleApi;
using AudibleApi.Common;
using DataLayer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Net.Http;

namespace FileLiberator.Tests;

/// <summary>
/// The inputs here are taken from the log attached to issue #1947: a Plus preorder Audible had no audio for,
/// and owned titles on an inactive account that Audible refused a license for.
/// </summary>
[TestClass]
public class DownloadFailureClassifierTests
{
	private const string LicenseRequestUri = "https://api.audible.com/1.0/content/B002V5B8OY/licenserequest";

	/// <summary>The preorder failure: a license request that comes back with no content reference.</summary>
	private const string NoAudioAssetJson = """
		{"http_response_code":"NotFound","response":"{\"message\":\"Unable to retrieve asset details from Sable(ACRInfos), for marketplaceId:AF2M0KC94RCEA, asin:B0H956N76W, acr:null, skuLite:OR_ORIG_003592, version:LATEST, aaaClientId:urn:cdo:AudibleApiExternalRouterService:Prod:Default\"}"}
		""";

	private static ContentLicenseDeniedException Denied(params (string ValidationType, string RejectionReason, string Message)[] reasons)
	{
		var license = new ContentLicense
		{
			Asin = "B002V5B8OY",
			StatusCode = "Denied",
			LicenseDenialReasons = [.. Array.ConvertAll(reasons, r => new LicenseDenialReason
			{
				ValidationType = r.ValidationType,
				RejectionReason = r.RejectionReason,
				Message = r.Message
			})]
		};

		return new ContentLicenseDeniedException(new Uri(LicenseRequestUri), license);
	}

	private static ApiErrorException ApiError(string requestUri, string json)
		=> new(requestUri, JObject.Parse(json), "License response not \"OK\"");

	[TestMethod]
	public void An_eligibility_refusal_is_a_license_denial()
	{
		// Verbatim from the issue's log: an owned title on an account that is no longer active.
		var ex = Denied(
			("Membership", RejectionReason.RequesterEligibility, "Customer is not part of any plans"),
			("Ownership", RejectionReason.RequesterEligibility, "Ownership: No Ownership information returned by DAOQS for customer [x] and for asin [B002V5B8OY]."),
			("Client", RejectionReason.RequesterEligibility, "does not has access to asin[B002V5B8OY]."),
			("AYCL", RejectionReason.ContentEligibility, "Asin: [B002V5B8OY] is not eligible for AYCL"));

		Assert.IsTrue(DownloadFailureClassifier.TryClassify(ex, out var diagnosis));
		Assert.AreEqual(DownloadFailureKind.LicenseDenied, diagnosis.Kind);
		// The reason names which check failed, so the log and the UI can say why without another request.
		StringAssert.StartsWith(diagnosis.Reason, "Ownership: ");
	}

	[TestMethod]
	public void A_Plus_title_no_longer_in_the_catalog_is_a_license_denial()
	{
		var ex = Denied(
			("Ownership", RejectionReason.RequesterEligibility, "No matching DAO benefit found"),
			("AYCL", RejectionReason.ContentEligibility, "Asin: [B0D5JLT7YG] is not eligible for AYCL"));

		Assert.IsTrue(DownloadFailureClassifier.TryClassify(ex, out var diagnosis));
		Assert.AreEqual(DownloadFailureKind.LicenseDenied, diagnosis.Kind);
	}

	[TestMethod]
	public void An_ownership_refusal_alone_is_a_license_denial()
	{
		// Audible does not always run every check. One stated eligibility refusal is still a refusal, and an
		// hourly schedule must not keep asking about it.
		var ex = Denied(("Ownership", RejectionReason.RequesterEligibility, "not owned"));

		Assert.IsTrue(DownloadFailureClassifier.TryClassify(ex, out var diagnosis));
		Assert.AreEqual(DownloadFailureKind.LicenseDenied, diagnosis.Kind);
	}

	[TestMethod]
	public void GenericError_is_read_as_a_possible_service_interruption()
	{
		// Matches the judgement the GUI queue already makes when it offers guidance for this failure.
		var ex = Denied(("AYCL", RejectionReason.GenericError, "Something went wrong"));

		Assert.IsTrue(DownloadFailureClassifier.TryClassify(ex, out var diagnosis));
		Assert.AreEqual(DownloadFailureKind.ServiceInterruption, diagnosis.Kind);
	}

	[TestMethod]
	public void GenericError_on_any_check_is_read_as_a_possible_service_interruption()
	{
		var ex = Denied(
			("Ownership", RejectionReason.GenericError, "Something went wrong"),
			("AYCL", RejectionReason.ContentEligibility, "Asin is not eligible for AYCL"));

		Assert.IsTrue(DownloadFailureClassifier.TryClassify(ex, out var diagnosis));
		Assert.AreEqual(DownloadFailureKind.ServiceInterruption, diagnosis.Kind);
	}

	[TestMethod]
	public void A_denial_with_no_reasons_at_all_is_not_treated_as_settled()
	{
		var ex = Denied();

		Assert.IsTrue(DownloadFailureClassifier.TryClassify(ex, out var diagnosis));
		Assert.AreEqual(DownloadFailureKind.ServiceInterruption, diagnosis.Kind);
		Assert.AreEqual(ex.Message, diagnosis.Reason);
	}

	[TestMethod]
	public void A_license_request_with_no_content_reference_means_there_is_no_audio_yet()
	{
		var ex = ApiError(LicenseRequestUri, NoAudioAssetJson);

		Assert.IsTrue(DownloadFailureClassifier.TryClassify(ex, out var diagnosis));
		Assert.AreEqual(DownloadFailureKind.AssetUnavailable, diagnosis.Kind);
		StringAssert.Contains(diagnosis.Reason, "preorder");
	}

	[TestMethod]
	public void An_acr_null_error_from_another_endpoint_is_not_classified()
	{
		// Only the license request tells us whether audio exists to download.
		var ex = ApiError("https://api.audible.com/1.0/content/B0H956N76W/metadata", NoAudioAssetJson);

		Assert.IsFalse(DownloadFailureClassifier.TryClassify(ex, out _));
	}

	[TestMethod]
	public void An_ordinary_api_error_is_not_classified()
	{
		var ex = ApiError(LicenseRequestUri, """{"message":"Internal server error"}""");

		Assert.IsFalse(DownloadFailureClassifier.TryClassify(ex, out _));
	}

	[TestMethod]
	public void Failures_that_are_nothing_to_do_with_Audible_keep_being_retried_every_run()
	{
		// Nothing here suggests the next attempt fails the same way, so these must not be waited on.
		Assert.IsFalse(DownloadFailureClassifier.TryClassify(new IOException("There is not enough space on the disk."), out _));
		Assert.IsFalse(DownloadFailureClassifier.TryClassify(new HttpRequestException("Connection reset"), out _));
		Assert.IsFalse(DownloadFailureClassifier.TryClassify(new OperationCanceledException(), out _));
		Assert.IsFalse(DownloadFailureClassifier.TryClassify(new InvalidDataException("Widevine license response is null."), out _));
	}

	[TestMethod]
	public void A_wrapped_denial_is_still_recognised()
	{
		// The Widevine path rethrows through whichever step gave up on it.
		var inner = Denied(("Ownership", RejectionReason.RequesterEligibility, "not owned"));
		var ex = new InvalidOperationException("Failed to request a Widevine license.", inner);

		Assert.IsTrue(DownloadFailureClassifier.TryClassify(ex, out var diagnosis));
		Assert.AreEqual(DownloadFailureKind.LicenseDenied, diagnosis.Kind);
	}

	[TestMethod]
	public void Classifying_null_is_harmless()
		=> Assert.IsNull(DownloadFailureClassifier.Classify(null));

	[TestMethod]
	public void CustomerThrottled_mixed_with_eligibility_is_still_a_license_denial()
	{
		// From Log202609: Ownership named CustomerThrottled while Membership/Client/AYCL still failed eligibility.
		var ex = Denied(
			("Membership", RejectionReason.RequesterEligibility, "Customer is not part of any plans"),
			("Ownership", RejectionReason.CustomerThrottled, "Customer id [##############] being throttled"),
			("Client", RejectionReason.RequesterEligibility, "does not has access to asin[B005EGKBYK]."),
			("AYCL", RejectionReason.ContentEligibility, "Asin: [B005EGKBYK] is not eligible for AYCL"));

		Assert.IsTrue(DownloadFailureClassifier.TryClassify(ex, out var diagnosis));
		Assert.AreEqual(DownloadFailureKind.LicenseDenied, diagnosis.Kind);
		Assert.IsTrue(ex.IsCustomerThrottled);
		StringAssert.Contains(diagnosis.Reason, "throttled");
	}

	[TestMethod]
	public void An_eligibility_refusal_without_CustomerThrottled_is_not_throttling()
	{
		var ex = Denied(
			("Ownership", RejectionReason.RequesterEligibility, "not owned"),
			("AYCL", RejectionReason.ContentEligibility, "Asin is not eligible for AYCL"));

		Assert.IsTrue(DownloadFailureClassifier.TryClassify(ex, out var diagnosis));
		Assert.AreEqual(DownloadFailureKind.LicenseDenied, diagnosis.Kind);
		Assert.IsFalse(ex.IsCustomerThrottled);
	}
}
