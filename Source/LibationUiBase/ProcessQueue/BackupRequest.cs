using DataLayer;
using Dinah.Core;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibationUiBase.ProcessQueue;

/// <summary>
/// Splits a multi-book backup request into the titles that can be queued and the reason each of the rest
/// was left out, so a request Libation understands and declines can be explained instead of ignored.
/// </summary>
internal sealed class BackupRequest
{
	public const string NothingQueuedCaption = "Download not queued";

	/// <summary>Why a title cannot be queued. Reported in declaration order.</summary>
	internal sealed record SkipReason(string Label, string Advice = "")
	{
		public static readonly SkipReason AlreadyDownloaded = new("Already downloaded");
		public static readonly SkipReason PreviousError = new("Previously failed to download", "set the download status to 'Not Downloaded' to try again");
		public static readonly SkipReason AbsentFromLastScan = new("Absent from your last library scan", "run Scan, or `libationcli scan`, then try again");

		public static readonly SkipReason[] All = [AlreadyDownloaded, PreviousError, AbsentFromLastScan];
	}

	/// <summary>The titles the caller asked to back up, including the ones that cannot be queued.</summary>
	public int RequestedCount { get; }
	public LibraryBook[] Queueable { get; }
	public int SkippedCount => RequestedCount - Queueable.Length;
	public int Skipped(SkipReason reason) => skipped.GetValueOrDefault(reason);

	private readonly Dictionary<SkipReason, int> skipped;

	private BackupRequest(int requestedCount, LibraryBook[] queueable, Dictionary<SkipReason, int> skipped)
	{
		RequestedCount = requestedCount;
		Queueable = queueable;
		this.skipped = skipped;
	}

	public static BackupRequest Create(IEnumerable<LibraryBook> libraryBooks)
	{
		var requestedCount = 0;
		var queueable = new List<LibraryBook>();
		var skipped = new Dictionary<SkipReason, int>();

		foreach (var libraryBook in libraryBooks)
		{
			requestedCount++;

			if (GetSkipReason(libraryBook) is not SkipReason reason)
				queueable.Add(libraryBook);
			else
				skipped[reason] = skipped.GetValueOrDefault(reason) + 1;
		}

		return new BackupRequest(requestedCount, [.. queueable], skipped);
	}

	/// <summary>Null when the title can be queued. Absent outranks status: Downloadable is false either way.</summary>
	private static SkipReason? GetSkipReason(LibraryBook libraryBook)
		=> libraryBook.NeedsBookDownload || libraryBook.NeedsPdfDownload ? null
		: libraryBook.AbsentFromLastScan ? SkipReason.AbsentFromLastScan
		: libraryBook.Book.UserDefinedItem.BookStatus is LiberatedStatus.Error ? SkipReason.PreviousError
		: SkipReason.AlreadyDownloaded;

	/// <summary>A compact breakdown for the log, eg: "already downloaded: 3, absent from your last library scan: 1".</summary>
	public string BuildSkippedLogSummary()
		=> SkippedCount == 0
		? "none"
		: string.Join(", ", Breakdown().Select(b => $"{b.Reason.Label.ToLowerInvariant()}: {b.Count}"));

	/// <summary>The dialog body shown when a backup request produced nothing to queue.</summary>
	public string BuildNothingQueuedBody()
	{
		if (RequestedCount == 0)
			return """
				Libation found no titles that need downloading.

				Titles that are already downloaded, and titles that were absent from your last library scan, are not queued.
				""";

		var sb = new StringBuilder();
		sb.AppendLine($"None of the {"title".PluralizeWithCount(RequestedCount)} could be queued for download.");
		sb.AppendLine();

		//the count comes before the advice so the numbers stay scannable
		foreach (var (reason, count) in Breakdown())
			sb.AppendLine($"{reason.Label}: {count}{(reason.Advice is "" ? "" : $"  ({reason.Advice})")}");

		return sb.ToString().TrimEnd();
	}

	private IEnumerable<(SkipReason Reason, int Count)> Breakdown()
		=> SkipReason.All.Where(skipped.ContainsKey).Select(reason => (reason, skipped[reason]));
}
