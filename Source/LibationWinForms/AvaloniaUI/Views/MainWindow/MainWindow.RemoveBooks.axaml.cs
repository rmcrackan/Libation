using AudibleUtilities;
using Avalonia.Controls;
using LibationWinForms.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibationWinForms.AvaloniaUI.Views
{
	//WORKING
	public partial class MainWindow
	{
		private void Configure_RemoveBooks() 
		{
			removeBooksBtn.IsVisible = false;
			doneRemovingBtn.IsVisible = false;
		}

		public async void removeBooksBtn_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
		{
			await productsDisplay.RemoveCheckedBooksAsync();
		}

		public void doneRemovingBtn_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
		{
			removeBooksBtn.IsVisible = false;
			doneRemovingBtn.IsVisible = false;

			productsDisplay.CloseRemoveBooksColumn();

			//Restore the filter
			filterSearchTb.IsEnabled = true;
			filterSearchTb.IsVisible = true;
			performFilter(filterSearchTb.Text);
		}

		public void removeLibraryBooksToolStripMenuItem_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
		{
			// if 0 accounts, this will not be visible
			// if 1 account, run scanLibrariesRemovedBooks() on this account
			// if multiple accounts, another menu set will open. do not run scanLibrariesRemovedBooks()
			using var persister = AudibleApiStorage.GetAccountsSettingsPersister();
			var accounts = persister.AccountsSettings.GetAll();

			if (accounts.Count != 1)
				return;

			var firstAccount = accounts.Single();
			scanLibrariesRemovedBooks(firstAccount);
		}

		// selectively remove books from all accounts
		public void removeAllAccountsToolStripMenuItem_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
		{
			using var persister = AudibleApiStorage.GetAccountsSettingsPersister();
			var allAccounts = persister.AccountsSettings.GetAll();
			scanLibrariesRemovedBooks(allAccounts.ToArray());
		}

		// selectively remove books from some accounts
		public void removeSomeAccountsToolStripMenuItem_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
		{
			using var scanAccountsDialog = new ScanAccountsDialog();

			if (scanAccountsDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
				return;

			if (!scanAccountsDialog.CheckedAccounts.Any())
				return;

			scanLibrariesRemovedBooks(scanAccountsDialog.CheckedAccounts.ToArray());
		}

		private async void scanLibrariesRemovedBooks(params Account[] accounts)
		{
			//This action is meant to operate on the entire library.
			//For removing books within a filter set, use
			//Visible Books > Remove from library

			filterSearchTb.IsEnabled = false;
			filterSearchTb.IsVisible = false;
			productsDisplay.Filter(null);

			removeBooksBtn.IsVisible = true;
			doneRemovingBtn.IsVisible = true;

			await productsDisplay.ScanAndRemoveBooksAsync(accounts);
		}

		public void productsDisplay_RemovableCountChanged(object sender, int removeCount)
		{
			removeBooksBtn.Content = removeCount switch
			{
				1 => "Remove 1 Book from Libation",
				_ => $"Remove {removeCount} Books from Libation"
			};
		}
	}
}
