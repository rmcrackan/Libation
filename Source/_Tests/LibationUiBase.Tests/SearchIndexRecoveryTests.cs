using LibationUiBase;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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

	/// <summary>A damaged index fails on every library change, and these steps only need following once.</summary>
	[TestMethod]
	public void the_user_is_told_once_per_session()
	{
		Assert.IsTrue(SearchIndexRecovery.ShouldNotify());
		Assert.IsFalse(SearchIndexRecovery.ShouldNotify());
		Assert.IsFalse(SearchIndexRecovery.ShouldNotify());
	}
}
