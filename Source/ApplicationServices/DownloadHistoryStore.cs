using DataLayer;
using LibationFileManager;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ApplicationServices;

/// <summary>
/// Reads and writes the record of successful audiobook downloads backing the daily download limit.
/// <para>
/// Caches nothing: every read is a fresh query, so a queue paused for hours sees entries age out of the
/// rolling window, and downloads performed by a concurrently running CLI or second container are counted too.
/// </para>
/// </summary>
public static class DownloadHistoryStore
{
	/// <summary>Kept a little beyond the 24 hour window so the table stays small without losing anything in use.</summary>
	private static readonly TimeSpan RetentionPeriod = TimeSpan.FromHours(48);

	/// <summary>
	/// Records a finished download. Never throws: bookkeeping must not fail a download that already succeeded.
	/// </summary>
	public static void Record(string? audibleProductId, bool isAudiblePlus, long bytes, DateTimeOffset? completedAt = null)
	{
		try
		{
			var when = completedAt ?? DateTimeOffset.Now;

			using var context = DbContexts.GetContext();
			context.DownloadHistory.Add(new DownloadHistory(when, audibleProductId, isAudiblePlus, bytes));

			var cutoff = (when - RetentionPeriod).UtcTicks;
			var expired = context.DownloadHistory.Where(dh => dh.CompletedAtUtcTicks < cutoff);
			context.DownloadHistory.RemoveRange(expired);

			context.SaveChanges();

			Serilog.Log.Logger.Debug(
				"Recorded download for the daily download limit. {@DebugInfo}",
				new { audibleProductId, isAudiblePlus, bytes, completedAt = when });
		}
		catch (Exception ex)
		{
			Serilog.Log.Logger.Error(
				ex,
				"Failed to record a completed download for the daily download limit. The download itself succeeded. {@DebugInfo}",
				new { audibleProductId, isAudiblePlus, bytes });
		}
	}

	/// <summary>Downloads completed at or after <paramref name="since"/>. Empty when the query fails.</summary>
	public static IReadOnlyList<DownloadHistoryEntry> GetSince(DateTimeOffset since)
	{
		try
		{
			var ticks = since.UtcTicks;

			using var context = DbContexts.GetContext();
			return context.DownloadHistory
				.AsNoTracking()
				.Where(dh => dh.CompletedAtUtcTicks >= ticks)
				.OrderBy(dh => dh.CompletedAtUtcTicks)
				.Select(dh => new { dh.CompletedAtUtcTicks, dh.AudibleProductId, dh.IsAudiblePlus, dh.Bytes })
				.ToList()
				.Select(dh => new DownloadHistoryEntry(
					new DateTimeOffset(dh.CompletedAtUtcTicks, TimeSpan.Zero),
					dh.AudibleProductId,
					dh.IsAudiblePlus,
					dh.Bytes))
				.ToList();
		}
		catch (Exception ex)
		{
			// Failing open is the safer default: a broken query must not block downloading.
			Serilog.Log.Logger.Error(ex, "Failed to read download history. Treating the last 24 hours as empty.");
			return [];
		}
	}

	/// <summary>The rolling window the daily download limit uses.</summary>
	public static IReadOnlyList<DownloadHistoryEntry> GetCurrentWindow(DateTimeOffset now)
		=> GetSince(now - DailyDownloadLimit.Window);
}
