namespace LibationUiBase;

/// <summary>
/// User-facing copy when Audible denies a content license (download/decrypt). Covers temporary
/// service issues and Audible Plus throttling — often mistaken for a Libation bug.
/// Shared by WinForms and Avalonia via the process queue.
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
			""";

	/// <summary>License denied on an Audible Plus title — often rate limiting, not a Libation defect.</summary>
	public static string BuildDialogBodyForPlusCatalog(string bookTitleWithSubtitle)
		=> $"""
			You were denied a content license for {bookTitleWithSubtitle}

			This title is from the Audible Plus catalog. Audible sometimes temporarily denies content licenses after heavy Plus use in a short period; community reports often mention on the order of dozens of downloads — Audible does not publish a fixed limit. This is usually not a Libation bug.

			Try waiting 24 to 48 hours and liberate again. If it still fails after several days, open an issue on Libation's GitHub with logs.

			If you should not have access to this title (for example it left Plus before you downloaded), confirm in the Audible app or website.
			""";
}
