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

	public static (Locale? Locale, bool HasAdditionalMarketplaces) GetAccountContext(LibraryBook book)
	{
		try
		{
			using var persister = AudibleApiStorage.GetAccountsSettingsPersister();
			return GetAccountContext(persister.AccountsSettings, book.Account, book.Book.Locale);
		}
		catch (Exception ex)
		{
			Serilog.Log.Logger.Warning(ex, "Unable to determine account region for license recovery guidance");
			return (null, false);
		}
	}

	public static Locale? GetRegistrationLocale(AccountsSettings accounts, string accountId, string bookLocale)
		=> GetAccountContext(accounts, accountId, bookLocale).Locale;

	public static (Locale? Locale, bool HasAdditionalMarketplaces) GetAccountContext(AccountsSettings accounts, string accountId, string bookLocale)
	{
		var account = accounts.GetAccount(accountId, bookLocale);
		return (account?.Locale, account?.AdditionalLocales.Count > 0);
	}

	public static string DeviceManagementUrl(Locale? locale)
		=> $"https://www.amazon.{(string.IsNullOrWhiteSpace(locale?.CountryCode) || string.IsNullOrWhiteSpace(locale.TopDomain) ? "com" : locale.TopDomain)}/hz/mycd/digital-console/devicedetails?deviceFamily=AUDIBLE_APP";

	public static string BuildSteps(Locale? locale, VersionCheckOutcome updateStatus, bool cli = false, bool hasAdditionalMarketplaces = false)
	{
		var steps = new List<string>();
		if (updateStatus != VersionCheckOutcome.UpToDate)
			steps.Add((updateStatus == VersionCheckOutcome.UpdateAvailable
				? "Upgrade to the latest Libation: "
				: "If you're not on the latest version, upgrade: ") + ReleasesUrl);
		if (cli)
		{
			var localeParam = string.IsNullOrWhiteSpace(locale?.Name) ? "<registration-region>" : locale.Name;
			var extra = hasAdditionalMarketplaces
				? " (Your library and configured additional marketplaces will be preserved automatically.)"
				: " (This preserves your library and account configuration.)";
			steps.Add($"Run login-external --account <email> --locale {localeParam} to re-authenticate and establish a fresh device registration.{extra}");
			steps.Add("Run scan, then retry with liberate <ASIN>.");
		}
		else
		{
			var extra = hasAdditionalMarketplaces
				? " (Your library and configured additional marketplaces under Marketplaces are preserved.)"
				: " (This preserves your library and account configuration.)";
			steps.Add($"Go to Settings > Accounts, uncheck the \"Signed in?\" checkbox for the affected account, and click Save.{extra}");
			steps.Add("Scan and sign in when prompted to establish a fresh device registration.");
			steps.Add("Retry the download (mark it Download Pending if needed).");
		}
		return string.Join(Environment.NewLine + Environment.NewLine, steps.Select((step, index) => $"{index + 1}. {step}"));
	}
}
