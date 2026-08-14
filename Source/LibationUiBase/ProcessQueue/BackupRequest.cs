using DataLayer;
using Dinah.Core;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibationUiBase.ProcessQueue;

/// <summary>Why a title in a backup request cannot be queued for download.</summary>
internal enum BackupSkipReason
{
	AlreadyDownloaded,
	PreviousError,
	AbsentFromLastScan,
	NoAudioOfItsOwn
}

/// <summary>
/// Splits a multi-book backup request into the titles that can be queued and the reason each of the rest
/// was left out, so a request Libation understands and declines can be explained instead of ignored.
/// </summary>
internal sealed class BackupRequest
{
	public const string NothingQueuedCaption = "Download not queued";

	/// <summary>Reasons are reported in this order, which runs from the most to the least common.</summary>
	private static readonly BackupSkipReason[] ReasonOrder =
	[
		BackupSkipReason.AlreadyDownloaded,
		BackupSkipReason.PreviousError,
		BackupSkipReason.AbsentFromLastScan,
		BackupSkipReason.NoAudioOfItsOwn
	];

	/// <summary>The titles the caller asked to back up, including the ones that cannot be queued.</summary>
	public int RequestedCount { get; }
	public LibraryBook[] Queueable { get; }
	public IReadOnlyDictionary<BackupSkipReason, int> SkippedByReason { get; }
	public int SkippedCount => RequestedCount - Queueable.Length;

	private BackupRequest(int requestedCount, LibraryBook[] queueable, IReadOnlyDictionary<BackupSkipReason, int> skippedByReason)
	{
		RequestedCount = requestedCount;
		Queueable = queueable;
		SkippedByReason = skippedByReason;
	}

	public static BackupRequest Create(IEnumerable<LibraryBook> libraryBooks)
	{
		var requestedCount = 0;
		var queueable = new List<LibraryBook>();
		var skipped = new Dictionary<BackupSkipReason, int>();

		foreach (var libraryBook in libraryBooks)
		{
			requestedCount++;

			if (GetSkipReason(libraryBook) is not BackupSkipReason reason)
				queueable.Add(libraryBook);
			else
				skipped[reason] = skipped.GetValueOrDefault(reason) + 1;
		}

		return new BackupRequest(requestedCount, [.. queueable], skipped);
	}

	/// <summary>
	/// Null when the title can be queued. The order of the checks mirrors LibraryBook.Downloadable: a title
	/// absent from the last scan is reported as such no matter what its download status says.
	/// </summary>
	private static BackupSkipReason? GetSkipReason(LibraryBook libraryBook)
		=> libraryBook.NeedsBookDownload || libraryBook.NeedsPdfDownload ? null
		: libraryBook.AbsentFromLastScan ? BackupSkipReason.AbsentFromLastScan
		: libraryBook.Book.ContentType is not (ContentType.Product or ContentType.Episode) ? BackupSkipReason.NoAudioOfItsOwn
		: libraryBook.Book.UserDefinedItem.BookStatus is LiberatedStatus.Error ? BackupSkipReason.PreviousError
		: BackupSkipReason.AlreadyDownloaded;

	/// <summary>A compact breakdown for the log, eg: "already downloaded: 3, absent from last scan: 1".</summary>
	public string BuildSkippedLogSummary()
		=> SkippedCount == 0
		? "none"
		: string.Join(", ", OrderedReasons().Select(r => $"{LogName(r.Reason)}: {r.Count}"));

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

		foreach (var (reason, count) in OrderedReasons())
			sb.AppendLine($"{Describe(reason)}: {count}");

		return sb.ToString().TrimEnd();
	}

	private IEnumerable<(BackupSkipReason Reason, int Count)> OrderedReasons()
		=> ReasonOrder
		.Where(SkippedByReason.ContainsKey)
		.Select(reason => (reason, SkippedByReason[reason]));

	private static string LogName(BackupSkipReason reason)
		=> reason switch
		{
			BackupSkipReason.AlreadyDownloaded => "already downloaded",
			BackupSkipReason.PreviousError => "previous error",
			BackupSkipReason.AbsentFromLastScan => "absent from last scan",
			_ => "no audio of its own"
		};

	private static string Describe(BackupSkipReason reason)
		=> reason switch
		{
			BackupSkipReason.AlreadyDownloaded => "Already downloaded",
			BackupSkipReason.PreviousError => "Previously failed to download (set the download status to 'Not Downloaded' to try again)",
			BackupSkipReason.AbsentFromLastScan => "Absent from your last library scan (run Scan, or `libationcli scan`, then try again)",
			_ => "Series or podcast parent, which has no audio of its own"
		};
}
