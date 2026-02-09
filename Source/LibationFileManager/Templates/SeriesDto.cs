using System;
using System.Text.RegularExpressions;

namespace LibationFileManager.Templates;

public partial record SeriesDto : IFormattable
{
	public string? Name { get; }

	public SeriesOrder Order { get; }
	public string AudibleSeriesId { get; }
	public SeriesDto(string? name, string? number, string audibleSeriesId)
	{
		Name = name;
		Order = SeriesOrder.Parse(number);
		AudibleSeriesId = audibleSeriesId;
	}

	public override string? ToString() => Name?.Trim();
	public string ToString(string? format, IFormatProvider? _)
		=> string.IsNullOrWhiteSpace(format) ? ToString() ?? string.Empty
			: FormatRegex().Replace(format, MatchEvaluator)
			.Replace("{N}", Name)
			.Replace("{ID}", AudibleSeriesId)
			.Trim();

	private string MatchEvaluator(Match match)
		=> Order?.ToString(match.Groups[1].Value, null) ?? "";

	/// <summary> Format must have at least one of the string {N}, {#}, {ID} </summary>
	[GeneratedRegex(@"{#(?:\:(.*?))?}")]
	public static partial Regex FormatRegex();
}
