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
		static readonly ReplacementCharacters Barebones = ReplacementCharacters.Barebones;

		[TestMethod]
		public void null_path_throws() => Assert.ThrowsException<ArgumentNullException>(() => FileUtility.GetSafePath(null, Default));

		[TestMethod]
		// non-empty replacement
		[DataRow("abc*abc.txt", "abc✱abc.txt", PlatformID.Win32NT)]
		[DataRow("abc*abc.txt", "abc*abc.txt", PlatformID.Unix)]
		// standardize slashes. There is no unix equivalent because there is no alt directory separator
		[DataRow(@"a/b\c/d", @"a\b\c\d", PlatformID.Win32NT)]
		// remove illegal chars
		[DataRow("a*?:z.txt", "a✱？꞉z.txt", PlatformID.Win32NT)]
		[DataRow("a*?:z.txt", "a*?:z.txt", PlatformID.Unix)]
		// retain drive letter path colon
		[DataRow(@"C:\az.txt", @"C:\az.txt", PlatformID.Win32NT)]
		[DataRow(@"/:/az.txt", @"/:/az.txt", PlatformID.Unix)]
		// replace all other colons
		[DataRow(@"a\b:c\d.txt", @"a\b꞉c\d.txt", PlatformID.Win32NT)]
		[DataRow(@"a/b:c/d.txt", @"a/b:c/d.txt", PlatformID.Unix)]
		// remove empty directories
		[DataRow(@"C:\a\\\b\c\\\d.txt", @"C:\a\b\c\d.txt", PlatformID.Win32NT)]
		[DataRow(@"/a///b/c///d.txt", @"/a/b/c/d.txt", PlatformID.Unix)]
		[DataRow(@"C:\""foo\<id>", @"C:\“foo\＜id＞", PlatformID.Win32NT)]
		[DataRow(@"/""foo/<id>", @"/“foo/<id>", PlatformID.Unix)]
		public void DefaultTests(string inStr, string outStr, PlatformID platformID)
			=> Test(inStr, outStr, Default, platformID);

		[TestMethod]
		// non-empty replacement
		[DataRow("abc*abc.txt", "abc_abc.txt", PlatformID.Win32NT)]
		[DataRow("abc*abc.txt", "abc*abc.txt", PlatformID.Unix)]
		// standardize slashes. There is no unix equivalent because there is no alt directory separator
		[DataRow(@"a/b\c/d", @"a\b\c\d", PlatformID.Win32NT)]
		// remove illegal chars
		[DataRow("a*?:z.txt", "a__-z.txt", PlatformID.Win32NT)]
		[DataRow("a*?:z.txt", "a*?:z.txt", PlatformID.Unix)]
		// retain drive letter path colon
		[DataRow(@"C:\az.txt", @"C:\az.txt", PlatformID.Win32NT)]
		[DataRow(@"/:az.txt", @"/:az.txt", PlatformID.Unix)]
		// replace all other colons
		[DataRow(@"a\b:c\d.txt", @"a\b-c\d.txt", PlatformID.Win32NT)]
		[DataRow(@"a/b:c/d.txt", @"a/b:c/d.txt", PlatformID.Unix)]
		// remove empty directories
		[DataRow(@"C:\a\\\b\c\\\d.txt", @"C:\a\b\c\d.txt", PlatformID.Win32NT)]
		[DataRow(@"/a///b/c///d.txt", @"/a/b/c/d.txt", PlatformID.Unix)]
		[DataRow(@"C:\""foo\<id>", @"C:\'foo\{id}", PlatformID.Win32NT)]
		[DataRow(@"/""foo/<id>", @"/""foo/<id>", PlatformID.Unix)]
		public void LoFiDefaultTests(string inStr, string outStr, PlatformID platformID)
			=> Test(inStr, outStr, LoFiDefault, platformID);

		[TestMethod]
		// empty replacement
		[DataRow("abc*abc.txt", "abc_abc.txt", PlatformID.Win32NT)]
		[DataRow("abc*abc.txt", "abc*abc.txt", PlatformID.Unix)]
		// standardize slashes. There is no unix equivalent because there is no alt directory separator
		[DataRow(@"a/b\c/d", @"a\b\c\d", PlatformID.Win32NT)]
		// remove illegal chars
		[DataRow("a*?:z.txt", "a___z.txt", PlatformID.Win32NT)]
		[DataRow("a*?:z.txt", "a*?:z.txt", PlatformID.Unix)]
		// retain drive letter path colon
		[DataRow(@"C:\az.txt", @"C:\az.txt", PlatformID.Win32NT)]
		[DataRow(@"/:az.txt", @"/:az.txt", PlatformID.Unix)]
		// replace all other colons
		[DataRow(@"a\b:c\d.txt", @"a\b_c\d.txt", PlatformID.Win32NT)]
		[DataRow(@"a/b:c/d.txt", @"a/b:c/d.txt", PlatformID.Unix)]
		// remove empty directories
		[DataRow(@"C:\a\\\b\c\\\d.txt", @"C:\a\b\c\d.txt", PlatformID.Win32NT)]
		[DataRow(@"/a///b/c///d.txt", @"/a/b/c/d.txt", PlatformID.Unix)]
		[DataRow(@"C:\""foo\<id>", @"C:\_foo\_id_", PlatformID.Win32NT)]
		[DataRow(@"/""foo/<id>", @"/""foo/<id>", PlatformID.Unix)]
		public void BarebonesDefaultTests(string inStr, string outStr, PlatformID platformID)
			=> Test(inStr, outStr, Barebones, platformID);

		private void Test(string inStr, string outStr, ReplacementCharacters replacements, PlatformID platformID)
		{
			if (Environment.OSVersion.Platform == platformID)
				FileUtility.GetSafePath(inStr, replacements).PathWithoutPrefix.Should().Be(outStr);
		}
	}

	[TestClass]
	public class GetSafeFileName
	{
		static readonly ReplacementCharacters Default = ReplacementCharacters.Default;
		static readonly ReplacementCharacters LoFiDefault = ReplacementCharacters.LoFiDefault;
		static readonly ReplacementCharacters Barebones = ReplacementCharacters.Barebones;

		// needs separate method. middle null param not running correctly in TestExplorer when used in DataRow()
		[TestMethod]
		[DataRow("http://test.com/a/b/c", "http꞉∕∕test.com∕a∕b∕c", PlatformID.Win32NT)]
		[DataRow("http://test.com/a/b/c", "http:∕∕test.com∕a∕b∕c", PlatformID.Unix)]
		public void url_null_replacement(string inStr, string outStr, PlatformID platformID) => DefaultReplacementTest(inStr, outStr, platformID);

		[TestMethod]
		// empty replacement
		[DataRow("http://test.com/a/b/c", "http꞉∕∕test.com∕a∕b∕c", PlatformID.Win32NT)]
		[DataRow("http://test.com/a/b/c", "http:∕∕test.com∕a∕b∕c", PlatformID.Unix)]
		public void DefaultReplacementTest(string inStr, string outStr, PlatformID platformID) => Test(inStr, outStr, Default, platformID);

		[TestMethod]
		// empty replacement
		[DataRow("http://test.com/a/b/c", "http-__test.com_a_b_c", PlatformID.Win32NT)]
		[DataRow("http://test.com/a/b/c", "http:__test.com_a_b_c", PlatformID.Unix)]
		public void LoFiDefaultReplacementTest(string inStr, string outStr, PlatformID platformID) => Test(inStr, outStr, LoFiDefault, platformID);

		[TestMethod]
		// empty replacement
		[DataRow("http://test.com/a/b/c", "http___test.com_a_b_c", PlatformID.Win32NT)]
		[DataRow("http://test.com/a/b/c", "http:__test.com_a_b_c", PlatformID.Unix)]
		public void BarebonesDefaultReplacementTest(string inStr, string outStr, PlatformID platformID) => Test(inStr, outStr, Barebones, platformID);

		private void Test(string inStr, string outStr, ReplacementCharacters replacements, PlatformID platformID)
		{
			if (Environment.OSVersion.Platform == platformID)
				replacements.ReplaceFilenameChars(inStr).Should().Be(outStr);
		}
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
		[DataRow(@"C:\a bc\x y z\.f i l e.txt", "txt", PlatformID.Win32NT)]
		[DataRow(@"/a bc/x y z/.f i l e.txt", "txt", PlatformID.Unix)]
		// dot-folders
		[DataRow(@"C:\a bc\.x y z\f i l e.txt", "txt", PlatformID.Win32NT)]
		[DataRow(@"/a bc/.x y z/f i l e.txt", "txt", PlatformID.Unix)]
		public void Valid(string input, string extension, PlatformID platformID) => Tests(input, extension, input, platformID);

		[TestMethod]
		// folder spaces
		[DataRow(@"C:\   a bc   \x y z   ","", @"C:\a bc\x y z", PlatformID.Win32NT)]
		[DataRow(@"/   a bc   /x y z   ", "", @"/a bc/x y z", PlatformID.Unix)]
		// file spaces
		[DataRow(@"C:\a bc\x y z\   f i l e.txt   ", "txt", @"C:\a bc\x y z\f i l e.txt", PlatformID.Win32NT)]
		[DataRow(@"/a bc/x y z/   f i l e.txt   ", "txt", @"/a bc/x y z/f i l e.txt", PlatformID.Unix)]
		// eliminate beginning space and end dots and spaces
		[DataRow(@"C:\a bc\   . . . x y z . . .   \f i l e.txt", "txt", @"C:\a bc\. . . x y z\f i l e.txt", PlatformID.Win32NT)]
		[DataRow(@"/a bc/   . . . x y z . . .   /f i l e.txt", "txt", @"/a bc/. . . x y z/f i l e.txt", PlatformID.Unix)]
		// file end dots
		[DataRow(@"C:\a bc\x y z\f i l e.txt . . .", "txt", @"C:\a bc\x y z\f i l e.txt", PlatformID.Win32NT)]
		[DataRow(@"/a bc/x y z/f i l e.txt . . .", "txt", @"/a bc/x y z/f i l e.txt", PlatformID.Unix)]
		public void Tests(string input, string extension, string expected, PlatformID platformID)
		{
			if (Environment.OSVersion.Platform == platformID)
				FileUtility.GetValidFilename(input, Replacements, extension).PathWithoutPrefix.Should().Be(expected);
		}
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
