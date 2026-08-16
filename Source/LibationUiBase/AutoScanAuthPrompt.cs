using System;
using AudibleUtilities;

namespace LibationUiBase;

/// <summary>Shared copy for the auto-scan "login required" pause dialog.</summary>
public static class AutoScanAuthPrompt
{
	public const string Caption = "Auto-scan paused - login required";

	public static string FormatBody(AuthenticationRequiredException ex)
	{
		ArgumentNullException.ThrowIfNull(ex);

		var label = AccountCredentialStatus.FormatAccountLabel(ex.Account);
		if (AccountCredentialStatus.LooksLikeMissingCredentials(ex.Account))
		{
			return $"Libation could not refresh the Audible library for {label} because that account has not been logged in (or stored credentials are missing).\n\n"
				+ "Background auto-scan has been paused. Use Import > Scan Library to log in for that account and resume periodic scans.";
		}

		return $"Libation could not refresh the Audible library for {label} because the stored login is missing or invalid.\n\n"
			+ "Background auto-scan has been paused. Use Import > Scan Library to log in again and resume periodic scans.";
	}
}
