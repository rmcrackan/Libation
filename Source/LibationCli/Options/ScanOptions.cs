using ApplicationServices;
using AudibleUtilities;
using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LibationCli
{
	[Verb("scan", HelpText = "Scan library. Default: scan all accounts. Optional: use 'account' flag to specify a single account.")]
	public class ScanOptions : OptionsBase
	{
		[Value(0, MetaName = "Accounts", HelpText = "Optional: user ID or nicknames of accounts to scan.", Required = false)]
		public IEnumerable<string> AccountNames { get; set; }

		protected override async Task ProcessAsync()
		{
			var accounts = getAccounts();
			if (!accounts.Any())
			{
				Console.WriteLine("No accounts. Exiting.");
				Environment.ExitCode = (int)ExitCode.RunTimeError;
				return;
			}

			var _accounts = accounts.ToArray();

			var intro
				= (_accounts.Length == 1)
				? "Scanning Audible library. This may take a few minutes."
				: $"Scanning Audible library: {_accounts.Length} accounts. This may take a few minutes per account.";
			Console.WriteLine(intro);

			var (TotalBooksProcessed, NewBooksAdded) = await LibraryCommands.ImportAccountAsync((a) => ApiExtended.CreateAsync(a), _accounts);

			Console.WriteLine("Scan complete.");
			Console.WriteLine($"Total processed: {TotalBooksProcessed}");
			Console.WriteLine($"New: {NewBooksAdded}");
		}

		private Account[] getAccounts()
		{
			using var persister = AudibleApiStorage.GetAccountsSettingsPersister();
			var allAccounts = persister.AccountsSettings.GetAll().ToArray();

			if (!AccountNames.Any())
				return allAccounts;

			var accountNames = AccountNames.Select(n => n.ToLower()).ToArray();

			var found
				= allAccounts
				.Where(acct => accountNames.Contains(acct.AccountName.ToLower()) || accountNames.Contains(acct.AccountId.ToLower()))
				.ToArray();

			var notFound = allAccounts.Except(found).ToArray();

			// no accounts found. do not continue
			if (!found.Any())
			{
				Console.WriteLine("Accounts not found:");
				foreach (var nf in notFound)
					Console.WriteLine($"- {nf}");
				return found;
			}

			// some accounts not found. continue after message
			if (notFound.Any())
			{
				Console.WriteLine("Accounts found:");
				foreach (var f in found)
					Console.WriteLine($"- {f}");
				Console.WriteLine("Accounts not found:");
				foreach (var nf in notFound)
					Console.WriteLine($"- {nf}");
			}

			// else: all accounts area found. silently continue

			return found;
		}
	}
}
