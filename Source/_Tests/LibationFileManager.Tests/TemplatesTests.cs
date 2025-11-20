using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AaxDecrypter;
using AssertionHelper;
using FileManager;
using FileManager.NamingTemplate;
using LibationFileManager.Templates;
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
		static Shared()
		{
			LibationFileManager.Configuration.CreateMockInstance().Books = Path.GetFullPath("Books");
		}

		public static LibraryBookDto GetLibraryBook()
			=> GetLibraryBook([new SeriesDto("Sherlock Holmes", "1", "B08376S3R2")]);

		public static LibraryBookDto GetLibraryBook(IEnumerable<SeriesDto> series)
			=> new()
			{
				Account = "myaccount@example.co",
				AccountNickname = "my account",
				DateAdded = new DateTime(2022, 6, 9, 0, 0, 0),
				DatePublished = new DateTime(2017, 2, 27, 0, 0, 0),
				FileDate = new DateTime(2023, 1, 28, 0, 0, 0),
				AudibleProductId = "asin",
				Title = "A Study in Scarlet: A Sherlock Holmes Novel",
				Locale = "us",
				YearPublished = 2017,
				Authors = [new("Arthur Conan Doyle", "B000AQ43GQ"), new("Stephen Fry - introductions", "B000APAGVS")],
				Narrators = [new("Stephen Fry", "B000APAGVS"), new("Some Narrator", "B000000000")],
				Series = series,
				BitRate = 128,
				SampleRate = 44100,
				Channels = 2,
                Language = "English",
				Subtitle = "An Audible Original Drama",
				TitleWithSubtitle = "A Study in Scarlet: An Audible Original Drama",
				Codec = "AAC-LC",
				FileVersion = "1.0",
				LibationVersion = "1.0.0",
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
		static ReplacementCharacters Replacements = ReplacementCharacters.Default(Environment.OSVersion.Platform == PlatformID.Win32NT);

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
					.ToArray() };

			Templates.TryGetTemplate<Templates.FileTemplate>(template, out var fileTemplate).Should().BeTrue();

			fileTemplate
				.GetFilename(GetLibraryBook(), dirFullPath, extension, replacements)
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
		[DataRow("<id> - <pubdate[MM/dd/yy HH:mm]>", @"C:\foo\bar", ".m4b", @"C:\foo\bar\asin - 02∕27∕17 00_00.m4b", PlatformID.Win32NT)]
		[DataRow("<id> - <pubdate[MM/dd/yy HH:mm]>", @"/foo/bar", ".m4b", @"/foo/bar/asin - 02∕27∕17 00:00.m4b", PlatformID.Unix)]
		[DataRow("<id> - <filedate[MM/dd/yy HH:mm]>", @"C:\foo\bar", ".m4b", @"C:\foo\bar\asin - 01∕28∕23 00_00.m4b", PlatformID.Win32NT)]
		[DataRow("<id> - <filedate[MM/dd/yy HH:mm]>", @"/foo/bar", ".m4b", @"/foo/bar/asin - 01∕28∕23 00:00.m4b", PlatformID.Unix)]
		[DataRow("<id> - <date added[MM/dd/yy HH:mm]>", @"C:\foo\bar", ".m4b", @"C:\foo\bar\asin - 06∕09∕22 00_00.m4b", PlatformID.Win32NT)]
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
				.GetFilename(bookDto, "", "", Replacements)
				.PathWithoutPrefix
				.Should().Be(expected);
		}

		[TestMethod]
		[DataRow("<author>", "Jill Conner Browne, Charles E. Gannon, Christopher John Fetherolf, Lucy Maud Montgomery, Jon Bon Jovi, Paul Van Doren")]
		[DataRow("<author[]>", "Jill Conner Browne, Charles E. Gannon, Christopher John Fetherolf, Lucy Maud Montgomery, Jon Bon Jovi, Paul Van Doren")]
		[DataRow("<author[sort(F)]>", "Charles E. Gannon, Christopher John Fetherolf, Jill Conner Browne, Jon Bon Jovi, Lucy Maud Montgomery, Paul Van Doren")]
		[DataRow("<author[sort(L)]>", "Jon Bon Jovi, Jill Conner Browne, Christopher John Fetherolf, Charles E. Gannon, Lucy Maud Montgomery, Paul Van Doren")]
		[DataRow("<author[sort(M)]>", "Jon Bon Jovi, Paul Van Doren, Jill Conner Browne, Charles E. Gannon, Christopher John Fetherolf, Lucy Maud Montgomery")]
		[DataRow("<author[sort(f)]>", "Jill Conner Browne, Charles E. Gannon, Christopher John Fetherolf, Lucy Maud Montgomery, Jon Bon Jovi, Paul Van Doren")]
		[DataRow("<author[sort(m)]>", "Jill Conner Browne, Charles E. Gannon, Christopher John Fetherolf, Lucy Maud Montgomery, Jon Bon Jovi, Paul Van Doren")]
		[DataRow("<author[sort(l)]>", "Jill Conner Browne, Charles E. Gannon, Christopher John Fetherolf, Lucy Maud Montgomery, Jon Bon Jovi, Paul Van Doren")]
		[DataRow("<author  [  max(  1  )  ]>", "Jill Conner Browne")]
		[DataRow("<author[max(2)]>", "Jill Conner Browne, Charles E. Gannon")]
		[DataRow("<author[max(3)]>", "Jill Conner Browne, Charles E. Gannon, Christopher John Fetherolf")]
		[DataRow("<author[format({L}, {F})]>", "Browne, Jill, Gannon, Charles, Fetherolf, Christopher, Montgomery, Lucy, Bon Jovi, Jon, Van Doren, Paul")]
		[DataRow("<author[format({L}, {F} {ID})]>", "Browne, Jill B1, Gannon, Charles B2, Fetherolf, Christopher B3, Montgomery, Lucy B4, Bon Jovi, Jon B5, Van Doren, Paul B6")]
		[DataRow("<author[format({ID})]>", "B1, B2, B3, B4, B5, B6")]
		[DataRow("<author[format({Id})]>", "Jill Conner Browne, Charles E. Gannon, Christopher John Fetherolf, Lucy Maud Montgomery, Jon Bon Jovi, Paul Van Doren")]
		[DataRow("<author[format({iD})]>", "Jill Conner Browne, Charles E. Gannon, Christopher John Fetherolf, Lucy Maud Montgomery, Jon Bon Jovi, Paul Van Doren")]
		[DataRow("<author[format({id})]>", "Jill Conner Browne, Charles E. Gannon, Christopher John Fetherolf, Lucy Maud Montgomery, Jon Bon Jovi, Paul Van Doren")]
		[DataRow("<author[format({f}, {l})]>", "Jill Conner Browne, Charles E. Gannon, Christopher John Fetherolf, Lucy Maud Montgomery, Jon Bon Jovi, Paul Van Doren")]
		[DataRow("<author[format(First={F}, Last={L})]>", "First=Jill, Last=Browne, First=Charles, Last=Gannon, First=Christopher, Last=Fetherolf, First=Lucy, Last=Montgomery, First=Jon, Last=Bon Jovi, First=Paul, Last=Van Doren")]
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
				new("Paul Van Doren", "B6")
			];

			Templates.TryGetTemplate<Templates.FileTemplate>(template, out var fileTemplate).Should().BeTrue();
			fileTemplate
				.GetFilename(bookDto, "", "", Replacements)
				.PathWithoutPrefix
				.Should().Be(expected);
		}

		[TestMethod]
		[DataRow("<has id->true<-has>", "true")]
		[DataRow("<has title->true<-has>", "true")]
		[DataRow("<has title short->true<-has>", "true")]
		[DataRow("<has audible title->true<-has>", "true")]
		[DataRow("<has audible subtitle->true<-has>", "true")]
		[DataRow("<has author->true<-has>", "true")]
		[DataRow("<has first author->true<-has>", "true")]
		[DataRow("<has narrator->true<-has>", "true")]
		[DataRow("<has first narrator->true<-has>", "true")]
		[DataRow("<has series->true<-has>", "true")]
		[DataRow("<has first series->true<-has>", "true")]
		[DataRow("<has series#->true<-has>", "true")]
		[DataRow("<has bitrate->true<-has>", "true")]
		[DataRow("<has samplerate->true<-has>", "true")]
		[DataRow("<has channels->true<-has>", "true")]
		[DataRow("<has codec->true<-has>", "true")]
		[DataRow("<has file version->true<-has>", "true")]
		[DataRow("<has libation version->true<-has>", "true")]
		[DataRow("<has account->true<-has>", "true")]
		[DataRow("<has account nickname->true<-has>", "true")]
		[DataRow("<has locale->true<-has>", "true")]
		[DataRow("<has year->true<-has>", "true")]
		[DataRow("<has language->true<-has>", "true")]
		[DataRow("<has language short->true<-has>", "true")]
		[DataRow("<has file date->true<-has>", "true")]
		[DataRow("<has pub date->true<-has>", "true")]
		[DataRow("<has date added->true<-has>", "true")]
		[DataRow("<has ch count->true<-has>", "true")]
		[DataRow("<has ch title->true<-has>", "true")]
		[DataRow("<has ch#->true<-has>", "true")]
		[DataRow("<has ch# 0->true<-has>", "true")]
		[DataRow("<has FAKE->true<-has>", "")]
		public void HasValue_test(string template, string expected)
		{
			var bookDto = GetLibraryBook();
			var multiDto = new MultiConvertFileProperties
			{
				PartsPosition = 1,
				PartsTotal = 2,
				Title = bookDto.Title,
			};

			Templates.TryGetTemplate<Templates.FileTemplate>(template, out var fileTemplate).Should().BeTrue();
			fileTemplate
				.GetFilename(bookDto, multiDto, "", "", Replacements)
				.PathWithoutPrefix
				.Should().Be(expected);
		}

		[TestMethod]
		[DataRow("<series>", "Series A, Series B, Series C, Series D")]
		[DataRow("<series[]>", "Series A, Series B, Series C, Series D")]
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
		[DataRow("<first series[{N}, {#:00.0}]>", "Series A, 01.0")]
		public void SeriesFormat_formatters(string template, string expected)
		{
			var bookDto = GetLibraryBook();
			bookDto.Series =
			[
				new("Series A", "1",  "B1"),
				new("Series B", "6",  "B2"),
				new("Series C", "2",  "B3"),
				new("Series D", "1-5",  "B4"),
			];
			
			Templates.TryGetTemplate<Templates.FileTemplate>(template, out var fileTemplate).Should().BeTrue();
			fileTemplate
				.GetFilename(bookDto, "", "", Replacements)
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
			bookDto.Series = [new("Series A", seriesOrder,  "B1")];

			Templates.TryGetTemplate<Templates.FileTemplate>(template, out var fileTemplate).Should().BeTrue();
			fileTemplate
				.GetFilename(bookDto, "", "", Replacements)
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
		static ReplacementCharacters Replacements = ReplacementCharacters.Default(Environment.OSVersion.Platform == PlatformID.Win32NT);

		[TestMethod]
		[DataRow(@"C:\foo\bar", @"\\Folder\<title>\[<id>]\\", @"C:\foo\bar\Folder\my_ book 000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000\[ID123456].txt", PlatformID.Win32NT)]
		[DataRow("/foo/bar", "/Folder/<title>/[<id>]/", @"/foo/bar/Folder/my: book 000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000/[ID123456].txt", PlatformID.Unix)]
		[DataRow(@"C:\foo\bar", @"\Folder\<title> [<id>]", @"C:\foo\bar\Folder\my_ book 0000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000 [ID123456].txt", PlatformID.Win32NT)]
		[DataRow("/foo/bar", "/Folder/<title> [<id>]", @"/foo/bar/Folder/my: book 0000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000 [ID123456].txt", PlatformID.Unix)]
		[DataRow(@"C:\foo\bar", @"\Folder\<title> <title> <title> <title> <title> <title> <title> <title> <title> [<id>]", @"C:\foo\bar\Folder\my_ book 0000000000000000 my_ book 0000000000000000 my_ book 0000000000000000 my_ book 0000000000000000 my_ book 0000000000000000 my_ book 0000000000000000 my_ book 0000000000000000 my_ book 00000000000000000 my_ book 00000000000000000 [ID123456].txt", PlatformID.Win32NT)]
		[DataRow("/foo/bar", "/Folder/<title> <title> <title> <title> <title> <title> <title> <title> <title> [<id>]", @"/foo/bar/Folder/my: book 0000000000000000 my: book 0000000000000000 my: book 0000000000000000 my: book 0000000000000000 my: book 0000000000000000 my: book 0000000000000000 my: book 0000000000000000 my: book 00000000000000000 my: book 00000000000000000 [ID123456].txt", PlatformID.Unix)]
		[DataRow(@"C:\foo\bar", @"\<title>\<title> [<id>]", @"C:\foo\bar\my_ book 000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000\my_ book 0000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000 [ID123456].txt", PlatformID.Win32NT)]
		[DataRow("/foo/bar", @"/<title>/<title> [<id>]", "/foo/bar/my: book 000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000/my: book 0000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000 [ID123456].txt", PlatformID.Unix)]
		public void Test_trim_to_max_path(string dirFullPath, string template, string expected, PlatformID platformID)
		{
			if (Environment.OSVersion.Platform != platformID)
				return;

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
				return;

			var sb = new System.Text.StringBuilder();
			sb.Append('0', 300);
			var longText = sb.ToString();
			Assert.ThrowsExactly<PathTooLongException>(() => NEW_GetValidFilename_FileNamingTemplate(baseDir, template, "my: book " + longText, "txt"));
		}

		private class TemplateTag : ITemplateTag
		{
			public string TagName { get; init; }
			public string DefaultValue { get; }
			public string Description { get; }
			public string Display { get; }
		}
		private static string NEW_GetValidFilename_FileNamingTemplate(string dirFullPath, string template, string title, string extension)
		{
			extension = FileUtility.GetStandardizedExtension(extension);

			var lbDto = GetLibraryBook();
			lbDto.TitleWithSubtitle = title;
			lbDto.AudibleProductId = "ID123456";

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
			lbDto.TitleWithSubtitle = suffix;

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
				lbDto.TitleWithSubtitle = @"s\l/a\s/h\e/s";

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
		static readonly ReplacementCharacters Default = ReplacementCharacters.Default(Environment.OSVersion.Platform == PlatformID.Win32NT);

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
