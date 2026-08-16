using DataLayer;
using Dinah.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ApplicationServices;

/// <summary>One title Libation is waiting on before attempting it again, and why.</summary>
public sealed record DeferredDownload(
	string Account,
	string AudibleProductId,
	DownloadFailureKind Kind,
	int ConsecutiveFailures,
	DateTimeOffset LastFailedAt,
	DateTimeOffset RetryAfter,
	string? Reason)
{
	/// <summary>The label used in the log, the CLI summary and the GUI's skipped-titles breakdown.</summary>
	public string KindLabel => Kind switch
	{
		DownloadFailureKind.LicenseDenied => "Audible denied a download license",
		DownloadFailureKind.AssetUnavailable => "Audible has no downloadable audio yet",
		DownloadFailureKind.ServiceInterruption => "A possible Audible service interruption",
		_ => "A previous failure"
	};
}

/// <summary>
/// The titles a bulk or automatic download run should leave alone for now, looked up by library book.
/// <para>
/// Read once per run: a run that takes hours must not have its own failures start suppressing the titles
/// still ahead of it in the same pass.
/// </para>
/// </summary>
public sealed class DownloadDeferrals
{
	/// <summary>No title is deferred. Used by targeted and forced runs, which must attempt what was asked.</summary>
	public static DownloadDeferrals None { get; } = new([]);

	private readonly Dictionary<(string Account, string AudibleProductId), DeferredDownload> byBook;

	private DownloadDeferrals(IEnumerable<DeferredDownload> deferred)
		=> byBook = deferred.ToDictionary(d => (d.Account, d.AudibleProductId));

	public static DownloadDeferrals Create(IEnumerable<DeferredDownload> deferred) => new(deferred);

	/// <summary>Reads the store. Never throws: a failure here must not stop a download run.</summary>
	public static DownloadDeferrals Load(DateTimeOffset now)
		=> Create(DownloadAttemptFailureStore.GetDeferred(now));

	public int Count => byBook.Count;
	public bool Any => byBook.Count > 0;

	public DeferredDownload? Find(LibraryBook libraryBook)
		=> libraryBook.Account is { } account
		&& byBook.TryGetValue((account, libraryBook.Book.AudibleProductId), out var deferred)
		? deferred
		: null;

	public bool IsDeferred(LibraryBook libraryBook) => Find(libraryBook) is not null;
}

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

		return wait <= TimeSpan.Zero ? "on the next run"
			: wait < TimeSpan.FromHours(1) ? $"in about {"minute".PluralizeWithCount(Math.Max(1, (int)wait.TotalMinutes))}"
			: wait < TimeSpan.FromDays(1) ? $"in about {"hour".PluralizeWithCount((int)Math.Round(wait.TotalHours))}"
			: $"in about {"day".PluralizeWithCount((int)Math.Round(wait.TotalDays))} ({when.ToLocalTime():d})";
	}

	private static IEnumerable<IGrouping<DownloadFailureKind, DeferredDownload>> GroupByKind(IEnumerable<DeferredDownload> deferred)
		=> deferred.GroupBy(d => d.Kind).OrderBy(g => g.Key);
}
