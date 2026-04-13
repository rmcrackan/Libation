using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using FileManager.NamingTemplate;

namespace LibationFileManager.Templates;

public partial record RegionInfoDto : IFormattable
{
	private RegionInfo? Value { get; }
	private CultureInfo? Culture { get; }
	private string DefaultFormat { get; }
	public string Original { get; }

	public RegionInfoDto(string hint) : this(hint, "{O}")
	{
	}

	public RegionInfoDto(string hint, string defaultFormat) : this(GetRegion(hint), hint, defaultFormat)
	{
	}

	public RegionInfoDto(RegionInfo? value, string hint, string defaultFormat)
	{
		Original = hint;
		DefaultFormat = defaultFormat;
		Value = value;
		Culture = value is null ? null : GetCultureInfo(value);
	}

	private static RegionInfo? GetRegion(string input)
	{
		if (input.StartsWith("pre-amazon - ", StringComparison.OrdinalIgnoreCase)) input = input.Substring(13);
		if (string.Equals(input, "uk", StringComparison.OrdinalIgnoreCase)) return new RegionInfo("GB");
		try
		{
			return new RegionInfo(input.ToUpperInvariant());
		}
		catch
		{
			return CultureInfo.GetCultures(CultureTypes.SpecificCultures)
				.Select(c => new RegionInfo(c.Name))
				.FirstOrDefault(r =>
					string.Equals(r.EnglishName, input, StringComparison.OrdinalIgnoreCase));
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


	private static readonly Dictionary<string, Func<RegionInfoDto, object?>> FormatReplacements = new(StringComparer.OrdinalIgnoreCase)
	{
		{ "ID", dto => dto.Value?.Name },
		{ "I", dto => dto.Value?.TwoLetterISORegionName },
		{ "I2", dto => dto.Value?.TwoLetterISORegionName },
		{ "I3", dto => dto.Value?.ThreeLetterISORegionName },
		{ "W", dto => dto.Value?.ThreeLetterWindowsRegionName },
		{ "E", dto => dto.Value?.EnglishName },
		{ "N", dto => dto.Value?.NativeName },
		{ "O", dto => dto.Original },
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