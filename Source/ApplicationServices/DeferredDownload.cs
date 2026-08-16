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
