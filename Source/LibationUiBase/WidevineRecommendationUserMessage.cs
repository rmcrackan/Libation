using FileLiberator;

namespace LibationUiBase;

/// <summary>
/// User-facing copy when ADRM licenserequest fails with Sable acr:null (Widevine required).
/// Shared by WinForms, Avalonia, and the process queue.
/// </summary>
public static class WidevineRecommendationUserMessage
{
	public const string DialogCaption = "Widevine DRM Enabled";

	public const string QueueStatusText = "Widevine enabled (re-login needed)";

	/// <summary>One line for the per-book process log in the queue UI.</summary>
	public static string BuildLogSummary(string bookTitleWithSubtitle, string? maskedAccount = null)
		=> WidevineRecommendation.BuildLogSummary(bookTitleWithSubtitle, maskedAccount);

	/// <summary>Shown once per queue run when the pattern is detected and Widevine was enabled.</summary>
	public static string BuildDialogBody(string bookTitleWithSubtitle)
		=> $"""
			Libation could not get a download license for {bookTitleWithSubtitle} using the standard (ADRM) format.

			This title delivers only via Widevine DRM. Libation has automatically enabled Widevine DRM in Settings and marked your account for a fresh sign-in to obtain Widevine credentials.

			Please sign into your account when prompted, then liberate this title again.

			(All of your account settings and marketplaces are preserved.)
			""";
}
