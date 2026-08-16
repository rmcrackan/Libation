using LibationSearchEngine;
using System;
using System.IO;
using System.Threading;

namespace LibationUiBase;

/// <summary>
/// Shared copy for telling the user that Libation's search index needs to be deleted by hand.
/// Reached only after the automatic delete-and-rebuild has already failed.
/// </summary>
public static class SearchIndexRecovery
{
	public const string Caption = "Search index needs attention";

	public const string ManualRecoveryInstructions
		= "Libation could not use its search index, and could not repair it automatically. "
		+ "Your library itself is fine; only searching and filtering are affected.\n\n"
		+ "To fix it by hand:\n"
		+ "1. In Settings, click 'Open log folder'\n"
		+ "2. Close Libation\n"
		+ "3. Delete the SearchEngine folder you find there\n"
		+ "4. Start Libation again";

	/// <summary>
	/// True when a search failed because the index could not be reached, rather than because the query was
	/// malformed. A damaged index, a held write.lock and a permission problem all arrive as IO-family exceptions,
	/// plus the cloud-sync debris that Lucene reports as an <see cref="ArgumentException"/>; a query Lucene cannot
	/// parse arrives as none of those. Getting this backwards would send the user hunting for a typo in a query
	/// that was fine.
	/// </summary>
	public static bool IsIndexUnavailable(Exception ex)
		=> ex is IOException or UnauthorizedAccessException
		|| SearchEngine.IsRecoverableCorruptIndexException(ex);

	private static int notified;

	/// <summary>
	/// True the first time the index fails in this session, false afterwards. A damaged index fails on every
	/// library change, and these steps only need following once.
	/// </summary>
	public static bool ShouldNotify() => Interlocked.Exchange(ref notified, 1) == 0;
}
