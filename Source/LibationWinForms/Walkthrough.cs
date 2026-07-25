using ApplicationServices;
using AudibleUtilities;
using Dinah.Core;
using Dinah.Core.StepRunner;
using LibationFileManager;
using LibationUiBase;
using LibationWinForms.Dialogs;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LibationWinForms;

internal class Walkthrough
{
	private readonly HashSet<string> shownSettingTabs = [];
	private static readonly Color FlashColor = Color.DodgerBlue;
	private readonly Form1 MainForm;
	private readonly AsyncStepSequence sequence = new();
	private readonly bool AutoScan;
	public Walkthrough(Form1 form1)
	{
		AutoScan = Configuration.Instance.AutoScan;
		Configuration.Instance.AutoScan = false;
		MainForm = form1;
		sequence[nameof(ShowAccountDialog)] = ShowAccountDialog;
		sequence[nameof(ShowSettingsDialog)] = ShowSettingsDialog;
		sequence[nameof(ShowAccountScanning)] = ShowAccountScanning;
		sequence[nameof(ShowSearching)] = ShowSearching;
		sequence[nameof(ShowQuickFilters)] = ShowQuickFilters;
		sequence[nameof(ShowTourComplete)] = ShowTourComplete;
	}

	public async Task RunAsync()
	{
		await sequence.RunAsync();
		Configuration.Instance.AutoScan = AutoScan;
	}

	private async Task<bool> ShowAccountDialog()
	{
		var proceed = WalkthroughMessages.AddAccountsProceed;
		if (!ProceedMessageBox(proceed.Message, proceed.Title))
			return false;

		await Task.Delay(750);
		await displayControlAsync(MainForm.settingsToolStripMenuItem);
		await displayControlAsync(MainForm.accountsToolStripMenuItem);

		using var accountSettings = MainForm.Invoke(() => new AccountsDialog());
		accountSettings.StartPosition = FormStartPosition.CenterParent;
		var onDialog = WalkthroughMessages.AddAccountOnDialog;
		accountSettings.Shown += (_, _) => MessageBox.Show(accountSettings, onDialog.Message, onDialog.Title);
		MainForm.Invoke(() => accountSettings.ShowDialog(MainForm));
		return true;
	}

	private async Task<bool> ShowSettingsDialog()
	{
		var proceed = WalkthroughMessages.ChangeSettingsProceed;
		if (!ProceedMessageBox(proceed.Message, proceed.Title))
			return false;

		await Task.Delay(750);
		await displayControlAsync(MainForm.settingsToolStripMenuItem);
		await displayControlAsync(MainForm.basicSettingsToolStripMenuItem);

		using var settingsDialog = MainForm.Invoke(() => new SettingsDialog());

		var tabsToVisit = settingsDialog.tabControl.TabPages.Cast<TabPage>().ToList();

		settingsDialog.StartPosition = FormStartPosition.CenterParent;
		settingsDialog.FormClosing += SettingsDialog_FormClosing;
		settingsDialog.Shown += TabControl_TabIndexChanged;
		settingsDialog.tabControl.SelectedIndexChanged += TabControl_TabIndexChanged;
		settingsDialog.cancelBtn.Text = "Next Tab";
		settingsDialog.saveBtn.Visible = false;

		MainForm.Invoke(() => settingsDialog.ShowDialog(MainForm));

		return true;

		void TabControl_TabIndexChanged(object? sender, EventArgs e)
		{
			var selectedTab = settingsDialog.tabControl.SelectedTab;
			if (selectedTab == null) return;

			tabsToVisit.Remove(selectedTab);

			if (tabsToVisit.Count == 0)
			{
				settingsDialog.cancelBtn.Text = "Cancel";
				settingsDialog.saveBtn.Visible = true;
			}

			if (!selectedTab.Visible
				|| !WalkthroughMessages.TryGetSettingsTab(selectedTab.Text, out var message)
				|| !shownSettingTabs.Add(selectedTab.Text))
				return;

			MessageBox.Show(selectedTab, message.Message, message.Title, MessageBoxButtons.OK);
		}

		void SettingsDialog_FormClosing(object? sender, FormClosingEventArgs e)
		{
			if (tabsToVisit.Count > 0)
			{
				settingsDialog.tabControl.SelectedTab = tabsToVisit[0];
				e.Cancel = true;
			}
		}
	}

	private async Task<bool> ShowAccountScanning()
	{
		var persister = AudibleApiStorage.GetAccountsSettingsPersister();
		var count = persister.AccountsSettings.Accounts.Count;
		persister.Dispose();

		if (count < 1)
		{
			var noAccounts = WalkthroughMessages.NoAccountsYet;
			MainForm.Invoke(() => MessageBox.Show(MainForm, noAccounts.Message, noAccounts.Title, MessageBoxButtons.OK, MessageBoxIcon.Information));
			return true;
		}

		var proceed = WalkthroughMessages.ScanProceed(count);
		if (!ProceedMessageBox(proceed.Message, proceed.Title))
			return false;

		var scanItem = count > 1 ? MainForm.scanLibraryOfAllAccountsToolStripMenuItem : MainForm.scanLibraryToolStripMenuItem;

		await Task.Delay(750);
		await displayControlAsync(MainForm.importToolStripMenuItem);
		await displayControlAsync(scanItem);

		MainForm.Invoke(scanItem.PerformClick);

		var tcs = new TaskCompletionSource();
		LibraryCommands.ScanEnd += LibraryCommands_ScanEnd;
		await tcs.Task;
		LibraryCommands.ScanEnd -= LibraryCommands_ScanEnd;

		return true;

		void LibraryCommands_ScanEnd(object? _, int __) => tcs.SetResult();
	}

	private async Task<bool> ShowSearching()
	{
		var books = DbContexts.GetLibrary_Flat_NoTracking();
		if (books.Count == 0) return true;

		var firstAuthor = getFirstAuthor()?.SurroundWithQuotes();
		if (firstAuthor == null) return true;

		var proceed = WalkthroughMessages.SearchingProceed;
		if (!ProceedMessageBox(proceed.Message, proceed.Title))
			return false;

		await displayControlAsync(MainForm.filterSearchTb);

		MainForm.Invoke(() => MainForm.filterSearchTb.Text = string.Empty);
		foreach (var c in firstAuthor)
		{
			MainForm.Invoke(() => MainForm.filterSearchTb.Text += c);
			await Task.Delay(150);
		}

		await displayControlAsync(MainForm.filterBtn);

		MainForm.Invoke(MainForm.filterBtn.PerformClick);

		await Task.Delay(1000);

		var cheatSheet = WalkthroughMessages.SearchCheatSheet;
		MessageBox.Show(MainForm, cheatSheet.Message, cheatSheet.Title);

		await displayControlAsync(MainForm.filterHelpBtn);

		using var filterHelp = MainForm.Invoke(MainForm.ShowSearchSyntaxDialog);
		var tcs = new TaskCompletionSource();
		filterHelp.FormClosed += (_, _) => tcs.SetResult();
		await tcs.Task;
		return true;
	}

	private async Task<bool> ShowQuickFilters()
	{
		var firstAuthor = getFirstAuthor()?.SurroundWithQuotes();
		if (firstAuthor == null) return true;

		var proceed = WalkthroughMessages.QuickFiltersProceed;
		if (!ProceedMessageBox(proceed.Message, proceed.Title))
			return false;

		MainForm.Invoke(() => MainForm.filterSearchTb.Text = firstAuthor);

		await Task.Delay(750);
		await displayControlAsync(MainForm.addQuickFilterBtn);
		MainForm.Invoke(MainForm.addQuickFilterBtn.PerformClick);
		await displayControlAsync(MainForm.quickFiltersToolStripMenuItem);
		await displayControlAsync(MainForm.editQuickFiltersToolStripMenuItem);

		var editQuickFilters = MainForm.Invoke(() => new EditQuickFilters());
		var editMsg = WalkthroughMessages.EditQuickFilters;
		editQuickFilters.Shown += (_, _) => MessageBox.Show(editQuickFilters, editMsg.Message, editMsg.Title);
		MainForm.Invoke(editQuickFilters.ShowDialog);

		return true;
	}

	private Task<bool> ShowTourComplete()
	{
		var finished = WalkthroughMessages.TourFinished;
		MessageBox.Show(MainForm, finished.Message, finished.Title);
		return Task.FromResult(true);
	}

	private string? getFirstAuthor()
	{
		var books = DbContexts.GetLibrary_Flat_NoTracking();
		return books.SelectMany(lb => lb.Book.Authors).FirstOrDefault(a => !string.IsNullOrWhiteSpace(a.Name))?.Name;
	}

	private async Task displayControlAsync(ToolStripMenuItem menuItem)
	{
		MainForm.Invoke(() => menuItem.Enabled = false);
		MainForm.Invoke(MainForm.productsDisplay.Focus);
		await flashControlAsync(menuItem);
		MainForm.Invoke(menuItem.ShowDropDown);
		await Task.Delay(500);
		MainForm.Invoke(() => menuItem.Enabled = true);
	}

	private async Task displayControlAsync(Control button)
	{
		MainForm.Invoke(() => button.Enabled = false);
		MainForm.Invoke(MainForm.productsDisplay.Focus);
		await flashControlAsync(button);
		await Task.Delay(500);
		MainForm.Invoke(() => button.Enabled = true);
	}

	private async Task flashControlAsync(Control control, int flashCount = 3)
	{
		var backColor = MainForm.Invoke(() => control.BackColor);
		for (int i = 0; i < flashCount; i++)
		{
			MainForm.Invoke(() => control.BackColor = FlashColor);
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
			MainForm.Invoke(() => control.BackColor = FlashColor);
			await Task.Delay(200);
			MainForm.Invoke(() => control.BackColor = backColor);
			await Task.Delay(200);
		}
	}

	private bool ProceedMessageBox(string message, string caption)
		=> MainForm.Invoke(() => MessageBox.Show(MainForm, message, caption, MessageBoxButtons.OKCancel)) is DialogResult.OK;
}
