using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using FileManager.NamingTemplate;

namespace LibationFileManager.Templates;

internal partial interface IListFormat<TList> where TList : IListFormat<TList>
{
	static IEnumerable<T> FilteredList<T>(string formatString, IEnumerable<T> items)
	{
		return Max(formatString, items);

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

	/// <summary> Max must have a 1 or 2-digit number </summary>
	[GeneratedRegex(@"[Mm]ax\(\s*(?<max>[1-9]\d?)\s*\)")]
	private static partial Regex MaxRegex();

	/// <summary> Separator can be anything </summary>
	[GeneratedRegex(@"[Ss]eparator\((?<separator>.*?)\)")]
	private static partial Regex SeparatorRegex();
}

static class RegExpExtensions
{
	extension(Group group)
	{
		public string? ValueOrNull() => group.Success ? group.Value : null;
		public ReadOnlySpan<char> ValueSpanOrNull() => group.Success ? group.ValueSpan : null;
	}

	extension(Match match)
	{
		public Group Resolve(string? groupName = null)
		{
			if (groupName is not null && match.Groups.TryGetValue(groupName, out var group))
				return group;
			return match.Groups.Count > 1 ? match.Groups[1] : match.Groups[0];
		}

		public string? ResolveValue(string? groupName = null) => match.Resolve(groupName).ValueOrNull();

		public bool TryParseInt(string? groupName, out int value)
		{
			var span = match.Resolve(groupName).ValueSpanOrNull();

			return int.TryParse(span, out value);
		}
	}
}
