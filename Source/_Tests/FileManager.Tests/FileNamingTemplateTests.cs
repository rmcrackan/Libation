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
		public void equiv_GetValidFilename()
		{
			var sb = new System.Text.StringBuilder();
			sb.Append('0', 300);
			var longText = sb.ToString();

			var expectedNew = "C:\\foo\\bar\\my꞉ book 000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000 [ID123456].txt";
			var f2 = NEW_GetValidFilename_FileNamingTemplate(@"C:\foo\bar", "my: book " + longText, "txt", "ID123456");

			f2.Should().Be(expectedNew);
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
		public void equiv_GetMultipartFileName()
		{
			var expected = @"C:\foo\bar\my file - 002 - title.txt";
			var f2 = NEW_GetMultipartFileName_FileNamingTemplate(@"C:\foo\bar\my file.txt", 2, 100, "title");

			f2.Should().Be(expected);
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
		public void remove_slashes()
		{
			var fileNamingTemplate = new FileNamingTemplate(@"\foo\<title>.txt");
			fileNamingTemplate.AddParameterReplacement("title", @"s\l/a\s/h\e/s");
			fileNamingTemplate.GetFilePath(Replacements).PathWithoutPrefix.Should().Be(@"\foo\sl∕as∕he∕s.txt");
		}
	}
}
