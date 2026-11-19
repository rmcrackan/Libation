using LibationUiBase.Resources;

namespace LibationUiBase;

/// <summary>
/// "1 book", "3 books": a count with its noun, worded by Strings so a translation can choose the noun's form.
/// Like English, it picks the singular for exactly one and the plural otherwise.
/// </summary>
public static class Counted
{
	public static string Books(int count) => Format(count, Strings.Counted_OneBook, Strings.Counted_Books);
	public static string Titles(int count) => Format(count, Strings.Counted_OneTitle, Strings.Counted_Titles);

	private static string Format(int count, string one, string other) => string.Format(count == 1 ? one : other, count);
}
