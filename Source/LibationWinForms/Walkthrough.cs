using AudibleUtilities;
using Dinah.Core.StepRunner;
using Dinah.Core.WindowsDesktop.Processes;
using LibationWinForms.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LibationWinForms
{
	internal class Walkthrough
	{
		private static Dictionary<string, string> settingTabMessages = new()
		{
			{ "Important settings", "Change where liberated books are stored."},
			{ "Import library", "Change how your library is scanned, imported, and liberated."},
			{ "Download/Decrypt", "Control how liberated files and folders are named and stored."},
			{ "Audio File Options", "Control how audio files are decrypted, including audio format and metadata handling."},
		};

		private readonly Form1 MainForm;
		private readonly AsyncStepSequence sequence = new();
		public Walkthrough(Form1 form1)
		{
			MainForm = form1;
			sequence[nameof(ShowAccountDialog)] = ShowAccountDialog;
			sequence[nameof(ShowSettingsDialog)] = ShowSettingsDialog;
			sequence[nameof(ScanAccounts)] = ScanAccounts;
		}

		public async Task RunAsync() => await sequence.RunAsync();

		private async Task<bool> ShowAccountDialog()
		{
			var result = MainForm.Invoke(() => MessageBox.Show(MainForm, "First, add you Audible account(s).", "Add Accounts", MessageBoxButtons.OKCancel));
			if (result is DialogResult.Cancel) return false;

			await Task.Delay(750);
			MainForm.Invoke(MainForm.settingsToolStripMenuItem.ShowDropDown);
			await Task.Delay(500);
			MainForm.Invoke(MainForm.accountsToolStripMenuItem.Select);
			await Task.Delay(1000);

			using var accountSettings = MainForm.Invoke(() => new AccountsDialog());
			accountSettings.StartPosition = FormStartPosition.CenterParent;
			accountSettings.Shown += (_, _) => MessageBox.Show(accountSettings, "Add your Audible account(s), then save.", "Add an Account");
			MainForm.Invoke(() => accountSettings.ShowDialog(MainForm));
			return true;
		}

		private async Task<bool> ShowSettingsDialog()
		{
			var result = MainForm.Invoke(() => MessageBox.Show(MainForm, "Next, adjust Libation's settings", "Change Settings", MessageBoxButtons.OKCancel));
			if (result is DialogResult.Cancel) return false;

			await Task.Delay(750);
			MainForm.Invoke(MainForm.settingsToolStripMenuItem.ShowDropDown);
			await Task.Delay(500);
			MainForm.Invoke(MainForm.basicSettingsToolStripMenuItem.Select);
			await Task.Delay(1000);

			using var settingsDialog = MainForm.Invoke(() => new SettingsDialog());

			var tabsToVisit = settingsDialog.tabControl.TabPages.Cast<TabPage>().ToList();

			settingsDialog.StartPosition = FormStartPosition.CenterParent;
			settingsDialog.FormClosing += SettingsDialog_FormClosing;
			settingsDialog.Shown += TabControl_TabIndexChanged;
			settingsDialog.tabControl.SelectedIndexChanged += TabControl_TabIndexChanged;

			MainForm.Invoke(() => settingsDialog.ShowDialog(MainForm));

			return true;

			void TabControl_TabIndexChanged(object sender, EventArgs e)
			{
				var selectedTab = settingsDialog.tabControl.SelectedTab;

				tabsToVisit.Remove(selectedTab);

				if (!selectedTab.Visible || !settingTabMessages.ContainsKey(selectedTab.Text)) return;

				MessageBox.Show(selectedTab, settingTabMessages[selectedTab.Text], selectedTab.Text + " Tab", MessageBoxButtons.OK);

				settingTabMessages.Remove(selectedTab.Text);
			}

			void SettingsDialog_FormClosing(object sender, FormClosingEventArgs e)
			{
				if (tabsToVisit.Count > 0)
				{
					var nextTab = tabsToVisit[0];
					tabsToVisit.RemoveAt(0);

					settingsDialog.tabControl.SelectedTab = nextTab;
					e.Cancel = true;
				}
			}
		}

		private async Task<bool> ScanAccounts()
		{
			using var persister = AudibleApiStorage.GetAccountsSettingsPersister();
			var count = persister.AccountsSettings.Accounts.Count;

			if (count < 1)
			{
				MainForm.Invoke(() => MessageBox.Show(MainForm, "Add an Audible account, then sync your library through the \"Import\" menu", "Add an Audible Account", MessageBoxButtons.OK, MessageBoxIcon.Information));
				return false;
			}

			var accounts = count > 1 ? "accounts" :"account";
			var library = count > 1 ? "libraries" : "library";
			var result = MainForm.Invoke(() => MessageBox.Show(MainForm, $"Finally, scan your Audible {accounts} to sync your {library} with Libation", $"Scan {accounts}", MessageBoxButtons.OKCancel));
			if (result is DialogResult.Cancel) return false;

			await Task.Delay(750);
			MainForm.Invoke(MainForm.importToolStripMenuItem.ShowDropDown);
			await Task.Delay(500);
			MainForm.Invoke(() => (count > 1 ? MainForm.scanLibraryOfAllAccountsToolStripMenuItem : MainForm.scanLibraryToolStripMenuItem).Select());
			await Task.Delay(1000);
			MainForm.Invoke(() => (count > 1 ? MainForm.scanLibraryOfAllAccountsToolStripMenuItem : MainForm.scanLibraryToolStripMenuItem).PerformClick());
			return true;
		}
	}
}
