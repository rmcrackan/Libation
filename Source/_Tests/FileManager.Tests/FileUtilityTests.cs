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
		static readonly ReplacementCharacters Default = ReplacementCharacters.Default;
		static readonly ReplacementCharacters LoFiDefault = ReplacementCharacters.LoFiDefault;
		static readonly ReplacementCharacters Barebones = ReplacementCharacters.Minimum;

		[TestMethod]
		public void null_path_throws() => Assert.ThrowsException<ArgumentNullException>(() => FileUtility.GetSafePath(null, Default));

		[TestMethod]
		// non-empty replacement
		[DataRow("abc*abc.txt", "abc✱abc.txt")]
		// standardize slashes
		[DataRow(@"a/b\c/d", @"a\b\c\d")]
		// remove illegal chars
		[DataRow("a*?:z.txt", "a✱？꞉z.txt")]
		// retain drive letter path colon
		[DataRow(@"C:\az.txt", @"C:\az.txt")]
		// replace all other colons
		[DataRow(@"a\b:c\d.txt", @"a\b꞉c\d.txt")]
		// remove empty directories
		[DataRow(@"C:\a\\\b\c\\\d.txt",  @"C:\a\b\c\d.txt")]
		[DataRow(@"C:\""foo\<id>", @"C:\“foo\＜id＞")]
		public void DefaultTests(string inStr, string outStr) => Assert.AreEqual(outStr, FileUtility.GetSafePath(inStr, Default).PathWithoutPrefix);

		[TestMethod]
		// non-empty replacement
		[DataRow("abc*abc.txt", "abc_abc.txt")]
		// standardize slashes
		[DataRow(@"a/b\c/d", @"a\b\c\d")]
		// remove illegal chars
		[DataRow("a*?:z.txt", "a__-z.txt")]
		// retain drive letter path colon
		[DataRow(@"C:\az.txt", @"C:\az.txt")]
		// replace all other colons
		[DataRow(@"a\b:c\d.txt", @"a\b-c\d.txt")]
		// remove empty directories
		[DataRow(@"C:\a\\\b\c\\\d.txt",  @"C:\a\b\c\d.txt")]
		[DataRow(@"C:\""foo\<id>", @"C:\'foo\{id}")]
		public void LoFiDefaultTests(string inStr, string outStr) => Assert.AreEqual(outStr, FileUtility.GetSafePath(inStr, LoFiDefault).PathWithoutPrefix);

		[TestMethod]
		// empty replacement
		[DataRow("abc*abc.txt", "abc_abc.txt")]
		// standardize slashes
		[DataRow(@"a/b\c/d", @"a\b\c\d")]
		// remove illegal chars
		[DataRow("a*?:z.txt", "a___z.txt")]
		// retain drive letter path colon
		[DataRow(@"C:\az.txt", @"C:\az.txt")]
		// replace all other colons
		[DataRow(@"a\b:c\d.txt", @"a\b_c\d.txt")]
		// remove empty directories
		[DataRow(@"C:\a\\\b\c\\\d.txt",  @"C:\a\b\c\d.txt")]
		[DataRow(@"C:\""foo\<id>", @"C:\_foo\_id_")]
		public void BarebonesDefaultTests(string inStr, string outStr) => Assert.AreEqual(outStr, FileUtility.GetSafePath(inStr, Barebones).PathWithoutPrefix);
	}

	[TestClass]
	public class GetSafeFileName
	{
		static readonly ReplacementCharacters Default = ReplacementCharacters.Default;
		static readonly ReplacementCharacters LoFiDefault = ReplacementCharacters.LoFiDefault;
		static readonly ReplacementCharacters Barebones = ReplacementCharacters.Minimum;

		// needs separate method. middle null param not running correctly in TestExplorer when used in DataRow()
		[TestMethod]
		[DataRow("http://test.com/a/b/c", "http꞉∕∕test.com∕a∕b∕c")]
		public void url_null_replacement(string inStr, string outStr) => DefaultReplacementTest(inStr, outStr);

		[TestMethod]
		// empty replacement
		[DataRow("http://test.com/a/b/c", "http꞉∕∕test.com∕a∕b∕c")]
		public void DefaultReplacementTest(string inStr, string outStr) => Default.ReplaceInvalidFilenameChars(inStr).Should().Be(outStr);

		[TestMethod]
		// empty replacement
		[DataRow("http://test.com/a/b/c", "http-__test.com_a_b_c")] 
		public void LoFiDefaultReplacementTest(string inStr, string outStr) => LoFiDefault.ReplaceInvalidFilenameChars(inStr).Should().Be(outStr);

		[TestMethod]
		// empty replacement
		[DataRow("http://test.com/a/b/c", "http___test.com_a_b_c")] 
		public void BarebonesDefaultReplacementTest(string inStr, string outStr) => Barebones.ReplaceInvalidFilenameChars(inStr).Should().Be(outStr);
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

	[TestClass]
    public class GetValidFilename
	{
		static ReplacementCharacters Replacements = ReplacementCharacters.Default;

		[TestMethod]
		// dot-files
		[DataRow(@"C:\a bc\x y z\.f i l e.txt")]
		// dot-folders
		[DataRow(@"C:\a bc\.x y z\f i l e.txt")]
		public void Valid(string input) => Tests(input, input);

		[TestMethod]
		// folder spaces
		[DataRow(@"C:\   a bc   \x y z   ", @"C:\a bc\x y z")]
		// file spaces
		[DataRow(@"C:\a bc\x y z\   f i l e.txt   ", @"C:\a bc\x y z\f i l e.txt")]
		// eliminate beginning space and end dots and spaces
		[DataRow(@"C:\a bc\   . . . x y z . . .   \f i l e.txt", @"C:\a bc\. . . x y z\f i l e.txt")]
		// file end dots
		[DataRow(@"C:\a bc\x y z\f i l e.txt . . .", @"C:\a bc\x y z\f i l e.txt")]
		public void Tests(string input, string expected)
			=> FileUtility.GetValidFilename(input, Replacements).PathWithoutPrefix.Should().Be(expected);
	}

	[TestClass]
    public class RemoveLastCharacter
	{
		[TestMethod]
		public void is_null() => Tests(null, null);

		[TestMethod]
		public void empty() => Tests("", "");

		[TestMethod]
		public void single_space() => Tests(" ", "");

		[TestMethod]
		public void multiple_space() => Tests("   ", "  ");

		[TestMethod]
		[DataRow("1", "")]
		[DataRow("1 ", "1")]
		[DataRow("12", "1")]
		[DataRow("123", "12")]
		public void Tests(string input, string expected)
			=> FileUtility.RemoveLastCharacter(input).Should().Be(expected);
	}
}
