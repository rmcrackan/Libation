using System;
using System.Collections.Generic;

namespace LibationUiBase.StatusIcons;

/// <summary>What the Liberate column's icon depicts.</summary>
public enum LiberateIconKind
{
	/// <summary>A stoplight, optionally with a PDF glyph beside it.</summary>
	Book,
	/// <summary>A series row which can be expanded.</summary>
	SeriesCollapsed,
	/// <summary>A series row which can be collapsed.</summary>
	SeriesExpanded,
	/// <summary>A book whose download errored.</summary>
	Error
}

/// <summary>Which of the stoplight's three lamps is lit.</summary>
public enum StoplightLamp { Red, Yellow, Green }

/// <summary>The PDF glyph drawn beside the stoplight, if any.</summary>
public enum PdfOverlay { None, Downloaded, NotDownloaded }

/// <summary>
/// Identifies one Liberate column icon. There is a small, finite number of these, so
/// <see cref="StatusImageGenerator"/> caches a rendering of each one.
/// </summary>
/// <remarks>
/// Use the factory methods rather than the constructor: they leave the members which don't apply to
/// a given <see cref="LiberateIconKind"/> at their default, so that (for example) every series row
/// shares one cache entry instead of one per lamp/PDF combination.
/// </remarks>
/// <param name="IsPlus">Whether the book is an Audible Plus title rather than a purchased one.</param>
public readonly record struct LiberateIconDescriptor(LiberateIconKind Kind, StoplightLamp Lamp, PdfOverlay Pdf, bool IsPlus, bool IsDark)
{
	public static LiberateIconDescriptor ForBook(StoplightLamp lamp, PdfOverlay pdf, bool isPlus, bool isDark)
		=> new(LiberateIconKind.Book, lamp, pdf, isPlus, isDark);

	public static LiberateIconDescriptor ForSeries(bool expanded, bool isDark)
		=> new(expanded ? LiberateIconKind.SeriesExpanded : LiberateIconKind.SeriesCollapsed, default, default, default, isDark);

	public static LiberateIconDescriptor ForError(bool isDark)
		=> new(LiberateIconKind.Error, default, default, default, isDark);

	/// <summary>Every icon the Liberate column can display.</summary>
	public static IEnumerable<LiberateIconDescriptor> All()
	{
		foreach (var isDark in new[] { false, true })
		{
			foreach (var lamp in Enum.GetValues<StoplightLamp>())
				foreach (var pdf in Enum.GetValues<PdfOverlay>())
					foreach (var isPlus in new[] { false, true })
						yield return ForBook(lamp, pdf, isPlus, isDark);

			yield return ForSeries(expanded: false, isDark);
			yield return ForSeries(expanded: true, isDark);
			yield return ForError(isDark);
		}
	}
}
