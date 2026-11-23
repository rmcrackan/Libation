using System;
using System.Collections.Generic;
using System.Linq;
using AssertionHelper;
using LibationSearchEngine;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SearchEngineTests;

[TestClass]
public class FieldNameUniquenessTests
{
	[TestMethod]
	public void NoFieldNameIsClaimedTwice()
	{
		var duplicates = SearchEngine.FieldIndexRules
			.SelectMany(rule => rule.FieldNames)
			.GroupBy(name => name, StringComparer.OrdinalIgnoreCase)
			.Where(g => g.Count() > 1)
			.Select(g => g.Key)
			.ToList();

		duplicates.Should().BeEquivalentTo(new List<string>());
	}
}
