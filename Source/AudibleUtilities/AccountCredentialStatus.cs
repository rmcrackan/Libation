using AudibleApi.Common;
using Dinah.Core;

namespace AudibleUtilities;

/// <summary>Authentication status of an account.</summary>
public enum AccountSignInStatus
{
	/// <summary>Account has not been authenticated, or is marked for re-registration.</summary>
	NotSignedIn,
	/// <summary>Account is authenticated with legacy tokens (e.g. ADP signature only, or obsolete registration), but missing modern Bearer credentials.</summary>
	Legacy,
	/// <summary>Account is authenticated with modern credentials (both ADP signature and Bearer token, and modern device profile).</summary>
	Modern
}

/// <summary>Describes whether an account has usable stored Audible tokens.</summary>
public static class AccountCredentialStatus
{
	/// <summary>
	/// Modern Audible operations require both ADP request signing and Bearer token headers.
	/// </summary>
	public const ApiAuthenticationMethod RequiredAuthenticationMethods =
		ApiAuthenticationMethod.AdpSignature | ApiAuthenticationMethod.BearerToken;

	/// <summary>
	/// Evaluates the sign-in status of an account: Modern, Legacy, or NotSignedIn.
	/// </summary>
	public static AccountSignInStatus GetSignInStatus(Account? account)
	{
		if (account is null || account.RequiresReRegistration || account.IdentityTokens is not { IsValid: true } tokens)
			return AccountSignInStatus.NotSignedIn;

		if (!tokens.HasRequiredTokens(RequiredAuthenticationMethods) || account.HasObsoleteDeviceRegistration())
			return AccountSignInStatus.Legacy;

		return AccountSignInStatus.Modern;
	}

	/// <summary>
	/// True when identity tokens are absent, carry no refresh token, or lack usable credentials,
	/// meaning the account was never fully logged in or its credentials were cleared, rather than
	/// merely holding an expired access token or legacy tokens.
	/// </summary>
	public static bool LooksLikeMissingCredentials(Account? account, ApiAuthenticationMethod? requiredAuth = null)
	{
		if (account is null || account.RequiresReRegistration)
			return true;

		if (account.IdentityTokens is not { } tokens)
			return true;

		if (tokens.IsValid)
			return false;

		// without a refresh token there is nothing to renew from, so this needs a fresh login rather than a retry
		return string.IsNullOrWhiteSpace(tokens.RefreshToken?.Reveal());
	}

	/// <summary>
	/// Account label for dialogs shown to the person who owns the account. Not for logs: those get attached to
	/// public issue reports, which is what <see cref="Account.MaskedLogEntry"/> is for.
	/// </summary>
	public static string FormatAccountLabel(Account? account)
	{
		if (account is null)
			return "an Audible account";

		return string.IsNullOrWhiteSpace(account.AccountName) || account.AccountName.EqualsInsensitive(account.AccountId)
			? $"'{account.AccountId}'"
			: $"'{account.AccountName}' ({account.AccountId})";
	}
}
