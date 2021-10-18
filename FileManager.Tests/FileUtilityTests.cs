using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dinah.Core;
using FileManager;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FileUtilityTests
{
	[TestClass]
	public class GetSafePath
	{
		[TestMethod]
		public void null_path_throws() => Assert.ThrowsException<ArgumentNullException>(() => FileUtility.GetSafePath(null));

		// needs separate method. middle null param not running correctly in TestExplorer when used in DataRow()
		[TestMethod]
		[DataRow("http://test.com/a/b/c", @"http\\test.com\a\b\c")]
		public void null_replacement(string inStr, string outStr) => Tests(inStr, null, outStr);

		[TestMethod]
		// empty replacement
		[DataRow("abc*abc.txt", "", "abcabc.txt")]
		// non-empty replacement
		[DataRow("abc*abc.txt", "ZZZ", "abcZZZabc.txt")]
		// standardize slashes
		[DataRow(@"a/b\c/d", "Z", @"a\b\c\d")]
		// remove illegal chars
		[DataRow("a*?:z.txt", "Z", "aZZZz.txt")]
		// retain drive letter path colon
		[DataRow(@"C:\az.txt", "Z", @"C:\az.txt")]
		// replace all other colongs
		[DataRow(@"a\b:c\d.txt", "ZZZ", @"a\bZZZc\d.txt")]
		public void Tests(string inStr, string replacement, string outStr) => Assert.AreEqual(outStr, FileUtility.GetSafePath(inStr, replacement));
	}

	[TestClass]
	public class GetSafeFileName
	{
		// needs separate method. middle null param not running correctly in TestExplorer when used in DataRow()
		[TestMethod]
		[DataRow("http://test.com/a/b/c", "httptest.comabc")]
		public void url_null_replacement(string inStr, string outStr) => ReplacementTests(inStr, null, outStr);

		[TestMethod]
		// empty replacement
		[DataRow("http://test.com/a/b/c", "", "httptest.comabc")]
		// single char replace
		[DataRow("http://test.com/a/b/c", "_", "http___test.com_a_b_c")]
		// multi char replace
		[DataRow("http://test.com/a/b/c", "!!!", "http!!!!!!!!!test.com!!!a!!!b!!!c")]
		public void ReplacementTests(string inStr, string replacement, string outStr) => FileUtility.GetSafeFileName(inStr, replacement).Should().Be(outStr);
	}

	[TestClass]
	public class GetValidFilename
	{
		[TestMethod]
		[DataRow(null, "name", "ext", "suffix")]
		[DataRow(@"C:\", null, "ext", "suffix")]
		[ExpectedException(typeof(ArgumentNullException))]
		public void arg_null_exception(string dirFullPath, string filename, string extension, string metadataSuffix)
			=> FileUtility.GetValidFilename(dirFullPath, filename, extension, metadataSuffix);

		[TestMethod]
		[DataRow("", "name", "ext", "suffix")]
		[DataRow("   ", "name", "ext", "suffix")]
		[DataRow(@"C:\", "", "ext", "suffix")]
		[DataRow(@"C:\", "   ", "ext", "suffix")]
		[ExpectedException(typeof(ArgumentException))]
		public void arg_exception(string dirFullPath, string filename, string extension, string metadataSuffix)
			=> FileUtility.GetValidFilename(dirFullPath, filename, extension, metadataSuffix);

		[TestMethod]
		public void null_extension() => Tests(@"C:\foo\bar", "my file", null, "meta", @"C:\foo\bar\my file [meta]");
		[TestMethod]
		public void null_metadataSuffix() => Tests(@"C:\foo\bar", "my file", "txt", null, @"C:\foo\bar\my file [].txt");

		[TestMethod]
		[DataRow(@"C:\foo\bar", "my file", "txt", "my id", @"C:\foo\bar\my file [my id].txt")]
		[DataRow(@"C:\foo\bar", "my file", "txt", "", @"C:\foo\bar\my file [].txt")]
		public void Tests(string dirFullPath, string filename, string extension, string metadataSuffix, string expected)
			=> FileUtility.GetValidFilename(dirFullPath, filename, extension, metadataSuffix).Should().Be(expected);
	}

	[TestClass]
	public class GetMultipartFileName
	{
		[TestMethod]
		public void null_path() => Assert.ThrowsException<ArgumentNullException>(() => FileUtility.GetMultipartFileName(null, 1, 1, ""));

		[TestMethod]
		public void null_suffix() => Tests(@"C:\foo\bar\my file.txt", 2, 100, null, @"C:\foo\bar\my file - 002 - .txt");

		[TestMethod]
		public void negative_partsPosition() => Assert.ThrowsException<ArgumentException>(()
			=> FileUtility.GetMultipartFileName("foo", -1, 2, "")
		);
		[TestMethod]
		public void zero_partsPosition() => Assert.ThrowsException<ArgumentException>(()
			=> FileUtility.GetMultipartFileName("foo", 0, 2, "")
		);

		[TestMethod]
		public void negative_partsTotal() => Assert.ThrowsException<ArgumentException>(()
			=> FileUtility.GetMultipartFileName("foo", 2, -1, "")
		);
		[TestMethod]
		public void zero_partsTotal() => Assert.ThrowsException<ArgumentException>(()
			=> FileUtility.GetMultipartFileName("foo", 2, 0, "")
		);

		[TestMethod]
		public void partsPosition_greater_than_partsTotal() => Assert.ThrowsException<ArgumentException>(()
			=> FileUtility.GetMultipartFileName("foo", 2, 1, "")
		);

		[TestMethod]
		// only part
		[DataRow(@"C:\foo\bar\my file.txt", 1, 1, "title", @"C:\foo\bar\my file - 1 - title.txt")]
		// 3 digits
		[DataRow(@"C:\foo\bar\my file.txt", 2, 100, "title", @"C:\foo\bar\my file - 002 - title.txt")]
		// no suffix
		[DataRow(@"C:\foo\bar\my file.txt", 2, 100, "", @"C:\foo\bar\my file - 002 - .txt")]
		public void Tests(string originalPath, int partsPosition, int partsTotal, string suffix, string expected)
			=> FileUtility.GetMultipartFileName(originalPath, partsPosition, partsTotal, suffix).Should().Be(expected);
	}
}
