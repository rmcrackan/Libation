using Dinah.Core;

namespace AudibleUtilities;

/// <summary>Describes whether an account has usable stored Audible tokens.</summary>
public static class AccountCredentialStatus
{
	/// <summary>
	/// True when identity tokens are absent or carry no refresh token, meaning the account was never fully logged
	/// in or its credentials were cleared, rather than merely holding an expired access token.
	/// </summary>
	public static bool LooksLikeMissingCredentials(Account? account)
	{
		if (account?.IdentityTokens is not { } tokens)
			return true;

		if (tokens.IsValid)
			return false;

		// without a refresh token there is nothing to renew from, so this needs a fresh login rather than a retry
		return string.IsNullOrWhiteSpace(tokens.RefreshToken?.Value);
	}

	/// <summary>Account label for dialogs and log messages.</summary>
	public static string FormatAccountLabel(Account? account)
	{
		if (account is null)
			return "an Audible account";

		return string.IsNullOrWhiteSpace(account.AccountName) || account.AccountName.EqualsInsensitive(account.AccountId)
			? $"'{account.AccountId}'"
			: $"'{account.AccountName}' ({account.AccountId})";
	}
}
