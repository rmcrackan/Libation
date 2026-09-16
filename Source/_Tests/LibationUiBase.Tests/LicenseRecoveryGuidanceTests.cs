using AppScaffolding;
using AudibleApi;
using AudibleApi.Authorization;
using AudibleUtilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LibationUiBase.Tests;

[TestClass]
public class LicenseRecoveryGuidanceTests
{
	[TestMethod]
	[DataRow("us", "com", false)]
	[DataRow("uk", "co.uk", true)]
	[DataRow("germany", "de", true)]
	public void Regional_links_use_locale_metadata_with_non_US_caveat(string name, string domain, bool caveat)
	{
		var locale = Localization.Get(name);
		Assert.AreEqual($"https://www.amazon.{domain}/hz/mycd/digital-console/devicedetails?deviceFamily=AUDIBLE_APP", LicenseRecoveryGuidance.DeviceManagementUrl(locale));
		var body = LicenseRecoveryGuidance.BuildSteps(locale, VersionCheckOutcome.UpToDate);
		Assert.AreEqual(caveat, body.Contains("regional link is unverified"));
		Assert.AreEqual(caveat, body.Contains("Manage Your Content and Devices"));
	}

	[TestMethod]
	public void Missing_or_empty_locale_uses_labeled_US_link_and_manual_route()
	{
		foreach (var locale in new Locale?[] { null, Locale.Empty })
		{
			var body = LicenseRecoveryGuidance.BuildSteps(locale, VersionCheckOutcome.UpToDate);
			StringAssert.Contains(body, "https://www.amazon.com/");
			StringAssert.Contains(body, "US link; account region is unknown");
			StringAssert.Contains(body, "Manage Your Content and Devices");
		}
	}

	[TestMethod]
	[DataRow(VersionCheckOutcome.UpToDate, false, false)]
	[DataRow(VersionCheckOutcome.UpdateAvailable, true, false)]
	[DataRow(VersionCheckOutcome.UnableToDetermine, true, true)]
	public void Upgrade_step_reflects_known_status_and_keeps_steps_in_order(VersionCheckOutcome status, bool upgrade, bool conditional)
	{
		var body = LicenseRecoveryGuidance.BuildSteps(Localization.Get("us"), status);
		Assert.AreEqual(upgrade, body.Contains(LicenseRecoveryGuidance.ReleasesUrl));
		Assert.AreEqual(conditional, body.Contains("If you're not on the latest version"));
		var ordered = new[] { "Deregister", "remove the affected account", "Save the removal", "close Libation", "Reopen Libation", "verify the old account is absent", "re-add the account", "Scan and sign in" };
		var previous = -1;
		foreach (var instruction in ordered)
		{
			var position = body.IndexOf(instruction, StringComparison.Ordinal);
			Assert.IsTrue(position > previous, instruction);
			previous = position;
		}
		StringAssert.Contains(body, "preserves your library and downloaded files");
		StringAssert.Contains(body, upgrade ? "5. Scan" : "4. Scan");
		if (upgrade)
			Assert.IsTrue(body.IndexOf(LicenseRecoveryGuidance.ReleasesUrl) < body.IndexOf("remove the affected account"));
	}

	[TestMethod]
	public void Additional_marketplace_uses_owning_accounts_registration_region()
	{
		var accounts = new AccountsSettings();
		var account = new Account("person@example.com") { IdentityTokens = new Identity(Localization.Get("uk")) };
		account.AddMarketplace("us");
		accounts.Add(account);
		accounts.Add(new Account("other@example.com") { IdentityTokens = new Identity(Localization.Get("us")) });
		var locale = LicenseRecoveryGuidance.GetRegistrationLocale(accounts, "PERSON@example.com", "us");
		Assert.AreEqual(account.Locale, locale);
		StringAssert.Contains(LicenseRecoveryGuidance.DeviceManagementUrl(locale), "amazon.co.uk/");
		Assert.IsNull(LicenseRecoveryGuidance.GetRegistrationLocale(accounts, "missing", "us"));
	}
}
