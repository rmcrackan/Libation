using ApplicationServices;
using LibationFileManager;
using System;

namespace LibationUiBase;

/// <summary>
/// User-facing copy when Audible denies a content license (download/decrypt). Covers temporary
/// service issues, explicit CustomerThrottled refusals, and Audible Plus denials — often mistaken
/// for a Libation bug. Shared by WinForms and Avalonia via the process queue.
/// </summary>
public static class ContentLicenseDeniedUserMessage
{
	public const string DialogCaption = "Content license denied";

	/// <summary>Generic outage / GenericError-style denial: not specific to Plus titles.</summary>
	public static string BuildDialogBodyForPossibleOutage(string bookTitleWithSubtitle, AudibleApi.Locale? locale = null,
		AppScaffolding.VersionCheckOutcome updateStatus = AppScaffolding.VersionCheckOutcome.UnableToDetermine, bool hasAdditionalMarketplaces = false)
		=> BuildBody(bookTitleWithSubtitle,
			"This may be a temporary interruption of service. If Audible's app can play the title, try these registration recovery steps.", locale, updateStatus, hasAdditionalMarketplaces);

	/// <summary>Audible named CustomerThrottled. Shown for any title, Plus or owned.</summary>
	public static string BuildDialogBodyForThrottling(string bookTitleWithSubtitle, AudibleApi.Locale? locale = null,
		AppScaffolding.VersionCheckOutcome updateStatus = AppScaffolding.VersionCheckOutcome.UnableToDetermine, bool hasAdditionalMarketplaces = false)
		=> BuildBody(bookTitleWithSubtitle,
			"Audible says this account is being throttled. This can also happen with an old device registration. Try these recovery steps first.", locale, updateStatus, hasAdditionalMarketplaces);

	/// <summary>Plus denials may indicate registration trouble, rate limits, or lost access.</summary>
	public static string BuildDialogBodyForPlusCatalog(string bookTitleWithSubtitle, AudibleApi.Locale? locale = null,
		AppScaffolding.VersionCheckOutcome updateStatus = AppScaffolding.VersionCheckOutcome.UnableToDetermine, bool hasAdditionalMarketplaces = false)
		=> BuildBody(bookTitleWithSubtitle,
			"This title is from the Audible Plus catalog. It may have left Plus, or Audible may be limiting downloads. Check your access in Audible's app; if it plays, try these recovery steps.", locale, updateStatus, hasAdditionalMarketplaces);

	private static string BuildBody(string title, string diagnosis, AudibleApi.Locale? locale, AppScaffolding.VersionCheckOutcome updateStatus, bool hasAdditionalMarketplaces)
		=> $"You were denied a content license for {title}\n\n{diagnosis}\n\n"
			+ AppScaffolding.LicenseRecoveryGuidance.Explanation + "\n\n"
			+ AppScaffolding.LicenseRecoveryGuidance.BuildSteps(locale, updateStatus, hasAdditionalMarketplaces: hasAdditionalMarketplaces) + "\n\n"
			+ AppScaffolding.LicenseRecoveryGuidance.Fallback + AppendSuggestion();

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
