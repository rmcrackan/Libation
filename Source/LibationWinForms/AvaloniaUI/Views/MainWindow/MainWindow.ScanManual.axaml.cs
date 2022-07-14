using ApplicationServices;
using AudibleUtilities;
using Avalonia.Controls;
using LibationFileManager;
using LibationWinForms.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LibationWinForms.AvaloniaUI.Views
{
	//DONE
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
			var count = persister.AccountsSettings.Accounts.Count;

			autoScanLibraryToolStripMenuItem.IsVisible = count > 0;

			noAccountsYetAddAccountToolStripMenuItem.IsVisible = count == 0;
			scanLibraryToolStripMenuItem.IsVisible = count == 1;
			scanLibraryOfAllAccountsToolStripMenuItem.IsVisible = count > 1;
			scanLibraryOfSomeAccountsToolStripMenuItem.IsVisible = count > 1;

			removeLibraryBooksToolStripMenuItem.IsVisible = count > 0;

			//Avalonia will not fire the Click event for a MenuItem with children,
			//so if only 1 account, remove the children. Otherwise add children
			//for multiple accounts.
			removeLibraryBooksToolStripMenuItem.Items = null;

			if (count > 1)
			{
				removeLibraryBooksToolStripMenuItem.Items = 
					new List<Control> 
					{
						removeSomeAccountsToolStripMenuItem,
						removeAllAccountsToolStripMenuItem 
					};
			}
		}

		public void noAccountsYetAddAccountToolStripMenuItem_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
		{
			System.Windows.Forms.MessageBox.Show("To load your Audible library, come back here to the Import menu after adding your account");
			new AccountsDialog().ShowDialog();
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
			using var scanAccountsDialog = new ScanAccountsDialog();

			if (scanAccountsDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
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
				var (totalProcessed, newAdded) = await LibraryCommands.ImportAccountAsync(Login.WinformLoginChoiceEager.ApiExtendedFunc, accounts);

				// this is here instead of ScanEnd so that the following is only possible when it's user-initiated, not automatic loop
				if (Configuration.Instance.ShowImportedStats && newAdded > 0)
					System.Windows.Forms.MessageBox.Show($"Total processed: {totalProcessed}\r\nNew: {newAdded}");
			}
			catch (Exception ex)
			{
				MessageBoxLib.ShowAdminAlert(
					null,
					"Error importing library. Please try again. If this still happens after 2 or 3 tries, stop and contact administrator",
					"Error importing library",
					ex);
			}
		}
	}
}
