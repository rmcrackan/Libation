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
		[DataRow("http://test.com/a/b/c", @"http\test.com\a\b\c")]
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
		// replace all other colons
		[DataRow(@"a\b:c\d.txt", "ZZZ", @"a\bZZZc\d.txt")]
		// remove empty directories
		[DataRow(@"C:\a\\\b\c\\\d.txt", "ZZZ", @"C:\a\b\c\d.txt")]
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
	public class GetSequenceFormatted
	{
		[TestMethod]
		public void negative_partsPosition() => Assert.ThrowsException<ArgumentException>(()
			=> FileUtility.GetSequenceFormatted(-1, 2)
		);
		[TestMethod]
		public void zero_partsPosition() => Assert.ThrowsException<ArgumentException>(()
			=> FileUtility.GetSequenceFormatted(0, 2)
		);

		[TestMethod]
		public void negative_partsTotal() => Assert.ThrowsException<ArgumentException>(()
			=> FileUtility.GetSequenceFormatted(2, -1)
		);
		[TestMethod]
		public void zero_partsTotal() => Assert.ThrowsException<ArgumentException>(()
			=> FileUtility.GetSequenceFormatted(2, 0)
		);

		[TestMethod]
		public void partsPosition_greater_than_partsTotal() => Assert.ThrowsException<ArgumentException>(()
			=> FileUtility.GetSequenceFormatted(2, 1)
		);

		[TestMethod]
		// only part
		[DataRow(1, 1, "1")]
		// 2 digits
		[DataRow(2, 90, "02")]
		// 3 digits
		[DataRow(2, 900, "002")]
		public void Tests(int partsPosition, int partsTotal, string expected)
			=> FileUtility.GetSequenceFormatted(partsPosition, partsTotal).Should().Be(expected);
	}

	[TestClass]
	public class GetStandardizedExtension
	{
		[TestMethod]
		public void is_null() => Tests(null, "");

		[TestMethod]
		public void is_empty() => Tests("", "");

		[TestMethod]
		public void is_whitespace() => Tests("   ", "");

		[TestMethod]
		[DataRow("txt", ".txt")]
		[DataRow(".txt", ".txt")]
		[DataRow("   .txt   ", ".txt")]
		public void Tests(string input, string expected)
			=> FileUtility.GetStandardizedExtension(input).Should().Be(expected);
	}
}
