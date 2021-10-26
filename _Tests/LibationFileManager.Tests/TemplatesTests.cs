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
	public class IsRecommended
	{
		[TestMethod]
		public void null_is_not_recommended() => Tests(null, false);

		[TestMethod]
		public void empty_is_not_recommended() => Tests("", false);

		[TestMethod]
		public void whitespace_is_not_recommended() => Tests("   ", false);

		[TestMethod]
		[DataRow(@"no tags", false)]
		[DataRow(@"<id>\foo\bar", true)]
		[DataRow("<ch#> <id>", false)]
		[DataRow("<ch#> chapter tag", false)]
		public void Tests(string template, bool expected) => Templates.Folder.IsRecommended(template).Should().Be(expected);
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
		[DataRow("<ch#> non-folder tag", 0)]
		[DataRow("<ID> case specific", 0)]
		public void Tests(string template, int expected) => Templates.Folder.TagCount(template).Should().Be(expected);
	}
}

namespace Templates_File_Tests
{
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

	// same as Templates.Folder.IsRecommended 
	//[TestClass]
	//public class IsRecommended { }

	// same as Templates.Folder.TagCount 
	//[TestClass]
	//public class TagCount { }
}

namespace Templates_ChapterFile_Tests
{
	// same as Templates.File.IsValid
	//[TestClass]
	//public class IsValid { }

	[TestClass]
	public class IsRecommended
	{
		[TestMethod]
		public void null_is_not_recommended() => Tests(null, false);

		[TestMethod]
		public void empty_is_not_recommended() => Tests("", false);

		[TestMethod]
		public void whitespace_is_not_recommended() => Tests("   ", false);

		[TestMethod]
		[DataRow(@"no tags", false)]
		[DataRow(@"<id>\foo\bar", false)]
		[DataRow("<ch#> <id>", true)]
		[DataRow("<ch#> -- chapter tag", true)]
		[DataRow("<chapter count> -- chapter tag but not ch# or ch_#", false)]
		public void Tests(string template, bool expected) => Templates.ChapterFile.IsRecommended(template).Should().Be(expected);
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
