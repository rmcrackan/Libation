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
using Microsoft.VisualStudio.TestPlatform.Common.Filtering;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TestCommon;

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
		[DataRow("[foo]", "tags:foo")]
		[DataRow("  [foo]", "  tags:foo")]
		[DataRow("[foo]  ", "tags:foo  ")]
		[DataRow("  [foo]  ", "  tags:foo  ")]
		[DataRow("-[foo]", "-tags:foo")]
		[DataRow("  -[foo]", "  -tags:foo")]
		[DataRow("-[foo]  ", "-tags:foo  ")]
		[DataRow("  -[foo]  ", "  -tags:foo  ")]

		// tag case irrelevant
		[DataRow("[FoO]", "tags:FoO")]

		// bool keyword surrounded by spaces
		[DataRow("israted", "israted:True")]
		[DataRow("  israted", "  israted:True")]
		[DataRow("israted  ", "israted:True  ")]
		[DataRow("  israted  ", "  israted:True  ")]
		[DataRow("-israted", "-israted:True")]
		[DataRow("  -israted", "  -israted:True")]
		[DataRow("-israted  ", "-israted:True  ")]
		[DataRow("  -israted  ", "  -israted:True  ")]

		// bool keyword. Append :True
		[DataRow("israted", "israted:True")]

		// bool keyword with [:bool]. Do not add :True
		[DataRow("israted:True", "israted:True")]
		[DataRow("isRated:false", "israted:false")]

		// tag which happens to be a bool keyword >> parse as tag
		[DataRow("[israted]", "tags:israted")]

		// numbers with "to". TO all caps, numbers [8.2] format
		[DataRow("1 to 10", "00000001.00 TO 00000010.00")]
		[DataRow("19990101 to 20001231", "19990101.00 TO 20001231.00")]

		// field to lowercase
		[DataRow("Author:Doyle", "author:Doyle")]
		// bool field to lowercase
		[DataRow("IsRated", "israted:True")]
		[DataRow("-isRATED", "-israted:True")]

		public void FormattingTest(string input, string output)
			=> SearchEngine.FormatSearchQuery(input).Should().Be(output);
	}
}
