using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

#nullable enable
namespace LibationFileManager.Templates;

internal partial interface IListFormat<TList> where TList : IListFormat<TList>
{
	static string Join<T>(string formatString, IEnumerable<T> items)
		where T : IFormattable
	{
		var itemFormatter = Formatter(formatString);
		var separatorString = Separator(formatString) ?? ", ";
		var maxValues = Max(formatString) ?? items.Count();

		var formattedValues = string.Join(separatorString, items.Take(maxValues).Select(n => n.ToString(itemFormatter, null)));

		while (formattedValues.Contains("  "))
			formattedValues = formattedValues.Replace("  ", " ");

		return formattedValues;

		static string? Formatter(string formatString)
		{
			var formatMatch = TList.FormatRegex().Match(formatString);
			return formatMatch.Success ? formatMatch.Groups[1].Value : null;
		}

		static int? Max(string formatString)
		{
			var maxMatch = MaxRegex().Match(formatString);
			return maxMatch.Success && int.TryParse(maxMatch.Groups[1].Value, out var max) ? int.Max(1, max) : null;
		}

		static string? Separator(string formatString)
		{
			var separatorMatch = SeparatorRegex().Match(formatString);
			return separatorMatch.Success ? separatorMatch.Groups[1].Value : ", ";
		}
	}

	static abstract Regex FormatRegex();

	/// <summary> Separator can be anything </summary>
	[GeneratedRegex(@"[Ss]eparator\((.*?)\)")]
	private static partial Regex SeparatorRegex();

	/// <summary> Max must have a 1 or 2-digit number </summary>
	[GeneratedRegex(@"[Mm]ax\(\s*?(\d{1,2})\s*?\)")]
	private static partial Regex MaxRegex();
}
