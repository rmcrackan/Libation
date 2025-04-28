using FileManager.NamingTemplate;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

#nullable enable
namespace LibationFileManager.Templates;

internal partial class NameListFormat : IListFormat<NameListFormat>
{
	public static string Formatter(ITemplateTag _, IEnumerable<ContributorDto>? names, string formatString)
		=> names is null ? string.Empty
		: IListFormat<NameListFormat>.Join(formatString, Sort(names, formatString));

	private static IEnumerable<ContributorDto> Sort(IEnumerable<ContributorDto> names, string formatString)
	{
		var sortMatch = SortRegex().Match(formatString);
		return
			sortMatch.Success
			? sortMatch.Groups[1].Value == "F" ? names.OrderBy(n => n.HumanName.First)
				: sortMatch.Groups[1].Value == "M" ? names.OrderBy(n => n.HumanName.Middle)
				: sortMatch.Groups[1].Value == "L" ? names.OrderBy(n => n.HumanName.Last)
				: names
			: names;
	}

	/// <summary> Sort must have exactly one of the characters F, M, or L </summary>
	[GeneratedRegex(@"[Ss]ort\(\s*?([FML])\s*?\)")]
	private static partial Regex SortRegex();
	/// <summary> Format must have at least one of the string {T}, {F}, {M}, {L}, {S}, or {ID} </summary>
	[GeneratedRegex(@"[Ff]ormat\((.*?(?:{[TFMLS]}|{ID})+.*?)\)")]
	public static partial Regex FormatRegex();
}
