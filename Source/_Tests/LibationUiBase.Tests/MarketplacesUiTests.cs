using AudibleApi;
using AudibleApi.Authorization;
using AudibleUtilities;
using LibationUiBase;

namespace LibationUiBase.Tests;

/// <summary>
/// The marketplaces summary reports on marketplaces that answered. A marketplace that could not be reached
/// says nothing about what is in it, and calling it empty would recreate the very silence the feature exists
/// to break - titles present, and no sign of them anywhere in the app.
/// </summary>
[TestClass]
public class MarketplacesUiTests
{
	private static Locale locale(string name) => Localization.Get(name);

	private static MarketplaceProbeResult found(string name, int count)
		=> new(locale(name), MarketplaceProbeOutcome.TitlesFound, count);

	private static MarketplaceProbeResult empty(string name)
		=> new(locale(name), MarketplaceProbeOutcome.Empty, 0);

	private static MarketplaceProbeResult failed(string name)
		=> new(locale(name), MarketplaceProbeOutcome.Failed, Error: "Request could not be authenticated");

	[TestMethod]
	public void nothing_checked_does_not_claim_there_is_nothing_to_find()
	{
		var summary = MarketplacesUi.Summary([failed("canada"), failed("uk")]);

		Assert.AreEqual(MarketplacesUi.NoneChecked, summary);
		Assert.IsFalse(summary.Contains("No other marketplace holds titles"));
	}

	[TestMethod]
	public void every_marketplace_answering_empty_is_a_real_answer()
		=> Assert.AreEqual(MarketplacesUi.NoneFound, MarketplacesUi.Summary([empty("canada"), empty("uk")]));

	[TestMethod]
	public void a_marketplace_that_could_not_be_checked_is_called_out_alongside_the_ones_that_could()
	{
		var summary = MarketplacesUi.Summary([empty("canada"), failed("uk")]);

		StringAssert.Contains(summary, MarketplacesUi.NoneFound);
		StringAssert.Contains(summary, "1 marketplace could not be checked");
	}

	[TestMethod]
	public void found_titles_are_named_with_their_counts()
	{
		var summary = MarketplacesUi.Summary([found("us", 50), empty("uk")]);

		StringAssert.Contains(summary, "us (50)");
		StringAssert.Contains(summary, "another marketplace");
	}

	[TestMethod]
	public void several_finds_are_counted()
	{
		var summary = MarketplacesUi.Summary([found("us", 50), found("uk", 3)]);

		StringAssert.Contains(summary, "2 other marketplaces");
		StringAssert.Contains(summary, "us (50)");
		StringAssert.Contains(summary, "uk (3)");
	}

	[TestMethod]
	public void a_find_still_reports_what_could_not_be_checked()
	{
		var summary = MarketplacesUi.Summary([found("us", 50), failed("uk"), failed("japan")]);

		StringAssert.Contains(summary, "us (50)");
		StringAssert.Contains(summary, "2 marketplaces could not be checked");
	}

	[TestMethod]
	public void one_title_is_not_reported_as_titles()
		=> StringAssert.Contains(
			MarketplacesUi.ResultText(found("us", 1)),
			"1 title");

	[TestMethod]
	public void a_reachable_marketplace_with_no_count_is_not_called_empty()
		=> StringAssert.Contains(
			MarketplacesUi.ResultText(new MarketplaceProbeResult(locale("us"), MarketplaceProbeOutcome.Empty)),
			"title count unknown");

	[TestMethod]
	public void the_scan_picker_names_every_marketplace_an_account_reads()
	{
		var account = new Account("user@example.com")
		{
			AccountName = "Mine",
			IdentityTokens = new Identity(Localization.Get("ca"))
		};
		account.AddMarketplace("us");

		var text = MarketplacesUi.ScanPickerText(account);

		StringAssert.Contains(text, "canada");
		StringAssert.Contains(text, "us");
	}

	[TestMethod]
	public void the_scan_picker_reads_as_it_always_did_for_a_single_marketplace_account()
	{
		var account = new Account("user@example.com")
		{
			AccountName = "Mine",
			IdentityTokens = new Identity(Localization.Get("ca"))
		};

		Assert.AreEqual("Mine (user@example.com - canada)", MarketplacesUi.ScanPickerText(account));
	}

	[TestMethod]
	public void the_accounts_grid_button_counts_marketplaces_only_when_there_is_more_than_one()
	{
		Assert.AreEqual("Marketplaces", MarketplacesUi.ButtonText(1));
		Assert.AreEqual("Marketplaces (3)", MarketplacesUi.ButtonText(3));
	}
}
