using System;
using System.Collections.Generic;
using FileManager.NamingTemplate;

namespace LibationFileManager.Templates;

public record StringDto(string Value) : IFormattable
{
	public static readonly Dictionary<string, Func<StringDto, object?>> FormatReplacements = new(StringComparer.OrdinalIgnoreCase)
	{
		{ "S", dto => dto.Value }
	};

	public override string ToString() => Value;

	public string ToString(string? format, IFormatProvider? provider)
		=> string.IsNullOrWhiteSpace(format)
			? ToString()
			: CommonFormatters.TemplateStringFormatter(this, format, provider, FormatReplacements);
}