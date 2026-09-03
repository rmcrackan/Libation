using ApplicationServices;
using AudibleApi;
using LibationFileManager;
using System;
using System.Collections.Generic;

namespace LibationCli;

internal static class ContentLicenseDeniedCliSummary
{
	/// <summary>Short lines for stderr when Audible denies a download license; mirrors log detail without dumping the full JSON.</summary>
	public static IEnumerable<string> Lines(ContentLicenseDeniedException ex)
	{
		ArgumentNullException.ThrowIfNull(ex);

		yield return ex.IsCustomerThrottled
			? "Audible denied a content license because this account is being throttled. Wait 24 to 48 hours before trying again. This is not a Libation bug."
			: "Audible denied a content license (download not allowed for this account/title).";
		yield return ex.Message;

		if (ex.Ownership?.Message is { } own && !string.IsNullOrWhiteSpace(own))
			yield return $"Ownership: {own}";
		if (ex.Client?.Message is { } cli && !string.IsNullOrWhiteSpace(cli))
			yield return $"Client: {cli}";
		if (ex.Membership?.Message is { } mem && !string.IsNullOrWhiteSpace(mem))
			yield return $"Membership: {mem}";
		if (ex.AYCL?.Message is { } aycl && !string.IsNullOrWhiteSpace(aycl))
			yield return $"AYCL (aka: Plus catalog): {aycl}";

		foreach (var line in SuggestDailyLimitLines())
			yield return line;
	}

	/// <summary>
	/// Extra pacing hint from Libation's own download record when Audible did not name CustomerThrottled.
	/// Silent unless that record makes throttling a plausible explanation and no limit is set yet.
	/// </summary>
	private static IEnumerable<string> SuggestDailyLimitLines()
	{
		string? suggestion;
		try
		{
			var now = DateTimeOffset.Now;
			suggestion = DailyDownloadLimitUserMessage.BuildSuggestionParagraph(
				Configuration.Instance,
				DownloadHistoryStore.GetCurrentWindow(now),
				now);
		}
		catch (Exception ex)
		{
			Serilog.Log.Logger.Error(ex, "Failed to build the daily download limit suggestion");
			yield break;
		}

		if (suggestion is null)
			yield break;

		Serilog.Log.Logger.Information("Suggesting a daily download limit after a license denial. {Suggestion}", suggestion);

		yield return string.Empty;
		foreach (var line in suggestion.Split('\n'))
			yield return line.TrimEnd('\r');
	}
}
