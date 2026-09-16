using AudibleApi;
using AudibleUtilities;
using DataLayer;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AppScaffolding;

/// <summary>Shared, offline recovery instructions for Audible license refusals.</summary>
public static class LicenseRecoveryGuidance
{
	public const string ReleasesUrl = "https://github.com/rmcrackan/Libation/releases/latest";
	public const string DocumentationUrl = "https://getlibation.com/docs/advanced/device-registration#how-to-register-an-account-again";
	public const string Explanation = "Audible changed something in early September 2026. This is our recommended workaround; we cannot be certain every step is necessary.";
	public const string Fallback = "If it still fails, wait 24 to 48 hours (sometimes a few days): Audible also imposes temporary rate limits and has outages. You may still be able to play the title in Audible's app or website. If problems persist after several days, report an issue on Libation's GitHub with logs.";

	public static Locale? GetRegistrationLocale(LibraryBook book)
	{
		try
		{
			using var persister = AudibleApiStorage.GetAccountsSettingsPersister();
			return GetRegistrationLocale(persister.AccountsSettings, book.Account, book.Book.Locale);
		}
		catch (Exception ex)
		{
			Serilog.Log.Logger.Warning(ex, "Unable to determine account region for license recovery guidance");
			return null;
		}
	}

	public static Locale? GetRegistrationLocale(AccountsSettings accounts, string accountId, string bookLocale)
		=> accounts.GetAccount(accountId, bookLocale)?.Locale;

	public static string DeviceManagementUrl(Locale? locale)
		=> $"https://www.amazon.{(string.IsNullOrWhiteSpace(locale?.CountryCode) || string.IsNullOrWhiteSpace(locale.TopDomain) ? "com" : locale.TopDomain)}/hz/mycd/digital-console/devicedetails?deviceFamily=AUDIBLE_APP";

	public static string BuildSteps(Locale? locale, VersionCheckOutcome updateStatus, bool cli = false)
	{
		var steps = new List<string>();
		var unknown = string.IsNullOrWhiteSpace(locale?.CountryCode);
		var caveat = unknown
			? " (US link; account region is unknown). For other regions, use your regional Amazon site: Manage Your Content and Devices > Devices > Audible."
			: !locale!.CountryCode.Equals("us", StringComparison.OrdinalIgnoreCase)
				? " (regional link is unverified). If it does not work, use your regional Amazon site: Manage Your Content and Devices > Devices > Audible."
				: ".";
		steps.Add($"Open {DeviceManagementUrl(locale)}{caveat} Deregister entries named 'Libation'.");
		if (updateStatus != VersionCheckOutcome.UpToDate)
			steps.Add((updateStatus == VersionCheckOutcome.UpdateAvailable
				? "Upgrade to the latest Libation: "
				: "If you're not on the latest version, upgrade: ") + ReleasesUrl);
		if (cli)
		{
			steps.Add("Stop Libation and any running CLI/Docker jobs. Back up AccountsSettings.json, then remove only the affected account object from its Accounts array and save. This preserves your library and downloaded files.");
			steps.Add("Run list-accounts to verify the old account is absent. Run login-external --account <email> --locale <registration-region> to re-add it and sign in.");
			steps.Add("Run scan, then retry with liberate <ASIN>.");
		}
		else
		{
			steps.Add("Go to Settings > Accounts and remove the affected account. This preserves your library and downloaded files. Save the removal, then close Libation.");
			steps.Add("Reopen Libation. In Settings > Accounts, verify the old account is absent, then re-add the account.");
			steps.Add("Scan and sign in, then retry the download (mark it Download Pending if needed).");
		}
		return string.Join(Environment.NewLine + Environment.NewLine, steps.Select((step, index) => $"{index + 1}. {step}"));
	}
}
