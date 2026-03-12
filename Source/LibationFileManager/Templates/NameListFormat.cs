using System;
using FileManager.NamingTemplate;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace LibationFileManager.Templates;

internal partial class NameListFormat : IListFormat<NameListFormat>
{
	public static string Formatter(ITemplateTag _, IEnumerable<ContributorDto>? names, string formatString, CultureInfo? culture)
		=> names is null
			? string.Empty
			: IListFormat<NameListFormat>.Join(formatString, Sort(names, formatString, ContributorDto.FormatReplacements), culture);

	private static IEnumerable<T> Sort<T>(IEnumerable<T> entries, string formatString, Dictionary<string, Func<T, object?>> formatReplacements)
	{
		var sortMatch = SortRegex().Match(formatString);
		if (!sortMatch.Success) return entries;

		IOrderedEnumerable<T>? ordered = null;
		foreach (Match m in SortTokenizer().Matches(sortMatch.Groups["pattern"].Value))
		{
			// Dictionary is case-insensitive, no ToUpper needed
			if (!formatReplacements.TryGetValue(m.Groups["token"].Value, out var selector))
				continue;

			ordered = ordered is null
				// ReSharper disable once PossibleMultipleEnumeration
				? entries.OrderBy(selector)
				: ordered.ThenBy(selector);
		}

		return ordered ?? entries;
	}

	private const string Token = @"(?:[TFMLS]|ID)";

	/// <summary> Sort must have at least one of the token labels T, F, M, L, S or ID.Add multiple tokens to sort by multiple fields. Spaces may be used to separate tokens.</summary>
	[GeneratedRegex($@"[Ss]ort\(\s*(?i:(?<pattern>(?:{Token}\s*?)+))\s*\)")]
	private static partial Regex SortRegex();

	[GeneratedRegex($@"\G(?<token>{Token})\s*", RegexOptions.IgnoreCase)]
	private static partial Regex SortTokenizer();

	/// <summary> Format must have at least one of the string {T}, {F}, {M}, {L}, {S}, or {ID} </summary>
	[GeneratedRegex($@"[Ff]ormat\((.*?\{{{Token}(?::.*?)?\}}.*?)\)")]
	public static partial Regex FormatRegex();
}
