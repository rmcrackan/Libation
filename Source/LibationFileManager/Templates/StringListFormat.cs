using System;
using FileManager.NamingTemplate;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace LibationFileManager.Templates;

internal partial class StringListFormat : IListFormat<StringListFormat>
{
	public static IEnumerable<string> Formatter(ITemplateTag _, IEnumerable<StringDto>? entries, string? formatString, CultureInfo? culture)
		=> entries is null
			? []
			: IListFormat<StringListFormat>.FormattedList(formatString, Sort(entries, formatString, StringDto.FormatReplacements), culture);

	public static string? Finalizer(ITemplateTag _, IEnumerable<string>? entries, CultureInfo? culture)
		=> IListFormat<StringListFormat>.Join(entries, culture);

	private static IEnumerable<T> Sort<T>(IEnumerable<T> entries, string? formatString, Dictionary<string, Func<T, object?>> formatReplacements)
	{
		var pattern = formatString is null ? null : SortRegex().Match(formatString).ResolveValue("pattern");
		if (pattern is null) return entries;

		IOrderedEnumerable<T>? ordered = null;
		foreach (Match m in SortTokenizer().Matches(pattern))
		{
			// Dictionary is case-insensitive, no ToUpper needed
			if (!formatReplacements.TryGetValue(m.Groups["token"].Value, out var selector))
				continue;

			ordered = m.Groups["descending"].Success
				? ordered is null
					// ReSharper disable once PossibleMultipleEnumeration
					? entries.OrderByDescending(selector)
					: ordered.ThenByDescending(selector)
				: ordered is null
					// ReSharper disable once PossibleMultipleEnumeration
					? entries.OrderBy(selector)
					: ordered.ThenBy(selector);
		}

		return ordered ?? entries;
	}

	private const string Token = "S";

	/// <summary> Sort must have the token label S. Use lower case for descending direction.</summary>
	[GeneratedRegex($@"[Ss]ort\(\s*(?i:(?<pattern>(?:{Token}\s*?)+))\s*\)")]
	private static partial Regex SortRegex();

	[GeneratedRegex($@"\G(?<token>{Token})(?<descending>(?-i:(?<=\G\P{{Lu}}+)))?\s*", RegexOptions.IgnoreCase)]
	private static partial Regex SortTokenizer();

	/// <summary> Format must have the string {S} (optionally with formatting like {S:u})</summary>
	[GeneratedRegex($@"[Ff]ormat\((?<format>.*?\{{{Token}(?::.*?)?\}}.*?)\)")]
	public static partial Regex FormatRegex();
}