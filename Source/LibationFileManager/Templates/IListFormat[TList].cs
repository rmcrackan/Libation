using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using FileManager.NamingTemplate;

namespace LibationFileManager.Templates;

internal partial interface IListFormat<TList> where TList : IListFormat<TList>
{
	static IEnumerable<T> FilteredList<T>(string formatString, IEnumerable<T> items, CultureInfo? culture) where T : IFormattable
	{
		return Max(formatString, Slice(formatString, Unique(formatString, Filter(formatString, items, culture), culture)));

		static StringComparer GetStringComparer(CultureInfo? culture)
		{
			return StringComparer.Create(culture ?? CultureInfo.CurrentCulture, ignoreCase: true);
		}

		static IEnumerable<T> Filter(string formatString, IEnumerable<T> items, CultureInfo? culture)
		{
			if (!FilterRegex().TryMatch(formatString, out var filterMatch))
				return items;

			// read the format to apply on each item
			var format = filterMatch.ResolveValue("format");
			// use the operator to get a predicate function that compares the formatted item to the value specified in the filter
			var predicate = CompareCondition.GetPredicate(filterMatch.Value, filterMatch.ResolveValue("op"));
			// the value to compare the formatted item to. Might be a number or a quoted string.
			CommonFormatters.TryGetLiteral(filterMatch.ResolveValue("value"), out var value);

			// return only the items that match the predicate
			return items.Where(FilterPredicate);

			bool FilterPredicate(T n) => predicate(n.ToString(format, culture), value, culture);
		}

		static IEnumerable<T> Unique(string formatString, IEnumerable<T> items, CultureInfo? culture)
		{
			return UniqueRegex().TryMatch(formatString, out var uniqueMatch)
				? items.DistinctBy(n => n.ToString(uniqueMatch.ResolveValue("format"), culture), GetStringComparer(culture))
				: items;
		}

		static IEnumerable<T> Slice(string formatString, IEnumerable<T> items)
		{
			if (!SliceRegex().TryMatch(formatString, out var sliceMatch)) return items;

			sliceMatch.TryParseInt("first", out var first);
			sliceMatch.TryParseInt("last", out var last);
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
		var filteredList = FilteredList(formatString, items, culture);

		if (CountRegex().TryMatch(formatString, out var countMatch))
		{
			var count = filteredList.Count();
			return count == 0 ? [] : [CommonFormatters._FloatFormatter(count, countMatch.ResolveValue("format"), culture)];
		}
		
		var format = TList.FormatRegex().Match(formatString).ResolveValue("format");
		var formattedItems = filteredList.Select(ItemFormatter);
		var separator = SeparatorRegex().Match(formatString).UnescapeValueOrNull("separator");
		if (separator is null)
			return formattedItems;

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
	[GeneratedRegex("""[Ss]eparator\((?<separator>(?:\\.|'[^']*'|"[^"]*"|[^\\'"])*?)\)""")]
	private static partial Regex SeparatorRegex();

	/// <summary> Count will substitute all list members with a single number equal to their count </summary>
	[GeneratedRegex("""[Cc]ount\((?<format>(?:\\.|'[^']*'|"[^"]*"|[^\\'"])*?)\)""")]
	private static partial Regex CountRegex();

	/// <summary> Unique will shrink the list to unique members after applying format to them </summary>
	[GeneratedRegex("""[Uu]nique\((?<format>(?:\\.|'[^']*'|"[^"]*"|[^\\'"])*?)\)""")]
	private static partial Regex UniqueRegex();

	/// <summary> The filter will reduce the list, keeping only the items that match the specified criteria. </summary>
	[GeneratedRegex("""
	                (?x)                         # option x: ignore all unescaped whitespace in pattern and allow comments starting with #
	                [Ff]ilter                    # name of the command 'filter' or 'Filter'
	                \(                           # details are enclosed in brackets
	                    (?<format>(?:            # the first part captured as <format> specifies how to format items before comparison
	                        \\.                  # - '\' escapes always the next character.
	                        | '[^']*'            # - allow 'string' to be included in the format, with '' being an escaped ' character
	                        | "[^"]*"            # - allow "string" to be included in the format, with "" being an escaped " character
	                        | [^\\'"]            # - match any other character. This will not catch the operator at first. Because ...
	                    ) *? )                   # With *? the pattern above tries not to consume the operator.
	                    \s*                      # Separate the following operator with whitespace
	                    (?<op>                   # capture operator in <op>
	                        [\#!≡=≠~<>≤≥&∉∌∈∌⋂⊆⊇⊂⊃-]+  # allow a wide range of operators, all non alphanumeric so that no operator is confused as value
	                        | :[a-z_]+:          # allow :named: operators for readability, e.g. :contains:
	                    ) \s*                    # ignore space between operator and second property
	                    (?<value>                # the second operand is captured as <value> and is a quoted string encapsulated in either single or double quotes
	                          '(?:[^']|'')*'     # - allow 'string' to be included in the format, with '' being an escaped ' character
	                        | "(?:[^"]|"")*"     # - allow "string" to be included in the format, with "" being an escaped " character
	                        | \d+                # - allow a number
	                    )                        #
	                \s* \)                       # end the filter details with optional whitespace and a closing bracket
	                """)]
	private static partial Regex FilterRegex();
}
