using AppScaffolding;
using AudibleApi;
using AudibleApi.Authorization;
using AudibleUtilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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
		var body = LicenseRecoveryGuidance.BuildSteps(Localization.Get("us"), status, hasAdditionalMarketplaces: true);
		Assert.AreEqual(upgrade, body.Contains(LicenseRecoveryGuidance.ReleasesUrl));
		Assert.AreEqual(conditional, body.Contains("If you're not on the latest version"));
		var ordered = new[] { "Deregister", "record its registration region", "all additional marketplaces", "remove the affected account", "Save the removal", "close Libation", "Reopen Libation", "verify the old account is absent", "re-add the account", "Scan and sign in", "check every recorded additional marketplace", "save both dialogs", "Scan again with all marketplaces restored", "retry the download" };
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
	public void Cli_instructions_record_and_restore_marketplaces_before_scanning()
	{
		var body = LicenseRecoveryGuidance.BuildSteps(Localization.Get("uk"), VersionCheckOutcome.UpToDate, cli: true, hasAdditionalMarketplaces: true);
		var ordered = new[] { "record its registration region", "all additional marketplaces", "Back up AccountsSettings.json", "remove only that account object", "Run login-external", "copy the saved AdditionalLocaleNames array", "Keep the new identity tokens", "verify Locale and Also scans", "Run scan" };
		var previous = -1;
		foreach (var instruction in ordered)
		{
			var position = body.IndexOf(instruction, StringComparison.Ordinal);
			Assert.IsTrue(position > previous, instruction);
			previous = position;
		}
		StringAssert.Contains(body, "\"AdditionalLocaleNames\": [\"us\"]");
	}

	[TestMethod]
	[DataRow(false, false)]
	[DataRow(false, true)]
	[DataRow(true, false)]
	[DataRow(true, true)]
	public void Additional_marketplace_advice_is_only_shown_for_the_affected_account(bool cli, bool hasAdditional)
	{
		var accounts = new AccountsSettings();
		var affected = accounts.Upsert("affected@example.com", "uk");
		if (hasAdditional) affected.AddMarketplace("us");
		// Another account's extra marketplaces must not affect this account's instructions.
		accounts.Upsert("other@example.com", "uk").AddMarketplace("us");
		var context = LicenseRecoveryGuidance.GetAccountContext(accounts, affected.AccountId, "uk");
		Assert.AreEqual(hasAdditional, context.HasAdditionalMarketplaces);
		var body = LicenseRecoveryGuidance.BuildSteps(context.Locale, VersionCheckOutcome.UpToDate, cli, context.HasAdditionalMarketplaces);
		Assert.AreEqual(hasAdditional, body.Contains("additional marketplaces"));
		Assert.AreEqual(cli && hasAdditional, body.Contains("AdditionalLocaleNames"));
		Assert.AreEqual(!cli && hasAdditional, body.Contains("save both dialogs"));
		StringAssert.Contains(body, cli ? "Run scan" : "Scan and sign in");
		Assert.IsFalse(LicenseRecoveryGuidance.GetAccountContext(accounts, "missing", "uk").HasAdditionalMarketplaces);
	}

	[TestMethod]
	[DataRow(false)]
	[DataRow(true)]
	public void Recreated_UK_account_resolves_US_title_after_restoring_and_persisting_marketplaces(bool cli)
	{
		var accounts = new AccountsSettings();
		var original = accounts.Upsert("person@example.com", "uk");
		original.AddMarketplace("us");
		var serializerSettings = Identity.GetJsonSerializerSettings();
		var backup = JObject.Parse(JsonConvert.SerializeObject(accounts, serializerSettings));
		var savedAdditionalNames = backup["Accounts"]![0]!["AdditionalLocaleNames"]!.DeepClone();
		var registrationRegion = original.Locale!.Name;
		Assert.IsTrue(accounts.Delete(original));
		var replacement = accounts.Upsert(original.AccountId, registrationRegion);
		Assert.AreNotSame(original.IdentityTokens, replacement.IdentityTokens);
		Assert.IsNull(accounts.GetAccount(original.AccountId, "us"), "Registration alone loses the additional marketplace.");

		string restoredJson;
		if (cli)
		{
			// Follow the documented JSON edit without restoring the old account/identity.
			var fresh = JObject.Parse(JsonConvert.SerializeObject(accounts, serializerSettings));
			fresh["Accounts"]![0]!["AdditionalLocaleNames"] = savedAdditionalNames;
			restoredJson = fresh.ToString();
		}
		else
		{
			// The GUI's Accounts dialog persists the selections through this method.
			replacement.SetAdditionalMarketplaces(savedAdditionalNames.ToObject<string[]>()!);
			restoredJson = JsonConvert.SerializeObject(accounts, serializerSettings);
		}
		var reloaded = JsonConvert.DeserializeObject<AccountsSettings>(restoredJson, serializerSettings)!;
		// Same account lookup used by FileLiberator.GetApiAsync for the US title.
		var credentialsForUS = reloaded.GetAccount(original.AccountId, "us");
		Assert.IsNotNull(credentialsForUS);
		Assert.AreEqual(registrationRegion, credentialsForUS.Locale!.Name);
		Assert.AreSame(reloaded.GetAccount(original.AccountId, registrationRegion)!.IdentityTokens, credentialsForUS.IdentityTokens);
		CollectionAssert.AreEquivalent(new[] { registrationRegion, "us" }, credentialsForUS.ScanLocales.Select(l => l.Name).ToArray());
		Assert.AreEqual(Localization.Get("uk"), LicenseRecoveryGuidance.GetRegistrationLocale(reloaded, original.AccountId, "us"));
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
