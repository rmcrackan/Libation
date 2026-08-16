using System;
using LibationSearchEngine;

namespace LibationUiBase;

/// <summary>
/// Shared copy for library-import failures caused by a corrupted Lucene SearchEngine index.
/// </summary>
public static class SearchIndexRecovery
{
	public const string Caption = "Error importing library";

	public static string ManualRecoveryInstructions => SearchEngine.ManualIndexRecoveryInstructions;

	public static bool TryFindFailure(Exception ex, out Exception? indexException)
		=> SearchEngine.TryFindSearchIndexFailure(ex, out indexException);
}
