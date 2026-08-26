using AudibleUtilities;
using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LibationCli;

[Verb("list-accounts", HelpText = "List configured Audible accounts: locale, any further marketplaces the account also scans, whether the account is included in automatic GUI scans ('Scan library'), and whether stored credentials are valid.")]
internal class ListAccountsOptions : OptionsBase
{
	[Option('b', "bare", HelpText = "Print tab-separated values without table borders (account id, name, locale, scan library, authenticated, also-scans).")]
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
				a.IdentityTokens?.IsValid == true ? "yes" : "no",
				string.Join(", ", a.AdditionalLocales.Select(l => l.Name))))
			.ToArray();

		if (Bare)
		{
			// the extra field goes last and is always present, so a script reading the first five keeps working
			foreach (var r in rows)
				Console.WriteLine($"{r.AccountId}\t{r.AccountName}\t{r.Locale}\t{r.LibraryScan}\t{r.Authenticated}\t{r.AlsoScans}");
		}
		else
		{
			// 'Locale' alone would misreport what a scan does for an account reading several marketplaces. The
			// column is left out entirely when no account has any, which is the ordinary case.
			var columns = new List<ColumnDef<AccountListRow>>
			{
				new("Account ID", r => r.AccountId),
				new("Name", r => r.AccountName),
				new("Locale", r => r.Locale)
			};

			if (rows.Any(r => r.AlsoScans.Length > 0))
				columns.Add(new ColumnDef<AccountListRow>("Also scans", r => r.AlsoScans));

			columns.Add(new ColumnDef<AccountListRow>("Scan library", r => r.LibraryScan));
			columns.Add(new ColumnDef<AccountListRow>("Authenticated", r => r.Authenticated));

			Console.Out.DrawTable(rows, new TextTableOptions(), columns.ToArray());
		}

		return Task.CompletedTask;
	}

	private sealed record AccountListRow(
		string AccountId,
		string AccountName,
		string Locale,
		string LibraryScan,
		string Authenticated,
		string AlsoScans);
}
