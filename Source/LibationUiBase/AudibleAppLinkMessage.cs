using System;

namespace LibationUiBase;

/// <summary>
/// Audible's Android sign-in pages switch between Amazon and Audible-username sign-in through audible:// links,
/// which only the Audible app can open. In the embedded browser those clicks do nothing, so both login dialogs
/// cancel the navigation and show this message. In Libation the switch is the account's locale instead.
/// </summary>
public static class AudibleAppLinkMessage
{
	public const string Caption = "Change the account's locale to switch sign-in";

	private const string ToUsername =
		"Audible usernames can't sign in on this page. It only accepts Amazon accounts.\r\n\r\n"
		+ "Close this window, open Settings > Accounts, and set this account's Locale to the matching pre-amazon locale "
		+ "(pre-amazon - us, pre-amazon - uk or pre-amazon - germany). Put your Audible username in \"Audible email/login\". "
		+ "Then scan the library again.";

	private const string ToAmazon =
		"This page only accepts Audible usernames.\r\n\r\n"
		+ "To sign in with an Amazon account, close this window, open Settings > Accounts, and set this account's Locale "
		+ "to one that doesn't start with pre-amazon. Then scan the library again.";

	/// <returns>The message to show instead of navigating to <paramref name="uri"/>, or null if it isn't an audible:// link.</returns>
	public static string? ForNavigation(Uri? uri)
	{
		if (uri?.Scheme != "audible")
			return null;
		return uri.Query.Contains("pageType=public", StringComparison.OrdinalIgnoreCase) ? ToAmazon : ToUsername;
	}
}
