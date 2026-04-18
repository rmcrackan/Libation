using System.Linq;
using System.Threading.Tasks;

namespace AudibleUtilities;

public enum Mkb79ImportOutcome
{
	Success,
	DuplicateAccount,
	InvalidFile,
}

public sealed record Mkb79ImportResult(Mkb79ImportOutcome Outcome, Account? Account = null, string? Message = null);

public static class Mkb79AuthImporter
{
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

		if (persister.AccountsSettings.Accounts.Any(a =>
			    a.AccountId == account.AccountId && a.IdentityTokens?.Locale.Name == account.Locale?.Name))
		{
			return new Mkb79ImportResult(Mkb79ImportOutcome.DuplicateAccount, account);
		}

		persister.AccountsSettings.Add(account);
		return new Mkb79ImportResult(Mkb79ImportOutcome.Success, account);
	}
}
