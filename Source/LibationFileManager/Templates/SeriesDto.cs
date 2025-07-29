using System;

#nullable enable
namespace LibationFileManager.Templates;

public record SeriesDto : IFormattable
{
	public string Name { get; }

	public string? Number { get; }
	public string AudibleSeriesId { get; }
	public SeriesDto(string name, string? number, string audibleSeriesId)
	{
		Name = name;
		Number = number;
		AudibleSeriesId = audibleSeriesId;
	}

	public override string ToString() => Name.Trim();
	public string ToString(string? format, IFormatProvider? _)
		=> string.IsNullOrWhiteSpace(format) ? ToString()
		: format
		.Replace("{N}", Name)
		.Replace("{#}", Number?.ToString())
		.Replace("{ID}", AudibleSeriesId)
		.Trim();
}
