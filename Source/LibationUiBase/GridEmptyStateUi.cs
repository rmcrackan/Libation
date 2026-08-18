namespace LibationUiBase;

/// <summary>
/// What to show in the middle of the grid when it has no rows to draw. Shared so both frontends word it
/// identically, and kept in one place so a third empty state does not become a third scattered set of strings.
/// </summary>
public static class GridEmptyStateUi
{
	#region A filter matched nothing

	/// <summary>Headline for a filter that matched nothing in the library.</summary>
	public static string NoMatchesText(string? searchString)
		=> string.IsNullOrWhiteSpace(searchString)
			? "No books match the current filter."
			: $"No books match \"{searchString.Trim()}\".";

	/// <summary>
	/// Follow-up naming matches hiding in the trash. Only worth saying when the filter matched something in
	/// there; "nothing here, and by the way the trash has things in it" would just be noise.
	/// </summary>
	public static string NoMatchesTrashHintText(int matchesInTrash)
		=> matchesInTrash == 1
			? "1 matching book is in the trash."
			: $"{matchesInTrash} matching books are in the trash.";

	public const string OpenTrashBinButton = "Open Trash Bin";

	#endregion

	#region The library itself is empty

	/// <summary>
	/// An empty library has two causes with different next steps, and telling someone with no account to scan
	/// their library is a dead end. <paramref name="anyAccounts"/> is what tells them apart.
	/// </summary>
	public static string EmptyLibraryHeadline(bool anyAccounts)
		=> anyAccounts
			? "No books in your library yet."
			: "Libation is empty.";

	public static string EmptyLibraryDetail(bool anyAccounts)
		=> anyAccounts
			? "Scan your Audible account to bring your books in."
			: "Add your Audible account, then scan your library to bring your books in.";

	public const string AddAccountButton = "Add Account";
	public const string ScanLibraryButton = "Scan Library";

	/// <summary>
	/// The walkthrough is offered once, on first launch, and never again once that is declined. A permanent
	/// way back in costs one button and is the only one a new user is likely to find.
	/// </summary>
	public const string TakeTheTourButton = "Take a Guided Tour";

	#endregion
}
