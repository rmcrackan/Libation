using AudibleUtilities;
using LibationWinForms.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LibationWinForms
{
	public partial class Form1
	{
		public void Configure_RemoveBooks() { }

		private async void removeBooksBtn_Click(object sender, EventArgs e)
			=> await productsDisplay.RemoveCheckedBooksAsync();

		private void openTrashBinToolStripMenuItem_Click(object sender, EventArgs e)
			=> new TrashBinDialog().ShowDialog(this);

		private void doneRemovingBtn_Click(object sender, EventArgs e)
		{
			removeBooksBtn.Visible = false;
			doneRemovingBtn.Visible = false;

			productsDisplay.CloseRemoveBooksColumn();

			//Restore the filter
			filterSearchTb.Enabled = true;
			filterSearchTb.Visible = true;
			performFilter(filterSearchTb.Text);
		}

		private void removeLibraryBooksToolStripMenuItem_Click(object sender, EventArgs e)
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
		private void removeAllAccountsToolStripMenuItem_Click(object sender, EventArgs e)
		{
			using var persister = AudibleApiStorage.GetAccountsSettingsPersister();
			var allAccounts = persister.AccountsSettings.GetAll();
			scanLibrariesRemovedBooks(allAccounts.ToArray());
		}

		// selectively remove books from some accounts
		private void removeSomeAccountsToolStripMenuItem_Click(object sender, EventArgs e)
		{
			using var scanAccountsDialog = new ScanAccountsDialog();

			if (scanAccountsDialog.ShowDialog() != DialogResult.OK)
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
			filterSearchTb.Enabled = false;
			filterSearchTb.Visible = false;
			productsDisplay.Filter(null);

			removeBooksBtn.Visible = true;
			doneRemovingBtn.Visible = true;
			await productsDisplay.ScanAndRemoveBooksAsync(accounts);
		}

		private void productsDisplay_RemovableCountChanged(object sender, int removeCount)
		{
			removeBooksBtn.Text = removeCount switch
			{
				1 => "Remove 1 Book from Libation",
				_ => $"Remove {removeCount} Books from Libation"
			};
		}
	}
}
