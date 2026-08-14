using ApplicationServices;
using LibationFileManager;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace LibationCli;

/// <summary>
/// How much a single <c>liberate</c> run may download, from the mutually exclusive <c>--limit-books</c>,
/// <c>--limit-mb</c> and <c>--limit-gb</c> options. Distinct from the daily download limit, which is a setting
/// spanning a rolling 24 hours and keeps applying on top of this one.
/// </summary>
public readonly record struct RunDownloadLimit(Configuration.DailyLimitUnit Unit, int Quantity)
{
	public string Describe()
		=> Unit is Configuration.DailyLimitUnit.Books ? $"{Quantity} book(s)" : $"{Quantity} {Unit}";

	public static string OptionName(Configuration.DailyLimitUnit unit)
		=> unit switch
		{
			Configuration.DailyLimitUnit.MB => "--limit-mb",
			Configuration.DailyLimitUnit.GB => "--limit-gb",
			_ => "--limit-books"
		};

	/// <summary>
	/// Builds the limit from the three options, at most one of which the parser lets through. Passing none is
	/// success with a null <paramref name="limit"/>: an unlimited run is the default, not an error.
	/// </summary>
	public static bool TryCreate(
		int? books,
		int? megabytes,
		int? gigabytes,
		bool pdfOnly,
		out RunDownloadLimit? limit,
		[NotNullWhen(false)] out string? error)
	{
		limit = null;
		error = null;

		(Configuration.DailyLimitUnit Unit, int Quantity)? specified
			= books is int b ? (Configuration.DailyLimitUnit.Books, b)
			: megabytes is int mb ? (Configuration.DailyLimitUnit.MB, mb)
			: gigabytes is int gb ? (Configuration.DailyLimitUnit.GB, gb)
			: null;

		if (specified is null)
			return true;

		var (unit, quantity) = specified.Value;

		if (quantity < 1)
		{
			error = $"{OptionName(unit)} must be at least 1.";
			return false;
		}

		// Pdf downloads are never recorded, so a limit combined with --pdf would silently never stop the run.
		if (pdfOnly)
		{
			error = $"{OptionName(unit)} cannot be used with --pdf. The limit counts audiobook downloads, and --pdf downloads no audiobooks.";
			return false;
		}

		limit = new RunDownloadLimit(unit, quantity);
		return true;
	}
}

/// <summary>
/// Tracks how much one run has downloaded and decides when it has had enough.
/// <para>
/// Counts the same downloads the daily limit counts, by reading the history rows written when a download
/// succeeds, so failed, cancelled and pdf-only work is never counted and the two limits cannot disagree about
/// what a book or a byte is worth. Only titles this run attempted are counted, so a Libation window or a second
/// container downloading at the same time does not consume this run's allowance.
/// </para>
/// </summary>
internal sealed class RunLimitTracker
{
	private const string StopSuffix = "Remaining titles are still un-liberated and will be tried on the next run.";

	private readonly RunDownloadLimit limit;
	private readonly DateTimeOffset runStart;
	private readonly Func<DateTimeOffset, IReadOnlyList<DownloadHistoryEntry>> readHistory;
	private readonly HashSet<string> attempted = new(StringComparer.OrdinalIgnoreCase);

	public RunLimitTracker(
		RunDownloadLimit limit,
		DateTimeOffset runStart,
		Func<DateTimeOffset, IReadOnlyList<DownloadHistoryEntry>>? readHistory = null)
	{
		this.limit = limit;
		this.runStart = runStart;
		this.readHistory = readHistory ?? DownloadHistoryStore.GetSince;
	}

	/// <summary>Call for every title handed to the processable, whether or not it ends up downloading.</summary>
	public void Attempting(string? audibleProductId)
	{
		if (!string.IsNullOrWhiteSpace(audibleProductId))
			attempted.Add(audibleProductId);
	}

	/// <summary>
	/// True when the run has downloaded all it may. Re-reads history on every call, so it reflects whatever the
	/// last title turned out to weigh rather than an estimate.
	/// </summary>
	public bool TryStop([NotNullWhen(true)] out string? message)
	{
		var (books, bytes) = Downloaded();

		// One download is always allowed: a limit smaller than a single estimated book would otherwise end the
		// run immediately and report a limit reached to someone who had downloaded nothing.
		if (books == 0 || DailyDownloadLimit.FitsAnother(limit.Unit, limit.Quantity, books, bytes))
		{
			message = null;
			return false;
		}

		message = limit.Unit is Configuration.DailyLimitUnit.Books
			? $"Reached this run's limit of {limit.Describe()}. Downloaded {books} title(s); stopping. {StopSuffix}"
			: $"Reached this run's limit of {limit.Describe()}. Downloaded about {DiskSpaceHelper.FormatBytes(bytes)} across {books} title(s); stopping. {StopSuffix}";
		return true;
	}

	private (int Books, long Bytes) Downloaded()
	{
		if (attempted.Count == 0)
			return (0, 0);

		var mine = readHistory(runStart)
			.Where(e => e.AudibleProductId is string id && attempted.Contains(id))
			.ToList();

		return (mine.Count, mine.Sum(e => e.Bytes));
	}
}
