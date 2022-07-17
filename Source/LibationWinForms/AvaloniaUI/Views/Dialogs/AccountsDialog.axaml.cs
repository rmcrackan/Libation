using AudibleUtilities;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace LibationWinForms.AvaloniaUI.Views.Dialogs
{
	public partial class AccountsDialog : DialogWindow
	{
		public ObservableCollection<AccountDto> Accounts { get; } = new();
		public class AccountDto 
		{
			public IList<AudibleApi.Locale> Locales { get; init; }
			public bool LibraryScan { get; set; } = true;
			public string AccountId { get; set; }
			public AudibleApi.Locale SelectedLocale { get; set; }
			public string AccountName { get; set; }
			public bool IsDefault => AccountId is null && SelectedLocale is null && AccountName is null;
		}

		private static string GetAudibleCliAppDataPath()
			=> Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Audible");

		private List<AudibleApi.Locale> Locales => AudibleApi.Localization.Locales.OrderBy(l => l.Name).ToList();
		public AccountsDialog()
		{
			InitializeComponent();

			// WARNING: accounts persister will write ANY EDIT to object immediately to file
			// here: copy strings and dispose of persister
			// only persist in 'save' step
			using var persister = AudibleApiStorage.GetAccountsSettingsPersister();
			var accounts = persister.AccountsSettings.Accounts;
			if (!accounts.Any())
				return;


			ControlToFocusOnShow = this.FindControl<Button>(nameof(AddAccountButton));

			DataContext = this;

			foreach (var account in accounts)
				AddAccountToGrid(account);
		}

		private void AddAccountToGrid(Account account)
		{
			//ObservableCollection doesn't fire CollectionChanged on Add, so use Insert instead
			Accounts.Insert(Accounts.Count, new()
			{
				LibraryScan = account.LibraryScan,
				AccountId = account.AccountId,
				SelectedLocale = Locales.Single(l => l.Name == account.Locale.Name),
				AccountName = account.AccountName,
				Locales = Locales
			});
		}

		public async void ImportButton_Clicked(object sender, Avalonia.Interactivity.RoutedEventArgs e)
		{

			OpenFileDialog ofd = new();
			ofd.Filters.Add(new() { Name = "JSON File", Extensions = new() { "json" } });
			ofd.Directory = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
			ofd.AllowMultiple = false;

			string audibleAppDataDir = GetAudibleCliAppDataPath();

			if (Directory.Exists(audibleAppDataDir))
				ofd.Directory = audibleAppDataDir;

			var filePath = await ofd.ShowAsync(this);

			if (filePath is null || filePath.Length == 0) return;

			try
			{
				var jsonText = File.ReadAllText(filePath[0]);
				var mkbAuth = Mkb79Auth.FromJson(jsonText);
				var account = await mkbAuth.ToAccountAsync();

				// without transaction, accounts persister will write ANY EDIT immediately to file
				using var persister = AudibleApiStorage.GetAccountsSettingsPersister();

				if (persister.AccountsSettings.Accounts.Any(a => a.AccountId == account.AccountId && a.IdentityTokens.Locale.Name == account.Locale.Name))
				{
					await MessageBox.Show(this, $"An account with that account id and country already exists.\r\n\r\nAccount ID: {account.AccountId}\r\nCountry: {account.Locale.Name}", "Cannot Add Duplicate Account");
					return;
				}

				persister.AccountsSettings.Add(account);

				AddAccountToGrid(account);
			}
			catch (Exception ex)
			{
				MessageBoxLib.ShowAdminAlert(
						null,
						$"An error occurred while importing an account from:\r\n{filePath[0]}\r\n\r\nIs the file encrypted?",
						"Error Importing Account",
						ex);
			}
		}

		public void AddAccountButton_Clicked(object sender, Avalonia.Interactivity.RoutedEventArgs e)
		{
			if (Accounts.Any(a => a.IsDefault))
				return;

			//ObservableCollection doesn't fire CollectionChanged on Add, so use Insert instead
			Accounts.Insert(Accounts.Count, new() { Locales = Locales });
		}

		public void DeleteButton_Clicked(object sender, Avalonia.Interactivity.RoutedEventArgs e)
		{
			if (e.Source is Button expBtn && expBtn.DataContext is AccountDto acc)
				Accounts.Remove(acc);
		}

		public void ExportButton_Clicked(object sender, Avalonia.Interactivity.RoutedEventArgs e)
		{
			if (e.Source is Button expBtn && expBtn.DataContext is AccountDto acc)
				Export(acc);
		}

		protected override async Task SaveAndCloseAsync()
		{

			try
			{
				if (!await inputIsValid())
					return;

				// without transaction, accounts persister will write ANY EDIT immediately to file
				using var persister = AudibleApiStorage.GetAccountsSettingsPersister();

				persister.BeginTransation();
				persist(persister.AccountsSettings);
				persister.CommitTransation();

				base.SaveAndClose();
			}
			catch (Exception ex)
			{
				MessageBoxLib.ShowAdminAlert(null, "Error attempting to save accounts", "Error saving accounts", ex);
			}
		}

		public async void SaveButton_Clicked(object sender, Avalonia.Interactivity.RoutedEventArgs e)
			=> await SaveAndCloseAsync();


		private void InitializeComponent()
		{
			AvaloniaXamlLoader.Load(this);
		}



		private void persist(AccountsSettings accountsSettings)
		{
			var existingAccounts = accountsSettings.Accounts;

			// editing account id is a special case. an account is defined by its account id, therefore this is really a different account. the user won't care about this distinction though.
			// these will be caught below by normal means and re-created minus the convenience of persisting identity tokens

			// delete
			for (var i = existingAccounts.Count - 1; i >= 0; i--)
			{
				var existing = existingAccounts[i];
				if (!Accounts.Any(dto =>
					dto.AccountId?.ToLower().Trim() == existing.AccountId.ToLower()
					&& dto.SelectedLocale.Name == existing.Locale?.Name))
				{
					accountsSettings.Delete(existing);
				}
			}

			// upsert each. validation occurs through Account and AccountsSettings
			foreach (var dto in Accounts)
			{
				var acct = accountsSettings.Upsert(dto.AccountId, dto.SelectedLocale.Name);
				acct.LibraryScan = dto.LibraryScan;
				acct.AccountName
					= string.IsNullOrWhiteSpace(dto.AccountName)
					? $"{dto.AccountId} - {dto.SelectedLocale.Name}"
					: dto.AccountName.Trim();
			}
		}
		private async Task<bool> inputIsValid()
		{
			foreach (var dto in Accounts.ToList())
			{
				if (dto.IsDefault)
				{
					Accounts.Remove(dto);
					continue;
				}

				if (string.IsNullOrWhiteSpace(dto.AccountId))
				{
					await MessageBox.Show(this, "Account id cannot be blank. Please enter an account id for all accounts.", "Blank account", MessageBoxButtons.OK, MessageBoxIcon.Error);
					return false;
				}

				if (string.IsNullOrWhiteSpace(dto.SelectedLocale.Name))
				{
					await MessageBox.Show(this, "Please select a locale (i.e.: country or region) for all accounts.", "Blank region", MessageBoxButtons.OK, MessageBoxIcon.Error);
					return false;
				}
			}

			return true;
		}

		private async void Export(AccountDto acc)
		{
			// without transaction, accounts persister will write ANY EDIT immediately to file
			using var persister = AudibleApiStorage.GetAccountsSettingsPersister();

			var account = persister.AccountsSettings.Accounts.FirstOrDefault(a => a.AccountId == acc.AccountId && a.Locale.Name == acc.SelectedLocale.Name);

			if (account is null)
				return;

			if (account.IdentityTokens?.IsValid != true)
			{
				await MessageBox.Show(this, "This account hasn't been authenticated yet. First scan your library to log into your account, then try exporting again.", "Account Not Authenticated");
				return;
			}

			SaveFileDialog sfd = new();
			sfd.Filters.Add(new() { Name = "JSON File", Extensions = new() { "json" } });

			string audibleAppDataDir = GetAudibleCliAppDataPath();

			if (Directory.Exists(audibleAppDataDir))
				sfd.Directory = audibleAppDataDir;

			string fileName = await sfd.ShowAsync(this);
			if (fileName is null)
				return;

			try
			{
				var mkbAuth = Mkb79Auth.FromAccount(account);
				var jsonText = mkbAuth.ToJson();

				File.WriteAllText(fileName, jsonText);

				await MessageBox.Show(this, $"Successfully exported {account.AccountName} to\r\n\r\n{fileName}", "Success!");
			}
			catch (Exception ex)
			{
				MessageBoxLib.ShowAdminAlert(
					null,
					$"An error occurred while exporting account:\r\n{account.AccountName}",
					"Error Exporting Account",
					ex);
			}
		}
	}
}
