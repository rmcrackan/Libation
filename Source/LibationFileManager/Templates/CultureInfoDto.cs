using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using FileManager.NamingTemplate;

namespace LibationFileManager.Templates;

public record CultureInfoDto : IFormattable
{
	private CultureInfo? Value { get; }
	private string DefaultFormat { get; }
	public string Original { get; }

	public static CultureInfoDto OfCurrentUi()
	{
		return new CultureInfoDto(CultureInfo.DefaultThreadCurrentUICulture ?? CultureInfo.CurrentUICulture, CultureInfo.CurrentUICulture.Name, "{N}");
	}

	public static CultureInfoDto OfCurrentOs()
	{
		return new CultureInfoDto(CultureInfo.DefaultThreadCurrentCulture ?? CultureInfo.CurrentCulture, CultureInfo.CurrentCulture.Name, "{N}");
	}

	public CultureInfoDto(string hint) : this(hint, "{O}")
	{
	}

	public CultureInfoDto(string hint, string defaultFormat) : this(GetCulture(hint), hint, defaultFormat)
	{
	}

	public CultureInfoDto(CultureInfo? value, string hint, string defaultFormat)
	{
		Original = hint;
		DefaultFormat = defaultFormat;
		Value = value;
	}

	private static CultureInfo? GetCulture(string input)
	{
		return
			GetCulture(input, CultureTypes.NeutralCultures) ??
			GetCulture(input, CultureTypes.SpecificCultures);
	}

	private static CultureInfo? GetCulture(string input, CultureTypes types)
	{
		var cultures = CultureInfo.GetCultures(types);
		return Match(cultures, input, c => c.Name) ??
		       Match(cultures, input, c => c.TwoLetterISOLanguageName) ??
		       Match(cultures, input, c => c.ThreeLetterISOLanguageName) ??
		       Match(cultures, input, c => c.EnglishName);
	}

	private static CultureInfo? Match(IEnumerable<CultureInfo> cultures, string input, Func<CultureInfo, string?> selector, StringComparison cmp = StringComparison.OrdinalIgnoreCase)
	{
		return cultures.FirstOrDefault(c => string.Equals(selector(c), input, cmp));
	}

	private static readonly Dictionary<string, Func<CultureInfoDto, object?>> FormatReplacements = new(StringComparer.OrdinalIgnoreCase)
	{
		{ "ID", dto => dto.Value?.Name },
		{ "I", dto => dto.Value?.TwoLetterISOLanguageName },
		{ "I2", dto => dto.Value?.TwoLetterISOLanguageName },
		{ "I3", dto => dto.Value?.ThreeLetterISOLanguageName },
		{ "W", dto => dto.Value?.ThreeLetterWindowsLanguageName },
		{ "E", dto => dto.Value?.EnglishName },
		{ "N", dto => dto.Value?.NativeName },
		{ "O", dto => dto.Original },
		{ "D", dto => dto.Value?.DisplayName }, // localized
	};

	public override string ToString() => ToString(DefaultFormat, CultureInfo.CurrentCulture);

	public string ToString(string? format, IFormatProvider? provider)
	{
		if (string.IsNullOrWhiteSpace(format)) format = DefaultFormat;
		return format switch
		{
			_ when CommonFormatters.TagFormatRegex().IsMatch(format) => CommonFormatters.TemplateStringFormatter(this, format, provider, FormatReplacements),
			_ when format == DefaultFormat => CommonFormatters._StringFormatter(Original, format, provider),
			_ => CommonFormatters._StringFormatter(CommonFormatters.TemplateStringFormatter(this, DefaultFormat, provider, FormatReplacements), format, provider)
		};
	}
}