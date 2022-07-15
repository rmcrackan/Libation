using AudibleUtilities;
using LibationWinForms.Dialogs;
using System;
using System.Linq;

namespace LibationWinForms.AvaloniaUI.Views
{
	//DONE
	public partial class MainWindow
	{
		private void Configure_RemoveBooks() 
		{
			if (Avalonia.Controls.Design.IsDesignMode)
				return;

			_viewModel.RemoveButtonsVisible = false;
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

			productsDisplay.Filter(null);

			_viewModel.RemoveBooksButtonEnabled = true;
			_viewModel.RemoveButtonsVisible = true;

			await productsDisplay.ScanAndRemoveBooksAsync(accounts);
		}

		public async void removeBooksBtn_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
		{
			_viewModel.RemoveBooksButtonEnabled = false;
			await productsDisplay.RemoveCheckedBooksAsync();
			_viewModel.RemoveBooksButtonEnabled = true;
		}

		public async void doneRemovingBtn_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
		{
			_viewModel.RemoveButtonsVisible = false;

			productsDisplay.CloseRemoveBooksColumn();

			//Restore the filter
			await performFilter(lastGoodFilter);
		}

		public void productsDisplay_RemovableCountChanged(object sender, int removeCount)
		{
			_viewModel.RemoveBooksButtonText = removeCount switch
			{
				1 => "Remove 1 Book from Libation",
				_ => $"Remove {removeCount} Books from Libation"
			};
		}
	}
}
