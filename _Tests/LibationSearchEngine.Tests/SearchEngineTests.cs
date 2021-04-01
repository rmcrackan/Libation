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
		[DataRow(null, SearchEngine.ALL_QUERY)]
		[DataRow("", SearchEngine.ALL_QUERY)]
		[DataRow("   ", SearchEngine.ALL_QUERY)]
		[DataRow("israted", "israted:True")]
		[DataRow("israted:True", "israted:True")]
		[DataRow("isRated:false", "israted:false")]
		[DataRow("[israted]", "tags:israted")]
		[DataRow("1 to 10", "00000001.00 TO 00000010.00")]
		[DataRow("19990101 to 20001231", "19990101.00 TO 20001231.00")]
		public void FormattingTest(string input, string output)
			=> SearchEngine.FormatSearchQuery(input).Should().Be(output);
	}
}
