using FileManager.NamingTemplate;
using NameParser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace LibationFileManager
{
	internal partial class NameListFormat
	{
		public static string Formatter(ITemplateTag _, IEnumerable<string> names, string formatString)
		{
			var humanNames = names.Select(n => new HumanName(RemoveSuffix(n), Prefer.FirstOverPrefix));

			var sortedNames = Sort(humanNames, formatString);
			var nameFormatString = Format(formatString, defaultValue: "{T} {F} {M} {L} {S}");
			var separatorString = Separator(formatString, defaultValue: ", ");
			var maxNames = Max(formatString, defaultValue: humanNames.Count());

			var formattedNames = string.Join(separatorString, sortedNames.Take(maxNames).Select(n => FormatName(n, nameFormatString)));

			while (formattedNames.Contains("  "))
				formattedNames = formattedNames.Replace("  ", " ");

			return formattedNames;
		}

		private static string RemoveSuffix(string namesString)
		{
			namesString = namesString.Replace('’', '\'').Replace(" - Ret.", ", Ret.");
			int dashIndex = namesString.IndexOf(" - ");
			return (dashIndex > 0 ? namesString[..dashIndex] : namesString).Trim();
		}

		private static IEnumerable<HumanName> Sort(IEnumerable<HumanName> humanNames, string formatString)
		{
			var sortMatch = SortRegex().Match(formatString);
			return
				sortMatch.Success
				? sortMatch.Groups[1].Value == "F" ? humanNames.OrderBy(n => n.First)
					: sortMatch.Groups[1].Value == "M" ? humanNames.OrderBy(n => n.Middle)
					: sortMatch.Groups[1].Value == "L" ? humanNames.OrderBy(n => n.Last)
					: humanNames
				: humanNames;
		}

		private static string Format(string formatString, string defaultValue)
		{
			var formatMatch = FormatRegex().Match(formatString);
			return formatMatch.Success ? formatMatch.Groups[1].Value : defaultValue;
		}

		private static string Separator(string formatString, string defaultValue)
		{
			var separatorMatch = SeparatorRegex().Match(formatString);
			return separatorMatch.Success ? separatorMatch.Groups[1].Value : defaultValue;
		}

		private static int Max(string formatString, int defaultValue)
		{
			var maxMatch = MaxRegex().Match(formatString);
			return maxMatch.Success && int.TryParse(maxMatch.Groups[1].Value, out var max) ? int.Max(1, max) : defaultValue;
		}

		private static string FormatName(HumanName humanName, string nameFormatString)
		{
			//Single-word names parse as first names. Use it as last name.
			var lastName = string.IsNullOrWhiteSpace(humanName.Last) ? humanName.First : humanName.Last;

			nameFormatString
				= nameFormatString
				.Replace("{T}", "{0}")
				.Replace("{F}", "{1}")
				.Replace("{M}", "{2}")
				.Replace("{L}", "{3}")
				.Replace("{S}", "{4}");

			return string.Format(nameFormatString, humanName.Title, humanName.First, humanName.Middle, lastName, humanName.Suffix).Trim();
		}

		/// <summary> Sort must have exactly one of the characters F, M, or L </summary>
		[GeneratedRegex(@"[Ss]ort\(\s*?([FML])\s*?\)")]
		private static partial Regex SortRegex();
		/// <summary> Format must have at least one of the string {T}, {F}, {M}, {L}, or {S} </summary>
		[GeneratedRegex(@"[Ff]ormat\((.*?(?:{[TFMLS]})+.*?)\)")]
		private static partial Regex FormatRegex();
		/// <summary> Separator can be anything </summary>
		[GeneratedRegex(@"[Ss]eparator\((.*?)\)")]
		private static partial Regex SeparatorRegex();
		/// <summary> Max must have a 1 or 2-digit number </summary>
		[GeneratedRegex(@"[Mm]ax\(\s*?(\d{1,2})\s*?\)")]
		private static partial Regex MaxRegex();
	}
}
