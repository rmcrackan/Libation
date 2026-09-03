using System;
using System.Collections.Generic;

namespace LibationFileManager;

/// <summary>
/// User-facing copy for the opt-in daily download limit, plus the suggestion to turn it on when Audible
/// looks like it is throttling. Lives here rather than in LibationUiBase because LibationCli needs the same
/// wording and does not reference LibationUiBase.
/// </summary>
public static class DailyDownloadLimitUserMessage
{
	public const string DialogCaption = "Daily download limit reached";

	private const string SettingsLocation = "Settings > Download/Decrypt > Daily download limit";

	/// <summary>Shown once when the queue pauses. Informational only: the queue resumes on its own.</summary>
	public static string BuildQueuePausedBody(DailyDownloadLimit.Allowance allowance, string bookTitleWithSubtitle)
		=> $"""
			Libation has paused before downloading {bookTitleWithSubtitle} because you have reached your daily download limit.

			{DescribeUsage(allowance)}

			Nothing is lost and nothing was cancelled. Your books are still queued, and Libation will continue on its own {DescribeResumption(allowance)}.

			To download more now, change or turn off the limit in {SettingsLocation}. Libation picks up the new setting within a few seconds, so there is no need to requeue anything. To stop instead, use Cancel All in the process queue.
			""";

	/// <summary>
	/// Per-book status shown in the queue while paused, recomputed on each re-check. Kept short: the process
	/// queue column is narrow and clips rather than wrapping, so the resume time has to fit on one short line.
	/// </summary>
	public static string BuildWaitingStatus(DailyDownloadLimit.Allowance allowance)
		=> allowance.NextCapacityAt is DateTimeOffset next
		? $"Daily limit, resumes {next.ToLocalTime():t}"
		: "Daily limit reached, waiting";

	public static string BuildQueueLogEntry(DailyDownloadLimit.Allowance allowance, string bookTitleWithSubtitle)
		=> $"Daily download limit reached. {DescribeUsage(allowance)} Waiting before downloading {bookTitleWithSubtitle}. "
		+ $"Libation will continue on its own {DescribeResumption(allowance)}, or change the limit in {SettingsLocation}.";

	public static string BuildDeferredLogEntry(DailyDownloadLimit.Allowance allowance, string bookTitleWithSubtitle)
		=> $"Daily download limit reached for Audible Plus titles. {DescribeUsage(allowance)} "
		+ $"Moved {bookTitleWithSubtitle} to the end of the queue and continued with titles the limit does not cover.";

	/// <summary>stderr lines for the CLI, which skips blocked titles instead of waiting.</summary>
	public static IEnumerable<string> BuildCliSkippedLines(DailyDownloadLimit.Allowance allowance)
	{
		yield return $"Daily download limit reached. {DescribeUsage(allowance)}";
		yield return $"Skipping the titles it covers. Capacity returns {DescribeResumption(allowance)}.";
		yield return $"To change or turn off the limit, set \"{nameof(Configuration.DailyDownloadLimit)}\" in Settings.json (or use {SettingsLocation} in the Libation app).";
	}

	public static string BuildCliSkippedSummary(int skippedCount)
		=> $"Skipped {skippedCount} title(s) because of your daily download limit. They remain un-liberated and will be tried on the next run.";

	/// <summary>
	/// Suggests turning the limit on after a license denial that looks like Audible throttling but did not
	/// name CustomerThrottled. Returns null when the suggestion would be unhelpful: a limit is already
	/// configured, or too little was downloaded recently for throttling to be a plausible explanation.
	/// </summary>
	public static string? BuildSuggestionParagraph(Configuration config, IReadOnlyList<DownloadHistoryEntry> history, DateTimeOffset now)
	{
		if (config.DailyDownloadLimit is not Configuration.DailyLimitScope.NoLimit)
			return null;

		var recent = DailyDownloadLimit.SummarizeRecent(history, now);
		if (recent.TotalDownloads < DailyDownloadLimit.SuggestionMinimumRecentDownloads)
			return null;

		var plus = recent.PlusDownloads == recent.TotalDownloads
			? "all of them from the Plus catalog"
			: $"{recent.PlusDownloads} of them from the Plus catalog";

		return $"""
			Libation successfully downloaded {recent.TotalDownloads} titles in the last 24 hours, {plus}. That is the kind of volume that leads Audible to deny licenses for a day or two.

			To have Libation pace itself from now on, turn on a daily download limit in {SettingsLocation}. "Plus titles only" with a limit of 50 books is a reasonable starting point; titles you own are not affected. Libation counts only the downloads it performs, over a rolling 24 hours.
			""";
	}

	/// <summary>A complete sentence, so it can stand alone as its own paragraph or line.</summary>
	private static string DescribeUsage(DailyDownloadLimit.Allowance allowance)
	{
		var scope = allowance.Scope is Configuration.DailyLimitScope.PlusOnly ? "Audible Plus titles" : "books";

		return allowance.LimitBytes is long limitBytes
			? $"Your limit is {allowance.Quantity} {allowance.Unit} of {scope} per 24 hours, and Libation has downloaded about {DiskSpaceHelper.FormatBytes(allowance.UsedBytes)} of {DiskSpaceHelper.FormatBytes(limitBytes)} in the last 24 hours, across {allowance.UsedBooks} title(s)."
			: $"Your limit is {allowance.Quantity} {scope} per 24 hours, and Libation has downloaded {allowance.UsedBooks} in the last 24 hours.";
	}

	private static string DescribeResumption(DailyDownloadLimit.Allowance allowance)
		=> allowance.NextCapacityAt is DateTimeOffset next
		? $"at about {next.ToLocalTime():t} ({next.ToLocalTime():d}), when the oldest of those downloads is more than 24 hours old"
		: "once the oldest of those downloads is more than 24 hours old";
}
