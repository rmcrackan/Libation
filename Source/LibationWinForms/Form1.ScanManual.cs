using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using ApplicationServices;
using AudibleUtilities;
using LibationFileManager;
using LibationWinForms.Dialogs;

#nullable enable
namespace LibationWinForms
{
	// this is for manual scan/import. Unrelated to auto-scan
    public partial class Form1
	{
		private void Configure_ScanManual()
        {
			this.Load += refreshImportMenu;
			AccountsSettingsPersister.Saved += (_, _) => Invoke(refreshImportMenu, null, null);
			locateAudiobooksToolStripMenuItem.ToolTipText = Configuration.GetHelpText("LocateAudiobooks");
		}

		private void refreshImportMenu(object? _, EventArgs? __)
		{
			using var persister = AudibleApiStorage.GetAccountsSettingsPersister();
			var count = persister.AccountsSettings.Accounts.Count;

			autoScanLibraryToolStripMenuItem.Visible = count > 0;

			noAccountsYetAddAccountToolStripMenuItem.Visible = count == 0;
			scanLibraryToolStripMenuItem.Visible = count == 1;
			scanLibraryOfAllAccountsToolStripMenuItem.Visible = count > 1;
			scanLibraryOfSomeAccountsToolStripMenuItem.Visible = count > 1;

			removeLibraryBooksToolStripMenuItem.Visible = count > 0;
			removeSomeAccountsToolStripMenuItem.Visible = count > 1;
			removeAllAccountsToolStripMenuItem.Visible = count > 1;
		}

		private void noAccountsYetAddAccountToolStripMenuItem_Click(object sender, EventArgs e)
		{
			MessageBox.Show("To load your Audible library, come back here to the Import menu after adding your account");
			new AccountsDialog().ShowDialog();
		}

		private async void scanLibraryToolStripMenuItem_Click(object sender, EventArgs e)
		{
			using var persister = AudibleApiStorage.GetAccountsSettingsPersister();
			if (persister.AccountsSettings.GetAll().FirstOrDefault() is { } firstAccount)
				await scanLibrariesAsync(firstAccount);
		}

		private async void scanLibraryOfAllAccountsToolStripMenuItem_Click(object sender, EventArgs e)
		{
			using var persister = AudibleApiStorage.GetAccountsSettingsPersister();
			var allAccounts = persister.AccountsSettings.GetAll();
			await scanLibrariesAsync(allAccounts);
		}

		private async void scanLibraryOfSomeAccountsToolStripMenuItem_Click(object sender, EventArgs e)
		{
			using var scanAccountsDialog = new ScanAccountsDialog();

			if (scanAccountsDialog.ShowDialog() != DialogResult.OK)
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
				var (totalProcessed, newAdded) = await Task.Run(() => LibraryCommands.ImportAccountAsync(accounts));

				// this is here instead of ScanEnd so that the following is only possible when it's user-initiated, not automatic loop
				if (Configuration.Instance.ShowImportedStats && newAdded > 0)
					MessageBox.Show($"Total processed: {totalProcessed}\r\nNew: {newAdded}");
			}
			catch (OperationCanceledException)
			{
				Serilog.Log.Information("Audible login attempt cancelled by user");
			}
			catch (Exception ex)
			{
				MessageBoxLib.ShowAdminAlert(
					this,
					"Error importing library. Please try again. If this still happens after 2 or 3 tries, stop and contact administrator",
					"Error importing library",
					ex);
			}
		}

		private void locateAudiobooksToolStripMenuItem_Click(object sender, EventArgs e)
		{
			var result = MessageBox.Show(
				this,
				Configuration.GetHelpText(nameof(LocateAudiobooksDialog)),
				"Locate Previously-Liberated Audiobook Files",
				MessageBoxButtons.OKCancel,
				MessageBoxIcon.Information,
				MessageBoxDefaultButton.Button1);

			if (result is DialogResult.OK)
				new LocateAudiobooksDialog().ShowDialog();
		}
	}
}
