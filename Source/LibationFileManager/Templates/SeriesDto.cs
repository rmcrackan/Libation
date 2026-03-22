using System;
using System.Collections.Generic;
using FileManager.NamingTemplate;

namespace LibationFileManager.Templates;

public record SeriesDto(string? Name, string? Number, string AudibleSeriesId) : IFormattable
{
	public SeriesOrder Order { get; } = SeriesOrder.Parse(Number);

	public static readonly Dictionary<string, Func<SeriesDto, object?>> FormatReplacements = new(StringComparer.OrdinalIgnoreCase)
	{
		{ "#", dto => dto.Order },
		{ "N", dto => dto.Name },
		{ "ID", dto => dto.AudibleSeriesId }
	};

	public override string? ToString() => Name?.Trim();

	public string ToString(string? format, IFormatProvider? provider)
		=> string.IsNullOrWhiteSpace(format)
			? ToString() ?? string.Empty
			: CommonFormatters.TemplateStringFormatter(this, format, provider, FormatReplacements);
}
