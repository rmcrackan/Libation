using FileManager.NamingTemplate;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace LibationFileManager.Templates;

internal partial class SeriesListFormat : IListFormat<SeriesListFormat>
{
	public static string Formatter(ITemplateTag _, IEnumerable<SeriesDto>? series, string formatString, CultureInfo? culture)
		=> series is null
			? string.Empty
			: IListFormat<SeriesListFormat>.Join(formatString, series, culture);

	/// <summary> Format must have at least one of the string {N}, {#}, {ID} </summary>
	[GeneratedRegex(@"[Ff]ormat\((.*?(?:{#(?:\:.*?)?}|{N}|{ID})+.*?)\)")]
	public static partial Regex FormatRegex();
}
