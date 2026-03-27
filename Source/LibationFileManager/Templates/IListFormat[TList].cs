using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using static FileManager.NamingTemplate.RegExpExtensions;

namespace LibationFileManager.Templates;

internal partial interface IListFormat<TList> where TList : IListFormat<TList>
{
	static IEnumerable<T> FilteredList<T>(string formatString, IEnumerable<T> items)
	{
		return Max(formatString, Slice(formatString, items));

		static IEnumerable<T> Slice(string formatString, IEnumerable<T> items)
		{
			if (!SliceRegex().TryMatch(formatString, out var sliceMatch)) return items;

			int.TryParse(sliceMatch.Groups["first"].ValueSpan, out var first);
			int.TryParse(sliceMatch.Groups["last"].ValueSpan, out var last);
			if (!sliceMatch.Groups["op"].Success) last = first;

			if (last > 0)
			{
				// ReSharper disable PossibleMultipleEnumeration

				// strange constellation which might not work as intended: slice(-2..3) needs at least 4 items to return anything
				// to get this working, we need to adjust the start-pointer based on the total count of items	
				if (first < 0)
					first += items.Count() + 1;
				items = items.Take(last);
				// ReSharper restore PossibleMultipleEnumeration
			}

			if (first > 1) items = items.Skip(first - 1);
			else if (first < 0) items = items.TakeLast(-first);

			if (last < -1) items = items.SkipLast(-last - 1);

			return items;
		}

		static IEnumerable<T> Max(string formatString, IEnumerable<T> items)
		{
			return MaxRegex().Match(formatString).TryParseInt("max", out var max)
				? items.Take(max)
				: items;
		}
	}

	static IEnumerable<string> FormattedList<T>(string? formatString, IEnumerable<T> items, CultureInfo? culture) where T : IFormattable
	{
		if (formatString is null) return items.Select(n => n.ToString(null, culture));
		var format = TList.FormatRegex().Match(formatString).ResolveValue("format");
		var separator = SeparatorRegex().Match(formatString).ResolveValue("separator");
		var formattedItems = FilteredList(formatString, items).Select(ItemFormatter);

		if (separator is null) return formattedItems;
		var joined = Join(separator, formattedItems);
		return joined is null ? [] : [joined];

		string ItemFormatter(T n) => n.ToString(format, culture);
	}

	static string? Join(IEnumerable<string>? formattedItems, CultureInfo? culture)
	{
		return formattedItems is null ? null : Join(", ", formattedItems);
	}

	private static string? Join(string separator, IEnumerable<string> strings)
	{
		// ReSharper disable PossibleMultipleEnumeration
		return strings.Any()
			? CollapseSpacesAndTrimRegex().Replace(string.Join(separator, strings), "")
			: null;
		// ReSharper restore PossibleMultipleEnumeration
	}

	// Matches runs of spaces followed by a space as well as runs of spaces at the beginning or the end of a string (does NOT touch tabs/newlines).
	[GeneratedRegex(@"^ +| +(?=$| )")]
	private static partial Regex CollapseSpacesAndTrimRegex();

	static abstract Regex FormatRegex();

	/// <summary>
	/// Slice can be a single number or a range like "start..end".
	/// Leaving out one value of a range, it will start on the first or end on the last respectively.
	/// Negative numbers will start counting from the end with "-1" being the last element.
	/// </summary>
	[GeneratedRegex(@"[Ss]lice\(\s*(?<first>-?[1-9]\d*)?\s*(?:(?<op>\.\.\.*)\s*(?<last>-?[1-9]\d*)?(?(first)|(?<=\d))\s*)?\)")]
	private static partial Regex SliceRegex();

	/// <summary> Max must have a 1 or 2-digit number </summary>
	[GeneratedRegex(@"[Mm]ax\(\s*(?<max>[1-9]\d?)\s*\)")]
	private static partial Regex MaxRegex();

	/// <summary> Separator can be anything </summary>
	[GeneratedRegex(@"[Ss]eparator\((?<separator>.*?)\)")]
	private static partial Regex SeparatorRegex();
}
