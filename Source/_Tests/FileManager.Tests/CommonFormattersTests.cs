using System;
using System.Collections.Generic;
using System.Globalization;
using AssertionHelper;
using FileManager.NamingTemplate;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FileManager.Tests;

[TestClass]
public class CommonFormattersTests
{
	[TestMethod]
	public void TemplateStringFormatter_UnknownTag_RemainsUnchanged()
	{
		// GIVEN
		var template = "Author: {AUTHOR}, Unknown: {UNKNOWN}, Title: {TITLE}";
		var replacements = new Dictionary<string, Func<TestClass, object?>>
		{
			["AUTHOR"] = obj => obj.Author,
			["TITLE"] = obj => obj.Title
		};
		var testObj = new TestClass { Author = "John Doe", Title = "Test Book" };

		// WHEN
		var result = CommonFormatters.TemplateStringFormatter(testObj, template, CultureInfo.InvariantCulture, replacements);

		// THEN
		Assert.AreEqual("Author: John Doe, Unknown: {UNKNOWN}, Title: Test Book", result);
	}

	[TestMethod]
	public void MinutesFormatter_Boundaries_ZeroMinutes()
	{
		// GIVEN
		var templateTag = new TemplateTag { TagName = "MINUTES" };
		var value = TimeSpan.FromMinutes(0);
		var format = @"h\:m";

		// WHEN
		var result = CommonFormatters.MinutesFormatter(templateTag, value, format, CultureInfo.InvariantCulture);

		// THEN
		Assert.AreEqual("0:0", result);
	}

	[TestMethod]
	public void MinutesFormatter_Boundaries_OneDay()
	{
		// GIVEN
		var templateTag = new TemplateTag { TagName = "MINUTES" };
		var value = TimeSpan.FromHours(24);
		var format = @"d'd 'h'h 'm\m";

		// WHEN
		var result = CommonFormatters.MinutesFormatter(templateTag, value, format, CultureInfo.InvariantCulture);

		// THEN
		Assert.AreEqual("1d 0h 0m", result);
	}

	[TestMethod]
	public void MinutesFormatter_Boundaries_LargeValue()
	{
		// GIVEN
		var templateTag = new TemplateTag { TagName = "MINUTES" };
		var value = TimeSpan.FromHours(50); // 50 hours
		var format = @"d'd 'h'h 'm\m";

		// WHEN
		var result = CommonFormatters.MinutesFormatter(templateTag, value, format, CultureInfo.InvariantCulture);

		// THEN
		Assert.AreEqual("2d 2h 0m", result); // 2 days, 2 hours
	}

	[TestMethod]
	public void StringFormatter_InvalidCombinedFormat_ReturnsOriginal()
	{
		// GIVEN
		var templateTag = new TemplateTag { TagName = "STRING" };
		var value = "TestString";
		var invalidFormat = "invalid format with spaces and numbers 123";

		// WHEN
		var result = CommonFormatters.StringFormatter(templateTag, value, invalidFormat, CultureInfo.InvariantCulture);

		// THEN
		Assert.AreEqual("TestString", result);
	}

	[TestMethod]
	public void TemplateStringFormatter_InvalidCombinedFormat_HandlesGracefully()
	{
		// GIVEN
		var template = "{AUTHOR:invalid:format}, {TITLE}";
		var replacements = new Dictionary<string, Func<TestClass, object?>>
		{
			["AUTHOR"] = obj => obj.Author,
			["TITLE"] = obj => obj.Title
		};
		var testObj = new TestClass { Author = "John Doe", Title = "Test Book" };

		// WHEN
		var result = CommonFormatters.TemplateStringFormatter(testObj, template, CultureInfo.InvariantCulture, replacements);

		// THEN
		// Since AUTHOR is IFormattable? No, it's string, so uses _StringFormatter with invalid format
		Assert.AreEqual("John Doe, Test Book", result);
	}

	[TestMethod]
	public void StringFormatter_Uppercase()
	{
		// GIVEN
		var templateTag = new TemplateTag { TagName = "STRING" };
		var value = "test string";
		var format = "U";

		// WHEN
		var result = CommonFormatters.StringFormatter(templateTag, value, format, CultureInfo.InvariantCulture);

		// THEN
		Assert.AreEqual("TEST STRING", result);
	}

	[TestMethod]
	public void StringFormatter_Lowercase()
	{
		// GIVEN
		var templateTag = new TemplateTag { TagName = "STRING" };
		var value = "TEST STRING";
		var format = "L";

		// WHEN
		var result = CommonFormatters.StringFormatter(templateTag, value, format, CultureInfo.InvariantCulture);

		// THEN
		Assert.AreEqual("test string", result);
	}

	[TestMethod]
	public void StringFormatter_TitleCase()
	{
		// GIVEN
		var templateTag = new TemplateTag { TagName = "STRING" };
		var value = "test string";
		var format = "T";

		// WHEN
		var result = CommonFormatters.StringFormatter(templateTag, value, format, CultureInfo.InvariantCulture);

		// THEN
		Assert.AreEqual("Test String", result);
	}

	[TestMethod]
	public void StringFormatter_TitleCaseWithLength()
	{
		// GIVEN
		var templateTag = new TemplateTag { TagName = "STRING" };
		var value = "test string longer";
		var format = "10T";

		// WHEN
		var result = CommonFormatters.StringFormatter(templateTag, value, format, CultureInfo.InvariantCulture);

		// THEN
		Assert.AreEqual("Test Strin", result); // Title case first 10 chars
	}

	[TestMethod]
	public void StringFormatter_MaxLength()
	{
		// GIVEN
		var templateTag = new TemplateTag { TagName = "STRING" };
		var value = "this is a very long string";
		var format = "20";

		// WHEN
		var result = CommonFormatters.StringFormatter(templateTag, value, format, CultureInfo.InvariantCulture);

		// THEN
		Assert.AreEqual("this is a very long ", result); // Truncated to 20 chars
	}

	[TestMethod]
	public void FormattableFormatter_Standard()
	{
		// GIVEN
		var templateTag = new TemplateTag { TagName = "FORMATTABLE" };
		var value = 123.45;
		var format = "F2";

		// WHEN
		var result = CommonFormatters.FormattableFormatter(templateTag, value, format, CultureInfo.InvariantCulture);

		// THEN
		Assert.AreEqual("123.45", result);
	}

	[TestMethod]
	public void IntegerFormatter_WithLengthAndPadding()
	{
		// GIVEN
		var templateTag = new TemplateTag { TagName = "INTEGER" };
		var value = 42;
		var format = "5"; // Zero-padded to 5 digits

		// WHEN
		var result = CommonFormatters.IntegerFormatter(templateTag, value, format, CultureInfo.InvariantCulture);

		// THEN
		Assert.AreEqual("00042", result);
	}

	[TestMethod]
	public void IntegerFormatter_StandardFormat()
	{
		// GIVEN
		var templateTag = new TemplateTag { TagName = "INTEGER" };
		var value = 1234;
		var format = "N0"; // Number format

		// WHEN
		var result = CommonFormatters.IntegerFormatter(templateTag, value, format, CultureInfo.InvariantCulture);

		// THEN
		Assert.AreEqual("1,234", result);
	}

	[TestMethod]
	public void FloatFormatter_WithLengthAndPadding()
	{
		// GIVEN
		var templateTag = new TemplateTag { TagName = "FLOAT" };
		var value = 12.34f;
		var format = "F3"; // Fixed-point with 3 decimals

		// WHEN
		var result = CommonFormatters.FloatFormatter(templateTag, value, format, CultureInfo.InvariantCulture);

		// THEN
		Assert.AreEqual("12.340", result);
	}

	[TestMethod]
	public void FloatFormatter_StandardFormat()
	{
		// GIVEN
		var templateTag = new TemplateTag { TagName = "FLOAT" };
		var value = 1234.567f;
		var format = "N2"; // Number format with 2 decimals

		// WHEN
		var result = CommonFormatters.FloatFormatter(templateTag, value, format, CultureInfo.InvariantCulture);

		// THEN
		Assert.AreEqual("1,234.57", result);
	}

	[TestMethod]
	public void DateTimeFormatter_Standard()
	{
		// GIVEN
		var templateTag = new TemplateTag { TagName = "DATETIME" };
		var value = new DateTime(2023, 10, 15, 14, 30, 0);
		var format = "yyyy-MM-dd";

		// WHEN
		var result = CommonFormatters.DateTimeFormatter(templateTag, value, format, CultureInfo.InvariantCulture);

		// THEN
		Assert.AreEqual("2023-10-15", result);
	}

	[TestMethod]
	public void LanguageShortFormatter_TrimToThreeAndUppercase()
	{
		// GIVEN
		var templateTag = new TemplateTag { TagName = "LANGUAGE" };
		var value = "english";
		var format = ""; // Assuming default or empty

		// WHEN
		var result = CommonFormatters.LanguageShortFormatter(templateTag, value, format, CultureInfo.InvariantCulture);

		// THEN
		Assert.AreEqual("ENG", result); // First 3 chars uppercase
	}

	[TestMethod]
	public void LanguageShortFormatter_ShortLanguage()
	{
		// GIVEN
		var templateTag = new TemplateTag { TagName = "LANGUAGE" };
		var value = "de";
		var format = "";

		// WHEN
		var result = CommonFormatters.LanguageShortFormatter(templateTag, value, format, CultureInfo.InvariantCulture);

		// THEN
		Assert.AreEqual("DE", result); // Uppercase, no trim needed
	}

	private class TestClass
	{
		public string? Author { get; set; }
		public string? Title { get; set; }
	}
}