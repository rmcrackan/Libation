using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace FileManager.NamingTemplate;

public static partial class CommonFormatters
{
	public const string DefaultDateFormat = "yyyy-MM-dd";

	public delegate TFormatted? PropertyFormatter<in TProperty, out TFormatted>(ITemplateTag templateTag, TProperty? value, string? formatString, CultureInfo? culture);

	public delegate string? PropertyFinalizer<in T>(ITemplateTag templateTag, T? value, CultureInfo? culture);

	public static PropertyFinalizer<TProperty> ToPropertyFormatter<TProperty, TPreFormatted>(PropertyFormatter<TProperty, TPreFormatted> preFormatter,
		PropertyFinalizer<TPreFormatted> finalizer)
	{
		return (templateTag, value, culture) => finalizer(templateTag, preFormatter(templateTag, value, null, culture), culture);
	}

	public static PropertyFinalizer<TPropertyValue> ToFinalizer<TPropertyValue>(PropertyFormatter<TPropertyValue, string> formatter)
	{
		return (templateTag, value, culture) => formatter(templateTag, value, null, culture);
	}

	public static string? StringFinalizer(ITemplateTag templateTag, string? value, CultureInfo? culture) => value;

	public static TPropertyValue? IdlePreFormatter<TPropertyValue>(ITemplateTag templateTag, TPropertyValue? value, string? formatString, CultureInfo? culture) => value;

	public static string StringFormatter(ITemplateTag _, string? value, string? formatString, CultureInfo? culture)
		=> _StringFormatter(value, formatString, culture);

	private static string _StringFormatter(string? value, string? formatString, CultureInfo? culture)
	{
		if (string.IsNullOrEmpty(value)) return string.Empty;
		if (string.IsNullOrWhiteSpace(formatString) || !StringFormatRegex().TryMatch(formatString, out var match)) return value;

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
		if (string.IsNullOrWhiteSpace(templateString)) return "";

		// is this function is called from toString implementation of the IFormattable interface, we only get a IFormatProvider
		var culture = provider as CultureInfo ?? provider?.GetFormat(typeof(CultureInfo)) as CultureInfo;
		return CollapseSpacesAndTrimRegex().Replace(TagFormatRegex().Replace(templateString, GetValueForMatchingTag), "");

		string GetValueForMatchingTag(Match m)
		{
			var tag = m.Groups["tag"].Value;
			if (!replacements.TryGetValue(tag, out var getter)) return m.Value;

			var value = getter(toFormat);
			var format = m.Groups["format"].ValueOrNull();
			return value switch
			{
				IFormattable formattable => formattable.ToString(format, provider),
				_ => _StringFormatter(value?.ToString(), format, culture),
			};
		}
	}

	// Matches runs of spaces followed by a space as well as runs of spaces at the beginning or the end of a string (does NOT touch tabs/newlines).
	[GeneratedRegex(@"^ +| +(?=$| )")]
	private static partial Regex CollapseSpacesAndTrimRegex();

	// The templateString is scanned for contained braces with an enclosed tagname.
	// The tagname may be followed by an optional format specifier separated by a colon.
	// All other parts of the template string are left untouched as well as the braces where the tagname is unknown.
	// TemplateStringFormatter will use a dictionary to lookup the tagname and the corresponding value getter.
	[GeneratedRegex("""\{(?<tag>[A-Z]+|#)(?::(?<format>(?:\\.|'(?:[^']|'')*'|"(?:[^"]|"")*"|.)*?))?\}""", RegexOptions.IgnoreCase)]
	private static partial Regex TagFormatRegex();

	public static string FormattableFormatter(ITemplateTag _, IFormattable? value, string? formatString, CultureInfo? culture)
		=> value?.ToString(formatString, culture) ?? "";

	public static string IntegerFormatter(ITemplateTag templateTag, int value, string? formatString, CultureInfo? culture)
		=> FloatFormatter(templateTag, value, formatString, culture);

	public static string FloatFormatter(ITemplateTag _, float value, string? formatString, CultureInfo? culture)
	{
		culture ??= CultureInfo.CurrentCulture;
		if (!int.TryParse(formatString, out var numDigits) || numDigits <= 0) return value.ToString(formatString, culture);
		//Zero-pad the integer part
		var strValue = value.ToString(culture);
		var decIndex = culture.CompareInfo.IndexOf(strValue, culture.NumberFormat.NumberDecimalSeparator);
		var zeroPad = decIndex == -1 ? int.Max(0, numDigits - strValue.Length) : int.Max(0, numDigits - decIndex);

		return new string('0', zeroPad) + strValue;
	}

	public static string MinutesFormatter(ITemplateTag templateTag, int value, string? formatString, CultureInfo? culture)
	{
		culture ??= CultureInfo.CurrentCulture;
		if (string.IsNullOrWhiteSpace(formatString))
			return value.ToString(culture);

		var timeSpan = TimeSpan.FromMinutes(value);
		var result = formatString;

		// replace all placeholders with formatted values
		result = RegexMinutesD().Replace(result, m =>
		{
			var val = (int)timeSpan.TotalDays;
			timeSpan = timeSpan.Subtract(TimeSpan.FromDays(val));

			return FloatFormatter(templateTag, val, m.Groups["format"].Value, culture);
		});
		result = RegexMinutesH().Replace(result, m =>
		{
			var val = (int)timeSpan.TotalHours;
			timeSpan = timeSpan.Subtract(TimeSpan.FromHours(val));

			return FloatFormatter(templateTag, val, m.Groups["format"].Value, culture);
		});
		result = RegexMinutesM().Replace(result, m =>
		{
			var val = (int)timeSpan.TotalMinutes;
			timeSpan = timeSpan.Subtract(TimeSpan.FromMinutes(val));

			return FloatFormatter(templateTag, val, m.Groups["format"].Value, culture);
		});

		return result;
	}

	public static string DateTimeFormatter(ITemplateTag _, DateTime value, string? formatString, CultureInfo? culture)
	{
		culture ??= CultureInfo.InvariantCulture;
		if (string.IsNullOrWhiteSpace(formatString))
			formatString = DefaultDateFormat;
		return value.ToString(formatString, culture);
	}

	public static string LanguageShortFormatter(ITemplateTag templateTag, string? language, string? formatString, CultureInfo? culture)
	{
		return StringFormatter(templateTag, language, "3u", culture);
	}

	// Regex to find patterns like {D:3}, {h:4}, {m}
	[GeneratedRegex("""\{D(?::(?<format>(?:\\.|'(?:[^']|'')*'|"(?:[^"]|"")*"|.)*?))?\}""", RegexOptions.IgnoreCase)]
	private static partial Regex RegexMinutesD();

	[GeneratedRegex("""\{H(?::(?<format>(?:\\.|'(?:[^']|'')*'|"(?:[^"]|"")*"|.)*?))?\}""", RegexOptions.IgnoreCase)]
	private static partial Regex RegexMinutesH();

	[GeneratedRegex("""\{M(?::(?<format>(?:\\.|'(?:[^']|'')*'|"(?:[^"]|"")*"|.)*?))?\}""", RegexOptions.IgnoreCase)]
	private static partial Regex RegexMinutesM();
}