using ApplicationServices;
using AudibleUtilities;
using Dinah.Core.StepRunner;
using LibationWinForms.Dialogs;
using System;
using System.Collections.Generic;
using System.Drawing;
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
			sequence[nameof(ShowAccountScanning)] = ShowAccountScanning;
			sequence[nameof(ShowSearching)] = ShowSearching;
			sequence[nameof(ShowQuickFilters)] = ShowQuickFilters;
		}

		public async Task RunAsync() => await sequence.RunAsync();

		private async Task<bool> ShowAccountDialog()
		{
			if (OkCancelMessageBox("First, add you Audible account(s).", "Add Accounts") is not DialogResult.OK) return false;

			await Task.Delay(750);
			await flashControlAsync(MainForm.settingsToolStripMenuItem);
			MainForm.Invoke(MainForm.settingsToolStripMenuItem.ShowDropDown);
			await Task.Delay(500);

			await flashControlAsync(MainForm.accountsToolStripMenuItem);
			MainForm.Invoke(MainForm.accountsToolStripMenuItem.Select);
			await Task.Delay(500);

			using var accountSettings = MainForm.Invoke(() => new AccountsDialog());
			accountSettings.StartPosition = FormStartPosition.CenterParent;
			accountSettings.Shown += (_, _) => MessageBox.Show(accountSettings, "Add your Audible account(s), then save.", "Add an Account");
			MainForm.Invoke(() => accountSettings.ShowDialog(MainForm));
			return true;
		}

		private async Task<bool> ShowSettingsDialog()
		{
			if (OkCancelMessageBox("Next, adjust Libation's settings", "Change Settings") is not DialogResult.OK) return false;

			await Task.Delay(750);
			await flashControlAsync(MainForm.settingsToolStripMenuItem);
			MainForm.Invoke(MainForm.settingsToolStripMenuItem.ShowDropDown);

			await Task.Delay(500);
			await flashControlAsync(MainForm.basicSettingsToolStripMenuItem);
			MainForm.Invoke(MainForm.basicSettingsToolStripMenuItem.Select);
			await Task.Delay(500);

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

		private async Task<bool> ShowAccountScanning()
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
			if (OkCancelMessageBox($"Finally, scan your Audible {accounts} to sync your {library} with Libation", $"Scan {accounts}") is not DialogResult.OK) return false;

			var scanItem = count > 1 ? MainForm.scanLibraryOfAllAccountsToolStripMenuItem : MainForm.scanLibraryToolStripMenuItem;

			await Task.Delay(750);
			await flashControlAsync(MainForm.importToolStripMenuItem);
			MainForm.Invoke(MainForm.importToolStripMenuItem.ShowDropDown);
			await Task.Delay(500);
			await flashControlAsync(scanItem);
			MainForm.Invoke(scanItem.Select);
			await Task.Delay(500);
			MainForm.Invoke(scanItem.PerformClick);

			var tcs = new TaskCompletionSource();
			LibraryCommands.ScanEnd += LibraryCommands_ScanEnd;
			await tcs.Task;
			LibraryCommands.ScanEnd -= LibraryCommands_ScanEnd;
			MainForm.productsDisplay.VisibleCountChanged -= productsDisplay_VisibleCountChanged;

			return true;

			void LibraryCommands_ScanEnd(object sender, int newCount)
			{
				//if we imported new books, wait for the grid to update before proceeding.
				if (newCount > 0)
					MainForm.productsDisplay.VisibleCountChanged += productsDisplay_VisibleCountChanged;
				else
					tcs.SetResult();
			}
			void productsDisplay_VisibleCountChanged(object sender, int e) => tcs.SetResult();
		}

		private async Task<bool> ShowSearching()
		{
			var books = DbContexts.GetLibrary_Flat_NoTracking();
			if (books.Count == 0) return true;

			var firstAuthor = getFirstAuthor();
			if (firstAuthor == null) return true;

			if (OkCancelMessageBox("You can filter the grid entries by searching", "Searching") is not DialogResult.OK) return false;

			MainForm.Invoke(MainForm.filterSearchTb.Focus);
			await flashControlAsync(MainForm.filterSearchTb);

			MainForm.Invoke(() => MainForm.filterSearchTb.Text = string.Empty);
			foreach (var c in firstAuthor)
			{
				MainForm.Invoke(() => MainForm.filterSearchTb.Text += c);
				await Task.Delay(200);
			}

			await flashControlAsync(MainForm.filterBtn);
			MainForm.Invoke(MainForm.filterBtn.Select);
			await Task.Delay(500);
			MainForm.Invoke(MainForm.filterBtn.PerformClick);
			await Task.Delay(1000);

			MessageBox.Show(MainForm, "Libation provides a built-in cheat sheet for its query language", "Search Cheat Sheet");

			await flashControlAsync(MainForm.filterHelpBtn);
			using var filterHelp = MainForm.Invoke(() => new SearchSyntaxDialog());
			MainForm.Invoke(filterHelp.ShowDialog);

			return true;
		}

		private async Task<bool> ShowQuickFilters()
		{
			var firstAuthor = getFirstAuthor();

			if (firstAuthor == null) return true;

			if (OkCancelMessageBox("Queries that you perform regularly can be added to 'Quick Filters'", "Quick Filters") is not DialogResult.OK) return false;

			MainForm.Invoke(() => MainForm.filterSearchTb.Text = firstAuthor);
			await Task.Delay(750);
			await flashControlAsync(MainForm.addQuickFilterBtn);
			await Task.Delay(750);

			MainForm.Invoke(MainForm.addQuickFilterBtn.PerformClick);

			await flashControlAsync(MainForm.quickFiltersToolStripMenuItem);
			MainForm.Invoke(MainForm.quickFiltersToolStripMenuItem.ShowDropDown);
			await Task.Delay(500);

			MainForm.Invoke(MainForm.editQuickFiltersToolStripMenuItem.Select);
			await flashControlAsync(MainForm.editQuickFiltersToolStripMenuItem);
			await Task.Delay(500);

			var editQuickFilters = MainForm.Invoke(() => new EditQuickFilters());
			editQuickFilters.Shown += (_, _) => MessageBox.Show(editQuickFilters, "From here you can edit, delete, and change the order of Quick Filters", "Editing Quick Filters");

			MainForm.Invoke(editQuickFilters.ShowDialog);
			return true;
		}

		private string getFirstAuthor()
		{
			var books = DbContexts.GetLibrary_Flat_NoTracking();
			return books.SelectMany(lb => lb.Book.Authors).FirstOrDefault(a => !string.IsNullOrWhiteSpace(a.Name))?.Name;
		}

		private async Task flashControlAsync(Control control, int flashCount = 3)
		{
			var backColor = MainForm.Invoke(() => control.BackColor);
			for (int i = 0; i < flashCount; i++)
			{
				MainForm.Invoke(() => control.BackColor = Color.Firebrick);
				await Task.Delay(200);
				MainForm.Invoke(() => control.BackColor = backColor);
				await Task.Delay(200);
			}
		}

		private async Task flashControlAsync(ToolStripItem control, int flashCount = 3)
		{
			var backColor = MainForm.Invoke(() => control.BackColor);
			for (int i = 0; i < flashCount; i++)
			{
				MainForm.Invoke(() => control.BackColor = Color.Firebrick);
				await Task.Delay(200);
				MainForm.Invoke(() => control.BackColor = backColor);
				await Task.Delay(200);
			}
		}

		private DialogResult OkCancelMessageBox(string message, string caption)
			=> MainForm.Invoke(() => MessageBox.Show(MainForm, message, caption, MessageBoxButtons.OKCancel));
	}
}
