using LibationUiBase.Resources;
using System;

namespace LibationUiBase;

/// <summary>
/// Audible's Android sign-in pages switch between Amazon and Audible-username sign-in through audible:// links,
/// which only the Audible app can open. In the embedded browser those clicks do nothing, so both login dialogs
/// cancel the navigation and show this message. In Libation the switch is the account's locale instead.
/// </summary>
public static class AudibleAppLinkMessage
{
	public static string Caption => Strings.AudibleAppLinkMessage_ChangeTheAccountsLocaleTo;

	/// <returns>The message to show instead of navigating to <paramref name="uri"/>, or null if it isn't an audible:// link.</returns>
	public static string? ForNavigation(Uri? uri)
	{
		if (uri?.Scheme != "audible")
			return null;
		return uri.Query.Contains("pageType=public", StringComparison.OrdinalIgnoreCase)
			? Strings.AudibleAppLinkMessage_ThisPageOnlyAcceptsAudible
			: Strings.AudibleAppLinkMessage_AudibleUsernamesCantSignIn;
	}
}
