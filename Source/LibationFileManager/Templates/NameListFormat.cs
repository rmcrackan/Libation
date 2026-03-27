using System;
using FileManager.NamingTemplate;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace LibationFileManager.Templates;

internal partial class NameListFormat : IListFormat<NameListFormat>
{
	public static IEnumerable<string> Formatter(ITemplateTag _, IEnumerable<ContributorDto>? names, string? formatString, CultureInfo? culture)
		=> names is null
			? []
			: IListFormat<NameListFormat>.FormattedList(formatString, Sort(names, formatString, ContributorDto.FormatReplacements), culture);

	public static string? Finalizer(ITemplateTag _, IEnumerable<string>? names, CultureInfo? culture)
		=> IListFormat<NameListFormat>.Join(names, culture);

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

	private const string Token = @"(?:[TFMLS]|ID)";

	/// <summary> Sort must have at least one of the token labels T, F, M, L, S or ID. Use lower case for descending direction and add multiple tokens to sort by multiple fields. Spaces may be used to separate tokens.</summary>
	[GeneratedRegex($@"[Ss]ort\(\s*(?i:(?<pattern>(?:{Token}\s*?)+))\s*\)")]
	private static partial Regex SortRegex();

	[GeneratedRegex($@"\G(?<token>{Token})(?<descending>(?-i:(?<=\G\P{{Lu}}+)))?\s*", RegexOptions.IgnoreCase)]
	private static partial Regex SortTokenizer();

	/// <summary> Format must have at least one of the strings {T}, {F}, {M}, {L}, {S}, or {ID} (optionally with formatting like {L:u})</summary>
	[GeneratedRegex($@"[Ff]ormat\((?<format>.*?\{{{Token}(?::.*?)?\}}.*?)\)")]
	public static partial Regex FormatRegex();
}
