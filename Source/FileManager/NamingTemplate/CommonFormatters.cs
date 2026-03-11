using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace FileManager.NamingTemplate;

public static partial class CommonFormatters
{
	public const string DefaultDateFormat = "yyyy-MM-dd";

	public delegate string PropertyFormatter<in T>(ITemplateTag templateTag, T value, string formatString);

	public static string StringFormatter(ITemplateTag _, string? value, string formatString)
	{
		if (value is null) return "";
		var culture = CultureInfo.CurrentCulture;

		return formatString switch
		{
			"u" or "U" => value.ToUpper(culture),
			"l" or "L" => value.ToLower(culture),
			_ => value,
		};
	}

	public static string FormattableFormatter(ITemplateTag _, IFormattable? value, string formatString)
		=> value?.ToString(formatString, null) ?? "";

	public static string IntegerFormatter(ITemplateTag templateTag, int value, string formatString)
		=> FloatFormatter(templateTag, value, formatString);

	public static string FloatFormatter(ITemplateTag _, float value, string formatString)
	{
		var culture = CultureInfo.CurrentCulture;
		if (!int.TryParse(formatString, out var numDigits) || numDigits <= 0) return value.ToString(formatString, culture);
		//Zero-pad the integer part
		var strValue = value.ToString(culture);
		var decIndex = culture.CompareInfo.IndexOf(strValue, culture.NumberFormat.NumberDecimalSeparator);
		var zeroPad = decIndex == -1 ? int.Max(0, numDigits - strValue.Length) : int.Max(0, numDigits - decIndex);

		return new string('0', zeroPad) + strValue;
	}

	public static string DateTimeFormatter(ITemplateTag _, DateTime value, string formatString)
	{
		var culture = CultureInfo.CurrentCulture;
		if (string.IsNullOrEmpty(formatString))
			formatString = CommonFormatters.DefaultDateFormat;
		return value.ToString(formatString, culture);
	}

	public static string LanguageShortFormatter(string? language)
	{
		if (language is null)
			return "";

		language = language.Trim();
		if (language.Length <= 3)
			return language.ToUpper();
		return language[..3].ToUpper();
	}
}