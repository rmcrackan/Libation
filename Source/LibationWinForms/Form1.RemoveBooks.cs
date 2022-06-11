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
		private ToolStripButton removeCheckedBtn = new();
		public void Configure_RemoveBooks()
		{

			#region Create and Add Tool Strip Button
			removeCheckedBtn.DisplayStyle = ToolStripItemDisplayStyle.Text;
			removeCheckedBtn.Name = "removeSelectedBtn";
			removeCheckedBtn.Text = "Remove 0 Books";
			removeCheckedBtn.AutoToolTip = false;
			removeCheckedBtn.ToolTipText = "Remove checked books and series\r\nfrom Libation's database.\r\n\r\nThey will remain in your Audible account.";
			removeCheckedBtn.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			removeCheckedBtn.Alignment = ToolStripItemAlignment.Left;
			removeCheckedBtn.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
			removeCheckedBtn.Font = new System.Drawing.Font(removeCheckedBtn.Font, System.Drawing.FontStyle.Bold);
			removeCheckedBtn.Click += (_, _) => productsDisplay.RemoveCheckedBooksAsync();
			removeCheckedBtn.Visible = false;
			statusStrip1.Items.Insert(1, removeCheckedBtn);
			#endregion
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
			productsDisplay.Filter(null);

			removeCheckedBtn.Visible = true;
			closeRemoveBooksColumnToolStripMenuItem.Visible = true;
			await productsDisplay.ScanAndRemoveBooksAsync(accounts);
		}

		private void closeRemoveBooksColumnToolStripMenuItem_Click(object sender, EventArgs e)
		{
			removeCheckedBtn.Visible = false;
			closeRemoveBooksColumnToolStripMenuItem.Visible = false;
			productsDisplay.CloseRemoveBooksColumn();

			//Restore the filter
			filterSearchTb.Enabled = true;
			performFilter(filterSearchTb.Text);
		}

		private void productsDisplay_RemovableCountChanged(object sender, int removeCount)
		{
			removeCheckedBtn.Text = removeCount switch
			{
				1 => "Remove 1 Book",
				_ => $"Remove {removeCount} Books"
			};
		}
	}
}
