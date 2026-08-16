using DataLayer;
using System;
using System.Collections.Generic;

namespace ApplicationServices;

/// <summary>
/// How long to leave a title alone after a download attempt Audible refused, so a scheduled run stops asking
/// for the same license every time.
/// <para>
/// The wait doubles with each failure in a row, up to a cap, and every kind of failure has a finite cap: a
/// title held back is always attempted again eventually. Audible never distinguishes "you will never have
/// rights to this" from "not right now", so nothing here may be permanent.
/// </para>
/// </summary>
public static class DownloadRetryBackoff
{
	private static readonly Dictionary<DownloadFailureKind, (TimeSpan First, TimeSpan Max)> schedule = new()
	{
		// An eligibility refusal changes only when the account or the catalog changes. A day matches the
		// advice Libation already gives for a Plus title ("try again in 1 to 2 days"), which is the most
		// common refusal that clears by itself.
		[DownloadFailureKind.LicenseDenied] = (TimeSpan.FromDays(1), TimeSpan.FromDays(30)),

		// A preorder becomes downloadable on its release date, which nobody can predict from the error, so
		// keep checking within a week.
		[DownloadFailureKind.AssetUnavailable] = (TimeSpan.FromHours(6), TimeSpan.FromDays(7)),

		// Short: an outage that has passed should not delay a title any longer than it has to.
		[DownloadFailureKind.ServiceInterruption] = (TimeSpan.FromHours(1), TimeSpan.FromHours(12)),
	};

	/// <summary>How long to wait after the <paramref name="consecutiveFailures"/>th failure in a row.</summary>
	public static TimeSpan GetWait(DownloadFailureKind kind, int consecutiveFailures)
	{
		var (first, max) = schedule.TryGetValue(kind, out var found)
			? found
			: schedule[DownloadFailureKind.ServiceInterruption];

		// Doubling in ticks would overflow long before the cap matters, so count the doublings first.
		var doublings = Math.Clamp(consecutiveFailures - 1, 0, 30);
		var wait = first * Math.Pow(2, doublings);

		return wait > max ? max : wait;
	}

	/// <summary>When a title becomes eligible for another automatic attempt.</summary>
	public static DateTimeOffset GetRetryAfter(DownloadFailureKind kind, int consecutiveFailures, DateTimeOffset failedAt)
		=> failedAt + GetWait(kind, consecutiveFailures);
}
