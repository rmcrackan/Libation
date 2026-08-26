using AudibleUtilities;
using System.Collections.Generic;
using System.Linq;

namespace LibationUiBase;

/// <summary>
/// Shared copy for the marketplaces feature, so Classic and Chardonnay say the same thing about the same
/// state. The wording assumes the reader does not know a marketplace other than their own can hold titles -
/// that is the whole reason they are here.
/// </summary>
public static class MarketplacesUi
{
	public const string DialogTitle = "Marketplaces";

	public const string Intro
		= "Audible keeps a separate library for each marketplace. A title bought while your Amazon address was "
		+ "set to another country stays in that country's library, and Libation only sees the marketplaces "
		+ "listed here.\r\n\r\nChecking one adds it to this account's library scans. No second login is needed - "
		+ "your existing credentials work in every marketplace.";

	public const string CheckButton = "Check other marketplaces";

	public const string Checking = "Checking marketplaces...";

	public const string NotAuthenticatedToolTip
		= "Scan this account's library first, so Libation has credentials to check the other marketplaces with.";

	public const string ButtonToolTip
		= "See whether this account holds titles in other Audible marketplaces.";

	public const string NoneFound
		= "No other marketplace holds titles for this account.";

	public const string NoneChecked
		= "No marketplace could be checked. Scan this account's library to refresh its credentials, then try again.";

	/// <summary>Label for the accounts grid button: the account's marketplaces, at a glance.</summary>
	public static string ButtonText(int marketplaceCount)
		=> marketplaceCount > 1 ? $"{DialogTitle} ({marketplaceCount})" : DialogTitle;

	/// <summary>One probed marketplace, as a line in the list.</summary>
	public static string ResultText(MarketplaceProbeResult result)
		=> result.Outcome switch
		{
			MarketplaceProbeOutcome.AlreadyScanned
				=> $"{result.Locale.Name} - already scanned by this account",
			MarketplaceProbeOutcome.ScannedByAnotherAccount
				=> $"{result.Locale.Name} - already scanned by {result.ClaimedBy}",
			MarketplaceProbeOutcome.TitlesFound when result.TitleCount == 1
				=> $"{result.Locale.Name} - 1 title",
			MarketplaceProbeOutcome.TitlesFound
				=> $"{result.Locale.Name} - {result.TitleCount} titles",
			MarketplaceProbeOutcome.Empty when result.TitleCount == 0
				=> $"{result.Locale.Name} - no titles",
			MarketplaceProbeOutcome.Empty
				=> $"{result.Locale.Name} - reachable, title count unknown",
			_ => $"{result.Locale.Name} - could not be checked ({result.Error})"
		};

	/// <summary>
	/// How an account's marketplaces read in a list of accounts. The scan picker in particular has to show
	/// these: one checkbox there can scan several marketplaces, which would otherwise be invisible.
	/// </summary>
	public static string MarketplacesSuffix(Account account)
	{
		var extras = account.AdditionalLocales;
		if (extras.Count == 0)
			return account.Locale?.Name ?? "";

		return string.Join(", ", new[] { account.Locale?.Name ?? "" }.Concat(extras.Select(l => l.Name)));
	}

	/// <summary>The row text used by both frontends' scan pickers.</summary>
	public static string ScanPickerText(Account account)
		=> $"{account.AccountName} ({account.AccountId} - {MarketplacesSuffix(account)})";

	/// <summary>
	/// What a probe turned up. A marketplace that could not be reached says nothing about what is in it, so
	/// failures are never folded into "nothing found" - reporting an unasked marketplace as empty is the exact
	/// silence this feature exists to break.
	/// </summary>
	public static string Summary(IEnumerable<MarketplaceProbeResult> results)
	{
		var all = results.ToList();

		var found = all.Where(r => r.Outcome is MarketplaceProbeOutcome.TitlesFound).ToList();
		var failed = all.Count(r => r.Outcome is MarketplaceProbeOutcome.Failed);
		var answered = all.Count(r => r.Outcome is MarketplaceProbeOutcome.TitlesFound or MarketplaceProbeOutcome.Empty);

		var unchecked_ = failed == 0
			? ""
			: $"\r\n\r\n{failed} marketplace{(failed == 1 ? "" : "s")} could not be checked, so what they hold is unknown.";

		if (found.Count == 0)
			return answered == 0
				? NoneChecked
				: NoneFound + unchecked_;

		var list = string.Join(", ", found.Select(r => $"{r.Locale.Name} ({r.TitleCount})"));
		var lead = found.Count == 1
			? $"Found titles in another marketplace: {list}.\r\n\r\nCheck it to include it in library scans."
			: $"Found titles in {found.Count} other marketplaces: {list}.\r\n\r\nCheck the ones to include in library scans.";

		return lead + unchecked_;
	}
}
