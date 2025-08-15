using FileManager.NamingTemplate;
using System.Collections.Generic;
using System.Text.RegularExpressions;

#nullable enable
namespace LibationFileManager.Templates;

internal partial class SeriesListFormat : IListFormat<SeriesListFormat>
{
	public static string Formatter(ITemplateTag _, IEnumerable<SeriesDto>? series, string formatString)
		=> series is null ? string.Empty
		: IListFormat<SeriesListFormat>.Join(formatString, series);

	/// <summary> Format must have at least one of the string {N}, {#}, {ID} </summary>
	[GeneratedRegex(@"[Ff]ormat\((.*?(?:{#(?:\:.*?)?}|{N}|{ID})+.*?)\)")]
	public static partial Regex FormatRegex();
}
