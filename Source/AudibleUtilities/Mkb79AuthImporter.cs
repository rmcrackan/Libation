using System;
using System.Threading.Tasks;

namespace AudibleUtilities;

public enum Mkb79ImportOutcome
{
	Success,
	DuplicateAccount,
	InvalidFile,
}

/// <param name="ClaimedBy">
/// For <see cref="Mkb79ImportOutcome.DuplicateAccount"/>, the account already scanning that marketplace. It may
/// be one registered with it, or one carrying it as an additional marketplace.
/// </param>
public sealed record Mkb79ImportResult(
	Mkb79ImportOutcome Outcome,
	Account? Account = null,
	string? Message = null,
	Account? ClaimedBy = null);

public static class Mkb79AuthImporter
{
	/// <summary>
	/// Why a duplicate import was refused, in the same words everywhere it is refused. Naming the account that
	/// already reads the marketplace matters now that it need not be a row registered with it - it may be one
	/// reading it as an additional marketplace, which is not obvious from the accounts grid.
	/// </summary>
	public static string DuplicateMessage(Mkb79ImportResult result)
	{
		var locale = result.Account?.Locale?.Name ?? "[unknown]";

		if (result.ClaimedBy is { } claimedBy && claimedBy.Locale?.Name != locale)
			return $"The '{locale}' marketplace is already scanned by the account "
				+ $"{AccountCredentialStatus.FormatAccountLabel(claimedBy)}, as an additional marketplace. "
				+ "Nothing was imported.";

		return "An account with that account id and country already exists."
			+ $"{Environment.NewLine}Account ID: {result.Account?.AccountId}"
			+ $"{Environment.NewLine}Country: {locale}";
	}

	/// <summary>
	/// Deserialize mkb79/audible-cli JSON, refresh tokens, and add the account if not already present.
	/// </summary>
	public static async Task<Mkb79ImportResult> ImportFromJsonTextAsync(string jsonText)
	{
		var mkbAuth = Mkb79Auth.FromJson(jsonText);
		if (mkbAuth is null)
		{
			return new Mkb79ImportResult(
				Mkb79ImportOutcome.InvalidFile,
				null,
				"File did not contain valid mkb79/audible-cli account data.");
		}

		var account = await mkbAuth.ToAccountAsync();

		using var persister = AudibleApiStorage.GetAccountsSettingsPersister();

		// An mkb79 file names one marketplace, and a marketplace can only be scanned by one account. Ask about
		// every claim on it, not just registrations: an existing account may already be reading this marketplace
		// as an additional one, in which case importing would scan it twice.
		var claimedBy = persister.AccountsSettings.GetAccountClaimingMarketplace(account.AccountId, account.Locale?.Name);
		if (claimedBy is not null)
			return new Mkb79ImportResult(Mkb79ImportOutcome.DuplicateAccount, account, ClaimedBy: claimedBy);

		persister.AccountsSettings.Add(account);
		return new Mkb79ImportResult(Mkb79ImportOutcome.Success, account);
	}
}
