using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using AudibleApi;
using FileManager.NamingTemplate;

namespace LibationFileManager.Templates;

public partial record LocaleDto : IFormattable
{
	public string Original { get; }
	public Locale Locale { get; set; }
	private RegionInfo? Value { get; }
	private CultureInfo? Culture { get; }
	private string DefaultFormat { get; }


	public LocaleDto(string hint) : this(hint, "{O}")
	{
	}

	public LocaleDto(string hint, string defaultFormat) : this(Localization.Get(hint), hint, defaultFormat)
	{
	}

	public LocaleDto(Locale locale, string hint, string defaultFormat)
	{
		var (regionInfo, cultureInfo) = GetRegion(locale.Language, locale.CountryCode, hint);

		Original = hint;
		Locale = locale;
		Value = regionInfo;
		Culture = cultureInfo ?? (regionInfo is null ? null : GetCultureInfo(regionInfo));
		DefaultFormat = defaultFormat;
	}

	private static (RegionInfo?, CultureInfo?) GetRegion(string language, string countrcode, string input)
	{
		CultureInfo? culture = null;
		if (language != string.Empty)
			try
			{
				culture = CultureInfo.GetCultureInfo(language.Length == 2 ? $"{language}-{countrcode}" : language);
			}
			catch
			{
				// ignored
			}

		try
		{
			return (new RegionInfo(countrcode), culture);
		}
		catch
		{
			// ignored
		}

		if (culture is not null)
			return (new RegionInfo(culture.Name), culture);

		try
		{
			return (new RegionInfo(input), culture);
		}
		catch
		{
			return CultureInfo.GetCultures(CultureTypes.SpecificCultures)
				.Select(c => (new RegionInfo(c.Name), c))
				.FirstOrDefault(rAndC =>
					string.Equals(rAndC.Item1.EnglishName, input, StringComparison.OrdinalIgnoreCase));
		}
	}

	private string? GetLocalizedRegionName()
	{
		return Culture is null ? null : GetCountryName(Culture.DisplayName) ?? GetCountryName(Culture.EnglishName);
	}

	private static string? GetCountryName(string localized)
	{
		return ExtractRegionName().Match(localized).Groups["displayName"].ValueOrNull();
	}

	[GeneratedRegex(@"\((?<displayName>.+)\)")]
	private static partial Regex ExtractRegionName();

	private static CultureInfo? GetCultureInfo(RegionInfo region)
	{
		// find culture for region
		return CultureInfo.GetCultures(CultureTypes.SpecificCultures)
			.FirstOrDefault(c => Equals(new RegionInfo(c.Name), region));
	}


	private static readonly Dictionary<string, Func<LocaleDto, object?>> FormatReplacements = new(StringComparer.OrdinalIgnoreCase)
	{
		{ "ID", dto => dto.Locale?.MarketPlaceId },
		{ "I", dto => dto.Value?.TwoLetterISORegionName },
		{ "I2", dto => dto.Value?.TwoLetterISORegionName },
		{ "I3", dto => dto.Value?.ThreeLetterISORegionName },
		{ "W", dto => dto.Value?.ThreeLetterWindowsRegionName },
		{ "E", dto => dto.Value?.EnglishName },
		{ "N", dto => dto.Value?.NativeName },
		{ "O", dto => dto.Original },
		{ "T", dto => dto.Locale.TopDomain },
		{ "L", dto => dto.Culture?.Name },
		{ "D", dto => dto.GetLocalizedRegionName() }, // localized
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