using FileLiberator;

namespace LibationUiBase;

/// <summary>
/// User-facing copy when ADRM licenserequest fails with Sable acr:null (rare; Widevine may work).
/// Shared by WinForms, Avalonia, and the process queue.
/// </summary>
public static class WidevineRecommendationUserMessage
{
	public const string DialogCaption = "Download license unavailable";

	public const string QueueStatusText = "License unavailable; try Widevine DRM";

	/// <summary>One line for the per-book process log in the queue UI.</summary>
	public static string BuildLogSummary(string bookTitleWithSubtitle)
		=> WidevineRecommendation.BuildLogSummary(bookTitleWithSubtitle);

	/// <summary>Shown once per queue run when the pattern is detected and Widevine is off.</summary>
	public static string BuildDialogBody(string bookTitleWithSubtitle)
		=> $"""
			Libation could not get a download license for {bookTitleWithSubtitle} using the standard (ADRM) format.

			This is uncommon. A small number of titles — including some Audible Plus catalog books — only deliver when Widevine DRM is enabled. Enabling it has fixed this for other users with the same error.

			Try Settings > Audio File Options > Use Widevine DRM, then log into your accounts again when prompted and liberate this title again.

			If it still fails, open an issue on Libation's GitHub and include your logs.
			""";
}
