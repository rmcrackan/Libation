using AudibleUtilities;
using CommandLine;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace LibationCli;

[Verb("list-accounts", HelpText = "List configured Audible accounts, locale, scan flag, and whether stored credentials are valid.")]
internal class ListAccountsOptions : OptionsBase
{
	[Option('b', "bare", HelpText = "Print tab-separated values without table borders (account id, name, locale, scan library, authenticated).")]
	public bool Bare { get; set; }

	protected override Task ProcessAsync()
	{
		using var persister = AudibleApiStorage.GetAccountsSettingsPersister();
		var accounts = persister.AccountsSettings.GetAll().ToArray();

		if (accounts.Length == 0)
		{
			Console.WriteLine("No accounts configured.");
			return Task.CompletedTask;
		}

		var rows = accounts
			.Select(a => new AccountListRow(
				a.AccountId,
				a.AccountName ?? "",
				a.Locale?.Name ?? "",
				a.LibraryScan ? "yes" : "no",
				a.IdentityTokens?.IsValid == true ? "yes" : "no"))
			.ToArray();

		if (Bare)
		{
			foreach (var r in rows)
				Console.WriteLine($"{r.AccountId}\t{r.AccountName}\t{r.Locale}\t{r.LibraryScan}\t{r.Authenticated}");
		}
		else
		{
			Console.Out.DrawTable(
				rows,
				new TextTableOptions(),
				new ColumnDef<AccountListRow>("Account ID", r => r.AccountId),
				new ColumnDef<AccountListRow>("Name", r => r.AccountName),
				new ColumnDef<AccountListRow>("Locale", r => r.Locale),
				new ColumnDef<AccountListRow>("Scan library", r => r.LibraryScan),
				new ColumnDef<AccountListRow>("Authenticated", r => r.Authenticated));
		}

		return Task.CompletedTask;
	}

	private sealed record AccountListRow(string AccountId, string AccountName, string Locale, string LibraryScan, string Authenticated);
}
