using AaxDecrypter;
using AssertionHelper;
using FileManager;
using FileManager.NamingTemplate;
using LibationFileManager.Templates;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using static TemplatesTests.Shared;

[assembly: Parallelize]

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
		static Shared()
		{
			LibationFileManager.Configuration.CreateMockInstance().Books = Path.GetFullPath("Books");
		}

		public static LibraryBookDto GetLibraryBook()
			=> GetLibraryBook([new SeriesDto("Sherlock Holmes", "1", "B08376S3R2")]);

		public static LibraryBookDto GetLibraryBook(IEnumerable<SeriesDto>? series)
			=> new()
			{
				Account = "myaccount@example.co",
				AccountNickname = "my account",
				DateAdded = new DateTime(2022, 6, 9, 0, 0, 0),
				DatePublished = new DateTime(2017, 2, 27, 0, 0, 0),
				FileDate = new DateTime(2023, 1, 28, 0, 0, 0),
				AudibleProductId = "asin",
				Title = "A Study in Scarlet: A Sherlock Holmes Novel",
				Locale = new LocaleDto("us"),
				YearPublished = null, // explicitly null
				Authors = [new("Arthur Conan Doyle", "B000AQ43GQ"), new("Stephen Fry - introductions", "B000APAGVS")],
				Narrators = [], // explicitly empty list
				Series = series,
				BitRate = 128,
				SampleRate = 44100,
				Channels = 2,
				Language = new CultureInfoDto("English"),
				Subtitle = "An Audible Original Drama",
				TitleWithSubtitle = "A Study in Scarlet: An Audible Original Drama",
				Codec = @"AAC[LC]\MP3", // special chars added
				FileVersion = null, // explicitly null
				LibationVersion = "", // explicitly empty string
				LengthInMinutes = TimeSpan.FromMinutes(100),
				IsAbridged = true,
				Tags = [new StringDto("Tag1"), new StringDto("Tag2"), new StringDto("Tag3")],
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
	public class GetFileNamingTemplate
	{
		static readonly ReplacementCharacters Replacements = ReplacementCharacters.Default(Environment.OSVersion.Platform == PlatformID.Win32NT);

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
		[DataRow("<year> - <channels>", @"C:\foo\bar", ".ext", @"C:\foo\bar\- 2.ext")]
		[DataRow("(000.0) <year> - <channels>", @"C:\foo\bar", "ext", @"C:\foo\bar\(000.0)  - 2.ext")]
		public void Tests(string template, string dirFullPath, string extension, string expected)
		{
			if (Environment.OSVersion.Platform is not PlatformID.Win32NT)
			{
				dirFullPath = dirFullPath.Replace("C:", "").Replace('\\', '/');
				expected = expected.Replace("C:", "").Replace('\\', '/');
			}

			Templates.TryGetTemplate<Templates.FileTemplate>(template, out var fileTemplate).Should().BeTrue();

			fileTemplate
				.GetFilename(GetLibraryBook(), dirFullPath, extension, culture: null, replacements: Replacements)
				.PathWithoutPrefix
				.Should().Be(expected);
		}

		[TestMethod]
		[DataRow("<samplerate>", "", "", "100")]
		[DataRow(" <samplerate> ", "", "", "100")]
		[DataRow("4<samplerate>4", "", "", "100")]
		[DataRow("<bitrate>   -   <bitrate>", "", "", "1 8 - 1 8")]
		[DataRow("<bitrate>   42   <bitrate>", "", "", "1 8 1 8")]
		[DataRow(" <bitrate> - <bitrate> ", "", "", "1 8 - 1 8")]
		[DataRow("4<bitrate> - <bitrate> 4", "", "", "1 8 - 1 8")]
		[DataRow("<channels><channels><samplerate><channels><channels>", "", "", "100")]
		[DataRow(" <channels> <channels> <samplerate> <channels> <channels>", "", "", "100")]
		[DataRow(" <channels> - <channels> <samplerate> <channels> - <channels>", "", "", "- 100 -")]

		public void Tests_removeSpaces(string template, string dirFullPath, string extension, string expected)
		{
			if (Environment.OSVersion.Platform is not PlatformID.Win32NT)
			{
				dirFullPath = dirFullPath.Replace("C:", "").Replace('\\', '/');
				expected = expected.Replace("C:", "").Replace('\\', '/');
			}
			var replacements
				= new ReplacementCharacters
				{
					Replacements = Replacements.Replacements
					.Append(new Replacement('4', "  ", ""))
					.Append(new Replacement('2', "  ", ""))
					.ToArray()
				};

			Templates.TryGetTemplate<Templates.FileTemplate>(template, out var fileTemplate).Should().BeTrue();

			fileTemplate
				.GetFilename(GetLibraryBook(), dirFullPath, extension, culture: null, replacements: replacements)
				.PathWithoutPrefix
				.Should().Be(expected);
		}

		[TestMethod]
		[DataRow("<bitrate>Kbps <samplerate>Hz", "128Kbps 44100Hz")]
		[DataRow("<bitrate>Kbps <samplerate[6]>Hz", "128Kbps 044100Hz")]
		[DataRow("<bitrate[1]>Kbps <samplerate>Hz", "128Kbps 44100Hz")]
		[DataRow("<bitrate[2]>Kbps <titleshort[u]>", "128Kbps A STUDY IN SCARLET")]
		[DataRow("<bitrate[3]>Kbps <titleshort[t]>", "128Kbps A Study In Scarlet")]
		[DataRow("<bitrate[4]>Kbps <titleshort[l]>", "0128Kbps a study in scarlet")]
		[DataRow(@"<bitrate[00'['0\#0']']>Kbps <titleshort[T]>", "01[2#8]Kbps A Study In Scarlet")]
		[DataRow("<codec[7t]> <samplerate[6]>Hz", "Aac[Lc] 044100Hz")]
		[DataRow("<codec[3T]> <titleshort[ 5 U ]>", "AAC A STU")]
		[DataRow("<bitrate  [ 4 ]  >Kbps <samplerate   [  6  ]   >Hz", "0128Kbps 044100Hz")]
		public void FormatTags(string template, string expected)
		{
			Templates.TryGetTemplate<Templates.FileTemplate>(template, out var fileTemplate).Should().BeTrue();
			fileTemplate.GetFilename(GetLibraryBook(), "", "", culture: null, replacements: Replacements).PathWithoutPrefix.Should().Be(expected);
		}

		[TestMethod]
		[DataRow("<narrator>", "")]
		[DataRow("<narrator[format({L})]>", "")]
		[DataRow("<first narrator>", "")]
		[DataRow("<file version>", "")]
		[DataRow("<libation version>", "")]
		[DataRow("<year>", "")]
		public void EmptyFields(string template, string expected)
		{
			var bookDto = GetLibraryBook();

			Templates.TryGetTemplate<Templates.FileTemplate>(template, out var fileTemplate).Should().BeTrue();
			fileTemplate.GetFilename(bookDto, "", "", Replacements).PathWithoutPrefix.Should().Be(expected);
		}

		[TestMethod]
		[DataRow("<minutes>", 100, "100")]
		[DataRow("<minutes[M]>", 100, "100")]
		[DataRow("<minutes[MM]>", 100, "100")]
		[DataRow(@"<minutes[H\-m]>", 100, "1-40")]
		[DataRow(@"<minutes[hh\-MM]>", 100, "01-100")]
		[DataRow(@"<minutes[%m\ m\ mm]>", 100, "40 40 40")]
		[DataRow(@"<minutes[\%M\ M\ MM]>", 100, "%0 1 00")]
		[DataRow(@"<minutes[D\.hh\-MM]>", 100, "0.01-100")]
		[DataRow(@"<minutes[dd\dhh\hmm\m]>", 100, "00d01h40m")]
		[DataRow("""<minutes[d'[days], 'h"(hours), "m'{minutes}']>""", 100, "0[days], 1(hours), 40{minutes}")]
		[DataRow(@"<minutes[H\-M]>", 2000, "33-20")]
		[DataRow(@"<minutes[DDD\-HHH\-MMM]>", 2000, "001-009-020")]
		[DataRow(@"<minutes[M\-H\-D]>", 2000, "20-9-1")]
		[DataRow(@"<minutes[D\-M]>", 100, "0-100")]
		[DataRow(@"<minutes[D\-M]>", 1500, "1-60")]
		[DataRow(@"<minutes[D\-M]>", 2000, "1-560")]
		[DataRow(@"<minutes[D\-M]>", 2880, "2-0")]
		[DataRow(@"<minutes[DD\-MM]>", 1500, "01-60")]
		[DataRow(@"<minutes[D\-MMM'{'MM\}]>", 2000, "1-005{60}")]
		public void MinutesFormat(string template, int minutes, string expected)
		{
			var bookDto = GetLibraryBook();
			bookDto.LengthInMinutes = TimeSpan.FromMinutes(minutes);

			Templates.TryGetTemplate<Templates.FileTemplate>(template, out var fileTemplate).Should().BeTrue();
			fileTemplate.GetFilename(bookDto, "", "", Replacements).PathWithoutPrefix.Should().Be(expected);
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
				.GetFilename(GetLibraryBook(), dirFullPath, extension, culture: null, replacements: Replacements)
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
				.GetFilename(GetLibraryBook(), dirFullPath, extension, culture: null, replacements: Replacements)
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
				.GetFilename(GetLibraryBook(), dirFullPath, extension, culture: null, replacements: Replacements)
				.PathWithoutPrefix
				.Should().Be(expected);
		}

		[TestMethod]
		[DataRow("<id> - <pubdate[MM/dd/yy HH:mm]>", @"C:\foo\bar", ".m4b", @"C:\foo\bar\asin - 02∕27∕17 00_00.m4b", PlatformID.Win32NT)]
		[DataRow("<id> - <pubdate[MM/dd/yy HH:mm]>", @"/foo/bar", ".m4b", @"/foo/bar/asin - 02∕27∕17 00:00.m4b", PlatformID.Unix)]
		[DataRow("<id> - <filedate[MM/dd/yy HH:mm]>", @"C:\foo\bar", ".m4b", @"C:\foo\bar\asin - 01∕28∕23 00_00.m4b", PlatformID.Win32NT)]
		[DataRow("<id> - <filedate[MM/dd/yy HH:mm]>", @"/foo/bar", ".m4b", @"/foo/bar/asin - 01∕28∕23 00:00.m4b", PlatformID.Unix)]
		[DataRow("<id> - <date added[MM/dd/yy HH:mm]>", @"C:\foo\bar", ".m4b", @"C:\foo\bar\asin - 06∕09∕22 00_00.m4b", PlatformID.Win32NT)]
		[DataRow("<id> - <date added[MM/dd/yy HH:mm]>", @"/foo/bar", ".m4b", @"/foo/bar/asin - 06∕09∕22 00:00.m4b", PlatformID.Unix)]
		public void DateFormat_illegal(string template, string dirFullPath, string extension, string expected, PlatformID platformId)
		{
			if (Environment.OSVersion.Platform != platformId) Assert.Inconclusive($"Skipped because OS is not {platformId}.");

			Templates.TryGetTemplate<Templates.FileTemplate>(template, out var fileTemplate).Should().BeTrue();

			fileTemplate.HasWarnings.Should().BeFalse();
			fileTemplate
				.GetFilename(GetLibraryBook(), dirFullPath, extension, culture: CultureInfo.InvariantCulture, replacements: Replacements)
				.PathWithoutPrefix
				.Should().Be(expected);
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
				.GetFilename(lbDto, dirFullPath, extension, culture: null, replacements: Replacements)
				.PathWithoutPrefix
				.Should().Be(expected);
		}

		[TestMethod]
		[DataRow("Bruce Bueno de Mesquita", "Title=, First=Bruce, Middle=Bueno Last=de Mesquita, Suffix=")]
		[DataRow("Ramon de Ocampo", "Title=, First=Ramon, Middle= Last=de Ocampo, Suffix=")]
		[DataRow("Ramon De Ocampo", "Title=, First=Ramon, Middle= Last=De Ocampo, Suffix=")]
		[DataRow("Jennifer Van Dyck", "Title=, First=Jennifer, Middle= Last=Van Dyck, Suffix=")]
		[DataRow("Carla Naumburg PhD", "Title=, First=Carla, Middle= Last=Naumburg, Suffix=PhD")]
		[DataRow("Doug Stanhope and Friends", "Title=, First=Doug, Middle= Last=Stanhope and Friends, Suffix=")]
		[DataRow("Tamara Lovatt-Smith", "Title=, First=Tamara, Middle= Last=Lovatt-Smith, Suffix=")]
		[DataRow("Common", "Title=, First=, Middle= Last=Common, Suffix=")]
		[DataRow("Doug Tisdale Jr.", "Title=, First=Doug, Middle= Last=Tisdale, Suffix=Jr")]
		[DataRow("Robert S. Mueller III", "Title=, First=Robert, Middle=S. Last=Mueller, Suffix=III")]
		[DataRow("Frank T Vertosick Jr. MD", "Title=, First=Frank, Middle=T Last=Vertosick, Suffix=Jr. MD")]
		[DataRow("The Arabian Nights", "Title=, First=The Arabian, Middle= Last=Nights, Suffix=")]
		[DataRow("The Great Courses", "Title=, First=The Great, Middle= Last=Courses, Suffix=")]
		[DataRow("The Laurie Berkner Band", "Title=, First=The Laurie, Middle=Berkner Last=Band, Suffix=")]
		[DataRow("Committee on Foreign Affairs", "Title=, First=Committee, Middle=on Last=Foreign Affairs, Suffix=")]
		[DataRow("House Permanent Select Committee on Intelligence", "Title=, First=House, Middle=Permanent Select Committee on Last=Intelligence, Suffix=")]
		[DataRow("Professor David K. Johnson PhD University of Oklahoma", "Title=Professor, First=David, Middle=K. Johnson PhD Last=University of Oklahoma, Suffix=")]
		[DataRow("Festival of the Spoken Nerd", "Title=, First=Festival of the Spoken, Middle= Last=Nerd, Suffix=")]
		[DataRow("Audible Original", "Title=, First=Audible, Middle= Last=Original, Suffix=")]
		[DataRow("Audible Originals", "Title=, First=Audible, Middle= Last=Originals, Suffix=")]
		[DataRow("Patrick O'Brian", "Title=, First=Patrick, Middle= Last=O'Brian, Suffix=")]
		[DataRow("Patrick O’Connell", "Title=, First=Patrick, Middle= Last=O'Connell, Suffix=")]
		[DataRow("L.E. Modesitt", "Title=, First=L.E., Middle= Last=Modesitt, Suffix=")]
		[DataRow("L. E. Modesitt Jr.", "Title=, First=L., Middle=E. Last=Modesitt, Suffix=Jr")]
		[DataRow("LE Modesitt, Jr.", "Title=, First=LE, Middle= Last=Modesitt, Suffix=Jr")]
		[DataRow("Marine Le Pen", "Title=, First=Marine, Middle= Last=Le Pen, Suffix=")]
		[DataRow("L. Sprague de Camp", "Title=, First=L., Middle=Sprague Last=de Camp, Suffix=")]
		[DataRow("Lt. Col. - Ret. Douglas L. Bland", "Title=, First=Ret., Middle=Douglas L. Bland Last=Lt. Col., Suffix=")]
		[DataRow("Col. Lee Ellis - Ret. - foreword", "Title=Col., First=Lee, Middle= Last=Ellis, Suffix=Ret")]
		public void NameFormat_unusual(string author, string expected)
		{
			var bookDto = GetLibraryBook();
			bookDto.Authors = [new(author, null)];
			Templates.TryGetTemplate<Templates.FileTemplate>("<author[format(Title={T}, First={F}, Middle={M} Last={L}, Suffix={S})]>", out var fileTemplate).Should().BeTrue();
			fileTemplate
				.GetFilename(bookDto, "", "", culture: null, replacements: Replacements)
				.PathWithoutPrefix
				.Should().Be(expected);
		}

		[TestMethod]
		[DataRow("<author>", "Jill Conner Browne, Charles E. Gannon, Christopher John Fetherolf, Lucy Maud Montgomery, Jon Bon Jovi, Paul Van Doren, Emma Gannon")]
		[DataRow("<author[]>", "Jill Conner Browne, Charles E. Gannon, Christopher John Fetherolf, Lucy Maud Montgomery, Jon Bon Jovi, Paul Van Doren, Emma Gannon")]
		[DataRow("<author[sort(F)]>", "Charles E. Gannon, Christopher John Fetherolf, Emma Gannon, Jill Conner Browne, Jon Bon Jovi, Lucy Maud Montgomery, Paul Van Doren")]
		[DataRow("<author[sort(M)]>", "Jon Bon Jovi, Paul Van Doren, Emma Gannon, Jill Conner Browne, Charles E. Gannon, Christopher John Fetherolf, Lucy Maud Montgomery")]
		[DataRow("<author[sort(L)]>", "Jon Bon Jovi, Jill Conner Browne, Christopher John Fetherolf, Charles E. Gannon, Emma Gannon, Lucy Maud Montgomery, Paul Van Doren")]
		[DataRow("<author[sort(f)]>", "Paul Van Doren, Lucy Maud Montgomery, Jon Bon Jovi, Jill Conner Browne, Emma Gannon, Christopher John Fetherolf, Charles E. Gannon")]
		[DataRow("<author[sort(m)]>", "Lucy Maud Montgomery, Christopher John Fetherolf, Charles E. Gannon, Jill Conner Browne, Jon Bon Jovi, Paul Van Doren, Emma Gannon")]
		[DataRow("<author[sort(l)]>", "Paul Van Doren, Lucy Maud Montgomery, Charles E. Gannon, Emma Gannon, Christopher John Fetherolf, Jill Conner Browne, Jon Bon Jovi")]
		[DataRow("<author  [  max(  1  )  ]>", "Jill Conner Browne")]
		[DataRow("<author[max(2)]>", "Jill Conner Browne, Charles E. Gannon")]
		[DataRow("<author[max(3)]>", "Jill Conner Browne, Charles E. Gannon, Christopher John Fetherolf")]
		[DataRow("<author[slice(3)]>", "Christopher John Fetherolf")]
		[DataRow("<author[slice(3...5)]>", "Christopher John Fetherolf, Lucy Maud Montgomery, Jon Bon Jovi")]
		[DataRow("<author[slice(-2)]>", "Paul Van Doren")]
		[DataRow("<author[slice(-3..-2)]>", "Jon Bon Jovi, Paul Van Doren")]
		[DataRow("<author[sort(LF) slice(4..5)]>", "Charles E. Gannon, Emma Gannon")]
		[DataRow("<author[sort(Lf) slice(4..5)]>", "Emma Gannon, Charles E. Gannon")]
		[DataRow("<author[format({L}, {F})]>", "Browne, Jill, Gannon, Charles, Fetherolf, Christopher, Montgomery, Lucy, Bon Jovi, Jon, Van Doren, Paul, Gannon, Emma")]
		[DataRow("<author[format({L}, {F} {ID})]>", "Browne, Jill B1, Gannon, Charles B2, Fetherolf, Christopher B3, Montgomery, Lucy B4, Bon Jovi, Jon B5, Van Doren, Paul B6, Gannon, Emma B7")]
		[DataRow("<author[format({ID})]>", "B1, B2, B3, B4, B5, B6, B7")]
		[DataRow("<author[format({Id})]>", "Jill Conner Browne, Charles E. Gannon, Christopher John Fetherolf, Lucy Maud Montgomery, Jon Bon Jovi, Paul Van Doren, Emma Gannon")]
		[DataRow("<author[format({iD})]>", "Jill Conner Browne, Charles E. Gannon, Christopher John Fetherolf, Lucy Maud Montgomery, Jon Bon Jovi, Paul Van Doren, Emma Gannon")]
		[DataRow("<author[format({id})]>", "Jill Conner Browne, Charles E. Gannon, Christopher John Fetherolf, Lucy Maud Montgomery, Jon Bon Jovi, Paul Van Doren, Emma Gannon")]
		[DataRow("<author[format({f}, {l})]>", "Jill Conner Browne, Charles E. Gannon, Christopher John Fetherolf, Lucy Maud Montgomery, Jon Bon Jovi, Paul Van Doren, Emma Gannon")]
		[DataRow("<author[format(First={F}, Last={L})]>",
			"First=Jill, Last=Browne, First=Charles, Last=Gannon, First=Christopher, Last=Fetherolf, First=Lucy, Last=Montgomery, First=Jon, Last=Bon Jovi, First=Paul, Last=Van Doren, First=Emma, Last=Gannon")]
		[DataRow("<author[format({L}, {F}) separator( - ) max(3)]>", "Browne, Jill - Gannon, Charles - Fetherolf, Christopher")]
		[DataRow("<author[sort(F) max(2) separator(; ) format({F})]>", "Charles; Christopher")]
		[DataRow("<author[sort(L) max(2) separator(; ) format({L})]>", "Bon Jovi; Browne")]
		//Jon Bon Jovi and Paul Van Doren don't have middle names, so they are sorted to the top.
		//Since only the middle names of the first 2 names are to be displayed, the name string is empty.
		[DataRow("<author[sort(M) max(2) separator(; ) format({M})]>", ";")]
		[DataRow("<first author>", "Jill Conner Browne")]
		[DataRow("<first author[]>", "Jill Conner Browne")]
		[DataRow("<first author[{L}, {F}]>", "Browne, Jill")]
		public void NameFormat_formatters(string template, string expected)
		{
			var bookDto = GetLibraryBook();
			bookDto.Authors =
			[
				new("Jill Conner Browne", "B1"),
				new("Charles E. Gannon", "B2"),
				new("Christopher John Fetherolf", "B3"),
				new("Lucy Maud Montgomery", "B4"),
				new("Jon Bon Jovi", "B5"),
				new("Paul Van Doren", "B6"),
				new("Emma Gannon", "B7"),
			];

			Templates.TryGetTemplate<Templates.FileTemplate>(template, out var fileTemplate).Should().BeTrue();
			fileTemplate
				.GetFilename(bookDto, "", "", culture: null, replacements: Replacements)
				.PathWithoutPrefix
				.Should().Be(expected);
		}

		[TestMethod]
		[DataRow("<has libation version->empty-string<-has>", "")]
		[DataRow("<!has libation version->empty-string<-has>", "empty-string")]
		[DataRow("<is libation version[=foobar]->empty-string<-is>", "")]
		[DataRow("<!is libation version[=foobar]->empty-string<-is>", "empty-string")]
		[DataRow("<is libation version[=]->empty-string<-is>", "empty-string")]
		[DataRow("<is libation version[#=0]->empty-string<-is>", "empty-string")]
		[DataRow("<is libation version[]->empty-string<-is>", "empty-string")]
		[DataRow("<has file version->null-string<-has>", "")]
		[DataRow("<!has file version->null-string<-has>", "null-string")]
		[DataRow("<is file version[=foobar]->null-string<-is>", "")]
		[DataRow("<is file version[=]->null-string<-is>", "")]
		[DataRow("<!is file version[=]->null-string<-is>", "null-string")]
		[DataRow("<is file version[#=0]->null-string<-is>", "")]
		[DataRow("<is file version[]->null-string<-is>", "")]
		[DataRow("<has year->null-int<-has>", "")]
		[DataRow("<is year[=]->null-int<-is>", "")]
		[DataRow("<is year[#=0]->null-int<-is>", "")]
		[DataRow("<is year[0]->null-int<-is>", "")]
		[DataRow("<!is year[0]->null-int<-is>", "null-int")]
		[DataRow("<is year[]->null-int<-is>", "")]
		[DataRow("<has FAKE->unknown-tag<-has>", "")]
		[DataRow("<is FAKE[=]->unknown-tag<-is>", "")]
		[DataRow("<!is FAKE[=]->unknown-tag<-is>", "unknown-tag")]
		[DataRow("<is FAKE[=foobar]->unknown-tag<-is>", "")]
		[DataRow("<is FAKE[#=0]->unknown-tag<-is>", "")]
		[DataRow("<is FAKE[]->unknown-tag<-is>", "")]
		[DataRow("<has narrator->empty-list<-has>", "")]
		[DataRow("<is narrator[=foobar]->empty-list<-is>", "")]
		[DataRow("<!is narrator[=foobar]->empty-list<-is>", "empty-list")]
		[DataRow("<is narrator[!=foobar]->empty-list<-is>", "")]
		[DataRow("<!is narrator[!=foobar]->empty-list<-is>", "empty-list")]
		[DataRow("<is narrator[=]->empty-list<-is>", "")]
		[DataRow("<is narrator[~.*]->empty-list<-is>", "")]
		[DataRow("<is narrator[<1]->empty-list<-is>", "empty-list")]
		[DataRow("<is narrator[#=0]->empty-list<-is>", "empty-list")]
		[DataRow("<is narrator[]->empty-list<-is>", "")]
		[DataRow("<is first narrator->no-first<-is>", "")]
		[DataRow("<is first narrator[=foobar]->no-first<-is>", "")]
		[DataRow("<is first narrator[=]->no-first<-is>", "")]
		[DataRow("<is first narrator[#=0]->no-first<-is>", "")]
		[DataRow("<is first narrator[]->no-first<-is>", "")]
		public void HasValue_on_empty_test(string template, string expected)
		{
			var bookDto = GetLibraryBook();
			var multiDto = new MultiConvertFileProperties
			{
				PartsPosition = 1,
				PartsTotal = 2,
				Title = bookDto.Title,
				OutputFileName = "outputfile.m4b"
			};

			Templates.TryGetTemplate<Templates.FileTemplate>(template, out var fileTemplate).Should().BeTrue();
			fileTemplate
				.GetFilename(bookDto, multiDto, "", "", culture: null, replacements: Replacements)
				.PathWithoutPrefix
				.Should().Be(expected);
			fileTemplate.Errors.Should().HaveCount(0);
			fileTemplate.Warnings.Should().HaveCount(1); // "Should use tags. Eg: <title>"
		}

		[TestMethod]
		[DataRow("<has id->true<-has>", "true")]
		[DataRow("<!has id->false<-has>", "")]
		[DataRow("<has title->true<-has>", "true")]
		[DataRow("<has title short->true<-has>", "true")]
		[DataRow("<has audible title->true<-has>", "true")]
		[DataRow("<has audible subtitle->true<-has>", "true")]
		[DataRow("<has author->true<-has>", "true")]
		[DataRow("<!has author->false<-has>", "")]
		[DataRow("<has first author->true<-has>", "true")]
		[DataRow("<has series->true<-has>", "true")]
		[DataRow("<has first series->true<-has>", "true")]
		[DataRow("<has series#->true<-has>", "true")]
		[DataRow("<has bitrate->true<-has>", "true")]
		[DataRow("<has samplerate->true<-has>", "true")]
		[DataRow("<has channels->true<-has>", "true")]
		[DataRow("<has codec->true<-has>", "true")]
		[DataRow(@"<is codec[=aac\[lc\]\\mp3]->true<-is>", "true")]
		[DataRow(@"<is codec[=aac\[lc\]\\mp4]->true<-is>", "")]
		[DataRow("<has account->true<-has>", "true")]
		[DataRow("<has account nickname->true<-has>", "true")]
		[DataRow("<has locale->true<-has>", "true")]
		[DataRow("<has language->true<-has>", "true")]
		[DataRow("<has language short->true<-has>", "true")]
		[DataRow("<has file date->true<-has>", "true")]
		[DataRow("<has pub date->true<-has>", "true")]
		[DataRow("<has date added->true<-has>", "true")]
		[DataRow("<has tag->true<-has>", "true")]
		[DataRow("<has first tag->true<-has>", "true")]
		[DataRow("<!has first tag->false<-has>", "")]
		[DataRow("<has ch count->true<-has>", "true")]
		[DataRow("<has ch title->true<-has>", "true")]
		[DataRow("<has ch#->true<-has>", "true")]
		[DataRow("<has ch# 0->true<-has>", "true")]
		[DataRow("<is title[=A Study in Scarlet: An Audible Original Drama]->true<-is>", "true")]
		[DataRow("<!is title[=A Study in Scarlet: An Audible Original Drama]->false<-is>", "")]
		[DataRow("<is title[U][=A STUDY IN SCARLET: AN AUDIBLE ORIGINAL DRAMA]->true<-is>", "true")]
		[DataRow("<is title[#=45]->true<-is>", "true")]
		[DataRow("<is title[!=foo]->true<-is>", "true")]
		[DataRow("<!is title[!=foo]->false<-is>", "")]
		[DataRow("<is title[~A Study.*]->true<-is>", "true")]
		[DataRow("<is title[foo]->true<-is>", "")]
		[DataRow("<is ch count[>=1]->true<-is>", "true")]
		[DataRow("<is ch count[>1]->true<-is>", "true")]
		[DataRow("<is ch count[<=100]->true<-is>", "true")]
		[DataRow("<is ch count[<100]->true<-is>", "true")]
		[DataRow("<is ch count[=2]->true<-is>", "true")]
		[DataRow("<is author[>=2]->true<-is>", "true")]
		[DataRow("<is author[#=2]->true<-is>", "true")]
		[DataRow("<is author[=Arthur Conan Doyle]->true<-is>", "true")]
		[DataRow("<is author[format({L})][=Doyle]->true<-is>", "true")]
		[DataRow("<!is author[format({L})][=Doyle]->false<-is>", "")]
		[DataRow("<is author[format({L})][!=Doyle]->true<-is>", "true")]
		[DataRow("<!is author[format({L})][!=Doyle]->false<-is>", "")]
		[DataRow("<is author[format({L})separator(:)][=Doyle:Fry]->true<-is>", "true")]
		[DataRow("<is author[>=3]->true<-is>", "")]
		[DataRow(@"<is author[slice(99)][~.\*]->true<-is>", "")]
		[DataRow("<is author[slice(99)separator(:)][~.*]->true<-is>", "")]
		[DataRow("<is author[slice(-9)separator(:)][~.*]->true<-is>", "")]
		[DataRow("<is author[slice(2..1)separator(:)][~.*]->true<-is>", "")]
		[DataRow("<is author[slice(-1..1)separator(:)][~.*]->true<-is>", "")]
		[DataRow("<is author[slice(-1..-2)separator(:)][~.*]->true<-is>", "")]
		[DataRow("<is author[=Sherlock]->true<-is>", "")]
		[DataRow("<!is author[=Sherlock]->false<-is>", "false")]
		[DataRow("<is author[!=Sherlock]->true<-is>", "true")]
		[DataRow("<!is author[!=Sherlock]->false<-is>", "")]
		[DataRow("<is tag[=Tag1]->true<-is>", "true")]
		[DataRow("<is tag[separator(:)slice(-2..)][=Tag2:Tag3]->true<-is>", "true")]
		[DataRow("<is audible subtitle[3][=an]->false<-is>", "")]
		[DataRow("<is audible subtitle[3][=an ]->true<-is>", "true")]
		[DataRow(@"<is audible subtitle[3][=an\ ]->true<-is>", "true")]
		[DataRow("<is audible subtitle[3][= an]->false<-is>", "")]
		[DataRow("<is audible subtitle[3][= an ]->false<-is>", "")]
		[DataRow(@"<is audible subtitle[3][= an\ ]->false<-is>", "")]
		[DataRow(@"<is audible subtitle[3][=\ an\ ]->false<-is>", "")]
		[DataRow("<is audible subtitle[3][ =an]->false<-is>", "")]
		[DataRow("<is audible subtitle[3][ =an ]->true<-is>", "true")]
		[DataRow(@"<is audible subtitle[3][ =an\ ]->true<-is>", "true")]
		[DataRow(@"<is minutes[>42]->true<-is>", "true")]
		public void HasValue_test(string template, string expected)
		{
			var bookDto = GetLibraryBook();
			var multiDto = new MultiConvertFileProperties
			{
				PartsPosition = 1,
				PartsTotal = 2,
				Title = bookDto.Title,
				OutputFileName = "outputfile.m4b"
			};

			Templates.TryGetTemplate<Templates.FileTemplate>(template, out var fileTemplate).Should().BeTrue();
			fileTemplate
				.GetFilename(bookDto, multiDto, "", "", culture: null, replacements: Replacements)
				.PathWithoutPrefix
				.Should().Be(expected);
			fileTemplate.Errors.Should().HaveCount(0);
			fileTemplate.Warnings.Should().HaveCount(1); // "Should use tags. Eg: <title>"
		}

		[TestMethod]
		[DataRow("<series>", "Series A, Series B, Series C, Series D")]
		[DataRow("<series[]>", "Series A, Series B, Series C, Series D")]
		[DataRow("<series[slice(2..3)]>", "Series B, Series C")]
		[DataRow("<series[max(1)]>", "Series A")]
		[DataRow("<series[max(2)]>", "Series A, Series B")]
		[DataRow("<series[max(3)]>", "Series A, Series B, Series C")]
		[DataRow("<series[max(4)]>", "Series A, Series B, Series C, Series D")]
		[DataRow("<series[format({N}, {#}, {ID}) separator(; )]>", "Series A, 1, B1; Series B, 6, B2; Series C, 2, B3; Series D, 1-5, B4")]
		[DataRow("<series[format({N}, {#}, {ID}) separator(; ) max(3)]>", "Series A, 1, B1; Series B, 6, B2; Series C, 2, B3")]
		[DataRow("<series[format({N}, {#}, {ID}) separator(; ) max(2)]>", "Series A, 1, B1; Series B, 6, B2")]
		[DataRow("<first series>", "Series A")]
		[DataRow("<first series[]>", "Series A")]
		[DataRow("<first series[{N}, {#}, {ID}]>", "Series A, 1, B1")]
		[DataRow("<first series[{N}, {#:0'{}'0.0}]>", "Series A, 0{}1.0")]
		public void SeriesFormat_formatters(string template, string expected)
		{
			var bookDto = GetLibraryBook();
			bookDto.Series =
			[
				new("Series A", "1", "B1"),
				new("Series B", "6", "B2"),
				new("Series C", "2", "B3"),
				new("Series D", "1-5", "B4"),
			];

			Templates.TryGetTemplate<Templates.FileTemplate>(template, out var fileTemplate).Should().BeTrue();
			fileTemplate
				.GetFilename(bookDto, "", "", culture: CultureInfo.InvariantCulture, replacements: Replacements)
				.PathWithoutPrefix
				.Should().Be(expected);
		}

		[TestMethod]
		[DataRow("<first series[{#}]>", "1-6", "1-6")]
		[DataRow("<series[format({#:F2})]>", "1-6", "1.00-6.00")]
		[DataRow("<first series[{#:F2}]>", "1-6", "1.00-6.00")]
		[DataRow("<series#[F2]>", "1-6", "1.00-6.00")]
		[DataRow("<series#[F2]>", "front 1-6 back", "front 1.00-6.00 back")]
		[DataRow("<series#[F2]>", "front    1 - 6    back", "front 1.00 - 6.00 back")]
		[DataRow("<series#[F2]>", "f.1", "f.1.00")]
		[DataRow("<series#[F2]>", "f1g", "f1.00g")]
		[DataRow("<series#[F2]>", "   f1g   ", "f1.00g")]
		[DataRow("<series#[]>", "1", "1")]
		[DataRow("<series#>", "1", "1")]
		[DataRow("<series#>", " 1 6 ", "1 6")]
		public void SeriesOrder_formatters(string template, string seriesOrder, string expected)
		{
			var bookDto = GetLibraryBook();
			bookDto.Series = [new("Series A", seriesOrder, "B1")];

			Templates.TryGetTemplate<Templates.FileTemplate>(template, out var fileTemplate).Should().BeTrue();
			fileTemplate
				.GetFilename(bookDto, "", "", culture: CultureInfo.InvariantCulture, replacements: Replacements)
				.PathWithoutPrefix
				.Should().Be(expected);
		}

		[TestMethod]
		[DataRow(@"C:\a\b", @"C:\a\b\foobar.ext", PlatformID.Win32NT)]
		[DataRow(@"/a/b", @"/a/b/foobar.ext", PlatformID.Unix)]
		public void IfSeries_empty(string directory, string expected, PlatformID platformId)
		{
			if (Environment.OSVersion.Platform != platformId)
				Assert.Inconclusive($"Skipped because OS {platformId}.");

			Templates.TryGetTemplate<Templates.FileTemplate>("foo<if series-><-if series>bar", out var fileTemplate).Should().BeTrue();

			fileTemplate
				.GetFilename(GetLibraryBook(), directory, "ext", culture: null, replacements: Replacements)
				.PathWithoutPrefix
				.Should().Be(expected);
		}

		[TestMethod]
		[DataRow(@"C:\a\b", @"C:\a\b\foobar.ext", PlatformID.Win32NT)]
		[DataRow(@"/a/b", @"/a/b/foobar.ext", PlatformID.Unix)]
		public void IfSeries_no_series(string directory, string expected, PlatformID platformId)
		{
			if (Environment.OSVersion.Platform != platformId)
				Assert.Inconclusive($"Skipped because OS is not {platformId}.");

			Templates.TryGetTemplate<Templates.FileTemplate>("foo<if series->-<series>-<id>-<-if series>bar", out var fileTemplate).Should().BeTrue();

			fileTemplate.GetFilename(GetLibraryBook(null), directory, "ext", culture: null, replacements: Replacements)
				.PathWithoutPrefix
				.Should().Be(expected);
		}

		[TestMethod]
		[DataRow(@"C:\a\b", @"C:\a\b\foo-Sherlock Holmes-asin-bar.ext", PlatformID.Win32NT)]
		[DataRow(@"/a/b", @"/a/b/foo-Sherlock Holmes-asin-bar.ext", PlatformID.Unix)]
		public void IfSeries_with_series(string directory, string expected, PlatformID platformId)
		{
			if (Environment.OSVersion.Platform != platformId)
				Assert.Inconclusive($"Skipped because OS is not {platformId}.");

			Templates.TryGetTemplate<Templates.FileTemplate>("foo<if series->-<series>-<id>-<-if series>bar", out var fileTemplate).Should().BeTrue();

			fileTemplate
				.GetFilename(GetLibraryBook(), directory, "ext", culture: null, replacements: Replacements)
				.PathWithoutPrefix
				.Should().Be(expected);
		}

		[TestMethod]
		[DataRow("<if abridged->Abridged<-if abridged>", "Abridged", true)]
		[DataRow("<if abridged->Abridged<-if abridged>", "", false)]
		public void IfAbridged_test(string template, string expected, bool isAbridged)
		{
			var bookDto = GetLibraryBook();
			bookDto.IsAbridged = isAbridged;

			Templates.TryGetTemplate<Templates.FileTemplate>(template, out var fileTemplate).Should().BeTrue();

			fileTemplate
				.GetName(bookDto, new MultiConvertFileProperties { OutputFileName = string.Empty })
				.Should().Be(expected);
		}

		[TestMethod]
		[DataRow("<audibletitle [u]>", "I", "en-US", "i")]
		[DataRow("<audibletitle [l]>", "ı", "tr-TR", "I")]
		[DataRow("<audibletitle [u]>", "İ", "tr-TR", "i")]
		[DataRow(@"<minutes[D,DDD.DDE-0\-H,HHH.HH\-#,##M.##]>", "8.573,30E1-0.021,00-9", "es-ES", "any")]
		[DataRow(@"<minutes[D,DDD.DDE-0\-H,HHH.HH\-#,##M.##]>", "8,573.30E1-0,021.00-9", "en-AU", "any")]
		[DataRow("<samplerate[#,##0'Hz ']>", "44,100Hz ", "en-CA", "any")]
		[DataRow("<samplerate[#,##0'Hz ']>", "44’100Hz ", "de-CH", "any")]
		[DataRow("<samplerate[#,##0'Hz ']>", "44\u00A0100Hz ", "fr-CA", "any")] // non-breaking-space
		public void Tag_culture_test(string template, string expected, string cultureName, string title)
		{
			var bookDto = Shared.GetLibraryBook();
			bookDto.Title = title;
			bookDto.LengthInMinutes = TimeSpan.FromMinutes(123456789);
			var culture = new CultureInfo(cultureName);

			Templates.TryGetTemplate<Templates.FileTemplate>(template, out var fileTemplate).Should().BeTrue();

			fileTemplate
				.GetName(bookDto, new MultiConvertFileProperties { OutputFileName = string.Empty }, culture)
				.Should().Be(expected);
		}

		[TestMethod]
		[DataRow("<tag>", "Tag1, Tag2, Tag3")]
		[DataRow("<tag [separator( - )]>", "Tag1 - Tag2 - Tag3")]
		[DataRow("<tag [format({S:u})]>", "TAG1, TAG2, TAG3")]
		[DataRow("<tag[format({S:l})]>", "tag1, tag2, tag3")]
		[DataRow("<tag[format(Tag: {S})]>", "Tag: Tag1, Tag: Tag2, Tag: Tag3")]
		[DataRow("<tag [max(1)]>", "Tag1")]
		[DataRow("<tag [slice(2..)]>", "Tag2, Tag3")]
		[DataRow("<tag[sort(s)]>", "Tag3, Tag2, Tag1")]
		[DataRow("<first tag>", "Tag1")]
		[DataRow("<first tag[]>", "Tag1")]
		[DataRow("<first tag[l]>", "tag1")]
		public void Tag_test(string template, string expected)
		{
			var bookDto = Shared.GetLibraryBook();

			Templates.TryGetTemplate<Templates.FileTemplate>(template, out var fileTemplate).Should().BeTrue();

			fileTemplate
				.GetName(bookDto, new MultiConvertFileProperties { OutputFileName = string.Empty })
				.Should().Be(expected);
		}

		[TestMethod]
		[DataRow("English", "<language>", "English")]
		[DataRow("English", "<language[4u]>", "ENGL")]
		[DataRow("English", "<language short>", "ENG")]
		[DataRow("English", "<language short[1l]>", "ENG")]
		[DataRow("English", "<language[ID:{ID}, 2:{I}, 3:{I3}, W:{W}, D:{D}, E:{E}, N:{N}, O:{O}]>", "ID:en, 2:en, 3:eng, W:ENU, D:inglés, E:English, N:English, O:English")]
		[DataRow("en", "<language[ID:{ID}, 2:{I}, 3:{I3}, W:{W}, D:{D}, E:{E}, N:{N}, O:{O}]>", "ID:en, 2:en, 3:eng, W:ENU, D:inglés, E:English, N:English, O:en")]
		[DataRow("fr", "<language[ID:{ID}, 2:{I}, 3:{I3}, W:{W}, D:{D}, E:{E}, N:{N}, O:{O}]>", "ID:fr, 2:fr, 3:fra, W:FRA, D:francés, E:French, N:français, O:fr")]
		[DataRow("fr-ca", "<language[ID:{ID}, 2:{I}, 3:{I3}, W:{W}, D:{D}, E:{E}, N:{N}, O:{O}]>",
			"ID:fr-CA, 2:fr, 3:fra, W:FRC, D:francés (Canadá), E:French (Canada), N:français (Canada), O:fr-ca")]
		[DataRow("Any", "<ui[ID:{ID}, 2:{I}, 3:{I3}, W:{W}, D:{D}, E:{E}, N:{N}, O:{O}]>",
			"ID:es-ES, 2:es, 3:spa, W:ESN, D:español (España), E:Spanish (Spain), N:español (España), O:es-ES")]
		[DataRow("Any", "<os[ID:{ID}, 2:{I}, 3:{I3}, W:{W}, D:{D}, E:{E}, N:{N}, O:{O}]>",
			"ID:sv-SE, 2:sv, 3:swe, W:SVE, D:sueco (Suecia), E:Swedish (Sweden), N:svenska (Sverige), O:sv-SE")]
		// different localizations
		[DataRow("fr", "<language[D:{D@de-DE}, E:{E@de-DE}, N:{N@de-DE}, O:{O@de-DE}]>", "D:Französisch, E:French, N:français, O:fr")]
		[DataRow("fr", "<language[D:{D@pl}]>", "D:francuski")]
		[DataRow("fr", "<language[D:{D@it}]>", "D:francese")]
		public void Language_test(string language, string template, string expected)
		{
			var bookDto = Shared.GetLibraryBook();
			bookDto.Language = new CultureInfoDto(language);

			var result = "";

			var old = Thread.CurrentThread.CurrentCulture;
			var oldUi = Thread.CurrentThread.CurrentUICulture;
			try
			{
				Thread.CurrentThread.CurrentCulture = new CultureInfo("sv-SE");
				Thread.CurrentThread.CurrentUICulture = new CultureInfo("es-ES");
				Templates.TryGetTemplate<Templates.FileTemplate>(template, out var fileTemplate).Should().BeTrue();
				result = fileTemplate
					.GetName(bookDto, new MultiConvertFileProperties { OutputFileName = string.Empty });
			}
			finally
			{
				Thread.CurrentThread.CurrentCulture = old;
				Thread.CurrentThread.CurrentUICulture = oldUi;
			}

			result.Should().Be(expected);
		}

		[TestMethod]
		// Audible does not provide a consistent or authoritative region code for its storefronts.
		// In most cases, the storefront region can be inferred from the EnglishName of a matching RegionInfo entry. However,
		// the US and UK storefronts do not follow this pattern, and the three historical “pre‑Amazon” storefront identifiers require
		// separate interpretation to remain globally usable for all users.
		// To ensure robustness, the tests attempt to cover all known Audible storefronts explicitly.

		// Skipping of NativeName: its output is influenced by external standards bodies and evolving globalization data (NLS vs. ICU),
		// not solely by the OSPlatform.
		// Because .NET provides no stability guarantees for NativeName across platforms or ICU/NLS versions, we do not include
		// platform-specific tests here—unlike path-related differences, which are defined and testable.

		// test known locales
		[DataRow("us", "<locale[ID:{ID}, 2:{I}, 3:{I3}, W:{W}, D:{D}, E:{E}, N:{N}, O:{O}, T:.{T}, L:{L}]>",
			"ID:AF2M0KC94RCEA, 2:US, 3:USA, W:USA, D:Estados Unidos, E:United States, N:United States, O:us, T:.com, L:en-US")]
		[DataRow("uk", "<locale[ID:{ID}, 2:{I}, 3:{I3}, W:{W}, D:{D}, E:{E}, N:{N}, O:{O}, T:.{T}, L:{L}]>",
			"ID:A2I9A3Q2GNFNGQ, 2:GB, 3:GBR, W:GBR, D:Reino Unido, E:United Kingdom, N:United Kingdom, O:uk, T:.co.uk, L:en-GB")]
		[DataRow("germany", "<locale[ID:{ID}, 2:{I}, 3:{I3}, W:{W}, D:{D}, E:{E}, N:{N}, O:{O}, T:.{T}, L:{L}]>",
			"ID:AN7V1F1VY261K, 2:DE, 3:DEU, W:DEU, D:Alemania, E:Germany, N:Deutschland, O:germany, T:.de, L:de-DE")]
		// Skip NativeName (see above)
		[DataRow("france", "<locale[ID:{ID}, 2:{I}, 3:{I3}, W:{W}, D:{D}, E:{E}, O:{O}, T:.{T}, L:{L}]>", "ID:A2728XDNODOQ8T, 2:FR, 3:FRA, W:FRA, D:Francia, E:France, O:france, T:.fr, L:fr-FR")]
		[DataRow("australia",
			"<locale[ID:{ID}, 2:{I}, 3:{I3}, W:{W}, D:{D}, E:{E}, O:{O}, T:.{T}, L:{L}]>", "ID:AN7EY7DTAW63G, 2:AU, 3:AUS, W:AUS, D:Australia, E:Australia, O:australia, T:.com.au, L:en-AU")]
		[DataRow("india", "<locale[ID:{ID}, 2:{I}, 3:{I3}, W:{W}, D:{D}, E:{E}, O:{O}, T:.{T}, L:{L}]>", "ID:AJO3FBRUE6J4S, 2:IN, 3:IND, W:IND, D:India, E:India, O:india, T:.in, L:en-IN")]
		[DataRow("spain", "<locale[ID:{ID}, 2:{I}, 3:{I3}, W:{W}, D:{D}, E:{E}, O:{O}, T:.{T}, L:{L}]>", "ID:ALMIKO4SZCSAR, 2:ES, 3:ESP, W:ESP, D:España, E:Spain, O:spain, T:.es, L:es-ES")]
		[DataRow("italy", "<locale[ID:{ID}, 2:{I}, 3:{I3}, W:{W}, D:{D}, E:{E}, O:{O}, T:.{T}, L:{L}]>", "ID:A2N7FU2W2BU2ZC, 2:IT, 3:ITA, W:ITA, D:Italia, E:Italy, O:italy, T:.it, L:it-IT")]
		[DataRow("canada", "<locale[ID:{ID}, 2:{I}, 3:{I3}, W:{W}, D:{D}, E:{E}, O:{O}, T:.{T}, L:{L}]>", "ID:A2CQZ5RBY40XE, 2:CA, 3:CAN, W:CAN, D:Canadá, E:Canada, O:canada, T:.ca, L:en-CA")]
		[DataRow("japan", "<locale[ID:{ID}, 2:{I}, 3:{I3}, W:{W}, D:{D}, E:{E}, O:{O}, T:.{T}, L:{L}]>", "ID:A1QAP3MOU4173J, 2:JP, 3:JPN, W:JPN, D:Japón, E:Japan, O:japan, T:.co.jp, L:ja-JP")]
		[DataRow("brazil", "<locale[ID:{ID}, 2:{I}, 3:{I3}, W:{W}, D:{D}, E:{E}, O:{O}, T:.{T}, L:{L}]>", "ID:A10J1VAYUDTYRN, 2:BR, 3:BRA, W:BRA, D:Brasil, E:Brazil, O:brazil, T:.com.br, L:pt-BR")]

		// test historical locales
		[DataRow("pre-amazon - us", "<locale[ID:{ID}, O:{O}, T:.{T}, L:{L}]>", "ID:AF2M0KC94RCEA, O:pre-amazon - us, T:.com, L:en-US")]
		[DataRow("pre-amazon - uk", "<locale[ID:{ID}, O:{O}, T:.{T}, L:{L}]>", "ID:A2I9A3Q2GNFNGQ, O:pre-amazon - uk, T:.co.uk, L:en-GB")]
		[DataRow("pre-amazon - germany", "<locale[ID:{ID}, O:{O}, T:.{T}, L:{L}]>", "ID:AN7V1F1VY261K, O:pre-amazon - germany, T:.de, L:de-DE")]

		// test upcoming locales
		[DataRow("be", "<locale[ID:{ID}, E:{E}, O:{O}]>", "ID:, E:Belgium, O:be")]
		[DataRow("nl", "<locale[ID:{ID}, E:{E}, O:{O}]>", "ID:, E:Netherlands, O:nl")]
		[DataRow("se", "<locale[ID:{ID}, E:{E}, O:{O}]>", "ID:, E:Sweden, O:se")]
		[DataRow("pl", "<locale[ID:{ID}, E:{E}, O:{O}]>", "ID:, E:Poland, O:pl")]
		[DataRow("ie", "<locale[ID:{ID}, E:{E}, O:{O}]>", "ID:, E:Ireland, O:ie")]
		[DataRow("sg", "<locale[ID:{ID}, E:{E}, O:{O}]>", "ID:, E:Singapore, O:sg")]
		[DataRow("za", "<locale[ID:{ID}, E:{E}, O:{O}]>", "ID:, E:South Africa, O:za")]
		[DataRow("ae", "<locale[ID:{ID}, E:{E}, O:{O}]>", "ID:, E:United Arab Emirates, O:ae")]
		[DataRow("sa", "<locale[ID:{ID}, E:{E}, O:{O}]>", "ID:, E:Saudi Arabia, O:sa")]
		[DataRow("eg", "<locale[ID:{ID}, E:{E}, O:{O}]>", "ID:, E:Egypt, O:eg")]
		// Skip EnglishName: the official English name of Turkey changed to 'Türkiye', and the returned value now depends on
		// the OS/globalization provider (Windows-NLS vs. ICU). Tests would not be stable.
		// A future lookup may still need to account for whichever English name Audible chooses to use.
		[DataRow("tr", "<locale[ID:{ID}, E:---, O:{O}]>", "ID:, E:---, O:tr")]

		// test some different localizations - should change only D(isplayNames)
		[DataRow("fr", "<locale[D:{D@de-DE}, E:{E@de-DE}, N:{N@de-DE}, O:{O@de-DE}]>", "D:Frankreich, E:France, N:France, O:fr")]
		[DataRow("fr", "<locale[D:{D@pl}]>", "D:Francja")]
		[DataRow("fr", "<locale[D:{D@it}]>", "D:Francia")]
		public void Locale_test(string country, string template, string expected)
		{
			var bookDto = Shared.GetLibraryBook();
			bookDto.Locale = new LocaleDto(country);

			var result = "";

			var old = Thread.CurrentThread.CurrentCulture;
			var oldUi = Thread.CurrentThread.CurrentUICulture;
			try
			{
				Thread.CurrentThread.CurrentCulture = new CultureInfo("fr-FR");
				Thread.CurrentThread.CurrentUICulture = new CultureInfo("es-ES");
				Templates.TryGetTemplate<Templates.FileTemplate>(template, out var fileTemplate).Should().BeTrue();
				result = fileTemplate
					.GetName(bookDto, new MultiConvertFileProperties { OutputFileName = string.Empty });
			}
			finally
			{
				Thread.CurrentThread.CurrentCulture = old;
				Thread.CurrentThread.CurrentUICulture = oldUi;
			}

			result.Should().Be(expected);
		}
	}
}


namespace Templates_Other
{

	[TestClass]
	public class GetFilePath
	{
		static readonly ReplacementCharacters Replacements = ReplacementCharacters.Default(Environment.OSVersion.Platform == PlatformID.Win32NT);

		[TestMethod]
		[DataRow(@"C:\foo\bar", @"\\Folder\<title>\[<id>]\\", @"C:\foo\bar\Folder\my_ book 000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000\[ID123456].txt", PlatformID.Win32NT)]
		[DataRow("/foo/bar", "/Folder/<title>/[<id>]/", @"/foo/bar/Folder/my: book 000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000/[ID123456].txt", PlatformID.Unix)]
		[DataRow(@"C:\foo\bar", @"\Folder\<title> [<id>]", @"C:\foo\bar\Folder\my_ book 0000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000 [ID123456].txt", PlatformID.Win32NT)]
		[DataRow("/foo/bar", "/Folder/<title> [<id>]", @"/foo/bar/Folder/my: book 0000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000 [ID123456].txt", PlatformID.Unix)]
		[DataRow(@"C:\foo\bar", @"\Folder\<title> <title> <title> <title> <title> <title> <title> <title> <title> [<id>]", @"C:\foo\bar\Folder\my_ book 0000000000000000 my_ book 0000000000000000 my_ book 0000000000000000 my_ book 0000000000000000 my_ book 0000000000000000 my_ book 0000000000000000 my_ book 0000000000000000 my_ book 00000000000000000 my_ book 00000000000000000 [ID123456].txt", PlatformID.Win32NT)]
		[DataRow("/foo/bar", "/Folder/<title> <title> <title> <title> <title> <title> <title> <title> <title> [<id>]", @"/foo/bar/Folder/my: book 0000000000000000 my: book 0000000000000000 my: book 0000000000000000 my: book 0000000000000000 my: book 0000000000000000 my: book 0000000000000000 my: book 0000000000000000 my: book 00000000000000000 my: book 00000000000000000 [ID123456].txt", PlatformID.Unix)]
		[DataRow(@"C:\foo\bar", @"\<title>\<title> [<id>]", @"C:\foo\bar\my_ book 000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000\my_ book 0000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000 [ID123456].txt", PlatformID.Win32NT)]
		[DataRow("/foo/bar", @"/<title>/<title> [<id>]", "/foo/bar/my: book 000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000/my: book 0000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000 [ID123456].txt", PlatformID.Unix)]
		public void Test_trim_to_max_path(string dirFullPath, string template, string expected, PlatformID platformId)
		{
			if (Environment.OSVersion.Platform != platformId)
				Assert.Inconclusive($"Skipped because OS is not {platformId}.");

			var sb = new System.Text.StringBuilder();
			sb.Append('0', 300);
			var longText = sb.ToString();

			NEW_GetValidFilename_FileNamingTemplate(dirFullPath, template, "my: book " + longText, "txt").Should().Be(expected);
		}

		[TestMethod]
		[DataRow(@"\foo\bar", @"<title>\<title>")]
		[DataRow(@"\foooo\barrrr", "<title>")]
		public void Test_windows_relative_path_too_long(string baseDir, string template)
		{
			if (Environment.OSVersion.Platform != PlatformID.Win32NT)
				Assert.Inconclusive($"Skipped because OS is not {PlatformID.Win32NT}.");

			var sb = new System.Text.StringBuilder();
			sb.Append('0', 300);
			var longText = sb.ToString();
			Assert.ThrowsExactly<PathTooLongException>(() => NEW_GetValidFilename_FileNamingTemplate(baseDir, template, "my: book " + longText, "txt"));
		}

		private static string NEW_GetValidFilename_FileNamingTemplate(string dirFullPath, string template, string title, string extension)
		{
			extension = FileUtility.GetStandardizedExtension(extension);

			var lbDto = GetLibraryBook();
			lbDto.TitleWithSubtitle = title;
			lbDto.AudibleProductId = "ID123456";

			Templates.TryGetTemplate<Templates.FolderTemplate>(template, out var fileNamingTemplate).Should().BeTrue();

			return fileNamingTemplate.GetFilename(lbDto, dirFullPath, extension, culture: null, replacements: Replacements).PathWithoutPrefix;
		}

		[TestMethod]
		[DataRow(@"C:\foo\bar\my file.txt", @"C:\foo\bar\my file - 002 - title.txt", PlatformID.Win32NT)]
		[DataRow(@"/foo/bar/my file.txt", @"/foo/bar/my file - 002 - title.txt", PlatformID.Unix)]
		public void equiv_GetMultipartFileName(string inStr, string outStr, PlatformID platformId)
		{
			if (Environment.OSVersion.Platform != platformId)
				Assert.Inconclusive($"Skipped because OS is not {platformId}.");

			NEW_GetMultipartFileName_FileNamingTemplate(inStr, 2, 100, "title").Should().Be(outStr);
		}

		private static string NEW_GetMultipartFileName_FileNamingTemplate(string originalPath, int partsPosition, int partsTotal, string suffix)
		{
			// 1-9     => 1-9
			// 10-99   => 01-99
			// 100-999 => 001-999

			var estension = Path.GetExtension(originalPath);
			var dir = Path.GetDirectoryName(originalPath)!;
			var template = Path.GetFileNameWithoutExtension(originalPath) + " - <ch# 0> - <title>" + estension;

			var lbDto = GetLibraryBook();
			lbDto.TitleWithSubtitle = suffix;

			Templates.TryGetTemplate<Templates.ChapterFileTemplate>(template, out var chapterFileTemplate).Should().BeTrue();

			return chapterFileTemplate
				.GetFilename(lbDto, new MultiConvertFileProperties { Title = suffix, PartsTotal = partsTotal, PartsPosition = partsPosition, OutputFileName = string.Empty }, dir, estension,
					culture: null, replacements: Replacements)
				.PathWithoutPrefix;
		}

		[TestMethod]
		[DataRow(@"\foo\<title>.txt", @"\foo\sl∕as∕he∕s.txt", PlatformID.Win32NT)]
		[DataRow(@"/foo/<title>.txt", @"/foo/s\l∕a\s∕h\e∕s.txt", PlatformID.Unix)]
		public void remove_slashes(string inStr, string outStr, PlatformID platformId)
		{
			if (Environment.OSVersion.Platform != platformId)
				Assert.Inconclusive($"Skipped because OS is not {platformId}.");

			var lbDto = GetLibraryBook();
			lbDto.TitleWithSubtitle = @"s\l/a\s/h\e/s";

			var directory = Path.GetDirectoryName(inStr)!;
			var fileName = Path.GetFileName(inStr);

			Templates.TryGetTemplate<Templates.FileTemplate>(fileName, out var fileNamingTemplate).Should().BeTrue();

			fileNamingTemplate.GetFilename(lbDto, directory, "txt", culture: null, replacements: Replacements).PathWithoutPrefix.Should().Be(outStr);
		}
	}
}

namespace Templates_Folder_Tests
{
	[TestClass]
	public class GetErrors
	{
		private static readonly PlatformID[] Win32NtAndUnix = [PlatformID.Win32NT, PlatformID.Unix];

		[TestMethod]
		public void null_is_invalid() => Tests(null, Win32NtAndUnix, new[] { NamingTemplate.ErrorNullIsInvalid });

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
		public void valid_tests(string template) => Tests(template, Win32NtAndUnix, Array.Empty<string>());

		[TestMethod]
		[DataRow([@"C:\", new[] { PlatformID.Win32NT }, Templates.ErrorFullPathIsInvalid])]
		public void Tests(string? template, PlatformID[] platformIds, params string[] expected)
		{
			if (!platformIds.Contains(Environment.OSVersion.Platform))
				Assert.Inconclusive($"Skipped because OS is not one of {platformIds}.");

			Templates.TryGetTemplate<Templates.FolderTemplate>(template, out var folderTemplate);
			var result = folderTemplate.Errors.ToList();
			result.Should().HaveCount(expected.Length);
			result.Should().BeEquivalentTo(expected);
		}
	}

	[TestClass]
	public class IsValid
	{
		[TestMethod]
		public void null_is_invalid() => Templates.TryGetTemplate<Templates.FolderTemplate>(null, out _).Should().BeFalse();

		[TestMethod]
		public void empty_is_valid() => Tests("", true, [PlatformID.Win32NT, PlatformID.Unix]);

		[TestMethod]
		public void whitespace_is_valid() => Tests("   ", true, [PlatformID.Win32NT, PlatformID.Unix]);

		[TestMethod]
		[DataRow(@"C:\", false, new[] { PlatformID.Win32NT })]
		[DataRow(@"foo", true, new[] { PlatformID.Win32NT, PlatformID.Unix })]
		[DataRow(@"\foo", true, new[] { PlatformID.Win32NT, PlatformID.Unix })]
		[DataRow(@"foo\", true, new[] { PlatformID.Win32NT, PlatformID.Unix })]
		[DataRow(@"\foo\", true, new[] { PlatformID.Win32NT, PlatformID.Unix })]
		[DataRow(@"foo\bar", true, new[] { PlatformID.Win32NT, PlatformID.Unix })]
		[DataRow(@"<id>", true, new[] { PlatformID.Win32NT, PlatformID.Unix })]
		[DataRow(@"<id>\<title>", true, new[] { PlatformID.Win32NT, PlatformID.Unix })]
		public void Tests(string template, bool expected, PlatformID[] platformIds)
		{
			if (!platformIds.Contains(Environment.OSVersion.Platform))
				Assert.Inconclusive($"Skipped because OS is not one of {platformIds}.");

			Templates.TryGetTemplate<Templates.FolderTemplate>(template, out var folderTemplate).Should().BeTrue();
			folderTemplate.IsValid.Should().Be(expected);
		}
	}

	[TestClass]
	public class GetWarnings
	{
		[TestMethod]
		public void null_is_invalid() => Tests(null, new[] { NamingTemplate.ErrorNullIsInvalid });

		[TestMethod]
		public void empty_has_warnings() => Tests("", NamingTemplate.WarningEmpty, NamingTemplate.WarningNoTags);

		[TestMethod]
		public void whitespace_has_warnings() => Tests("   ", NamingTemplate.WarningWhiteSpace, NamingTemplate.WarningNoTags);

		[TestMethod]
		[DataRow(@"<id>\foo\bar")]
		public void valid_tests(string template) => Tests(template, Array.Empty<string>());

		[TestMethod]
		[DataRow(@"no tags", NamingTemplate.WarningNoTags)]
		[DataRow("<ch#> chapter tag", NamingTemplate.WarningNoTags)]
		public void Tests(string? template, params string[] expected)
		{
			Templates.TryGetTemplate<Templates.FolderTemplate>(template, out var folderTemplate);
			var result = folderTemplate.Warnings.ToList();
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
		public void Tests(string? template, bool expected)
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
		public void Empty() => Tests("", 0);

		[TestMethod]
		public void Whitespace() => Tests("   ", 0);

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
		public void null_is_invalid() => Tests(null, Environment.OSVersion.Platform, new[] { NamingTemplate.ErrorNullIsInvalid });

		[TestMethod]
		public void empty_is_valid() => valid_tests("");

		[TestMethod]
		public void whitespace_is_valid() => valid_tests("   ");

		[TestMethod]
		[DataRow(@"foo")]
		[DataRow(@"<id>")]
		public void valid_tests(string template) => Tests(template, Environment.OSVersion.Platform, Array.Empty<string>());

		private void Tests(string? template, PlatformID platformId, params string[] expected)
		{
			if (Environment.OSVersion.Platform != platformId)
				Assert.Inconclusive($"Skipped because OS is not {platformId}.");

			Templates.TryGetTemplate<Templates.FileTemplate>(template, out var fileTemplate);
			var result = fileTemplate.Errors.ToList();
			result.Should().HaveCount(expected.Length);
			result.Should().BeEquivalentTo(expected);
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
		public void null_is_invalid() => Tests(null, null, new[] { NamingTemplate.ErrorNullIsInvalid, Templates.WarningNoChapterNumberTag });

		[TestMethod]
		public void empty_has_warnings() => Tests("", null, NamingTemplate.WarningEmpty, NamingTemplate.WarningNoTags, Templates.WarningNoChapterNumberTag);

		[TestMethod]
		public void whitespace_has_warnings() => Tests("   ", null, NamingTemplate.WarningWhiteSpace, NamingTemplate.WarningNoTags, Templates.WarningNoChapterNumberTag);

		[TestMethod]
		[DataRow("<ch#>")]
		[DataRow("<ch#> <id>")]
		public void valid_tests(string template) => Tests(template, null, Array.Empty<string>());

		[TestMethod]
		[DataRow(@"no tags", null, NamingTemplate.WarningNoTags, Templates.WarningNoChapterNumberTag)]
		[DataRow(@"<id>\foo\bar", PlatformID.Win32NT, Templates.WarningNoChapterNumberTag)]
		[DataRow(@"<id>/foo/bar", PlatformID.Unix, Templates.WarningNoChapterNumberTag)]
		[DataRow("<chapter count> -- chapter tag but not ch# or ch_#", null, NamingTemplate.WarningNoTags, Templates.WarningNoChapterNumberTag)]
		public void Tests(string? template, PlatformID? platformId, params string[] expected)
		{
			if (platformId is not null && Environment.OSVersion.Platform != platformId)
				Assert.Inconclusive($"Skipped because OS is not {platformId}.");

			Templates.TryGetTemplate<Templates.ChapterFileTemplate>(template, out var chapterFileTemplate);
			var result = chapterFileTemplate.Warnings.ToList();
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
		[DataRow(@"<id>\foo\bar", true)]
		[DataRow("<ch#> <id>", false)]
		[DataRow("<ch#> -- chapter tag", false)]
		[DataRow("<chapter count> -- chapter tag but not ch# or ch_#", true)]
		public void Tests(string? template, bool expected)
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
		static readonly ReplacementCharacters Default = ReplacementCharacters.Default(Environment.OSVersion.Platform == PlatformID.Win32NT);

		[TestMethod]
		[DataRow("[<id>] <ch# 0> of <ch count> - <ch title>", @"C:\foo\", "txt", 6, 10, "chap", @"C:\foo\[asin] 06 of 10 - chap.txt", PlatformID.Win32NT)]
		[DataRow("[<id>] <ch# 0> of <ch count> - <ch title>", @"/foo/", "txt", 6, 10, "chap", @"/foo/[asin] 06 of 10 - chap.txt", PlatformID.Unix)]
		[DataRow("<ch#>", @"C:\foo\", "txt", 6, 10, "chap", @"C:\foo\6.txt", PlatformID.Win32NT)]
		[DataRow("<ch#>", @"/foo/", "txt", 6, 10, "chap", @"/foo/6.txt", PlatformID.Unix)]
		public void Tests(string template, string dir, string ext, int pos, int total, string chapter, string expected, PlatformID platformId)
		{
			if (Environment.OSVersion.Platform != platformId)
				Assert.Inconclusive($"Skipped because OS is not {platformId}.");

			Templates.TryGetTemplate<Templates.ChapterFileTemplate>(template, out var chapterTemplate).Should().BeTrue();
			chapterTemplate
				.GetFilename(GetLibraryBook(), new() { OutputFileName = $"xyz.{ext}", PartsPosition = pos, PartsTotal = total, Title = chapter }, dir, ext, culture: null, replacements: Default)
				.PathWithoutPrefix
				.Should().Be(expected);
		}
	}
}
