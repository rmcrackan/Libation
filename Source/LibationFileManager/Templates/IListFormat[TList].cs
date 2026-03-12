using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace LibationFileManager.Templates;

internal partial interface IListFormat<TList> where TList : IListFormat<TList>
{
	static IEnumerable<T> FilteredList<T>(string formatString, IEnumerable<T> items)
	{
		return Max(formatString, items);

		static IEnumerable<T> Max(string formatString, IEnumerable<T> items)
		{
			var maxMatch = MaxRegex().Match(formatString);
			return maxMatch.Success && int.TryParse(maxMatch.Groups[1].ValueSpan, out var max)
				? items.Take(max)
				: items;
		}
	}

	static IEnumerable<string> FormattedList<T>(string formatString, IEnumerable<T> items, CultureInfo? culture) where T : IFormattable
	{
		var format = FormatElement(formatString, TList.FormatRegex);
		var separator = FormatElement(formatString, SeparatorRegex);
		var formattedItems = FilteredList(formatString, items).Select(ItemFormatter);

		// ReSharper disable PossibleMultipleEnumeration
		return separator is null
			? formattedItems
			: formattedItems.Any()
				? [Join(separator, formattedItems)]
				: [];
		// ReSharper restore PossibleMultipleEnumeration

		string ItemFormatter(T n) => n.ToString(format, culture);

		static string? FormatElement(string formatString, Func<Regex> regex)
		{
			var match = regex().Match(formatString);
			return match.Success ? match.Groups[1].Value : null;
		}
	}

	static string Join<T>(string formatString, IEnumerable<T> items, CultureInfo? culture) where T : IFormattable
	{
		return Join(", ", FormattedList(formatString, items, culture));
	}

	private static string Join(string separator, IEnumerable<string> strings)
	{
		return CollapseSpacesRegex().Replace(string.Join(separator, strings), " ");
	}

	// Collapses runs of 2+ spaces into a single space (does NOT touch tabs/newlines).
	[GeneratedRegex(@" {2,}")]
	private static partial Regex CollapseSpacesRegex();

	static abstract Regex FormatRegex();

	/// <summary> Max must have a 1 or 2-digit number </summary>
	[GeneratedRegex(@"[Mm]ax\(\s*([1-9]\d?)\s*\)")]
	private static partial Regex MaxRegex();

	/// <summary> Separator can be anything </summary>
	[GeneratedRegex(@"[Ss]eparator\((.*?)\)")]
	private static partial Regex SeparatorRegex();
}
