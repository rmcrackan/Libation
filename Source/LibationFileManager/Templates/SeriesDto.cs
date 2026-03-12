using System;
using System.Text.RegularExpressions;

namespace LibationFileManager.Templates;

public partial record SeriesDto(string? Name, string? Number, string AudibleSeriesId) : IFormattable
{
	public SeriesOrder Order { get; } = SeriesOrder.Parse(Number);

	public override string? ToString() => Name?.Trim();
	public string ToString(string? format, IFormatProvider? provider)
		=> string.IsNullOrWhiteSpace(format) ? ToString() ?? string.Empty
			: FormatRegex().Replace(format, MatchEvaluator)
			.Replace("{N}", Name)
			.Replace("{ID}", AudibleSeriesId)
			.Trim();

	private string MatchEvaluator(Match match)
		=> Order?.ToString(match.Groups[1].Value, null) ?? "";

	/// <summary> Format must have at least one of the string {N}, {#}, {ID} </summary>
	[GeneratedRegex(@"{#(?:\:(.*?))?}")]
	private static partial Regex FormatRegex();
}
