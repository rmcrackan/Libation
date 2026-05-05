using AudibleApi;
using System;
using System.Collections.Generic;

namespace LibationCli;

internal static class ContentLicenseDeniedCliSummary
{
	/// <summary>Short lines for stderr when Audible denies a download license; mirrors log detail without dumping the full JSON.</summary>
	public static IEnumerable<string> Lines(ContentLicenseDeniedException ex)
	{
		ArgumentNullException.ThrowIfNull(ex);

		yield return "Audible denied a content license (download not allowed for this account/title).";
		yield return ex.Message;

		if (ex.Ownership?.Message is { } own && !string.IsNullOrWhiteSpace(own))
			yield return $"Ownership: {own}";
		if (ex.Client?.Message is { } cli && !string.IsNullOrWhiteSpace(cli))
			yield return $"Client: {cli}";
		if (ex.Membership?.Message is { } mem && !string.IsNullOrWhiteSpace(mem))
			yield return $"Membership: {mem}";
		if (ex.AYCL?.Message is { } aycl && !string.IsNullOrWhiteSpace(aycl))
			yield return $"AYCL (aka: Plus catalog): {aycl}";
	}
}
