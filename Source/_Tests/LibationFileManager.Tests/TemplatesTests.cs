using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dinah.Core;
using FileManager;
using FluentAssertions;
using LibationFileManager;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using static TemplatesTests.Shared;

namespace TemplatesTests
{
	/////////////////////////////////////////////////
	//                                             //
	//    add general tag replacement tests to:    //
	//                                             //
	//    getFileNamingTemplate.Tests              //
	//                                             //
	/////////////////////////////////////////////////

	public static class Shared
	{
		public static LibraryBookDto GetLibraryBook(string seriesName = "Sherlock Holmes")
			=> new()
			{
				Account = "my account",
				DateAdded = new DateTime(2022, 6, 9, 0, 0, 0),
				DatePublished = new DateTime(2017, 2, 27, 0, 0, 0),
				FileDate = new DateTime(2023, 1, 28, 0, 0, 0),
				AudibleProductId = "asin",
				Title = "A Study in Scarlet: A Sherlock Holmes Novel",
				Locale = "us",
				YearPublished = 2017,
				Authors = new List<string> { "Arthur Conan Doyle", "Stephen Fry - introductions" },
				Narrators = new List<string> { "Stephen Fry" },
				SeriesName = seriesName ?? "",
				SeriesNumber = "1",
				BitRate = 128,
				SampleRate = 44100,
				Channels = 2
			};

		public static LibraryBookDto GetLibraryBookWithNullDates(string seriesName = "Sherlock Holmes")
			=> new()
			{
				Account = "my account",
				FileDate = new DateTime(2023, 1, 28, 0, 0, 0),
				AudibleProductId = "asin",
				Title = "A Study in Scarlet: A Sherlock Holmes Novel",
				Locale = "us",
				YearPublished = 2017,
				Authors = new List<string> { "Arthur Conan Doyle", "Stephen Fry - introductions" },
				Narrators = new List<string> { "Stephen Fry" },
				SeriesName = seriesName ?? "",
				SeriesNumber = "1",
				BitRate = 128,
				SampleRate = 44100,
				Channels = 2
			};
	}

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

	[TestClass]
	public class getFileNamingTemplate
	{
		static ReplacementCharacters Replacements = ReplacementCharacters.Default;


		[TestMethod]
		[DataRow(null, @"C:\", "ext")]
		[ExpectedException(typeof(ArgumentNullException))]
		public void arg_null_exception(string template, string dirFullPath, string extension)
			=> Templates.getFileNamingTemplate(GetLibraryBook(), template, dirFullPath, extension, Replacements);

		[TestMethod]
		[DataRow("", @"C:\foo\bar", "ext")]
		[DataRow("   ", @"C:\foo\bar", "ext")]
		[ExpectedException(typeof(ArgumentException))]
		public void arg_exception(string template, string dirFullPath, string extension)
			=> Templates.getFileNamingTemplate(GetLibraryBook(), template, dirFullPath, extension, Replacements);

		[TestMethod]
		[DataRow("f.txt", @"C:\foo\bar", "", @"C:\foo\bar\f.txt", PlatformID.Win32NT)]
		[DataRow("f.txt", @"/foo/bar", "", @"/foo/bar/f.txt", PlatformID.Unix)]
		[DataRow("f.txt", @"C:\foo\bar", ".ext", @"C:\foo\bar\f.txt.ext", PlatformID.Win32NT)]
		[DataRow("f.txt", @"/foo/bar", ".ext", @"/foo/bar/f.txt.ext", PlatformID.Unix)]
		[DataRow("f", @"C:\foo\bar", ".ext", @"C:\foo\bar\f.ext", PlatformID.Win32NT)]
		[DataRow("f", @"/foo/bar", ".ext", @"/foo/bar/f.ext", PlatformID.Unix)]
		[DataRow("<id>", @"C:\foo\bar", ".ext", @"C:\foo\bar\asin.ext", PlatformID.Win32NT)]
		[DataRow("<id>", @"/foo/bar", ".ext", @"/foo/bar/asin.ext", PlatformID.Unix)]
        [DataRow("<bitrate> - <samplerate> - <channels>", @"C:\foo\bar", ".ext", @"C:\foo\bar\128 - 44100 - 2.ext", PlatformID.Win32NT)]
        [DataRow("<bitrate> - <samplerate> - <channels>", @"/foo/bar", ".ext", @"/foo/bar/128 - 44100 - 2.ext", PlatformID.Unix)]
        [DataRow("<year> - <channels>", @"C:\foo\bar", ".ext", @"C:\foo\bar\2017 - 2.ext", PlatformID.Win32NT)]
        [DataRow("<year> - <channels>", @"/foo/bar", ".ext", @"/foo/bar/2017 - 2.ext", PlatformID.Unix)]
		[DataRow("(000.0) <year> - <channels>", @"C:\foo\bar", "ext", @"C:\foo\bar\(000.0) 2017 - 2.ext", PlatformID.Win32NT)]
		[DataRow("(000.0) <year> - <channels>", @"/foo/bar", ".ext", @"/foo/bar/(000.0) 2017 - 2.ext", PlatformID.Unix)]
		public void Tests(string template, string dirFullPath, string extension, string expected, PlatformID platformID)
		{
			if (Environment.OSVersion.Platform == platformID)
				Templates.getFileNamingTemplate(GetLibraryBook(), template, dirFullPath, extension, Replacements)
				.GetFilePath(extension)
				.PathWithoutPrefix
				.Should().Be(expected);
		}

		[TestMethod]
		[DataRow("<id> - <filedate[yy-MM-dd]>", @"C:\foo\bar", "m4b", @"C:\foo\bar\asin - 23-01-28.m4b")]
		[DataRow("<id> - <filedate  [  yy-MM-dd    ]  >", @"C:\foo\bar", "m4b", @"C:\foo\bar\asin - 23-01-28.m4b")]
		[DataRow("<id> - <file date  [yy-MM-dd]  >", @"C:\foo\bar", "m4b", @"C:\foo\bar\asin - 23-01-28.m4b")]
		[DataRow("<id> - <file     date[yy-MM-dd]>", @"C:\foo\bar", "m4b", @"C:\foo\bar\asin - 23-01-28.m4b")]
		[DataRow("<id> - <file date[]>", @"C:\foo\bar", "m4b", @"C:\foo\bar\asin - 2023-01-28.m4b")]
		[DataRow("<id> - <filedate[]>", @"C:\foo\bar", "m4b", @"C:\foo\bar\asin - 2023-01-28.m4b")]
		[DataRow("<id> - <filedate [    ]  >", @"C:\foo\bar", "m4b", @"C:\foo\bar\asin - 2023-01-28.m4b")]
		[DataRow("<id> - <filedate>", @"C:\foo\bar", "m4b", @"C:\foo\bar\asin - 2023-01-28.m4b")]
		[DataRow("<id> - <filedate  >", @"C:\foo\bar", "m4b", @"C:\foo\bar\asin - 2023-01-28.m4b")]
		[DataRow("<id> - <file date>", @"C:\foo\bar", "m4b", @"C:\foo\bar\asin - 2023-01-28.m4b")]
		[DataRow("<id> - <file     date>", @"C:\foo\bar", "m4b", @"C:\foo\bar\asin - 2023-01-28.m4b")]
		public void DateFormat_pattern(string template, string dirFullPath, string extension, string expected)
		{
			if (Environment.OSVersion.Platform is not PlatformID.Win32NT)
			{
				dirFullPath = dirFullPath.Replace("C:", "").Replace('\\', '/');
				expected = expected.Replace("C:", "").Replace('\\', '/');
			}

			Templates.getFileNamingTemplate(GetLibraryBook(), template, dirFullPath, extension, Replacements)
				.GetFilePath(extension)
				.PathWithoutPrefix
				.Should().Be(expected);
		}

		[TestMethod]
		[DataRow("<filedate[h]>", @"C:\foo\bar", ".m4b", @"C:\foo\bar\＜filedate[h]＞.m4b")]
		[DataRow("< filedate[yyyy]>", @"C:\foo\bar", ".m4b", @"C:\foo\bar\＜ filedate[yyyy]＞.m4b")]
		[DataRow("<filedate yyyy]>", @"C:\foo\bar", ".m4b", @"C:\foo\bar\＜filedate yyyy]＞.m4b")]
		[DataRow("<filedate [yyyy>", @"C:\foo\bar", ".m4b", @"C:\foo\bar\＜filedate [yyyy＞.m4b")]
		[DataRow("<filedate yyyy>", @"C:\foo\bar", ".m4b", @"C:\foo\bar\＜filedate yyyy＞.m4b")]
		[DataRow("<filedate[yyyy]", @"C:\foo\bar", ".m4b", @"C:\foo\bar\＜filedate[yyyy].m4b")]
		[DataRow("<fil edate[yyyy]>", @"C:\foo\bar", ".m4b", @"C:\foo\bar\＜fil edate[yyyy]＞.m4b")]
		[DataRow("<filed ate[yyyy]>", @"C:\foo\bar", ".m4b", @"C:\foo\bar\＜filed ate[yyyy]＞.m4b")]
		public void DateFormat_invalid(string template, string dirFullPath, string extension, string expected)
		{
			if (Environment.OSVersion.Platform is not PlatformID.Win32NT)
			{
				dirFullPath = dirFullPath.Replace("C:", "").Replace('\\', '/');
				expected = expected.Replace("C:", "").Replace('\\', '/').Replace('＜', '<').Replace('＞','>');
			}

			Templates.getFileNamingTemplate(GetLibraryBook(), template, dirFullPath, extension, Replacements)
				.GetFilePath(extension)
				.PathWithoutPrefix
				.Should().Be(expected);
		}

		[TestMethod]
		[DataRow("<filedate[yy-MM-dd]> <date added[yy-MM-dd]> <pubdate[yy-MM]>", @"C:\foo\bar", ".m4b", @"C:\foo\bar\23-01-28 22-06-09 17-02.m4b")]
		[DataRow("<filedate[yy-MM-dd]> <filedate[yy-MM-dd]> <filedate[yy-MM-dd]>", @"C:\foo\bar", ".m4b", @"C:\foo\bar\23-01-28 23-01-28 23-01-28.m4b")]
		[DataRow("<file date     [             yy-MM-dd      ]         > <filedate   [  yy-MM-dd ] > <file  date   [ yy-MM-dd]    >", @"C:\foo\bar", ".m4b", @"C:\foo\bar\23-01-28 23-01-28 23-01-28.m4b")]
		public void DateFormat_multiple(string template, string dirFullPath, string extension, string expected)
		{
			if (Environment.OSVersion.Platform is not PlatformID.Win32NT)
			{
				dirFullPath = dirFullPath.Replace("C:", "").Replace('\\', '/');
				expected = expected.Replace("C:", "").Replace('\\', '/');
			}

			Templates.getFileNamingTemplate(GetLibraryBook(), template, dirFullPath, extension, Replacements)
				.GetFilePath(extension)
				.PathWithoutPrefix
				.Should().Be(expected);
		}

		[TestMethod]
		[DataRow("<id> - <pubdate[MM/dd/yy HH:mm]>", @"C:\foo\bar", ".m4b", @"C:\foo\bar\asin - 02∕27∕17 00꞉00.m4b", PlatformID.Win32NT)]
		[DataRow("<id> - <pubdate[MM/dd/yy HH:mm]>", @"/foo/bar", ".m4b", @"/foo/bar/asin - 02∕27∕17 00:00.m4b", PlatformID.Unix)]
		[DataRow("<id> - <filedate[MM/dd/yy HH:mm]>", @"C:\foo\bar", ".m4b", @"C:\foo\bar\asin - 01∕28∕23 00꞉00.m4b", PlatformID.Win32NT)]
		[DataRow("<id> - <filedate[MM/dd/yy HH:mm]>", @"/foo/bar", ".m4b", @"/foo/bar/asin - 01∕28∕23 00:00.m4b", PlatformID.Unix)]
		[DataRow("<id> - <date added[MM/dd/yy HH:mm]>", @"C:\foo\bar", ".m4b", @"C:\foo\bar\asin - 06∕09∕22 00꞉00.m4b", PlatformID.Win32NT)]
		[DataRow("<id> - <date added[MM/dd/yy HH:mm]>", @"/foo/bar", ".m4b", @"/foo/bar/asin - 06∕09∕22 00:00.m4b", PlatformID.Unix)]
		public void DateFormat_illegal(string template, string dirFullPath, string extension, string expected, PlatformID platformID)
		{
			if (Environment.OSVersion.Platform == platformID)
			{
				Templates.File.HasWarnings(template).Should().BeTrue();
				Templates.File.HasWarnings(Templates.File.Sanitize(template, Replacements)).Should().BeFalse();
				Templates.getFileNamingTemplate(GetLibraryBook(), template, dirFullPath, extension, Replacements)
					.GetFilePath(extension)
					.PathWithoutPrefix
					.Should().Be(expected);

			}
		}


		[TestMethod]
		[DataRow("<filedate[yy-MM-dd]> <date added[yy-MM-dd]> <pubdate[yy-MM]>", @"C:\foo\bar", ".m4b", @"C:\foo\bar\23-01-28.m4b")]
		public void DateFormat_null(string template, string dirFullPath, string extension, string expected)
		{
			if (Environment.OSVersion.Platform is not PlatformID.Win32NT)
			{
				dirFullPath = dirFullPath.Replace("C:", "").Replace('\\', '/');
				expected = expected.Replace("C:", "").Replace('\\', '/');
			}

			Templates.getFileNamingTemplate(GetLibraryBookWithNullDates(), template, dirFullPath, extension, Replacements)
				.GetFilePath(extension)
				.PathWithoutPrefix
				.Should().Be(expected);

		}

		[TestMethod]
		[DataRow(@"C:\a\b", @"C:\a\b\foobar.ext", PlatformID.Win32NT)]
		[DataRow(@"/a/b", @"/a/b/foobar.ext", PlatformID.Unix)]
		public void IfSeries_empty(string directory, string expected, PlatformID platformID)
		{
			if (Environment.OSVersion.Platform == platformID)
				Templates.getFileNamingTemplate(GetLibraryBook(), "foo<if series-><-if series>bar", directory, "ext", Replacements)
				.GetFilePath(".ext")
				.PathWithoutPrefix
				.Should().Be(expected);
		}

		[TestMethod]
		[DataRow(@"C:\a\b", @"C:\a\b\foobar.ext", PlatformID.Win32NT)]
		[DataRow(@"/a/b", @"/a/b/foobar.ext", PlatformID.Unix)]
		public void IfSeries_no_series(string directory, string expected, PlatformID platformID)
		{
			if (Environment.OSVersion.Platform == platformID)
				Templates.getFileNamingTemplate(GetLibraryBook(null), "foo<if series->-<series>-<id>-<-if series>bar", directory, "ext", Replacements)
				.GetFilePath(".ext")
				.PathWithoutPrefix
				.Should().Be(expected);
		}

		[TestMethod]
		[DataRow(@"C:\a\b", @"C:\a\b\foo-Sherlock Holmes-asin-bar.ext", PlatformID.Win32NT)]
		[DataRow(@"/a/b", @"/a/b/foo-Sherlock Holmes-asin-bar.ext", PlatformID.Unix)]
		public void IfSeries_with_series(string directory, string expected, PlatformID platformID)
		{
			if (Environment.OSVersion.Platform == platformID)
				Templates.getFileNamingTemplate(GetLibraryBook(), "foo<if series->-<series>-<id>-<-if series>bar", directory, "ext", Replacements)
				.GetFilePath(".ext")
				.PathWithoutPrefix
				.Should().Be(expected);
		}
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
		public void null_is_invalid() => Tests(null, Environment.OSVersion.Platform, new[] { Templates.ERROR_NULL_IS_INVALID });

		[TestMethod]
		public void empty_is_valid() => valid_tests("");

		[TestMethod]
		public void whitespace_is_valid() => valid_tests("   ");

		[TestMethod]
		[DataRow(@"foo")]
		[DataRow(@"<id>")]
		public void valid_tests(string template) => Tests(template, Environment.OSVersion.Platform, Array.Empty<string>());


		[TestMethod]
		[DataRow(@"C:\", PlatformID.Win32NT, Templates.ERROR_INVALID_FILE_NAME_CHAR)]
		[DataRow(@"/", PlatformID.Unix, Templates.ERROR_INVALID_FILE_NAME_CHAR)]
		[DataRow(@"\foo", PlatformID.Win32NT, Templates.ERROR_INVALID_FILE_NAME_CHAR)]
		[DataRow(@"/foo", PlatformID.Win32NT, Templates.ERROR_INVALID_FILE_NAME_CHAR)]
		[DataRow(@"/foo", PlatformID.Unix, Templates.ERROR_INVALID_FILE_NAME_CHAR)]
		public void Tests(string template, PlatformID platformID, params string[] expected)
		{
			if (Environment.OSVersion.Platform == platformID)
			{
				var result = Templates.File.GetErrors(template);
				result.Count().Should().Be(expected.Length);
				result.Should().BeEquivalentTo(expected);
			}
		}
	}

	[TestClass]
	public class IsValid
	{
		[TestMethod]
		public void null_is_invalid() => Tests(null, false, Environment.OSVersion.Platform);

		[TestMethod]
		public void empty_is_valid() => Tests("", true, Environment.OSVersion.Platform);

		[TestMethod]
		public void whitespace_is_valid() => Tests("   ", true, Environment.OSVersion.Platform);

		[TestMethod]
		[DataRow(@"C:\", false, PlatformID.Win32NT)]
		[DataRow(@"/", false, PlatformID.Unix)]
		[DataRow(@"foo", true, PlatformID.Win32NT)]
		[DataRow(@"foo", true, PlatformID.Unix)]
		[DataRow(@"\foo", false, PlatformID.Win32NT)]
		[DataRow(@"\foo", true, PlatformID.Unix)]
		[DataRow(@"/foo", false, PlatformID.Win32NT)]
		[DataRow(@"<id>", true, PlatformID.Win32NT)]
		[DataRow(@"<id>", true, PlatformID.Unix)]
		public void Tests(string template, bool expected, PlatformID platformID)
		{
			if (Environment.OSVersion.Platform == platformID)
						Templates.File.IsValid(template).Should().Be(expected);	
		}
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
		public void null_is_invalid() => Tests(null, null, new[] { Templates.ERROR_NULL_IS_INVALID });

		[TestMethod]
		public void empty_has_warnings() => Tests("", null, Templates.WARNING_EMPTY, Templates.WARNING_NO_TAGS, Templates.WARNING_NO_CHAPTER_NUMBER_TAG);

		[TestMethod]
		public void whitespace_has_warnings() => Tests("   ", null, Templates.WARNING_WHITE_SPACE, Templates.WARNING_NO_TAGS, Templates.WARNING_NO_CHAPTER_NUMBER_TAG);

		[TestMethod]
		[DataRow("<ch#>")]
		[DataRow("<ch#> <id>")]
		public void valid_tests(string template) => Tests(template, null, Array.Empty<string>());

		[TestMethod]
		[DataRow(@"no tags", null, Templates.WARNING_NO_TAGS, Templates.WARNING_NO_CHAPTER_NUMBER_TAG)]
		[DataRow(@"<id>\foo\bar", true, Templates.ERROR_INVALID_FILE_NAME_CHAR, Templates.WARNING_NO_CHAPTER_NUMBER_TAG)]
		[DataRow(@"<id>/foo/bar", false, Templates.ERROR_INVALID_FILE_NAME_CHAR, Templates.WARNING_NO_CHAPTER_NUMBER_TAG)]
		[DataRow("<chapter count> -- chapter tag but not ch# or ch_#", null, Templates.WARNING_NO_TAGS, Templates.WARNING_NO_CHAPTER_NUMBER_TAG)]
		public void Tests(string template, bool? windows, params string[] expected)
		{
			if(windows is null
			|| (windows is true && Environment.OSVersion.Platform is PlatformID.Win32NT)
			|| (windows is false && Environment.OSVersion.Platform is PlatformID.Unix))
			{
				var result = Templates.ChapterFile.GetWarnings(template);
				result.Count().Should().Be(expected.Length);
				result.Should().BeEquivalentTo(expected);
			}
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

	[TestClass]
	public class GetPortionFilename
	{
		static readonly ReplacementCharacters Default = ReplacementCharacters.Default;

		[TestMethod]
		[DataRow("[<id>] <ch# 0> of <ch count> - <ch title>", @"C:\foo\", "txt", 6, 10, "chap", @"C:\foo\[asin] 06 of 10 - chap.txt", PlatformID.Win32NT)]
		[DataRow("[<id>] <ch# 0> of <ch count> - <ch title>", @"/foo/", "txt", 6, 10, "chap", @"/foo/[asin] 06 of 10 - chap.txt", PlatformID.Unix)]
		[DataRow("<ch#>", @"C:\foo\", "txt", 6, 10, "chap", @"C:\foo\6.txt", PlatformID.Win32NT)]
		[DataRow("<ch#>", @"/foo/", "txt", 6, 10, "chap", @"/foo/6.txt", PlatformID.Unix)]
		public void Tests(string template, string dir, string ext, int pos, int total, string chapter, string expected, PlatformID platformID)
		{
			if (Environment.OSVersion.Platform == platformID)
				Templates.ChapterFile.GetPortionFilename(GetLibraryBook(), template, new() { OutputFileName = $"xyz.{ext}", PartsPosition = pos, PartsTotal = total, Title = chapter }, dir, Default)
				.Should().Be(expected);
		}
	}
}
