using ApplicationServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace EnsureScanCanIdentifyInactiveBooksTests;

/// <summary>
/// "Remove books no longer in your account" decides from a single scan. A scan that could not see the whole
/// library would flag books the user still owns, so it has to be rejected rather than believed.
/// </summary>
[TestClass]
public class EnsureScanCanIdentifyInactiveBooks
{
	[TestMethod]
	public void a_complete_scan_is_accepted()
		=> LibraryCommands.EnsureScanCanIdentifyInactiveBooks(scannedItemCount: 434, existingBookCount: 12, failedAccounts: []);

	[TestMethod]
	public void a_null_failed_account_list_is_treated_as_no_failures()
		=> LibraryCommands.EnsureScanCanIdentifyInactiveBooks(scannedItemCount: 434, existingBookCount: 12, failedAccounts: null);

	[TestMethod]
	public void an_account_that_failed_to_scan_is_rejected()
	{
		var ex = Assert.ThrowsExactly<LibraryScanIncompleteException>(
			() => LibraryCommands.EnsureScanCanIdentifyInactiveBooks(434, 12, ["me@example.com"]));

		CollectionAssert.AreEqual(new[] { "me@example.com" }, ex.FailedAccounts.ToArray());
		StringAssert.Contains(ex.Message, "me@example.com");
	}

	[TestMethod]
	public void a_failed_account_is_rejected_even_when_other_accounts_returned_books()
	{
		// The books that did arrive prove nothing about the account that never answered.
		var ex = Assert.ThrowsExactly<LibraryScanIncompleteException>(
			() => LibraryCommands.EnsureScanCanIdentifyInactiveBooks(5000, 12, ["second@example.com"]));

		CollectionAssert.AreEqual(new[] { "second@example.com" }, ex.FailedAccounts.ToArray());
	}

	[TestMethod]
	public void every_failed_account_is_named()
	{
		var ex = Assert.ThrowsExactly<LibraryScanIncompleteException>(
			() => LibraryCommands.EnsureScanCanIdentifyInactiveBooks(0, 12, ["a@example.com", "b@example.com"]));

		CollectionAssert.AreEquivalent(new[] { "a@example.com", "b@example.com" }, ex.FailedAccounts.ToArray());
	}

	[TestMethod]
	public void a_scan_returning_nothing_is_rejected_when_books_are_at_stake()
	{
		var ex = Assert.ThrowsExactly<LibraryScanIncompleteException>(
			() => LibraryCommands.EnsureScanCanIdentifyInactiveBooks(0, 300, []));

		Assert.AreEqual(0, ex.FailedAccounts.Count);
		StringAssert.Contains(ex.Message, "300");
	}

	[TestMethod]
	public void a_scan_returning_nothing_is_fine_when_no_books_are_at_stake()
		=> LibraryCommands.EnsureScanCanIdentifyInactiveBooks(scannedItemCount: 0, existingBookCount: 0, failedAccounts: []);

	[TestMethod]
	public void a_single_scanned_item_is_enough_to_proceed()
		=> LibraryCommands.EnsureScanCanIdentifyInactiveBooks(scannedItemCount: 1, existingBookCount: 300, failedAccounts: []);
}
