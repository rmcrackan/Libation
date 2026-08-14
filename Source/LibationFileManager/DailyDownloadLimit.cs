using System;
using System.Collections.Generic;
using System.Linq;

namespace LibationFileManager;

/// <summary>One successful audiobook download recorded by Libation.</summary>
/// <param name="CompletedAt">When the download finished. Compared against <see cref="DateTimeOffset.Now"/>, so a
/// queue that runs for days across a DST change still measures exactly 24 hours.</param>
/// <param name="Bytes">Size on disk of the files written to the Books directory for this title.</param>
public record DownloadHistoryEntry(DateTimeOffset CompletedAt, string? AudibleProductId, bool IsAudiblePlus, long Bytes);

/// <summary>
/// Decides whether another audiobook may be downloaded, given the user's opt-in daily limit and the
/// downloads Libation recorded in the last 24 hours. Pure: no clock, no I/O, no caching. Callers pass
/// a fresh <c>now</c> and a fresh history read on every check so a paused queue can resume days later.
/// </summary>
public static class DailyDownloadLimit
{
	/// <summary>Rolling window. Not a calendar day: capacity frees up 24 hours after each download.</summary>
	public static readonly TimeSpan Window = TimeSpan.FromHours(24);

	/// <summary>
	/// Below this many recent downloads, a license denial is unlikely to be Audible throttling, so
	/// suggesting a daily limit would point the user at the wrong problem.
	/// </summary>
	public const int SuggestionMinimumRecentDownloads = 10;

	private const long BytesPerMB = 1024L * 1024;
	private const long BytesPerGB = 1024L * 1024 * 1024;

	/// <summary>
	/// The limit state for the configured scope. "Unlimited" is expressed by <see cref="IsLimited"/> and a null
	/// <see cref="RemainingBooks"/> rather than a sentinel count, so no caller can mistake it for a real number.
	/// </summary>
	/// <param name="AllowsAnother">Whether one more counted download would stay within the limit.</param>
	/// <param name="UsedBooks">Downloads inside the window that count against the configured scope.</param>
	/// <param name="RemainingBooks">Display only, null when unlimited. Approximate in MB/GB mode.</param>
	/// <param name="NextCapacityAt">When enough of the window ages out to allow another download. Null when not blocked.</param>
	public readonly record struct Allowance(
		bool IsLimited,
		bool AllowsAnother,
		Configuration.DailyLimitScope Scope,
		Configuration.DailyLimitUnit Unit,
		int Quantity,
		int UsedBooks,
		long UsedBytes,
		int? RemainingBooks,
		DateTimeOffset? NextCapacityAt)
	{
		/// <summary>True when this title specifically cannot be downloaded right now.</summary>
		public bool Blocks(bool isPlus) => IsLimited && !AllowsAnother && ScopeCounts(Scope, isPlus);

		/// <summary>Limit expressed in bytes, or null when the limit is a book count.</summary>
		public long? LimitBytes => Unit is Configuration.DailyLimitUnit.Books ? null : ToBytes(Quantity, Unit);
	}

	/// <summary>Recent activity regardless of the limit setting, for the throttling suggestion.</summary>
	public readonly record struct RecentActivity(int TotalDownloads, int PlusDownloads, long TotalBytes);

	/// <summary>Whether the configured scope subjects this title to the limit at all.</summary>
	public static bool AppliesTo(bool isPlus, Configuration config) => ScopeCounts(config.DailyDownloadLimit, isPlus);

	private static bool ScopeCounts(Configuration.DailyLimitScope scope, bool isPlus)
		=> scope switch
		{
			Configuration.DailyLimitScope.AllBooks => true,
			Configuration.DailyLimitScope.PlusOnly => isPlus,
			_ => false
		};

	/// <summary>The limit expressed in bytes. Zero for a book count, which is not a size at all.</summary>
	public static long ToBytes(int quantity, Configuration.DailyLimitUnit unit)
		=> unit switch
		{
			Configuration.DailyLimitUnit.MB => quantity * BytesPerMB,
			Configuration.DailyLimitUnit.GB => quantity * BytesPerGB,
			_ => 0
		};

	/// <summary>
	/// Whether one more download stays within <paramref name="quantity"/> of <paramref name="unit"/>, given what
	/// has been downloaded already. Shared with the CLI's per-run limit so the two cannot disagree about what a
	/// book or a byte is worth. In MB/GB mode the answer is necessarily an estimate: a title's size is unknown
	/// until it has been downloaded.
	/// </summary>
	public static bool FitsAnother(Configuration.DailyLimitUnit unit, int quantity, int usedBooks, long usedBytes)
		=> unit is Configuration.DailyLimitUnit.Books
		? usedBooks < quantity
		: usedBytes + DiskSpaceHelper.EstimatedBytesPerAudiobookBackup <= ToBytes(quantity, unit);

	public static Allowance Evaluate(Configuration config, IReadOnlyList<DownloadHistoryEntry> history, DateTimeOffset now)
	{
		var scope = config.DailyDownloadLimit;
		var unit = config.DailyDownloadLimitUnit;
		var quantity = Math.Max(1, config.DailyDownloadLimitQuantity);

		if (scope is Configuration.DailyLimitScope.NoLimit)
			return new Allowance(false, true, scope, unit, quantity, 0, 0, null, null);

		var counted = history
			.Where(e => e.CompletedAt > now - Window && ScopeCounts(scope, e.IsAudiblePlus))
			.OrderBy(e => e.CompletedAt)
			.ToList();

		var usedBooks = counted.Count;
		var usedBytes = counted.Sum(e => e.Bytes);

		// A byte limit smaller than one estimated book would otherwise block downloading forever, and the
		// user would see "limit reached" having downloaded nothing.
		var allowsAnother = usedBooks == 0 || WouldFit(usedBytes, usedBooks);

		return new Allowance(
			IsLimited: true,
			AllowsAnother: allowsAnother,
			Scope: scope,
			Unit: unit,
			Quantity: quantity,
			UsedBooks: usedBooks,
			UsedBytes: usedBytes,
			RemainingBooks: RemainingBooks(usedBytes, usedBooks, allowsAnother),
			NextCapacityAt: allowsAnother ? null : NextCapacityAt(counted, usedBytes));

		bool WouldFit(long bytes, int books) => FitsAnother(unit, quantity, books, bytes);

		int RemainingBooks(long bytes, int books, bool allows)
		{
			if (unit is Configuration.DailyLimitUnit.Books)
				return Math.Max(0, quantity - books);

			var free = ToBytes(quantity, unit) - bytes;
			var whole = free <= 0 ? 0 : (int)Math.Min(int.MaxValue, free / DiskSpaceHelper.EstimatedBytesPerAudiobookBackup);
			// Keep the count honest about the always-allow-one rule.
			return whole == 0 && allows ? 1 : whole;
		}

		// Walk oldest first: capacity returns when enough entries have aged out for one more book to fit.
		DateTimeOffset? NextCapacityAt(List<DownloadHistoryEntry> countedOldestFirst, long bytes)
		{
			long freed = 0;
			for (var i = 0; i < countedOldestFirst.Count; i++)
			{
				freed += countedOldestFirst[i].Bytes;
				if (WouldFit(bytes - freed, countedOldestFirst.Count - i - 1))
					return countedOldestFirst[i].CompletedAt + Window;
			}
			return countedOldestFirst.Count == 0 ? null : countedOldestFirst[^1].CompletedAt + Window;
		}
	}

	public static RecentActivity SummarizeRecent(IReadOnlyList<DownloadHistoryEntry> history, DateTimeOffset now)
	{
		var recent = history.Where(e => e.CompletedAt > now - Window).ToList();
		return new RecentActivity(recent.Count, recent.Count(e => e.IsAudiblePlus), recent.Sum(e => e.Bytes));
	}
}
