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
		[TestMethod]
		public void equiv_GetValidFilename()
		{
			var expected = @"C:\foo\bar\my_ book LONG_1234567890_1234567890_1234567890_123 [ID123456].txt";
			var f1 = OLD_GetValidFilename(@"C:\foo\bar", "my: book LONG_1234567890_1234567890_1234567890_12345", "txt", "ID123456");
			var f2 = NEW_GetValidFilename_FileNamingTemplate(@"C:\foo\bar", "my: book LONG_1234567890_1234567890_1234567890_12345", "txt", "ID123456");

			f1.Should().Be(expected);
			f1.Should().Be(f2);
		}
		private static string OLD_GetValidFilename(string dirFullPath, string filename, string extension, string metadataSuffix)
		{
			ArgumentValidator.EnsureNotNullOrWhiteSpace(dirFullPath, nameof(dirFullPath));

			filename ??= "";

			// sanitize. omit invalid characters. exception: colon => underscore
			filename = filename.Replace(":", "_");
			filename = FileUtility.GetSafeFileName(filename);

			if (filename.Length > 50)
				filename = filename.Substring(0, 50);

			if (!string.IsNullOrWhiteSpace(metadataSuffix))
				filename += $" [{metadataSuffix}]";

			// extension is null when this method is used for directory names
			extension = FileUtility.GetStandardizedExtension(extension);

			// ensure uniqueness
			var fullfilename = Path.Combine(dirFullPath, filename + extension);
			var i = 0;
			while (File.Exists(fullfilename))
				fullfilename = Path.Combine(dirFullPath, filename + $" ({++i})" + extension);

			return fullfilename;
		}
		private static string NEW_GetValidFilename_FileNamingTemplate(string dirFullPath, string filename, string extension, string metadataSuffix)
		{
			var template = $"<title> [<id>]";

			var fullfilename = Path.Combine(dirFullPath, template + FileUtility.GetStandardizedExtension(extension));

			var fileNamingTemplate = new FileNamingTemplate(fullfilename) { IllegalCharacterReplacements = "_" };
			fileNamingTemplate.AddParameterReplacement("title", filename);
			fileNamingTemplate.AddParameterReplacement("id", metadataSuffix);
			return fileNamingTemplate.GetFilePath();
		}

		[TestMethod]
		public void equiv_GetMultipartFileName()
		{
			var expected = @"C:\foo\bar\my file - 002 - title.txt";
			var f1 = OLD_GetMultipartFileName(@"C:\foo\bar\my file.txt", 2, 100, "title");
			var f2 = NEW_GetMultipartFileName_FileNamingTemplate(@"C:\foo\bar\my file.txt", 2, 100, "title");

			f1.Should().Be(expected);
			f1.Should().Be(f2);
		}
		private static string OLD_GetMultipartFileName(string originalPath, int partsPosition, int partsTotal, string suffix)
		{
			// 1-9     => 1-9
			// 10-99   => 01-99
			// 100-999 => 001-999
			var chapterCountLeadingZeros = partsPosition.ToString().PadLeft(partsTotal.ToString().Length, '0');

			string extension = Path.GetExtension(originalPath);

			var filenameBase = $"{Path.GetFileNameWithoutExtension(originalPath)} - {chapterCountLeadingZeros}";
			if (!string.IsNullOrWhiteSpace(suffix))
				filenameBase += $" - {suffix}";

			// Replace illegal path characters with spaces
			var fileName = FileUtility.GetSafeFileName(filenameBase, " ");
			var path = Path.Combine(Path.GetDirectoryName(originalPath), fileName + extension);
			return path;
		}
		private static string NEW_GetMultipartFileName_FileNamingTemplate(string originalPath, int partsPosition, int partsTotal, string suffix)
		{
			// 1-9     => 1-9
			// 10-99   => 01-99
			// 100-999 => 001-999
			var chapterCountLeadingZeros = partsPosition.ToString().PadLeft(partsTotal.ToString().Length, '0');

			var t = Path.ChangeExtension(originalPath, null) + " - <chapter> - <title>" + Path.GetExtension(originalPath);

			var fileNamingTemplate = new FileNamingTemplate(t) { IllegalCharacterReplacements = " " };
			fileNamingTemplate.AddParameterReplacement("chapter", chapterCountLeadingZeros);
			fileNamingTemplate.AddParameterReplacement("title", suffix);
			return fileNamingTemplate.GetFilePath();
		}

		[TestMethod]
		public void remove_slashes()
		{
			var fileNamingTemplate = new FileNamingTemplate(@"\foo\<title>.txt");
			fileNamingTemplate.AddParameterReplacement("title", @"s\l/a\s/h\e/s");
			fileNamingTemplate.GetFilePath().Should().Be(@"\foo\slashes.txt");
		}
	}
}
