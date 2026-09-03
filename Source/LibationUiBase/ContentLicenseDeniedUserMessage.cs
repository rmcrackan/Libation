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
	public static string BuildDialogBodyForPossibleOutage(string bookTitleWithSubtitle)
		=> $"""
			You were denied a content license for {bookTitleWithSubtitle}

			This error often reflects a temporary interruption of service on Audible's side. It usually resolves within about 1 to 2 days, and in the meantime you should still be able to access your books through Audible's website or app.

			Heavy use of the Audible Plus catalog in a short time can also produce "license denied" responses; community reports often involve on the order of dozens of titles — Audible does not publish a fixed limit. Waiting 24 to 48 hours before trying again is usually enough.

			If the problem continues after several days, open an issue on Libation's GitHub and include your logs.
			""" + AppendSuggestion();

	/// <summary>Audible named CustomerThrottled. Shown for any title, Plus or owned.</summary>
	public static string BuildDialogBodyForThrottling(string bookTitleWithSubtitle)
		=> $"""
			You were denied a content license for {bookTitleWithSubtitle}

			Audible refused this download because your account is being throttled. This is a temporary rate limit on Audible's side, not a Libation bug.

			Wait 24 to 48 hours before trying again. In the meantime you should still be able to play this title in the Audible app or website.

			If it still fails after several days, open an issue on Libation's GitHub and include your logs.
			""" + AppendSuggestion();

	/// <summary>License denied on an Audible Plus title — often rate limiting, not a Libation defect.</summary>
	public static string BuildDialogBodyForPlusCatalog(string bookTitleWithSubtitle)
		=> $"""
			You were denied a content license for {bookTitleWithSubtitle}

			This title is from the Audible Plus catalog. Audible sometimes temporarily denies content licenses after heavy Plus use in a short period; community reports often mention on the order of dozens of downloads — Audible does not publish a fixed limit. This is usually not a Libation bug.

			Try waiting 24 to 48 hours and liberate again. If it still fails after several days, open an issue on Libation's GitHub with logs.

			If you should not have access to this title (for example it left Plus before you downloaded), confirm in the Audible app or website.
			""" + AppendSuggestion();

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
