using AudibleApi;
using AudibleApi.Common;
using DataLayer;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace FileLiberator;

/// <summary>What Libation understood about a failed download attempt, and how long to wait because of it.</summary>
public sealed record DownloadFailureDiagnosis(DownloadFailureKind Kind, string Reason);

/// <summary>
/// Recognises the download failures that mean "asking again right now will fail the same way": Audible
/// refusing a license, and Audible having no audio to deliver.
/// <para>
/// Only failures recognised here are recorded and waited on. Everything else - a dropped connection, a
/// decrypt error, a full disk - keeps the long-standing behaviour of being retried on the next run, because
/// there is no reason to believe the next attempt fails for the same reason.
/// </para>
/// </summary>
public static class DownloadFailureClassifier
{
	/// <summary>
	/// Substring in Audible's Sable error when no audio asset exists for the title, which is what an
	/// unreleased preorder looks like. Shared with <see cref="WidevineRecommendation.SableAcrNullMarker"/>,
	/// which pairs it with an error code to spot a much narrower case.
	/// </summary>
	private const string NoAudioAssetMarker = "acr:null";

	public static bool TryClassify(Exception ex, [NotNullWhen(true)] out DownloadFailureDiagnosis? diagnosis)
	{
		diagnosis = Classify(ex);
		return diagnosis is not null;
	}

	public static DownloadFailureDiagnosis? Classify(Exception? ex)
		=> ex switch
		{
			null => null,
			ContentLicenseDeniedException denied => ClassifyLicenseDenial(denied),
			ApiErrorException api => ClassifyApiError(api),
			// A rethrown Widevine failure arrives wrapped by whichever step gave up on it.
			_ => Classify(ex.InnerException)
		};

	/// <summary>
	/// Audible attaches a rejection reason per validation type it ran. <c>GenericError</c> is Audible
	/// declining to say why, which in practice means an outage rather than a decision about the title;
	/// the GUI already reads it that way when it chooses which guidance to offer. Explicit
	/// <c>CustomerThrottled</c> is a license denial the UI names separately; backoff still treats it as
	/// a settled refusal. Anything else names an eligibility problem with the account or the title,
	/// which will not change within the hour. Saying nothing at all is also treated as an outage: a
	/// refusal with no stated reason is not a settled one.
	/// </summary>
	private static DownloadFailureDiagnosis ClassifyLicenseDenial(ContentLicenseDeniedException ex)
	{
		LicenseDenialReason?[] reasons = [ex.Ownership, ex.AYCL, ex.Membership, ex.Client];

		var stated = reasons.Where(r => !string.IsNullOrWhiteSpace(r?.RejectionReason)).ToArray();
		var looksLikeOutage
			= stated.Length == 0
			|| stated.Any(r => r!.RejectionReason is RejectionReason.GenericError);

		return new DownloadFailureDiagnosis(
			looksLikeOutage ? DownloadFailureKind.ServiceInterruption : DownloadFailureKind.LicenseDenied,
			BuildLicenseDenialReason(reasons) ?? ex.Message);
	}

	/// <summary>The most specific message Audible gave, prefixed with which check it failed.</summary>
	private static string? BuildLicenseDenialReason(IEnumerable<LicenseDenialReason?> reasons)
		=> reasons
			.Where(r => !string.IsNullOrWhiteSpace(r?.Message))
			.Select(r => r!.ValidationType is { Length: > 0 } type ? $"{type}: {r.Message}" : r.Message)
			.FirstOrDefault();

	/// <summary>
	/// A license request that fails with no content reference (<c>acr:null</c>) means Audible has nothing to
	/// deliver for this title yet, which is what a preorder that has not been released looks like.
	/// </summary>
	private static DownloadFailureDiagnosis? ClassifyApiError(ApiErrorException ex)
	{
		if (ex.RequestUri?.Contains("/licenserequest", StringComparison.OrdinalIgnoreCase) is not true
			|| ex.JsonMessage?.Contains(NoAudioAssetMarker, StringComparison.Ordinal) is not true)
			return null;

		return new DownloadFailureDiagnosis(
			DownloadFailureKind.AssetUnavailable,
			"Audible returned no audio for this title. An unreleased preorder looks like this; so does a title Audible has not finished preparing.");
	}
}
