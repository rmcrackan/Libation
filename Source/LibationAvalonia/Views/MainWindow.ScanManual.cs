using ApplicationServices;
using AudibleUtilities;
using LibationAvalonia.Dialogs;
using LibationFileManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LibationAvalonia.Views
{
	public partial class MainWindow
	{
		private void Configure_ScanManual()
		{
			Load += refreshImportMenu;
			AccountsSettingsPersister.Saved += refreshImportMenu;
		}

		private void refreshImportMenu(object _, EventArgs __)
		{
			using var persister = AudibleApiStorage.GetAccountsSettingsPersister();
			_viewModel.AccountsCount = persister.AccountsSettings.Accounts.Count;
		}

		public async void noAccountsYetAddAccountToolStripMenuItem_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
		{
			await MessageBox.Show("To load your Audible library, come back here to the Import menu after adding your account");
			await new Dialogs.AccountsDialog().ShowDialog(this);
		}

		public async void scanLibraryToolStripMenuItem_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
		{
			using var persister = AudibleApiStorage.GetAccountsSettingsPersister();
			var firstAccount = persister.AccountsSettings.GetAll().FirstOrDefault();
			await scanLibrariesAsync(firstAccount);
		}

		public async void scanLibraryOfAllAccountsToolStripMenuItem_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
		{
			using var persister = AudibleApiStorage.GetAccountsSettingsPersister();
			var allAccounts = persister.AccountsSettings.GetAll();
			await scanLibrariesAsync(allAccounts);
		}

		public async void scanLibraryOfSomeAccountsToolStripMenuItem_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
		{
			var scanAccountsDialog = new Dialogs.ScanAccountsDialog();

			if (await scanAccountsDialog.ShowDialog<DialogResult>(this) != DialogResult.OK)
				return;

			if (!scanAccountsDialog.CheckedAccounts.Any())
				return;

			await scanLibrariesAsync(scanAccountsDialog.CheckedAccounts);
		}

		private async Task scanLibrariesAsync(IEnumerable<Account> accounts) => await scanLibrariesAsync(accounts.ToArray());
		private async Task scanLibrariesAsync(params Account[] accounts)
		{
			try
			{
				var (totalProcessed, newAdded) = await LibraryCommands.ImportAccountAsync(Dialogs.Login.AvaloniaLoginChoiceEager.ApiExtendedFunc, accounts);

				// this is here instead of ScanEnd so that the following is only possible when it's user-initiated, not automatic loop
				if (Configuration.Instance.ShowImportedStats && newAdded > 0)
					await MessageBox.Show($"Total processed: {totalProcessed}\r\nNew: {newAdded}");
			}
			catch (OperationCanceledException)
			{
				Serilog.Log.Information("Audible login attempt cancelled by user");
			}
			catch (Exception ex)
			{
				await MessageBox.ShowAdminAlert(
					this,
					"Error importing library. Please try again. If this still happens after 2 or 3 tries, stop and contact administrator",
					"Error importing library",
					ex);
			}
		}

		private async void locateAudiobooksToolStripMenuItem_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
		{
			var locateDialog = new LocateAudiobooksDialog();
			await locateDialog.ShowDialog(this);
		}
	}
}
