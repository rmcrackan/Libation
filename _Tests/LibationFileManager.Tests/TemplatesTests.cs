using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dinah.Core;
using FluentAssertions;
using LibationFileManager;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TemplatesTests
{
	[TestClass]
	public class ContainsChapterOnlyTags
	{
		[TestMethod]
		[DataRow("<ch>", false)]
		[DataRow("<ch#>", true)]
		[DataRow("<id>", false)]
		[DataRow("<id><ch#>", true)]
		public void Tests(string template, bool expected) => Templates.ContainsChapterOnlyTags(template).Should().Be(expected);
	}

	[TestClass]
	public class ContainsTag
	{
		[TestMethod]
		[DataRow("<ch#>", "ch#", true)]
		[DataRow("<id>", "ch#", false)]
		[DataRow("<id><ch#>", "ch#", true)]
		public void Tests(string template, string tag, bool expected) => Templates.ContainsTag(template, tag).Should().Be(expected);
	}
}

namespace Templates_Folder_Tests
{
	[TestClass]
	public class GetErrors
	{
		[TestMethod]
		public void null_is_invalid() => Tests(null, new[] { Templates.ERROR_NULL_IS_INVALID });

		[TestMethod]
		public void empty_is_valid() => valid_tests("");

		[TestMethod]
		public void whitespace_is_valid() => valid_tests("   ");

		[TestMethod]
		[DataRow(@"foo")]
		[DataRow(@"\foo")]
		[DataRow(@"foo\")]
		[DataRow(@"\foo\")]
		[DataRow(@"foo\bar")]
		[DataRow(@"<id>")]
		[DataRow(@"<id>\<title>")]
		public void valid_tests(string template) => Tests(template, Array.Empty<string>());

		[TestMethod]
		[DataRow(@"C:\", Templates.ERROR_FULL_PATH_IS_INVALID)]
		public void Tests(string template, params string[] expected)
		{
			var result = Templates.Folder.GetErrors(template);
			result.Count().Should().Be(expected.Length);
			result.Should().BeEquivalentTo(expected);
		}
	}

	[TestClass]
	public class IsValid
	{
		[TestMethod]
		public void null_is_invalid() => Tests(null, false);

		[TestMethod]
		public void empty_is_valid() => Tests("", true);

		[TestMethod]
		public void whitespace_is_valid() => Tests("   ", true);

		[TestMethod]
		[DataRow(@"C:\", false)]
		[DataRow(@"foo", true)]
		[DataRow(@"\foo", true)]
		[DataRow(@"foo\", true)]
		[DataRow(@"\foo\", true)]
		[DataRow(@"foo\bar", true)]
		[DataRow(@"<id>", true)]
		[DataRow(@"<id>\<title>", true)]
		public void Tests(string template, bool expected) => Templates.Folder.IsValid(template).Should().Be(expected);
	}

	[TestClass]
	public class GetWarnings
	{
		[TestMethod]
		public void null_is_invalid() => Tests(null, new[] { Templates.ERROR_NULL_IS_INVALID });

		[TestMethod]
		public void empty_has_warnings() => Tests("", Templates.WARNING_EMPTY, Templates.WARNING_NO_TAGS);

		[TestMethod]
		public void whitespace_has_warnings() => Tests("   ", Templates.WARNING_WHITE_SPACE, Templates.WARNING_NO_TAGS);

		[TestMethod]
		[DataRow(@"<id>\foo\bar")]
		public void valid_tests(string template) => Tests(template, Array.Empty<string>());

		[TestMethod]
		[DataRow(@"no tags", Templates.WARNING_NO_TAGS)]
		[DataRow("<ch#> <id>", Templates.WARNING_HAS_CHAPTER_TAGS)]
		[DataRow("<ch#> chapter tag", Templates.WARNING_NO_TAGS, Templates.WARNING_HAS_CHAPTER_TAGS)]
		public void Tests(string template, params string[] expected)
		{
			var result = Templates.Folder.GetWarnings(template);
			result.Count().Should().Be(expected.Length);
			result.Should().BeEquivalentTo(expected);
		}
	}

	[TestClass]
	public class HasWarnings
	{
		[TestMethod]
		public void null_has_warnings() => Tests(null, true);

		[TestMethod]
		public void empty_has_warnings() => Tests("", true);

		[TestMethod]
		public void whitespace_has_warnings() => Tests("   ", true);

		[TestMethod]
		[DataRow(@"no tags", true)]
		[DataRow(@"<id>\foo\bar", false)]
		[DataRow("<ch#> <id>", true)]
		[DataRow("<ch#> chapter tag", true)]
		public void Tests(string template, bool expected) => Templates.Folder.HasWarnings(template).Should().Be(expected);
	}

	[TestClass]
	public class TagCount
	{
		[TestMethod]
		public void null_throws() => Assert.ThrowsException<NullReferenceException>(() => Templates.Folder.TagCount(null));

		[TestMethod]
		public void empty() => Tests("", 0);

		[TestMethod]
		public void whitespace() => Tests("   ", 0);

		[TestMethod]
		[DataRow("no tags", 0)]
		[DataRow(@"<id>\foo\bar", 1)]
		[DataRow("<id> <id>", 2)]
		[DataRow("<id <id> >", 1)]
		[DataRow("<id> <title>", 2)]
		[DataRow("id> <title incomplete tags", 0)]
		[DataRow("<not a real tag>", 0)]
		[DataRow("<ch#> non-folder tag", 0)]
		[DataRow("<ID> case specific", 0)]
		public void Tests(string template, int expected) => Templates.Folder.TagCount(template).Should().Be(expected);
	}
}

namespace Templates_File_Tests
{
	[TestClass]
	public class GetErrors
	{
		[TestMethod]
		public void null_is_invalid() => Tests(null, new[] { Templates.ERROR_NULL_IS_INVALID });

		[TestMethod]
		public void empty_is_valid() => valid_tests("");

		[TestMethod]
		public void whitespace_is_valid() => valid_tests("   ");

		[TestMethod]
		[DataRow(@"foo")]
		[DataRow(@"<id>")]
		public void valid_tests(string template) => Tests(template, Array.Empty<string>());


		[TestMethod]
		[DataRow(@"C:\", Templates.ERROR_INVALID_FILE_NAME_CHAR)]
		[DataRow(@"\foo", Templates.ERROR_INVALID_FILE_NAME_CHAR)]
		[DataRow(@"/foo", Templates.ERROR_INVALID_FILE_NAME_CHAR)]
		[DataRow(@"C:\", Templates.ERROR_INVALID_FILE_NAME_CHAR)]
		public void Tests(string template, params string[] expected)
		{
			var result = Templates.File.GetErrors(template);
			result.Count().Should().Be(expected.Length);
			result.Should().BeEquivalentTo(expected);
		}
	}

	[TestClass]
	public class IsValid
	{
		[TestMethod]
		public void null_is_invalid() => Tests(null, false);

		[TestMethod]
		public void empty_is_valid() => Tests("", true);

		[TestMethod]
		public void whitespace_is_valid() => Tests("   ", true);

		[TestMethod]
		[DataRow(@"C:\", false)]
		[DataRow(@"foo", true)]
		[DataRow(@"\foo", false)]
		[DataRow(@"/foo", false)]
		[DataRow(@"<id>", true)]
		public void Tests(string template, bool expected) => Templates.File.IsValid(template).Should().Be(expected);
	}

	// same as Templates.Folder.GetWarnings
	//[TestClass]
	//public class GetWarnings { }

	// same as Templates.Folder.HasWarnings 
	//[TestClass]
	//public class HasWarnings { }

	// same as Templates.Folder.TagCount 
	//[TestClass]
	//public class TagCount { }
}

namespace Templates_ChapterFile_Tests
{
	// same as Templates.File.GetErrors
	//[TestClass]
	//public class GetErrors { }

	// same as Templates.File.IsValid
	//[TestClass]
	//public class IsValid { }

	[TestClass]
	public class GetWarnings
	{
		[TestMethod]
		public void null_is_invalid() => Tests(null, new[] { Templates.ERROR_NULL_IS_INVALID });

		[TestMethod]
		public void empty_has_warnings() => Tests("", Templates.WARNING_EMPTY, Templates.WARNING_NO_TAGS, Templates.WARNING_NO_CHAPTER_NUMBER_TAG);

		[TestMethod]
		public void whitespace_has_warnings() => Tests("   ", Templates.WARNING_WHITE_SPACE, Templates.WARNING_NO_TAGS, Templates.WARNING_NO_CHAPTER_NUMBER_TAG);

		[TestMethod]
		[DataRow("<ch#>")]
		[DataRow("<ch#> <id>")]
		public void valid_tests(string template) => Tests(template, Array.Empty<string>());

		[TestMethod]
		[DataRow(@"no tags", Templates.WARNING_NO_TAGS, Templates.WARNING_NO_CHAPTER_NUMBER_TAG)]
		[DataRow(@"<id>\foo\bar", Templates.ERROR_INVALID_FILE_NAME_CHAR, Templates.WARNING_NO_CHAPTER_NUMBER_TAG)]
		[DataRow("<chapter count> -- chapter tag but not ch# or ch_#", Templates.WARNING_NO_TAGS, Templates.WARNING_NO_CHAPTER_NUMBER_TAG)]
		public void Tests(string template, params string[] expected)
		{
			var result = Templates.ChapterFile.GetWarnings(template);
			result.Count().Should().Be(expected.Length);
			result.Should().BeEquivalentTo(expected);
		}
	}

	[TestClass]
	public class HasWarnings
	{
		[TestMethod]
		public void null_has_warnings() => Tests(null, true);

		[TestMethod]
		public void empty_has_warnings() => Tests("", true);

		[TestMethod]
		public void whitespace_has_warnings() => Tests("   ", true);

		[TestMethod]
		[DataRow(@"no tags", true)]
		[DataRow(@"<id>\foo\bar", true)]
		[DataRow("<ch#> <id>", false)]
		[DataRow("<ch#> -- chapter tag", false)]
		[DataRow("<chapter count> -- chapter tag but not ch# or ch_#", true)]
		public void Tests(string template, bool expected) => Templates.ChapterFile.HasWarnings(template).Should().Be(expected);
	}

	[TestClass]
	public class TagCount
	{
		[TestMethod]
		public void null_is_not_recommended() => Assert.ThrowsException<NullReferenceException>(() => Tests(null, -1));

		[TestMethod]
		public void empty_is_not_recommended() => Tests("", 0);

		[TestMethod]
		public void whitespace_is_not_recommended() => Tests("   ", 0);

		[TestMethod]
		[DataRow("no tags", 0)]
		[DataRow(@"<id>\foo\bar", 1)]
		[DataRow("<id> <id>", 2)]
		[DataRow("<id <id> >", 1)]
		[DataRow("<id> <title>", 2)]
		[DataRow("id> <title incomplete tags", 0)]
		[DataRow("<not a real tag>", 0)]
		[DataRow("<ch#> non-folder tag", 1)]
		[DataRow("<ID> case specific", 0)]
		public void Tests(string template, int expected) => Templates.ChapterFile.TagCount(template).Should().Be(expected);
	}
}
