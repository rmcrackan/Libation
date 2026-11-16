using AudibleUtilities;
using System;
using LibationUiBase.Resources;

namespace LibationUiBase;

/// <summary>Shared copy for the dialog shown when auto-scan pauses itself waiting for a login.</summary>
public static class AutoScanAuthPrompt
{
	public static string Caption => Strings.AutoScanAuthPrompt_AutoScanPausedLoginRequired;

	public static string FormatBody(AuthenticationRequiredException ex)
	{
		ArgumentNullException.ThrowIfNull(ex);

		// the owner is looking at their own screen, so the dialog names the account in full. the log gets
		// ex.AccountInfo.MaskedLogEntry instead
		var account = ex.AccountInfo?.RevealOwnerFacingLabel() ?? Strings.AutoScanAuthPrompt_AnAudibleAccount;
		var cause = ex.AccountInfo?.RequiresReRegistration == true
			? Strings.AutoScanAuthPrompt_ThatAccountRequiresAFreshLogin
			: ex.AccountInfo?.LooksLikeMissingCredentials ?? true
				? Strings.AutoScanAuthPrompt_ThatAccountHasNeverBeenLogged
				: Strings.AutoScanAuthPrompt_TheStoredLoginForThatAccount;

		return string.Format(Strings.AutoScanAuthPrompt_LibationCouldNotRefreshTheAudible, account, cause);
	}
}
