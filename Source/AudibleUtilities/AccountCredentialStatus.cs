namespace AudibleUtilities;

/// <summary>Helpers for describing whether an account has usable stored Audible tokens.</summary>
public static class AccountCredentialStatus
{
	/// <summary>
	/// True when identity tokens are absent or have no refresh token, i.e. the account was never
	/// fully logged in or credentials were cleared -- not merely an expired access token.
	/// </summary>
	public static bool LooksLikeMissingCredentials(Account? account)
	{
		var tokens = account?.IdentityTokens;
		if (tokens is null)
			return true;
		if (tokens.IsValid)
			return false;

		return tokens.RefreshToken is null
			|| string.IsNullOrWhiteSpace(tokens.RefreshToken.Value);
	}

	/// <summary>User-facing account label for dialogs and log messages.</summary>
	public static string FormatAccountLabel(Account? account)
	{
		if (account is null)
			return "an Audible account";

		if (!string.IsNullOrWhiteSpace(account.AccountName)
			&& !string.Equals(account.AccountName, account.AccountId, StringComparison.OrdinalIgnoreCase))
			return $"'{account.AccountName}' ({account.AccountId})";

		return $"'{account.AccountId}'";
	}
}
