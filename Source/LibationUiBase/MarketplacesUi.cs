using AudibleUtilities;
using System.Collections.Generic;
using System.Linq;
using LibationUiBase.Resources;

namespace LibationUiBase;

/// <summary>
/// Shared copy for the marketplaces feature, so Classic and Chardonnay say the same thing about the same
/// state. The wording assumes the reader does not know a marketplace other than their own can hold titles -
/// that is the whole reason they are here.
/// </summary>
public static class MarketplacesUi
{
	public static string DialogTitle => Strings.MarketplacesUi_Marketplaces;

	public static string Intro
		=> Strings.MarketplacesUi_AudibleKeepsASeparateLibraryFor;

	public static string CheckButton => Strings.MarketplacesUi_CheckOtherMarketplaces;

	public static string Checking => Strings.MarketplacesUi_CheckingMarketplaces;

	public static string NotAuthenticatedToolTip
		=> Strings.MarketplacesUi_ScanThisAccountSLibraryFirst;

	public static string ButtonToolTip
		=> Strings.MarketplacesUi_SeeWhetherThisAccountHoldsTitles;

	public static string NoneFound
		=> Strings.MarketplacesUi_NoOtherMarketplaceHoldsTitlesFor;

	public static string NoneChecked
		=> Strings.MarketplacesUi_NoMarketplaceCouldBeCheckedScan;

	/// <summary>Label for the accounts grid button: the account's marketplaces, at a glance.</summary>
	public static string ButtonText(int marketplaceCount)
		=> marketplaceCount > 1 ? $"{DialogTitle} ({marketplaceCount})" : DialogTitle;

	/// <summary>One probed marketplace, as a line in the list.</summary>
	public static string ResultText(MarketplaceProbeResult result)
		=> result.Outcome switch
		{
			MarketplaceProbeOutcome.AlreadyScanned
				=> string.Format(Strings.MarketplacesUi_AlreadyScannedByThisAccount, result.Locale.Name),
			MarketplaceProbeOutcome.ScannedByAnotherAccount
				=> string.Format(Strings.MarketplacesUi_AlreadyScannedBy, result.Locale.Name, result.ClaimedBy),
			MarketplaceProbeOutcome.TitlesFound when result.TitleCount == 1
				=> string.Format(Strings.MarketplacesUi_1Title, result.Locale.Name),
			MarketplaceProbeOutcome.TitlesFound
				=> string.Format(Strings.MarketplacesUi_Titles, result.Locale.Name, result.TitleCount),
			MarketplaceProbeOutcome.Empty when result.TitleCount == 0
				=> string.Format(Strings.MarketplacesUi_NoTitles, result.Locale.Name),
			MarketplaceProbeOutcome.Empty
				=> string.Format(Strings.MarketplacesUi_ReachableTitleCountUnknown, result.Locale.Name),
			_ => string.Format(Strings.MarketplacesUi_CouldNotBeChecked, result.Locale.Name, result.Error)
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
			: "\r\n\r\n" + string.Format(failed == 1 ? Strings.MarketplacesUi_MarketplaceCouldNotBeChecked : Strings.MarketplacesUi_MarketplacesCouldNotBeChecked, failed);

		if (found.Count == 0)
			return answered == 0
				? NoneChecked
				: NoneFound + unchecked_;

		var list = string.Join(", ", found.Select(r => $"{r.Locale.Name} ({r.TitleCount})"));
		var lead = found.Count == 1
			? string.Format(Strings.MarketplacesUi_FoundTitlesInAnotherMarketplaceCheck, list)
			: string.Format(Strings.MarketplacesUi_FoundTitlesInOtherMarketplacesCheck, found.Count, list);

		return lead + unchecked_;
	}
}
