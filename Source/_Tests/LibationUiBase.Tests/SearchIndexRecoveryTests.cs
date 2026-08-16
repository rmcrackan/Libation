using LibationUiBase;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.Store;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

namespace LibationUiBase.Tests;

[TestClass]
public class SearchIndexRecoveryTests
{
	[TestMethod]
	public void instructions_name_the_folder_and_how_to_find_it()
	{
		StringAssert.Contains(SearchIndexRecovery.ManualRecoveryInstructions, "SearchEngine");
		StringAssert.Contains(SearchIndexRecovery.ManualRecoveryInstructions, "Open log folder");
		// the whole point of the guard is that the library survived
		StringAssert.Contains(SearchIndexRecovery.ManualRecoveryInstructions, "library itself is fine");
	}

	/// <summary>
	/// Every way of failing to reach the index. Reporting one of these as a bad filter string sends the user
	/// looking for a typo in a query that was fine.
	/// </summary>
	[TestMethod]
	public void failures_to_reach_the_index_are_told_apart_from_a_bad_query()
	{
		Assert.IsTrue(SearchIndexRecovery.IsIndexUnavailable(new IOException("read past EOF")));
		Assert.IsTrue(SearchIndexRecovery.IsIndexUnavailable(new CorruptIndexException("checksum mismatch in segments file")));
		Assert.IsTrue(SearchIndexRecovery.IsIndexUnavailable(new LockObtainFailedException("Lock obtain timed out")));
		Assert.IsTrue(SearchIndexRecovery.IsIndexUnavailable(new UnauthorizedAccessException()));
		// cloud-sync debris, which Lucene reports while parsing segments file names
		Assert.IsTrue(SearchIndexRecovery.IsIndexUnavailable(new ArgumentException("Invalid or unsupported character in number: )")));

		Assert.IsFalse(SearchIndexRecovery.IsIndexUnavailable(new ParseException("Cannot parse '[unclosed'")));
		Assert.IsFalse(SearchIndexRecovery.IsIndexUnavailable(new ArgumentException("some other argument problem")));
	}

	/// <summary>A damaged index fails on every library change, and these steps only need following once.</summary>
	[TestMethod]
	public void the_user_is_told_once_per_session()
	{
		Assert.IsTrue(SearchIndexRecovery.ShouldNotify());
		Assert.IsFalse(SearchIndexRecovery.ShouldNotify());
		Assert.IsFalse(SearchIndexRecovery.ShouldNotify());
	}
}
