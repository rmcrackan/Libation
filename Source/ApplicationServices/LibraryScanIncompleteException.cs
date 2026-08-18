using Dinah.Core;
using System;
using System.Collections.Generic;

namespace ApplicationServices;

/// <summary>
/// A library scan finished without throwing but cannot be trusted to say what is no longer in the account.
/// Deciding "inactive" from such a scan would offer to remove books the user still owns.
/// </summary>
public class LibraryScanIncompleteException : Exception
{
	/// <summary>Accounts that could not be scanned, if that is why the scan is untrustworthy.</summary>
	public IReadOnlyCollection<string> FailedAccounts { get; }

	public LibraryScanIncompleteException(string message, IReadOnlyCollection<string>? failedAccounts = null)
		: base(message)
		=> FailedAccounts = failedAccounts ?? [];

	internal static LibraryScanIncompleteException ForFailedAccounts(IReadOnlyCollection<string> failedAccounts)
		=> new(
			$"{"account".PluralizeWithCount(failedAccounts.Count)} could not be scanned, so Libation cannot tell which books are no longer in your library: "
			+ string.Join(", ", failedAccounts),
			failedAccounts);

	internal static LibraryScanIncompleteException ForEmptyScan(int existingBookCount)
		=> new(
			$"The library scan returned no books while Libation still holds {"book".PluralizeWithCount(existingBookCount)}. "
			+ "Treating that as an empty Audible library would offer to remove books you still own.");
}
