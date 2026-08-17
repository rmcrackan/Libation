using AudibleUtilities;
using System;

namespace LibationUiBase;

/// <summary>Shared copy for the dialog shown when auto-scan pauses itself waiting for a login.</summary>
public static class AutoScanAuthPrompt
{
	public const string Caption = "Auto-scan paused - login required";

	public static string FormatBody(AuthenticationRequiredException ex)
	{
		ArgumentNullException.ThrowIfNull(ex);

		// the owner is looking at their own screen, so the dialog names the account in full. the log gets
		// ex.AccountInfo.MaskedLogEntry instead
		var account = ex.AccountInfo?.RevealOwnerFacingLabel() ?? "an Audible account";
		var cause = ex.AccountInfo?.LooksLikeMissingCredentials ?? true
			? "that account has never been logged in, or its stored credentials are missing"
			: "the stored login for that account expired";

		return $"Libation could not refresh the Audible library for {account} because {cause}.\n\n"
			+ "Background auto-scan has been paused. Use Import > Scan Library to log in for that account and resume periodic scans.";
	}
}
