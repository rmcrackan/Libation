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
		=> _StringFormatter(value, formatString, culture);

	private static string _StringFormatter(string? value, string formatString, CultureInfo? culture)
	{
		if (string.IsNullOrEmpty(value)) return string.Empty;
		if (string.IsNullOrEmpty(formatString)) return value;

		var match = StringFormatRegex().Match(formatString);
		if (!match.Success) return value;

		// first shorten the string if a number is specified in the format string
		if (int.TryParse(match.Groups["left"].ValueSpan, out var length) && length < value.Length)
			value = value[..length];

		culture ??= CultureInfo.CurrentCulture;

		return match.Groups["case"].ValueSpan switch
		{
			"u" or "U" => value.ToUpper(culture),
			"l" or "L" => value.ToLower(culture),
			"T" => culture.TextInfo.ToTitleCase(value),
			"t" => culture.TextInfo.ToTitleCase(value.ToLower(culture)),
			_ => value,
		};
	}

	[GeneratedRegex(@"^\s*(?<left>\d+)?\s*(?<case>[uUlLtT])?\s*$")]
	private static partial Regex StringFormatRegex();

	public static string TemplateStringFormatter<T>(T toFormat, string? templateString, IFormatProvider? provider, Dictionary<string, Func<T, object?>> replacements)
	{
		if (string.IsNullOrEmpty(templateString)) return "";

		// is this function is called from toString implementation of the IFormattable interface, we only get a IFormatProvider
		var culture = provider as CultureInfo ?? (provider?.GetFormat(typeof(CultureInfo)) as CultureInfo);
		return TagFormatRegex().Replace(templateString, GetValueForMatchingTag);

		string GetValueForMatchingTag(Match m)
		{
			var tag = m.Groups["tag"].Value;
			if (!replacements.TryGetValue(tag, out var getter)) return m.Value;

			var value = getter(toFormat);
			var format = m.Groups["format"].Value;
			return value switch
			{
				IFormattable formattable => formattable.ToString(format, provider),
				_ => _StringFormatter(value?.ToString(), format, culture),
			};
		}
	}

	// The templateString is scanned for contained braces with an enclosed tagname.
	// The tagname may be followed by an optional format specifier separated by a colon.
	// All other parts of the template string are left untouched as well as the braces where the tagname is unknown.
	// TemplateStringFormatter will use a dictionary to lookup the tagname and the corresponding value getter.
	[GeneratedRegex(@"\{(?<tag>[[A-Z]+|#)(?::(?<format>.*?))?\}", RegexOptions.IgnoreCase)]
	private static partial Regex TagFormatRegex();

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
		return StringFormatter(templateTag, language?.Trim(), "3u", culture);
	}
}