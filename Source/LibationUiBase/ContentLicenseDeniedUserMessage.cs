using ApplicationServices;
using LibationFileManager;
using System;
using LibationUiBase.Resources;

namespace LibationUiBase;

/// <summary>
/// User-facing copy when Audible denies a content license (download/decrypt). Covers temporary
/// service issues, explicit CustomerThrottled refusals, and Audible Plus denials — often mistaken
/// for a Libation bug. Shared by WinForms and Avalonia via the process queue.
/// </summary>
public static class ContentLicenseDeniedUserMessage
{
	public static string DialogCaption => Strings.ContentLicenseDeniedUserMessage_ContentLicenseDenied;

	/// <summary>Generic outage / GenericError-style denial: not specific to Plus titles.</summary>
	public static string BuildDialogBodyForPossibleOutage(string bookTitleWithSubtitle, AudibleApi.Locale? locale = null,
		AppScaffolding.VersionCheckOutcome updateStatus = AppScaffolding.VersionCheckOutcome.UnableToDetermine, bool hasAdditionalMarketplaces = false)
		=> BuildBody(bookTitleWithSubtitle,
			Strings.ContentLicenseDeniedUserMessage_ThisMayBeATemporaryInterruption, locale, updateStatus, hasAdditionalMarketplaces);

	/// <summary>Audible named CustomerThrottled. Shown for any title, Plus or owned.</summary>
	public static string BuildDialogBodyForThrottling(string bookTitleWithSubtitle, AudibleApi.Locale? locale = null,
		AppScaffolding.VersionCheckOutcome updateStatus = AppScaffolding.VersionCheckOutcome.UnableToDetermine, bool hasAdditionalMarketplaces = false)
		=> BuildBody(bookTitleWithSubtitle,
			Strings.ContentLicenseDeniedUserMessage_AudibleSaysThisAccountIsBeing, locale, updateStatus, hasAdditionalMarketplaces);

	/// <summary>Audible throttled on legacy or non-Widevine registration; settings updated and account marked for re-registration.</summary>
	public static string BuildDialogBodyForThrottlingRecovered(string bookTitleWithSubtitle)
		=> string.Format(Strings.ContentLicenseDeniedUserMessage_YouWereDeniedAContentLicense, bookTitleWithSubtitle);

	/// <summary>Plus denials may indicate registration trouble, rate limits, or lost access.</summary>
	public static string BuildDialogBodyForPlusCatalog(string bookTitleWithSubtitle, AudibleApi.Locale? locale = null,
		AppScaffolding.VersionCheckOutcome updateStatus = AppScaffolding.VersionCheckOutcome.UnableToDetermine, bool hasAdditionalMarketplaces = false)
		=> BuildBody(bookTitleWithSubtitle,
			Strings.ContentLicenseDeniedUserMessage_ThisTitleIsFromTheAudible, locale, updateStatus, hasAdditionalMarketplaces);

	private static string BuildBody(string title, string diagnosis, AudibleApi.Locale? locale, AppScaffolding.VersionCheckOutcome updateStatus, bool hasAdditionalMarketplaces)
		=> string.Format(Strings.ContentLicenseDeniedUserMessage_YouWereDeniedAContentLicense2, title, diagnosis, AppScaffolding.LicenseRecoveryGuidance.Explanation, AppScaffolding.LicenseRecoveryGuidance.BuildSteps(locale, updateStatus, hasAdditionalMarketplaces: hasAdditionalMarketplaces), AppScaffolding.LicenseRecoveryGuidance.Fallback, AppendSuggestion());

	/// <summary>
	/// When Audible names CustomerThrottled, the throttling dialog already says so. This extra paragraph is
	/// for denials without that reason: it only appears when Libation's own record shows enough recent
	/// downloads for throttling to be plausible, and when the user has no daily limit configured yet.
	/// Logged as well as shown.
	/// </summary>
	private static string AppendSuggestion()
	{
		try
		{
			var now = DateTimeOffset.Now;
			var suggestion = DailyDownloadLimitUserMessage.BuildSuggestionParagraph(
				Configuration.Instance,
				DownloadHistoryStore.GetCurrentWindow(now),
				now);

			if (suggestion is null)
				return string.Empty;

			Serilog.Log.Logger.Information("Suggesting a daily download limit after a license denial. {Suggestion}", suggestion);
			return Environment.NewLine + Environment.NewLine + suggestion;
		}
		catch (Exception ex)
		{
			Serilog.Log.Logger.Error(ex, "Failed to build the daily download limit suggestion");
			return string.Empty;
		}
	}
}
