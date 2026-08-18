using Dinah.Core;

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
		=> booksInTrash > 0 ? $"Trash Bin ({booksInTrash})" : "Trash Bin";

	/// <summary>Status bar text. Only meaningful when <see cref="ShowStatus"/> is true.</summary>
	public static string StatusText(int booksInTrash)
		=> $"{"book".PluralizeWithCount(booksInTrash)} in trash";

	/// <summary>An empty trash is the normal case and says nothing useful, so it stays off screen.</summary>
	public static bool ShowStatus(int booksInTrash) => booksInTrash > 0;

	public const string StatusToolTip = "These books are hidden from your library. Open the trash bin to restore them.";

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
}
