using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace FileManager.NamingTemplate;

public static partial class CommonFormatters
{
	public const string DefaultDateFormat = "yyyy-MM-dd";

	public delegate string PropertyFormatter<in T>(ITemplateTag templateTag, T? value, string formatString, CultureInfo? culture);

	public static string StringFormatter(ITemplateTag _, string? value, string formatString, CultureInfo? culture)
	{
		if (value is null) return "";
		culture ??= CultureInfo.CurrentCulture;

		return formatString switch
		{
			"u" or "U" => value.ToUpper(culture),
			"l" or "L" => value.ToLower(culture),
			_ => value,
		};
	}

	public static string FormattableFormatter(ITemplateTag _, IFormattable? value, string formatString, CultureInfo? culture)
		=> value?.ToString(formatString, culture) ?? "";

	public static string IntegerFormatter(ITemplateTag templateTag, int value, string formatString, CultureInfo? culture)
		=> FloatFormatter(templateTag, value, formatString, culture);

	public static string FloatFormatter(ITemplateTag _, float value, string formatString, CultureInfo? culture)
	{
		culture ??= CultureInfo.CurrentCulture;
		if (!int.TryParse(formatString, out var numDigits) || numDigits <= 0) return value.ToString(formatString, culture);
		//Zero-pad the integer part
		var strValue = value.ToString(culture);
		var decIndex = culture.CompareInfo.IndexOf(strValue, culture.NumberFormat.NumberDecimalSeparator);
		var zeroPad = decIndex == -1 ? int.Max(0, numDigits - strValue.Length) : int.Max(0, numDigits - decIndex);

		return new string('0', zeroPad) + strValue;
	}

	public static string DateTimeFormatter(ITemplateTag _, DateTime value, string formatString, CultureInfo? culture)
	{
		culture ??= CultureInfo.InvariantCulture;
		if (string.IsNullOrEmpty(formatString))
			formatString = DefaultDateFormat;
		return value.ToString(formatString, culture);
	}

	public static string LanguageShortFormatter(ITemplateTag templateTag, string? language, string formatString, CultureInfo? culture)
	{
		if (language is null)
			return "";

		language = language.Trim();
		if (language.Length > 3) language = language[..3];

		return StringFormatter(templateTag, language, formatString, culture);
	}
}