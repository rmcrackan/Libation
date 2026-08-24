using System.Globalization;
using AssertionHelper;
using LibationSearchEngine;
using Lucene.Net.Analysis.Standard;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[assembly: Parallelize]

namespace SearchEngineTests;

[TestClass]
public class FormatSearchQuery
{
	[TestMethod]
	// null, empty, whitespace -- *:*
	[DataRow(null, "*:*")]
	[DataRow("", "*:*")]
	[DataRow("   ", "*:*")]

	// tag surrounded by spaces
	[DataRow("[foo]", "tags:foo ")]
	[DataRow("  [foo]", "  tags:foo ")]
	[DataRow("  [   foo   ]", "  tags:foo ")]
	[DataRow("[foo]  ", "tags:foo   ")]
	[DataRow("  [foo]  ", "  tags:foo   ")]
	[DataRow("-[foo]", "-tags:foo ")]
	[DataRow("  -[foo]", "  -tags:foo ")]
	[DataRow("-[foo]  ", "-tags:foo   ")]
	[DataRow("  -[foo]  ", "  -tags:foo   ")]
	[DataRow("[foo_bar]", "tags:foo_bar ")]
	[DataRow("-[foo_bar]", "-tags:foo_bar ")]
	[DataRow("[foo_bar] [foo_bar2]", "tags:foo_bar  tags:foo_bar2 ")]

	// tag case irrelevant
	[DataRow("[FoO]", "tags:FoO ")]

	// bool keyword surrounded by spaces
	[DataRow("israted", "israted:True")]
	[DataRow("  israted", "  israted:True")]
	[DataRow("israted  ", "israted:True  ")]
	[DataRow("  israted  ", "  israted:True  ")]
	[DataRow("-israted", "-israted:True")]
	[DataRow("  -israted", "  -israted:True")]
	[DataRow("-israted  ", "-israted:True  ")]
	[DataRow("  -israted  ", "  -israted:True  ")]

	//ID Tags to lowercase and not parsed as numbers
	[DataRow("id:0000000123", "id:0000000123")]
	[DataRow("id:B000000123", "id:b000000123")]
	[DataRow("ASIN:B000000123", "asin:b000000123")]
	[DataRow("AudibleProductId:B000000123", "audibleproductid:b000000123")]
	[DataRow("ProductId:B000000123", "productid:b000000123")]

	// bool keyword with [:bool]. Do not add :True
	[DataRow("israted:True", "israted:True")]
	[DataRow("isRated:false", "israted:false")]
	[DataRow("liberated AND isRated:false", "liberated:True AND israted:false")]

	// tag which happens to be a bool keyword >> parse as tag
	[DataRow("[israted]", "tags:israted ")]
	[DataRow("[tags]    [israted] [tags] [tags]  [isliberated] [israted]   ", "tags:tags     tags:israted  tags:tags  tags:tags   tags:isliberated  tags:israted    ")]
	[DataRow("[tags][israted]", "tags:tags tags:israted ")]

	// numbers with "to". TO all caps, numbers [8.2] format
	[DataRow("1 to 10", "(1 OR 00000001.00) TO (10 OR 00000010.00)")]
	[DataRow("19990101 to 20001231", "(19990101 OR 19990101.00) TO (20001231 OR 20001231.00)")]

	// a number field is indexed zero-padded so a range sorts, so its values are padded to match
	[DataRow("LengthInMinutes:600", "lengthinminutes:00000600.00")]
	[DataRow("Rating:[1 to 5]", "rating:[00000001.00 TO 00000005.00]")]
	[DataRow("DatePublished:19990101", "datepublished:19990101.00")]
	// every other field is indexed as written, so padding its values found nothing
	[DataRow("title:1984", "title:1984")]
	[DataRow("title:\"2001 a space odyssey\"", "title:\"2001 a space odyssey\"")]
	[DataRow("[14]", "tags:14 ")]
	// no field named, so the default field holds both spellings and both are searched
	[DataRow("14", "(14 OR 00000014.00)")]
	[DataRow("-1984", "-(1984 OR 00001984.00)")]
	[DataRow("1984 AND liberated", "(1984 OR 00001984.00) AND liberated:True")]
	// a bare range still needs the padded spelling, and cannot hold a disjunction
	[DataRow("[1 to 10]", "[00000001.00 TO 00000010.00]")]
	// nor can a phrase, which is text
	[DataRow("\"2001 a space odyssey\"", "\"2001 a space odyssey\"")]

	// subtitle keywords are bool fields, not text fields
	[DataRow("HasSubtitle", "hassubtitle:True")]
	[DataRow("-TitleHasColon", "-titlehascolon:True")]
	[DataRow("HasSubtitle OR TitleHasColon", "hassubtitle:True OR titlehascolon:True")]

	// field to lowercase
	[DataRow("Author:Doyle", "author:Doyle")]
	// bool field to lowercase
	[DataRow("IsRated", "israted:True")]
	[DataRow("-isRATED", "-israted:True")]

	// a value which happens to be named like a search field stays a value. Lucene cannot parse a second
	// colon, so "title:absent" used to throw rather than search titles for the word
	[DataRow("title:absent", "title:absent")]
	[DataRow("Title:Absent", "title:Absent")]
	[DataRow("category:podcast", "category:podcast")]
	[DataRow("author:plus", "author:plus")]
	[DataRow("title:\"absent friends\"", "title:\"absent friends\"")]
	[DataRow("-title:absent", "-title:absent")]
	// a bool field keeps its own handling, including a value that is not a bool
	[DataRow("israted:absent", "israted:absent")]
	// only the term right after the colon is a value. The next one is a field again
	[DataRow("title:absent absent", "title:absent absent:True")]
	[DataRow("title:absent AND liberated", "title:absent AND liberated:True")]

	public void FormattingTest(string input, string output)
	{
		CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;

		using var analyzer = new StandardAnalyzer(SearchEngine.Version);

		QuerySanitizer.Sanitize(input, analyzer).Should().Be(output);
	}
}
