using AudibleApi.Authorization;

namespace AudibleUtilities;

/// <summary>
/// Alignment and conversion helpers for identity tokens in <c>AccountsSettings.json</c>.
/// Preference changes alone do not convert; conversion is explicit via <see cref="ConvertAllAccounts"/>.
/// </summary>
public static class AccountTokenStorage
{
	/// <summary>
	/// Compare persisted encryption metadata for all account identities to <paramref name="method"/>.
	/// </summary>
	public static TokenStorageAlignment GetAccountsAlignment(TokenStorageMethod method, string? accountsSettingsFile = null)
	{
		var path = accountsSettingsFile ?? AudibleApiStorage.AccountsSettingsFile;
		if (!File.Exists(path))
			return TokenStorageAlignment.NoApplicableTokens;

		using var persister = new AccountsSettingsPersister(path);
		var alignments = new List<TokenStorageAlignment>();

		foreach (var account in persister.AccountsSettings.Accounts)
		{
			if (account.IdentityTokens is null)
				continue;

			alignments.Add(IdentityTokenConversion.GetAlignment(account.IdentityTokens, method));
		}

		return CombineAlignments(alignments);
	}

	/// <summary>
	/// Convert every identity in the accounts settings file to <paramref name="method"/>.
	/// All-or-nothing: failure leaves the original file unchanged.
	/// </summary>
	public static IdentityTokenConversionResult ConvertAllAccounts(TokenStorageMethod method, string? accountsSettingsFile = null)
	{
		var path = accountsSettingsFile ?? AudibleApiStorage.AccountsSettingsFile;
		return IdentityTokenConversion.ConvertAllIdentitiesInFile(path, method);
	}

	private static TokenStorageAlignment CombineAlignments(IReadOnlyList<TokenStorageAlignment> alignments)
	{
		if (alignments.Count == 0)
			return TokenStorageAlignment.NoApplicableTokens;
		if (alignments.Any(a => a == TokenStorageAlignment.Indeterminate))
			return TokenStorageAlignment.Indeterminate;
		if (alignments.All(a => a is TokenStorageAlignment.AllMatch or TokenStorageAlignment.NoApplicableTokens))
		{
			return alignments.Any(a => a == TokenStorageAlignment.AllMatch)
				? TokenStorageAlignment.AllMatch
				: TokenStorageAlignment.NoApplicableTokens;
		}

		return TokenStorageAlignment.SomeMismatch;
	}
}
