using Dinah.Core;
using LibationUiBase.Resources;

namespace LibationUiBase;

/// <summary>
/// Wording for the trash bin affordances, shared so both frontends say the same thing.
/// Removal is a soft delete, so a book can sit in the trash indefinitely with nothing on screen
/// suggesting it exists. These make a non-empty trash visible without nagging about an empty one.
/// </summary>
public static class TrashBinUi
{
	/// <summary>Menu item text, carrying the count only when there is something in the trash.</summary>
	public static string MenuText(int booksInTrash)
		=> booksInTrash > 0 ? string.Format(Strings.TrashBinUi_TrashBin, booksInTrash) : Strings.TrashBinUi_TrashBin2;

	/// <summary>Status bar text. Only meaningful when <see cref="ShowStatus"/> is true.</summary>
	public static string StatusText(int booksInTrash)
		=> string.Format(Strings.TrashBinUi_InTrash, "book".PluralizeWithCount(booksInTrash));

	/// <summary>An empty trash is the normal case and says nothing useful, so it stays off screen.</summary>
	public static bool ShowStatus(int booksInTrash) => booksInTrash > 0;

	public static string StatusToolTip => Strings.TrashBinUi_TheseBooksAreHiddenFromYour;
}
