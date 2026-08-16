using DataLayer;
using Dinah.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ApplicationServices;

/// <summary>What to tell the user about titles a run held back, instead of the full warning per title per run.</summary>
public static class DeferredDownloadUserMessage
{
	/// <summary>
	/// A compact breakdown for the log, eg:
	/// "Audible denied a download license: 3, Audible has no downloadable audio yet: 1".
	/// </summary>
	public static string BuildLogBreakdown(IEnumerable<DeferredDownload> deferred)
	{
		var breakdown = string.Join(", ", GroupByKind(deferred).Select(g => $"{g.First().KindLabel}: {g.Count()}"));
		return breakdown is "" ? "none" : breakdown;
	}

	/// <summary>
	/// The lines a CLI run prints in place of a full warning per title. Says how many were held back, why,
	/// when the soonest will be attempted again, and how to override.
	/// </summary>
	public static IEnumerable<string> BuildCliSkippedLines(IReadOnlyCollection<DeferredDownload> skipped, DateTimeOffset now)
	{
		if (skipped.Count == 0)
			yield break;

		yield return $"Skipped {"title".PluralizeWithCount(skipped.Count)} that recently failed to download. Libation will try again by itself.";

		foreach (var group in GroupByKind(skipped))
			yield return $"  {group.First().KindLabel}: {group.Count()} (next attempt {DescribeWhen(group.Min(d => d.RetryAfter), now)})";

		yield return "  To try one now: libationcli liberate <ASIN>. For all of them: libationcli liberate --force.";
	}

	/// <summary>"in about 3 hours" / "in about 12 days (9/14/2026)" - a summary should not need a clock to read.</summary>
	public static string DescribeWhen(DateTimeOffset when, DateTimeOffset now)
	{
		var wait = when - now;

		if (wait <= TimeSpan.Zero)
			return "on the next run";
		if (wait < TimeSpan.FromHours(1))
			return $"in about {"minute".PluralizeWithCount(Math.Max(1, (int)wait.TotalMinutes))}";

		// Rounded to hours first, so a wait of exactly one day does not read as 23 or 24 hours depending on
		// how long the run took to reach this line.
		var hours = (int)Math.Round(wait.TotalHours);
		return hours < 24
			? $"in about {"hour".PluralizeWithCount(hours)}"
			: $"in about {"day".PluralizeWithCount((int)Math.Round(hours / 24d))} ({when.ToLocalTime():d})";
	}

	private static IEnumerable<IGrouping<DownloadFailureKind, DeferredDownload>> GroupByKind(IEnumerable<DeferredDownload> deferred)
		=> deferred.GroupBy(d => d.Kind).OrderBy(g => g.Key);
}
