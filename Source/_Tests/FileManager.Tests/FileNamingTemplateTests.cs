using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dinah.Core;
using FileManager;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FileNamingTemplateTests
{
	[TestClass]
	public class GetFilePath
	{
		static ReplacementCharacters Replacements = ReplacementCharacters.Default;

		[TestMethod]
		[DataRow(@"C:\foo\bar", @"C:\foo\bar\my꞉ book 0000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000 [ID123456].txt", PlatformID.Win32NT)]
		[DataRow(@"/foo/bar", @"/foo/bar/my: book 0000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000 [ID123456].txt", PlatformID.Unix)]
		public void equiv_GetValidFilename(string dirFullPath, string expected, PlatformID platformID)
		{
			if (Environment.OSVersion.Platform != platformID)
				return;

			var sb = new System.Text.StringBuilder();
			sb.Append('0', 300);
			var longText = sb.ToString();

			NEW_GetValidFilename_FileNamingTemplate(dirFullPath, "my: book " + longText, "txt", "ID123456").Should().Be(expected);
		}

		private static string NEW_GetValidFilename_FileNamingTemplate(string dirFullPath, string filename, string extension, string metadataSuffix)
		{
			var template = $"<title> [<id>]";

			var fullfilename = Path.Combine(dirFullPath, template + FileUtility.GetStandardizedExtension(extension));

			var fileNamingTemplate = new FileNamingTemplate(fullfilename);
			fileNamingTemplate.AddParameterReplacement("title", filename);
			fileNamingTemplate.AddParameterReplacement("id", metadataSuffix);
			return fileNamingTemplate.GetFilePath(Replacements).PathWithoutPrefix;
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
			var chapterCountLeadingZeros = partsPosition.ToString().PadLeft(partsTotal.ToString().Length, '0');

			var t = Path.ChangeExtension(originalPath, null) + " - <chapter> - <title>" + Path.GetExtension(originalPath);

			var fileNamingTemplate = new FileNamingTemplate(t);
			fileNamingTemplate.AddParameterReplacement("chapter", chapterCountLeadingZeros);
			fileNamingTemplate.AddParameterReplacement("title", suffix);
			return fileNamingTemplate.GetFilePath(Replacements).PathWithoutPrefix;
		}

		[TestMethod]
		[DataRow(@"\foo\<title>.txt", @"\foo\sl∕as∕he∕s.txt", PlatformID.Win32NT)]
		[DataRow(@"/foo/<title>.txt", @"/foo/s\l∕a\s∕h\e∕s.txt", PlatformID.Unix)]
		public void remove_slashes(string inStr, string outStr, PlatformID platformID)
		{
			if (Environment.OSVersion.Platform == platformID)
			{
				var fileNamingTemplate = new FileNamingTemplate(inStr);
				fileNamingTemplate.AddParameterReplacement("title", @"s\l/a\s/h\e/s");
				fileNamingTemplate.GetFilePath(Replacements).PathWithoutPrefix.Should().Be(outStr);
			}
		}
	}
}
