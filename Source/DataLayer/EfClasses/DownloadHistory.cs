using System;

namespace DataLayer;

/// <summary>
/// One successful audiobook download, recorded so the opt-in daily download limit survives restarts.
/// Recorded for every download regardless of whether a limit is configured: a user who turns the limit
/// on after a heavy session gets a limit that reflects what actually happened.
/// <para>
/// The database is deliberately the home for this instead of a file under LibationFiles: in Docker,
/// LibationFiles is a throwaway directory inside the container and only the database is on a volume.
/// </para>
/// </summary>
public class DownloadHistory
{
	internal int DownloadHistoryId { get; private set; }

	/// <summary>
	/// UTC ticks rather than a DateTime so range queries mean the same thing on SQLite and PostgreSQL, and so a
	/// window that spans a DST change stays exactly 24 hours. Local time is for display only.
	/// </summary>
	public long CompletedAtUtcTicks { get; private set; }

	public string? AudibleProductId { get; private set; }

	public bool IsAudiblePlus { get; private set; }

	/// <summary>Size on disk of the files written to the Books directory for this title.</summary>
	public long Bytes { get; private set; }

	public DateTimeOffset CompletedAt => new(CompletedAtUtcTicks, TimeSpan.Zero);

	private DownloadHistory() { }

	public DownloadHistory(DateTimeOffset completedAt, string? audibleProductId, bool isAudiblePlus, long bytes)
	{
		CompletedAtUtcTicks = completedAt.UtcTicks;
		AudibleProductId = audibleProductId;
		IsAudiblePlus = isAudiblePlus;
		Bytes = bytes;
	}

	public override string ToString()
		=> $"{AudibleProductId} {CompletedAt.ToLocalTime()} {(IsAudiblePlus ? "Plus" : "owned")} {Bytes} bytes";
}
