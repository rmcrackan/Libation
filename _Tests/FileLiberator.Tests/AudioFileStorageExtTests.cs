using System;
using System.Collections.Generic;
using System.Linq;
using Dinah.Core;
using FileLiberator;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using static AudioFileStorageExtTests.Shared;

namespace AudioFileStorageExtTests
{
	public static class Shared
	{
		public static LibationFileManager.LibraryBookDto GetLibraryBook(string asin)
			=> new()
			{
				Account = "my account",
				AudibleProductId = asin,
				Title = "A Study in Scarlet: A Sherlock Holmes Novel",
				Locale = "us",
				Authors = new List<string> { "Arthur Conan Doyle", "Stephen Fry - introductions" },
				Narrators = new List<string> { "Stephen Fry" },
				SeriesName = "Sherlock Holmes",
				SeriesNumber = "1"
			};
	}

	[TestClass]
	public class MultipartRenamer_MultipartFilename
	{
		[TestMethod]
		[DataRow("asin", "[<id>] <ch# 0> of <ch count> - <ch title>", @"C:\foo\", "txt", 6, 10, "chap", @"C:\foo\[asin] 06 of 10 - chap.txt")]
		[DataRow("asin", "<ch#>", @"C:\foo\", "txt", 6, 10, "chap", @"C:\foo\6.txt")]
		public void Tests(string asin, string template, string dir, string ext, int pos, int total, string chapter, string expected)
			=> new AudioFileStorageExt.MultipartRenamer(GetLibraryBook(asin))
			.MultipartFilename(new() { OutputFileName = $"xyz.{ext}", PartsPosition = pos, PartsTotal = total, Title = chapter }, template, dir)
			.Should().Be(expected);
	}

	[TestClass]
	public class GetFileNamingTemplate
	{
		[TestMethod]
		[DataRow(null, "asin", @"C:\", "ext")]
		[ExpectedException(typeof(ArgumentNullException))]
		public void arg_null_exception(string template, string asin, string dirFullPath, string extension)
			=> AudioFileStorageExt.GetFileNamingTemplate(template, GetLibraryBook(asin), dirFullPath, extension);

		[TestMethod]
		[DataRow("", "asin", @"C:\foo\bar", "ext")]
		[DataRow("   ", "asin", @"C:\foo\bar", "ext")]
		[ExpectedException(typeof(ArgumentException))]
		public void arg_exception(string template, string asin, string dirFullPath, string extension)
			=> AudioFileStorageExt.GetFileNamingTemplate(template, GetLibraryBook(asin), dirFullPath, extension);

		[TestMethod]
		public void null_extension() => Tests("f.txt", "asin", @"C:\foo\bar", null, @"C:\foo\bar\f.txt");

		[TestMethod]
		[DataRow("f.txt", "asin", @"C:\foo\bar", "ext", @"C:\foo\bar\f.txt.ext")]
		[DataRow("f", "asin", @"C:\foo\bar", "ext", @"C:\foo\bar\f.ext")]
		[DataRow("<id>", "asin", @"C:\foo\bar", "ext", @"C:\foo\bar\asin.ext")]
		public void Tests(string template, string asin, string dirFullPath, string extension, string expected)
			=> AudioFileStorageExt.GetFileNamingTemplate(template, GetLibraryBook(asin), dirFullPath, extension)
			.GetFilePath()
			.Should().Be(expected);
	}
}
