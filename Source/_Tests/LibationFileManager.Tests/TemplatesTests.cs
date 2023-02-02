using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dinah.Core;
using FileManager;
using FileManager.NamingTemplate;
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
				Channels = 2,
                Language = "English"
            };
	}

	[TestClass]
	public class ContainsTag
	{
		[TestMethod]
		[DataRow("<ch#>", 0)]
		[DataRow("<id>", 1)]
		[DataRow("<id><ch#>", 1)]
		public void Tests(string template, int numTags)
		{
			Templates.TryGetTemplate<Templates.FileTemplate>(template, out var fileTemplate).Should().BeTrue();

			fileTemplate.TagsInUse.Should().HaveCount(numTags);
		}
	}

	[TestClass]
	public class getFileNamingTemplate
	{
		static ReplacementCharacters Replacements = ReplacementCharacters.Default;

		[TestMethod]
		[DataRow(null)]
		public void template_null(string template)
		{
			Templates.TryGetTemplate<Templates.FileTemplate>(template, out var t).Should().BeFalse();
			t.IsValid.Should().BeFalse();
		}

		[TestMethod]
		[DataRow("")]
		[DataRow("   ")]
		public void template_empty(string template)
		{
			Templates.TryGetTemplate<Templates.FileTemplate>(template, out var t).Should().BeTrue();
			t.Warnings.Should().HaveCount(2);
		}

		[TestMethod]
		[DataRow("f.txt", @"C:\foo\bar", "", @"C:\foo\bar\f.txt")]
		[DataRow("f.txt", @"C:\foo\bar", ".ext", @"C:\foo\bar\f.txt.ext")]
		[DataRow("f", @"C:\foo\bar", ".ext", @"C:\foo\bar\f.ext")]
		[DataRow("<id>", @"C:\foo\bar", ".ext", @"C:\foo\bar\asin.ext")]
        [DataRow("<bitrate> - <samplerate> - <channels>", @"C:\foo\bar", ".ext", @"C:\foo\bar\128 - 44100 - 2.ext")]
        [DataRow("<year> - <channels>", @"C:\foo\bar", ".ext", @"C:\foo\bar\2017 - 2.ext")]
		[DataRow("(000.0) <year> - <channels>", @"C:\foo\bar", "ext", @"C:\foo\bar\(000.0) 2017 - 2.ext")]
		public void Tests(string template, string dirFullPath, string extension, string expected)
		{
			if (Environment.OSVersion.Platform is not PlatformID.Win32NT)
			{
				dirFullPath = dirFullPath.Replace("C:", "").Replace('\\', '/');
				expected = expected.Replace("C:", "").Replace('\\', '/');
			}

			Templates.TryGetTemplate<Templates.FileTemplate>(template, out var fileTemplate).Should().BeTrue();

			fileTemplate
				.GetFilename(GetLibraryBook(), dirFullPath, extension, Replacements)
				.PathWithoutPrefix
				.Should().Be(expected);
		}

		[TestMethod]
		[DataRow("<bitrate>Kbps <samplerate>Hz", "128Kbps 44100Hz")]
		[DataRow("<bitrate>Kbps <samplerate[6]>Hz", "128Kbps 044100Hz")]
		[DataRow("<bitrate[4]>Kbps <samplerate>Hz", "0128Kbps 44100Hz")]
		[DataRow("<bitrate[4]>Kbps <titleshort[u]>", "0128Kbps A STUDY IN SCARLET")]
		[DataRow("<bitrate[4]>Kbps <titleshort[l]>", "0128Kbps a study in scarlet")]
		[DataRow("<bitrate[4]>Kbps <samplerate[6]>Hz", "0128Kbps 044100Hz")]
		[DataRow("<bitrate  [ 4 ]  >Kbps <samplerate   [  6  ]   >Hz", "0128Kbps 044100Hz")]
		public void FormatTags(string template, string expected)
		{
			Templates.TryGetTemplate<Templates.FileTemplate>(template, out var fileTemplate).Should().BeTrue();
			fileTemplate.GetFilename(GetLibraryBook(), "", "", Replacements).PathWithoutPrefix.Should().Be(expected);
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

			Templates.TryGetTemplate<Templates.FileTemplate>(template, out var fileTemplate).Should().BeTrue();
			fileTemplate
				.GetFilename(GetLibraryBook(), dirFullPath, extension, Replacements)
				.PathWithoutPrefix
				.Should().Be(expected);
		}

		[TestMethod]
		[DataRow("<filedate[h]>", @"C:\foo\bar", ".m4b", @"C:\foo\bar\＜filedate[h]＞.m4b")]
		[DataRow("< filedate[yyyy]>", @"C:\foo\bar", ".m4b", @"C:\foo\bar\＜ filedate[yyyy]＞.m4b")]
		[DataRow("<filedate[yyyy][]>", @"C:\foo\bar", ".m4b", @"C:\foo\bar\＜filedate[yyyy][]＞.m4b")]
		[DataRow("<filedate[[yyyy]]>", @"C:\foo\bar", ".m4b", @"C:\foo\bar\＜filedate[[yyyy]]＞.m4b")]
		[DataRow("<filedate[yyyy[]]>", @"C:\foo\bar", ".m4b", @"C:\foo\bar\＜filedate[yyyy[]]＞.m4b")]
		[DataRow("<filedate yyyy]>", @"C:\foo\bar", ".m4b", @"C:\foo\bar\＜filedate yyyy]＞.m4b")]
		[DataRow("<filedate ]yyyy]>", @"C:\foo\bar", ".m4b", @"C:\foo\bar\＜filedate ]yyyy]＞.m4b")]
		[DataRow("<filedate [yyyy>", @"C:\foo\bar", ".m4b", @"C:\foo\bar\＜filedate [yyyy＞.m4b")]
		[DataRow("<filedate [yyyy[>", @"C:\foo\bar", ".m4b", @"C:\foo\bar\＜filedate [yyyy[＞.m4b")]
		[DataRow("<filedate yyyy>", @"C:\foo\bar", ".m4b", @"C:\foo\bar\＜filedate yyyy＞.m4b")]
		[DataRow("<filedate[yyyy]", @"C:\foo\bar", ".m4b", @"C:\foo\bar\＜filedate[yyyy].m4b")]
		[DataRow("<fil edate[yyyy]>", @"C:\foo\bar", ".m4b", @"C:\foo\bar\＜fil edate[yyyy]＞.m4b")]
		[DataRow("<filed ate[yyyy]>", @"C:\foo\bar", ".m4b", @"C:\foo\bar\＜filed ate[yyyy]＞.m4b")]
		public void DateFormat_invalid(string template, string dirFullPath, string extension, string expected)
		{
			if (Environment.OSVersion.Platform is not PlatformID.Win32NT)
			{
				dirFullPath = dirFullPath.Replace("C:", "").Replace('\\', '/');
				expected = expected.Replace("C:", "").Replace('\\', '/').Replace('＜', '<').Replace('＞', '>');
			}

			Templates.TryGetTemplate<Templates.FileTemplate>(template, out var fileTemplate).Should().BeTrue();
			fileTemplate
				.GetFilename(GetLibraryBook(), dirFullPath, extension, Replacements)
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

			Templates.TryGetTemplate<Templates.FileTemplate>(template, out var fileTemplate).Should().BeTrue();
			fileTemplate
				.GetFilename(GetLibraryBook(), dirFullPath, extension, Replacements)
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
				Templates.TryGetTemplate<Templates.FileTemplate>(template, out var fileTemplate).Should().BeTrue();

				fileTemplate.HasWarnings.Should().BeFalse();
				fileTemplate
					.GetFilename(GetLibraryBook(), dirFullPath, extension, Replacements)
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

			var lbDto = GetLibraryBook();
			lbDto.DatePublished = null;
			lbDto.DateAdded = null;

			Templates.TryGetTemplate<Templates.FileTemplate>(template, out var fileTemplate).Should().BeTrue();
			fileTemplate
				.GetFilename(lbDto, dirFullPath, extension, Replacements)
				.PathWithoutPrefix
				.Should().Be(expected);
		}

		[TestMethod]
		[DataRow(@"C:\a\b", @"C:\a\b\foobar.ext", PlatformID.Win32NT)]
		[DataRow(@"/a/b", @"/a/b/foobar.ext", PlatformID.Unix)]
		public void IfSeries_empty(string directory, string expected, PlatformID platformID)
		{
			if (Environment.OSVersion.Platform == platformID)
			{
				Templates.TryGetTemplate<Templates.FileTemplate>("foo<if series-><-if series>bar", out var fileTemplate).Should().BeTrue();

				fileTemplate
					.GetFilename(GetLibraryBook(), directory, "ext", Replacements)
					.PathWithoutPrefix
					.Should().Be(expected);
			}
		}

		[TestMethod]
		[DataRow(@"C:\a\b", @"C:\a\b\foobar.ext", PlatformID.Win32NT)]
		[DataRow(@"/a/b", @"/a/b/foobar.ext", PlatformID.Unix)]
		public void IfSeries_no_series(string directory, string expected, PlatformID platformID)
		{
			if (Environment.OSVersion.Platform == platformID)
			{
				Templates.TryGetTemplate<Templates.FileTemplate>("foo<if series->-<series>-<id>-<-if series>bar", out var fileTemplate).Should().BeTrue();

				fileTemplate.GetFilename(GetLibraryBook(null), directory, "ext", Replacements)
				.PathWithoutPrefix
				.Should().Be(expected);
			}
		}

		[TestMethod]
		[DataRow(@"C:\a\b", @"C:\a\b\foo-Sherlock Holmes-asin-bar.ext", PlatformID.Win32NT)]
		[DataRow(@"/a/b", @"/a/b/foo-Sherlock Holmes-asin-bar.ext", PlatformID.Unix)]
		public void IfSeries_with_series(string directory, string expected, PlatformID platformID)
		{
			if (Environment.OSVersion.Platform == platformID)
			{
				Templates.TryGetTemplate<Templates.FileTemplate>("foo<if series->-<series>-<id>-<-if series>bar", out var fileTemplate).Should().BeTrue();

				fileTemplate
					.GetFilename(GetLibraryBook(), directory, "ext", Replacements)
					.PathWithoutPrefix
					.Should().Be(expected);
			}
		}
	}
}


namespace Templates_Other
{

	[TestClass]
	public class GetFilePath
	{
		static ReplacementCharacters Replacements = ReplacementCharacters.Default;

		[TestMethod]
		[DataRow(@"C:\foo\bar", @"C:\foo\bar\Folder\my꞉ book 000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000\[ID123456].txt", PlatformID.Win32NT)]
		[DataRow(@"/foo/bar", @"/foo/bar/Folder/my: book 000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000/[ID123456].txt", PlatformID.Unix)]
		public void equiv_GetValidFilename(string dirFullPath, string expected, PlatformID platformID)
		{
			if (Environment.OSVersion.Platform != platformID)
				return;

			var sb = new System.Text.StringBuilder();
			sb.Append('0', 300);
			var longText = sb.ToString();

			NEW_GetValidFilename_FileNamingTemplate(dirFullPath, "my: book " + longText, "txt", "ID123456").Should().Be(expected);
		}

		private class TemplateTag : ITemplateTag
		{
			public string TagName { get; init; }
			public string DefaultValue { get; }
			public string Description { get; }
			public string Display { get; }
		}
		private static string NEW_GetValidFilename_FileNamingTemplate(string dirFullPath, string filename, string extension, string metadataSuffix)
		{
			char slash = Path.DirectorySeparatorChar;

			var template = $"{slash}Folder{slash}<title>{slash}[<id>]{slash}";

			extension = FileUtility.GetStandardizedExtension(extension);

			var lbDto = GetLibraryBook();
			lbDto.Title = filename;
			lbDto.AudibleProductId = metadataSuffix;

			Templates.TryGetTemplate<Templates.FolderTemplate>(template, out var fileNamingTemplate).Should().BeTrue();

			return fileNamingTemplate.GetFilename(lbDto, dirFullPath, extension, Replacements).PathWithoutPrefix;
		}

		[TestMethod]
		[DataRow(@"C:\foo\bar\my file.txt", @"C:\foo\bar\my file - 002 - title.txt", PlatformID.Win32NT)]
		[DataRow(@"/foo/bar/my file.txt", @"/foo/bar/my file - 002 - title.txt", PlatformID.Unix)]
		public void equiv_GetMultipartFileName(string inStr, string outStr, PlatformID platformID)
		{
			if (Environment.OSVersion.Platform == platformID)
				NEW_GetMultipartFileName_FileNamingTemplate(inStr, 2, 100, "title").Should().Be(outStr);
		}

		private static string NEW_GetMultipartFileName_FileNamingTemplate(string originalPath, int partsPosition, int partsTotal, string suffix)
		{
			// 1-9     => 1-9
			// 10-99   => 01-99
			// 100-999 => 001-999

			var estension = Path.GetExtension(originalPath);
			var dir = Path.GetDirectoryName(originalPath);
			var template = Path.GetFileNameWithoutExtension(originalPath) + " - <ch# 0> - <title>" + estension;

			var lbDto = GetLibraryBook();
			lbDto.Title = suffix;

			Templates.TryGetTemplate<Templates.ChapterFileTemplate>(template, out var chapterFileTemplate).Should().BeTrue();

			return chapterFileTemplate
				.GetFilename(lbDto, new AaxDecrypter.MultiConvertFileProperties { Title = suffix, PartsTotal = partsTotal, PartsPosition = partsPosition }, dir, estension, Replacements)
				.PathWithoutPrefix;
		}

		[TestMethod]
		[DataRow(@"\foo\<title>.txt", @"\foo\sl∕as∕he∕s.txt", PlatformID.Win32NT)]
		[DataRow(@"/foo/<title>.txt", @"/foo/s\l∕a\s∕h\e∕s.txt", PlatformID.Unix)]
		public void remove_slashes(string inStr, string outStr, PlatformID platformID)
		{
			if (Environment.OSVersion.Platform == platformID)
			{
				var lbDto = GetLibraryBook();
				lbDto.Title = @"s\l/a\s/h\e/s";

				var directory = Path.GetDirectoryName(inStr);
				var fileName = Path.GetFileName(inStr);

				Templates.TryGetTemplate<Templates.FileTemplate>(fileName, out var fileNamingTemplate).Should().BeTrue();

				fileNamingTemplate.GetFilename(lbDto, directory, "txt", Replacements).PathWithoutPrefix.Should().Be(outStr);
			}
		}
	}
}

namespace Templates_Folder_Tests
{
	[TestClass]
	public class GetErrors
	{
		[TestMethod]
		public void null_is_invalid() => Tests(null, PlatformID.Win32NT | PlatformID.Unix, new[] { NamingTemplate.ERROR_NULL_IS_INVALID });

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
		public void valid_tests(string template) => Tests(template, PlatformID.Win32NT | PlatformID.Unix, Array.Empty<string>());

		[TestMethod]
		[DataRow(@"C:\", PlatformID.Win32NT, Templates.ERROR_FULL_PATH_IS_INVALID)]
		public void Tests(string template, PlatformID platformID, params string[] expected)
		{
			if ((platformID & Environment.OSVersion.Platform) == Environment.OSVersion.Platform)
			{
				Templates.TryGetTemplate<Templates.FolderTemplate>(template, out var folderTemplate);
				var result = folderTemplate.Errors;
				result.Should().HaveCount(expected.Length);
				result.Should().BeEquivalentTo(expected);
			}
		}
	}

	[TestClass]
	public class IsValid
	{
		[TestMethod]
		public void null_is_invalid() => Templates.TryGetTemplate<Templates.FolderTemplate>(null, out _).Should().BeFalse();

		[TestMethod]
		public void empty_is_valid() => Tests("", true, PlatformID.Win32NT | PlatformID.Unix);

		[TestMethod]
		public void whitespace_is_valid() => Tests("   ", true, PlatformID.Win32NT | PlatformID.Unix);

		[TestMethod]
		[DataRow(@"C:\", false, PlatformID.Win32NT)]
		[DataRow(@"foo", true, PlatformID.Win32NT | PlatformID.Unix)]
		[DataRow(@"\foo", true, PlatformID.Win32NT | PlatformID.Unix)]
		[DataRow(@"foo\", true, PlatformID.Win32NT | PlatformID.Unix)]
		[DataRow(@"\foo\", true, PlatformID.Win32NT | PlatformID.Unix)]
		[DataRow(@"foo\bar", true, PlatformID.Win32NT | PlatformID.Unix)]
		[DataRow(@"<id>", true, PlatformID.Win32NT | PlatformID.Unix)]
		[DataRow(@"<id>\<title>", true, PlatformID.Win32NT | PlatformID.Unix)]
		public void Tests(string template, bool expected, PlatformID platformID)
		{
			if ((platformID & Environment.OSVersion.Platform) == Environment.OSVersion.Platform)
			{
				Templates.TryGetTemplate<Templates.FolderTemplate>(template, out var folderTemplate).Should().BeTrue();
				folderTemplate.IsValid.Should().Be(expected);
			}
		}
	}

	[TestClass]
	public class GetWarnings
	{
		[TestMethod]
		public void null_is_invalid() => Tests(null, new[] { NamingTemplate.ERROR_NULL_IS_INVALID });

		[TestMethod]
		public void empty_has_warnings() => Tests("", NamingTemplate.WARNING_EMPTY, NamingTemplate.WARNING_NO_TAGS);

		[TestMethod]
		public void whitespace_has_warnings() => Tests("   ", NamingTemplate.WARNING_WHITE_SPACE, NamingTemplate.WARNING_NO_TAGS);

		[TestMethod]
		[DataRow(@"<id>\foo\bar")]
		public void valid_tests(string template) => Tests(template, Array.Empty<string>());

		[TestMethod]
		[DataRow(@"no tags", NamingTemplate.WARNING_NO_TAGS)]
		[DataRow("<ch#> chapter tag", NamingTemplate.WARNING_NO_TAGS)]
		public void Tests(string template, params string[] expected)
		{
			Templates.TryGetTemplate<Templates.FolderTemplate>(template, out var folderTemplate);
			var result = folderTemplate.Warnings;
			result.Should().HaveCount(expected.Length);
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
		[DataRow("<ch#> chapter tag", true)]
		public void Tests(string template, bool expected)
		{
			Templates.TryGetTemplate<Templates.FolderTemplate>(template, out var folderTemplate);
			folderTemplate.HasWarnings.Should().Be(expected);
		}
	}

	[TestClass]
	public class TagCount
	{
		[TestMethod]
		public void null_invalid()
		{
			Templates.TryGetTemplate<Templates.FolderTemplate>(null, out var template).Should().BeFalse();
			template.IsValid.Should().BeFalse();
		}

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
		public void Tests(string template, int expected)
		{
			Templates.TryGetTemplate<Templates.FolderTemplate>(template, out var folderTemplate).Should().BeTrue();
			folderTemplate.TagsInUse.Count().Should().Be(expected);
		}
	}
}

namespace Templates_File_Tests
{
	[TestClass]
	public class GetErrors
	{
		[TestMethod]
		public void null_is_invalid() => Tests(null, Environment.OSVersion.Platform, new[] { NamingTemplate.ERROR_NULL_IS_INVALID });

		[TestMethod]
		public void empty_is_valid() => valid_tests("");

		[TestMethod]
		public void whitespace_is_valid() => valid_tests("   ");

		[TestMethod]
		[DataRow(@"foo")]
		[DataRow(@"<id>")]
		public void valid_tests(string template) => Tests(template, Environment.OSVersion.Platform, Array.Empty<string>());

		public void Tests(string template, PlatformID platformID, params string[] expected)
		{
			if (Environment.OSVersion.Platform == platformID)
			{
				Templates.TryGetTemplate<Templates.FileTemplate>(template, out var fileTemplate);
				var result = fileTemplate.Errors;
				result.Should().HaveCount(expected.Length);
				result.Should().BeEquivalentTo(expected);
			}
		}
	}

	[TestClass]
	public class IsValid
	{
		[TestMethod]
		public void null_is_invalid() => Templates.TryGetTemplate<Templates.FileTemplate>(null, out _).Should().BeFalse();

		[TestMethod]
		public void empty_is_valid() => Tests("", true);

		[TestMethod]
		public void whitespace_is_valid() => Tests("   ", true);

		[TestMethod]
		[DataRow(@"foo", true)]
		[DataRow(@"\foo", true)]
		[DataRow(@"foo\", true)]
		[DataRow(@"\foo\", true)]
		[DataRow(@"foo\bar", true)]
		[DataRow(@"<id>", true)]
		[DataRow(@"<id>\<title>", true)]
		public void Tests(string template, bool expected)
		{
			Templates.TryGetTemplate<Templates.FileTemplate>(template, out var folderTemplate).Should().BeTrue();
			folderTemplate.IsValid.Should().Be(expected);
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
		public void null_is_invalid() => Tests(null, null, new[] { NamingTemplate.ERROR_NULL_IS_INVALID, Templates.WARNING_NO_CHAPTER_NUMBER_TAG });

		[TestMethod]
		public void empty_has_warnings() => Tests("", null, NamingTemplate.WARNING_EMPTY, NamingTemplate.WARNING_NO_TAGS, Templates.WARNING_NO_CHAPTER_NUMBER_TAG);

		[TestMethod]
		public void whitespace_has_warnings() => Tests("   ", null, NamingTemplate.WARNING_WHITE_SPACE, NamingTemplate.WARNING_NO_TAGS, Templates.WARNING_NO_CHAPTER_NUMBER_TAG);

		[TestMethod]
		[DataRow("<ch#>")]
		[DataRow("<ch#> <id>")]
		public void valid_tests(string template) => Tests(template, null, Array.Empty<string>());

		[TestMethod]
		[DataRow(@"no tags", null, NamingTemplate.WARNING_NO_TAGS, Templates.WARNING_NO_CHAPTER_NUMBER_TAG)]
		[DataRow(@"<id>\foo\bar", true, Templates.WARNING_NO_CHAPTER_NUMBER_TAG)]
		[DataRow(@"<id>/foo/bar", false, Templates.WARNING_NO_CHAPTER_NUMBER_TAG)]
		[DataRow("<chapter count> -- chapter tag but not ch# or ch_#", null, NamingTemplate.WARNING_NO_TAGS, Templates.WARNING_NO_CHAPTER_NUMBER_TAG)]
		public void Tests(string template, bool? windows, params string[] expected)
		{
			if (windows is null
			|| (windows is true && Environment.OSVersion.Platform is PlatformID.Win32NT)
			|| (windows is false && Environment.OSVersion.Platform is PlatformID.Unix))
			{

				Templates.TryGetTemplate<Templates.ChapterFileTemplate>(template, out var chapterFileTemplate);
				var result = chapterFileTemplate.Warnings;
				result.Should().HaveCount(expected.Length);
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
		public void Tests(string template, bool expected)
		{
			Templates.TryGetTemplate<Templates.ChapterFileTemplate>(template, out var chapterFileTemplate);
			chapterFileTemplate.HasWarnings.Should().Be(expected);
		}
	}

	[TestClass]
	public class TagCount
	{
		[TestMethod]
		public void null_is_not_recommended() => Templates.TryGetTemplate<Templates.ChapterFileTemplate>(null, out _).Should().BeFalse();

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
		public void Tests(string template, int expected)
		{
			Templates.TryGetTemplate<Templates.ChapterFileTemplate>(template, out var chapterFileTemplate).Should().BeTrue();
			chapterFileTemplate.TagsInUse.Count().Should().Be(expected);
		}
	}

	[TestClass]
	public class GetFilename
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
			{
				Templates.TryGetTemplate<Templates.ChapterFileTemplate>(template, out var chapterTemplate).Should().BeTrue();
				chapterTemplate
					.GetFilename(GetLibraryBook(), new() { OutputFileName = $"xyz.{ext}", PartsPosition = pos, PartsTotal = total, Title = chapter }, dir, ext, Default)
					.PathWithoutPrefix
					.Should().Be(expected);
			}
		}
	}
}
