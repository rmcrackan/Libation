using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Dinah.Core;
using FluentAssertions;
using FluentAssertions.Common;
using LibationSearchEngine;
using Lucene.Net.Analysis.Standard;
using Microsoft.VisualStudio.TestPlatform.Common.Filtering;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SearchEngineTests
{
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

		// bool keyword. Append :True
		[DataRow("israted", "israted:True")]

		// bool keyword with [:bool]. Do not add :True
		[DataRow("israted:True", "israted:True")]
		[DataRow("isRated:false", "israted:false")]
		[DataRow("liberated AND isRated:false", "liberated:True AND israted:false")]

		// tag which happens to be a bool keyword >> parse as tag
		[DataRow("[israted]", "tags:israted ")]
		[DataRow("[tags]    [israted] [tags] [tags]  [isliberated] [israted]   ", "tags:tags     tags:israted  tags:tags  tags:tags   tags:isliberated  tags:israted    ")]
		[DataRow("[tags][israted]", "tags:tags tags:israted ")]

		// numbers with "to". TO all caps, numbers [8.2] format
		[DataRow("1 to 10", "00000001.00 TO 00000010.00")]
		[DataRow("19990101 to 20001231", "19990101.00 TO 20001231.00")]

		// field to lowercase
		[DataRow("Author:Doyle", "author:Doyle")]
		// bool field to lowercase
		[DataRow("IsRated", "israted:True")]
		[DataRow("-isRATED", "-israted:True")]

		public void FormattingTest(string input, string output)
		{
			using var analyzer = new StandardAnalyzer(SearchEngine.Version);

			QuerySanitizer.Sanitize(input, analyzer).Should().Be(output);
		}
	}
}
