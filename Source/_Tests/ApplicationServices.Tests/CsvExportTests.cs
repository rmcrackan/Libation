using ApplicationServices;
using AudibleApi.Common;
using DataLayer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using ContentType = DataLayer.ContentType;

namespace CsvExportTests;

/// <summary>
/// Golden-output tests for the internal CSV writer that replaced CsvHelper. The expected
/// strings below are byte-for-byte what CsvHelper 33.1.0 produced for the same input:
/// delimiter is the culture's list separator, records end with CRLF (including the last),
/// fields containing the delimiter, quotes, newlines, or leading/trailing spaces are quoted
/// with embedded quotes doubled, and values are formatted with the culture.
/// </summary>
[TestClass]
public class CsvExportTests
{
	private static readonly DateTimeOffset Created = new(2026, 8, 26, 13, 5, 7, TimeSpan.Zero);
	private static readonly DateTimeOffset Modified = new(2026, 8, 26, 14, 6, 8, TimeSpan.Zero);

	private static List<IRecord> allRecordTypes() =>
	[
		new Clip(Created, TimeSpan.FromMilliseconds(1234.5), "anno-1", Modified, TimeSpan.FromMilliseconds(5678), "text with, comma", "title with \"quotes\""),
		new Note(Created, TimeSpan.FromMilliseconds(1000), "anno-2", Modified, TimeSpan.FromMilliseconds(2000), "line1\nline2"),
		new Note(Created, TimeSpan.FromMilliseconds(1000), null, Modified, TimeSpan.FromMilliseconds(2000), null),
		new Bookmark(Created, TimeSpan.FromMilliseconds(3000), " leading space", Modified),
		new LastHeard(Created, TimeSpan.FromMilliseconds(4000)),
	];

	private static List<LibraryBook> library()
	{
		var contributor = Contributor.GetEmpty();
		var book1 = new Book(new AudibleProductId("B0TEST0001"), "Title, with comma", "Sub \"quoted\"", "Desc line1\nline2", 123, ContentType.Product, [contributor], [contributor], "us");
		book1.UpdateProductRating(4.5f, 3.5f, 0f);
		book1.UpdateBookDetails(true, true, new DateTime(2020, 1, 2), "English");
		var book2 = new Book(new AudibleProductId("B0TEST0002"), "Plain", null, null, 45, ContentType.Product, [contributor], [contributor], "us");
		return
		[
			new LibraryBook(book1, new DateTime(2026, 8, 15, 10, 30, 0), "account@example.com"),
			new LibraryBook(book2, new DateTime(2026, 8, 16, 11, 45, 30), "acct;semi"),
		];
	}

	private static string export(Action<string> exportToPath, CultureInfo culture)
	{
		var originalCulture = CultureInfo.CurrentCulture;
		var path = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".csv");
		try
		{
			CultureInfo.CurrentCulture = culture;
			exportToPath(path);
			return File.ReadAllText(path);
		}
		finally
		{
			CultureInfo.CurrentCulture = originalCulture;
			File.Delete(path);
		}
	}

	private const string RecordsHeader = "Type,Created,Start,AnnotationId,LastModified,End,Text,Title,RecordType";

	[TestMethod]
	public void records_of_every_type_match_the_csvhelper_golden_output()
	{
		var csv = export(path => RecordExporter.ToCsv(path, allRecordTypes()), CultureInfo.InvariantCulture);

		// Header comes from the present record type with the most properties (Clip); rows of
		// other types simply have fewer fields. Both quirks are inherited from the old code.
		var expected =
			RecordsHeader + "\r\n" +
			"Clip,08/26/2026 13:05:07 +00:00,00:00:01.2345000,anno-1,08/26/2026 14:06:08 +00:00,00:00:05.6780000,\"text with, comma\",\"title with \"\"quotes\"\"\",clip\r\n" +
			"Note,08/26/2026 13:05:07 +00:00,00:00:01,anno-2,08/26/2026 14:06:08 +00:00,00:00:02,\"line1\nline2\",note\r\n" +
			"Note,08/26/2026 13:05:07 +00:00,00:00:01,,08/26/2026 14:06:08 +00:00,00:00:02,,note\r\n" +
			"Bookmark,08/26/2026 13:05:07 +00:00,00:00:03,\" leading space\",08/26/2026 14:06:08 +00:00,bookmark\r\n" +
			"LastHeard,08/26/2026 13:05:07 +00:00,00:00:04,last_heard\r\n";

		Assert.AreEqual(expected, csv);
	}

	[TestMethod]
	public void the_header_shrinks_to_the_widest_record_type_present()
	{
		var records = new List<IRecord> { new LastHeard(Created, TimeSpan.FromMilliseconds(4000)) };
		var csv = export(path => RecordExporter.ToCsv(path, records), CultureInfo.InvariantCulture);

		var expected =
			"Type,Created,Start,RecordType\r\n" +
			"LastHeard,08/26/2026 13:05:07 +00:00,00:00:04,last_heard\r\n";

		Assert.AreEqual(expected, csv);
	}

	private const string LibraryHeader =
		"Account,Date Added to library,Is Audible Plus?,Absent from last scan?,Audible Product Id,Locale,Title,Subtitle,Authors,Narrators," +
		"Length In Minutes,Description,Publisher,Has PDF,Series Names,Series Order,Community Rating: Overall,Community Rating: Performance," +
		"Community Rating: Story,Cover Id,Cover Id Large,Is Abridged?,Date Published,Categories,My Rating: Overall,My Rating: Performance," +
		"My Rating: Story,My Libation Tags,Book Liberated Status,PDF Liberated Status,Content Type,Language,Last Downloaded," +
		"Last Downloaded Version,Is Finished?,Is Spatial?,Included Until,Last Downloaded File Version,Last Downloaded Codec," +
		"Last Downloaded Sample rate,Last Downloaded Audio Channels,Last Downloaded Bitrate";

	[TestMethod]
	public void a_library_export_matches_the_csvhelper_golden_output()
	{
		var csv = export(path => LibraryExporter.ToCsv(path, library()), CultureInfo.InvariantCulture);

		var expected =
			LibraryHeader + "\r\n" +
			"account@example.com,08/15/2026 10:30:00,False,False,B0TEST0001,us,\"Title, with comma\",\"Sub \"\"quoted\"\"\",,,123,\"Desc line1\nline2\",,False,,,4.5,3.5,,,,True,01/02/2020 00:00:00,,,,,,NotLiberated,,Product,English,,,False,True,,,,,,\r\n" +
			"acct;semi,08/16/2026 11:45:30,False,False,B0TEST0002,us,Plain,,,,45,,,False,,,,,,,,False,,,,,,,NotLiberated,,Product,,,,False,False,,,,,,\r\n";

		Assert.AreEqual(expected, csv);
	}

	/// <summary>
	/// Like CsvHelper, the writer takes the delimiter from the culture (";" for de-DE), which in
	/// turn changes what needs quoting: a comma no longer does, a semicolon now does. Dates are
	/// formatted with the culture, so they are computed rather than hard-coded (their exact
	/// pattern depends on the ICU version).
	/// </summary>
	[TestMethod]
	public void a_semicolon_list_separator_culture_changes_the_delimiter_and_quoting()
	{
		var culture = CultureInfo.GetCultureInfo("de-DE");
		var csv = export(path => LibraryExporter.ToCsv(path, library()), culture);

		var dateAdded1 = new DateTime(2026, 8, 15, 10, 30, 0).ToString(culture);
		var dateAdded2 = new DateTime(2026, 8, 16, 11, 45, 30).ToString(culture);
		var datePublished = new DateTime(2020, 1, 2).ToString(culture);

		var expected =
			LibraryHeader.Replace(',', ';') + "\r\n" +
			$"account@example.com;{dateAdded1};False;False;B0TEST0001;us;Title, with comma;\"Sub \"\"quoted\"\"\";;;123;\"Desc line1\nline2\";;False;;;4,5;3,5;;;;True;{datePublished};;;;;;NotLiberated;;Product;English;;;False;True;;;;;;\r\n" +
			$"\"acct;semi\";{dateAdded2};False;False;B0TEST0002;us;Plain;;;;45;;;False;;;;;;;;False;;;;;;;NotLiberated;;Product;;;;False;False;;;;;;\r\n";

		Assert.AreEqual(expected, csv);
	}
}
