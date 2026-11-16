using LibationUiBase.Resources;

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
			? Strings.GridEmptyStateUi_NoBooksMatchTheCurrentFilter
			: string.Format(Strings.GridEmptyStateUi_NoBooksMatch, searchString.Trim());

	/// <summary>
	/// Follow-up naming matches hiding in the trash. Only worth saying when the filter matched something in
	/// there; "nothing here, and by the way the trash has things in it" would just be noise.
	/// </summary>
	public static string NoMatchesTrashHintText(int matchesInTrash)
		=> matchesInTrash == 1
			? Strings.GridEmptyStateUi_1MatchingBookIsInThe
			: string.Format(Strings.GridEmptyStateUi_MatchingBooksAreInTheTrash, matchesInTrash);

	public static string OpenTrashBinButton => Strings.GridEmptyStateUi_OpenTrashBin;

	#endregion

	#region The library itself is empty

	/// <summary>
	/// An empty library has two causes with different next steps, and telling someone with no account to scan
	/// their library is a dead end. <paramref name="anyAccounts"/> is what tells them apart.
	/// </summary>
	public static string EmptyLibraryHeadline(bool anyAccounts)
		=> anyAccounts
			? Strings.GridEmptyStateUi_NoBooksInYourLibraryYet
			: Strings.GridEmptyStateUi_LibationIsEmpty;

	public static string EmptyLibraryDetail(bool anyAccounts)
		=> anyAccounts
			? Strings.GridEmptyStateUi_ScanYourAudibleAccountToBring
			: Strings.GridEmptyStateUi_AddYourAudibleAccountThenScan;

	/// <summary>
	/// Books in the trash while the library itself is empty. Without this the headline above reads as though
	/// the books were never there, when in fact they are one click away.
	/// </summary>
	public static string EmptyLibraryTrashHintText(int booksInTrash)
		=> booksInTrash == 1
			? Strings.GridEmptyStateUi_1BookIsInTheTrash
			: string.Format(Strings.GridEmptyStateUi_BooksAreInTheTrash, booksInTrash);

	public static string AddAccountButton => Strings.GridEmptyStateUi_AddAccount;
	public static string ScanLibraryButton => Strings.GridEmptyStateUi_ScanLibrary;

	/// <summary>
	/// The walkthrough is offered once, on first launch, and never again once that is declined. A permanent
	/// way back in costs one button and is the only one a new user is likely to find.
	/// </summary>
	public static string TakeTheTourButton => Strings.GridEmptyStateUi_TakeAGuidedTour;

	#endregion
}
