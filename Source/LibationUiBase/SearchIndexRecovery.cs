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
		= "Libation could not update its search index, and could not repair it automatically. "
		+ "Your library itself is fine -- only searching and filtering are affected.\n\n"
		+ "To fix it by hand:\n"
		+ "1. In Settings, click 'Open log folder'\n"
		+ "2. Close Libation\n"
		+ "3. Delete the SearchEngine folder you find there\n"
		+ "4. Start Libation again";

	private static int notified;

	/// <summary>
	/// True the first time the index fails in this session, false afterwards. A damaged index fails on every
	/// library change, and these steps only need following once, so repeats are left to the log.
	/// </summary>
	public static bool ShouldNotify() => Interlocked.Exchange(ref notified, 1) == 0;
}
