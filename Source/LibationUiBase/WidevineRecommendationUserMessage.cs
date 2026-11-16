using FileLiberator;
using LibationUiBase.Resources;

namespace LibationUiBase;

/// <summary>
/// User-facing copy when ADRM licenserequest fails with Sable acr:null (Widevine required).
/// Shared by WinForms, Avalonia, and the process queue.
/// </summary>
public static class WidevineRecommendationUserMessage
{
	public static string DialogCaption => Strings.WidevineRecommendationUserMessage_WidevineDRMEnabled;

	public static string QueueStatusText => Strings.WidevineRecommendationUserMessage_WidevineEnabledReLoginNeeded;

	/// <summary>One line for the per-book process log in the queue UI.</summary>
	public static string BuildLogSummary(string bookTitleWithSubtitle, string? maskedAccount = null)
		=> WidevineRecommendation.BuildLogSummary(bookTitleWithSubtitle, maskedAccount);

	/// <summary>Shown once per queue run when the pattern is detected and Widevine was enabled.</summary>
	public static string BuildDialogBody(string bookTitleWithSubtitle)
		=> string.Format(Strings.WidevineRecommendationUserMessage_LibationCouldNotGetADownload, bookTitleWithSubtitle);
}
