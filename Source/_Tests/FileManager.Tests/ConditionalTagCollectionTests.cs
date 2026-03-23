using System;
using System.Globalization;
using System.Reflection;
using FileManager.NamingTemplate;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FileManager.Tests;

[TestClass]
public class ConditionalTagCollectionTests
{
	private class TestObject
	{
		public string? Value { get; init; }
	}

	private class TestTag : ITemplateTag
	{
		public string TagName => "testcond";
	}

	private readonly ConditionalTagCollection<TestObject> _conditionalTags = new()
	{
		{ new TestTag(), TryGetValue }
	};

	private static object? TryGetValue(ITemplateTag _, TestObject obj, string condition, CultureInfo? culture)
		=> obj.Value;

	/// <summary>
	/// Test that invalid regex patterns throw InvalidOperationException during evaluation.
	/// Tests include malformed patterns and catastrophic backtracking scenarios.
	/// </summary>
	[TestMethod]
	[DataRow("[abc", "test_value", DisplayName = "InvalidRegexPattern_UnmatchedBracket")]
	[DataRow("(?'name)abc", "test_value", DisplayName = "InvalidRegexPattern_InvalidGroup")]
	[DataRow("(a+)+b", "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaa", DisplayName = "CatastrophicBacktracking_NestedQuantifiers")]
	[DataRow("(a|aa|aaa|aaaa)*?b", "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaa", DisplayName = "CatastrophicBacktracking_AlternationOverlap")]
	[DataRow("(a+a+)+b", "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaa", DisplayName = "CatastrophicBacktracking_RepeatedConcatenation")]
	[DataRow("^(a+)+$", "aaaaaaaaaaaaaaaaaaaaaab", DisplayName = "CatastrophicBacktracking_AnchoredRepeated")]
	[DataRow("(a*)*b", "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaa", DisplayName = "CatastrophicBacktracking_StarStar")]
	public void ConditionalTag_InvalidRegexPattern_ThrowsInvalidOperationException(string pattern, string testValue)
	{
		// Arrange: Invalid regex patterns that should throw InvalidOperationException during evaluation
		var template = $"<testcond foobar[~{pattern}]->content<-testcond>";
		var namingTemplate = NamingTemplate.NamingTemplate.Parse(template, [_conditionalTags]);

		var testObj = new TestObject { Value = testValue };

		// Act & Assert: Evaluate template with invalid regex, should throw InvalidOperationException
		try
		{
			namingTemplate.Evaluate(testObj);
			Assert.Fail($"Expected InvalidOperationException for pattern: {pattern}");
		}
		catch (TargetInvocationException ex)
		{
			// if evaluation of the template started but the regex is running into a timeout an InvalidOperationException is thrown
			Assert.IsInstanceOfType<InvalidOperationException>(ex.InnerException);
		}
		catch (InvalidOperationException)
		{
			// Expected behavior - regex is invalid and parsing should fail
		}
	}

	/// <summary>
	/// Test that valid simple regex patterns parse successfully and don't throw during evaluation.
	/// </summary>
	[TestMethod]
	public void ConditionalTag_ValidRegexPattern_ParsesSuccessfully()
	{
		// Arrange: Valid simple regex pattern with proper closing tag
		var template = "<testcond foobar[~test.*]->content<-testcond>";

		// Act: Parse should succeed without throwing exceptions
		var namingTemplate = NamingTemplate.NamingTemplate.Parse(template, [_conditionalTags]);

		// Assert: Should parse successfully (may have warnings but no exceptions)
		Assert.IsNotNull(namingTemplate);
	}

	/// <summary>
	/// Test that regex patterns with special characters don't cause issues.
	/// </summary>
	[TestMethod]
	[DataRow("^test$", DisplayName = "RegexAnchors")]
	[DataRow("test.*value", DisplayName = "RegexWildcard")]
	[DataRow(@"[a-z\]+", DisplayName = "RegexCharacterClass")]
	[DataRow("test|value", DisplayName = "RegexAlternation")]
	public void ConditionalTag_ValidComplexRegexPatterns_ParseSuccessfully(string pattern)
	{
		// Arrange: Valid complex regex patterns with proper closing tags
		var template = $"<testcond foobar[~{pattern}]->c<-testcond>";

		// Act: Parse should succeed without throwing
		var namingTemplate = NamingTemplate.NamingTemplate.Parse(template, [_conditionalTags]);

		// Assert: Should parse successfully without exceptions
		Assert.IsNotNull(namingTemplate);
	}
}