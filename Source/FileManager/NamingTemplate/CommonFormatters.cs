using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading;

namespace FileManager.NamingTemplate;

public static partial class CommonFormatters
{
	public const string DefaultDateFormat = "yyyy-MM-dd";
	public const string DefaultTimeSpanFormat = "MMM";

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

	public static string _StringFormatter(string? value, string? formatString, IFormatProvider? provider)
		=> _StringFormatter(value, formatString, GetCultureInfo(provider));

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
		var culture = GetCultureInfo(provider);
		var oldUiCulture = Thread.CurrentThread.CurrentUICulture;
		var result = CollapseSpacesAndTrimRegex().Replace(TagFormatRegex().Replace(templateString, GetValueForMatchingTag), "");
		Thread.CurrentThread.CurrentUICulture = oldUiCulture;
		return result;

		string GetValueForMatchingTag(Match m)
		{
			var tag = m.Groups["tag"].Value;
			if (!replacements.TryGetValue(tag, out var getter)) return m.Value;

			var lang = m.Groups["lang"].ValueOrNull();
			var cultureToUse = lang is null ? culture : CultureInfo.GetCultureInfo(lang);
			Thread.CurrentThread.CurrentUICulture = cultureToUse ?? oldUiCulture;
			
			var value = getter(toFormat);
			var format = m.Groups["format"].ValueOrNull();
			return value switch
			{
				IFormattable formattable => formattable.ToString(format, cultureToUse),
				_ => _StringFormatter(value?.ToString(), format, cultureToUse),
			};
		}
	}

	private static CultureInfo? GetCultureInfo(IFormatProvider? provider)
	{
		return provider as CultureInfo ?? provider?.GetFormat(typeof(CultureInfo)) as CultureInfo;
	}

	// Matches runs of spaces followed by a space as well as runs of spaces at the beginning or the end of a string (does NOT touch tabs/newlines).
	[GeneratedRegex(@"^ +| +(?=$| )")]
	private static partial Regex CollapseSpacesAndTrimRegex();

	// The templateString is scanned for contained braces with an enclosed tagname.
	// The tagname may be followed by an optional format specifier separated by a colon.
	// All other parts of the template string are left untouched as well as the braces where the tagname is unknown.
	// TemplateStringFormatter will use a dictionary to lookup the tagname and the corresponding value getter.
	[GeneratedRegex("""\{(?<tag>[A-Z0-9]+|#)(?:@(?<lang>[a-z-]+))?(?::(?<format>(?:\\.|'(?:[^']|'')*'|"(?:[^"]|"")*"|.)*?))?\}""", RegexOptions.IgnoreCase)]
	public static partial Regex TagFormatRegex();

	public static string FormattableFormatter(ITemplateTag _, IFormattable? value, string? formatString, CultureInfo? culture)
		=> value?.ToString(formatString, culture) ?? "";

	public static string IntegerFormatter(ITemplateTag templateTag, int value, string? formatString, CultureInfo? culture)
		=> FloatFormatter(templateTag, value, formatString, culture);

	public static string FloatFormatter(ITemplateTag _, float value, string? formatString, CultureInfo? culture)
	{
		culture ??= CultureInfo.CurrentCulture;
		if (!int.TryParse(formatString, out var numDigits) || numDigits <= 0) return value.ToString(formatString, culture);

		//Zero-pad the integer part
		formatString = new string('0', numDigits) + ".################";
		return value.ToString(formatString, culture);
	}

	public static string MinutesFormatter(ITemplateTag templateTag, TimeSpan value, string? formatString, CultureInfo? culture)
	{
		culture ??= CultureInfo.CurrentCulture;
		formatString ??= DefaultTimeSpanFormat;

		// the format string is build as a custom format for TimeSpans. Time portion like 'h' and 'm' are used to format the minutes and hours part of the TimeSpan.
		// They are limited by the next greater domain. So 'h' will be between 0 and 23, 'm' between 0 and 59. 'd' will be the total number of days.
		// To get the total timespan display in terms of total hours or total minutes, we allow the format string to include number formats with uppercase D, H or M.
		// As there might be up to three numbers shown in the format string, we distinguish between total days, hours and minutes with uppercase D, H and M instead of zeros.
		// A format "#,##0'minutes'" would for example become "#,##M'minutes'". If you combine them the lower units will be reduced by the higher units.
		// "D'days and'#,##0'minutes'" will show 1,439 minutes at maximum.
		// In the first step we search for number formats with uppercase D, H or M, format their values as number and replace them as quoted strings in the format string.
		var timeSpanForTotal = value;
		formatString = FormatAsNumberIntoTemplate(templateTag, formatString, culture, ref timeSpanForTotal, RegexMinutesTotalD(), TimeSpan.TicksPerDay);
		formatString = FormatAsNumberIntoTemplate(templateTag, formatString, culture, ref timeSpanForTotal, RegexMinutesTotalH(), TimeSpan.TicksPerHour);
		formatString = FormatAsNumberIntoTemplate(templateTag, formatString, culture, ref timeSpanForTotal, RegexMinutesTotalM(), TimeSpan.TicksPerMinute);

		// The formatString should now be a valid TimeSpan format string.
		return value.ToString(formatString, culture);
	}

	private static string FormatAsNumberIntoTemplate(ITemplateTag templateTag, string formatString, CultureInfo culture, ref TimeSpan timeSpanForTotal, Regex regex, long ticks)
	{
		var total = timeSpanForTotal.Ticks / ticks;
		var matched = false;
		var result = regex.Replace(formatString, m =>
		{
			matched = true;
			var numPattern = RegexTimeStampToNumberPattern().Replace(m.Groups["format"].Value, "0");
			var formatted = FloatFormatter(templateTag, total, numPattern, culture);
			return $"'{formatted}'";
		});
		if (matched) timeSpanForTotal = TimeSpan.FromTicks(timeSpanForTotal.Ticks % ticks);
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

	// These search for number formats with all notions of escaping and quoting, but all zeros replaced with D, H, or M to indicate that they should be replaced with the total number of
	// days hours or minutes in the timespan (not just the minutes part). Only one of them is written commented. The others are identical except for the letter D, H or M.
	// I most cases this regex will only find a straight bunch of D's, H's or M's, but it also allows for more complex formats.
	[GeneratedRegex("""
	                (?x)                          # option x: ignore all unescaped whitespace in pattern and allow comments starting with #
	                (?<=\G(?:                     # We lookbehind up to the start or the end of the last match for a number format.
	                    \\.                       # - '\' escapes always the next character. Especially further '\' and the closing ']'
	                    | '(?:[^']|'')*'          # - allow 'string' to be included in the format, with '' being an escaped ' character
	                    | "(?:[^"]|"")*"          # - allow "string" to be included in the format, with "" being an escaped " character
	                    | .                       # - match any character. This will not catch the number format at first. Because ...
	                ) *? )                        # With *? the pattern above tries not to consume the number format.
	                (?<format>                    # We capture the whole number format in a group called '<format>'.
	                    (?:\#[\#,.]*)?            # - For grouping a number format may start with `#` and grouping hints `,` or even a decimal point `.`.
	                    D                         # - At least one unescaped, unquoted uppercase D must be included in the format to indicate that this is a total days format.
	                    (?:(?:                    # - Before further D's, there may be any combination of escaped characters and quoted strings.
	                            \\.               # - '\' escapes always the next character. Especially further '\' and the closing ']' 
	                            | '(?:[^']|'')*'  # - allow 'string' to be included in the format, with '' being an escaped ' character 
	                            | "(?:[^"]|"")*"  # - allow "string" to be included in the format, with "" being an escaped " character 
	                        )* [\#,.%‰D]+         # After escaped characters and quoted strings, there needs to be at least one more real number format character (which may be D as well).
	                    )*                        # This may extend the format several times, for example in `D\:DD` or `D' days 'D\-D`.
	                    (?:[Ee][+-]?0+)?          # The original number format may end with an optional scientific notation part. This is also optional.
	                )                             # end of capture group '<format>'
	                """)]
	private static partial Regex RegexMinutesTotalD();

	[GeneratedRegex("""(?<=\G(?:\\.|'(?:[^']|'')*'|"(?:[^"]|"")*"|.)*?)(?<format>(?:#[#,.]*)?H(?:(?:\\.|'(?:[^']|'')*'|"(?:[^"]|"")*")*[H%‰#,.]+)*(?:[Ee][+-]?0+)?)""")]
	private static partial Regex RegexMinutesTotalH();

	[GeneratedRegex("""(?<=\G(?:\\.|'(?:[^']|'')*'|"(?:[^"]|"")*"|.)*?)(?<format>(?:#[#,.]*)?M(?:(?:\\.|'(?:[^']|'')*'|"(?:[^"]|"")*")*[M%‰#,.]+)*(?:[Ee][+-]?0+)?)""")]
	private static partial Regex RegexMinutesTotalM();

	// Capture all D H or M characters in the number format, so that they can be replaced with zeros.
	[GeneratedRegex("""(?<=\G(?:\\.|'(?:[^']|'')*'|"(?:[^"]|"")*"|.)*?)[DHM]""")]
	private static partial Regex RegexTimeStampToNumberPattern();
}