using System;

namespace DataLayer;

/// <summary>
/// Why a download attempt failed, coarse enough to choose how long to wait before trying again.
/// Persisted as an int; do not renumber.
/// </summary>
public enum DownloadFailureKind
{
	/// <summary>
	/// Audible refused a content license and named an eligibility reason: the title is not owned, is not in
	/// the Plus catalog, or the account is not entitled to it. Changes only when the account or the catalog
	/// changes, so this is worth waiting a long time on.
	/// </summary>
	LicenseDenied = 0,

	/// <summary>
	/// Audible accepted the request but has no downloadable asset, as for a preorder that has not been
	/// released. Expected to start working by itself once the title is published.
	/// </summary>
	AssetUnavailable = 1,

	/// <summary>
	/// Looks like a service interruption or throttling rather than a decision about this title. Retried soon.
	/// </summary>
	ServiceInterruption = 2,
}

/// <summary>
/// The most recent failed attempt to download one title, so that a title Audible has just refused is not
/// requested again on every run. One row per (account, title): the same ASIN can be refused on one account
/// and downloadable on another.
/// <para>
/// Nothing here is permanent. <see cref="RetryAfterUtcTicks"/> always names a time, so a title held back
/// because of an outage, throttling or an unreleased preorder starts being attempted again on its own.
/// </para>
/// <para>
/// The database is deliberately the home for this instead of a file under LibationFiles: in Docker,
/// LibationFiles is a throwaway directory inside the container and only the database is on a volume, so a
/// file-based record would forget every failure on each container start - exactly the case this fixes.
/// </para>
/// </summary>
public class DownloadAttemptFailure
{
	internal int DownloadAttemptFailureId { get; private set; }

	public string AudibleProductId { get; private set; }

	/// <summary>The <see cref="LibraryBook.Account"/> the attempt was made with.</summary>
	public string Account { get; private set; }

	public DownloadFailureKind Kind { get; private set; }

	/// <summary>Failures in a row without an intervening success. Drives how long the next wait is.</summary>
	public int ConsecutiveFailures { get; private set; }

	/// <summary>
	/// UTC ticks rather than a DateTime so range queries mean the same thing on SQLite and PostgreSQL.
	/// Local time is for display only.
	/// </summary>
	public long LastFailedAtUtcTicks { get; private set; }

	/// <summary>When this title becomes eligible for another automatic attempt, in UTC ticks.</summary>
	public long RetryAfterUtcTicks { get; private set; }

	/// <summary>One line from Audible, kept so the user can be told why without re-requesting a license.</summary>
	public string? Reason { get; private set; }

	public DateTimeOffset LastFailedAt => new(LastFailedAtUtcTicks, TimeSpan.Zero);
	public DateTimeOffset RetryAfter => new(RetryAfterUtcTicks, TimeSpan.Zero);

	private DownloadAttemptFailure()
	{
		// for EF
		AudibleProductId = null!;
		Account = null!;
	}

	public DownloadAttemptFailure(string audibleProductId, string account, DownloadFailureKind kind, int consecutiveFailures, DateTimeOffset lastFailedAt, DateTimeOffset retryAfter, string? reason)
	{
		AudibleProductId = audibleProductId;
		Account = account;
		Record(kind, consecutiveFailures, lastFailedAt, retryAfter, reason);
	}

	public void Record(DownloadFailureKind kind, int consecutiveFailures, DateTimeOffset lastFailedAt, DateTimeOffset retryAfter, string? reason)
	{
		Kind = kind;
		ConsecutiveFailures = consecutiveFailures;
		LastFailedAtUtcTicks = lastFailedAt.UtcTicks;
		RetryAfterUtcTicks = retryAfter.UtcTicks;
		Reason = reason;
	}

	public override string ToString()
		=> $"{AudibleProductId} {Kind} x{ConsecutiveFailures}, retry after {RetryAfter.ToLocalTime()}";
}
