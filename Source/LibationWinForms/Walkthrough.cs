using ApplicationServices;
using AudibleUtilities;
using Dinah.Core;
using Dinah.Core.StepRunner;
using LibationFileManager;
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
	private Dictionary<string, string> settingTabMessages = new()
	{
		{ "Important settings", "From here you can change where liberated books are stored and how detailed Libation's logs are.\r\n\r\nIf you experience a problem and need help, you'll be asked to provide your log file. In certain circumstances we may need you to reproduce the error with a higher level of logging detail."},
		{ "Import library", "In this tab you can change how your library is scanned and imported into Libation, as well as automatic liberation."},
		{ "Download/Decrypt", "These settings allow you to control how liberated files and folders are named and stored.\r\nYou can customize the 'Naming Templates' to use any number of the audiobook's properties to build a customized file and folder naming format. Learn more about the syntax from the wiki at\r\n\r\nhttps://github.com/rmcrackan/Libation/blob/master/Documentation/NamingTemplates.md"},
		{ "Audio File Options", "Control how audio files are decrypted, including audio format and metadata handling.\r\n\r\nIf you choose to split your audiobook into multiple files by chapter marker, you may edit the chapter file 'Naming Template' to control how each chapter file is named."},
	};

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
		if (!ProceedMessageBox("First, add your Audible account(s).", "Add Accounts"))
			return false;

		await Task.Delay(750);
		await displayControlAsync(MainForm.settingsToolStripMenuItem);
		await displayControlAsync(MainForm.accountsToolStripMenuItem);

		using var accountSettings = MainForm.Invoke(() => new AccountsDialog());
		accountSettings.StartPosition = FormStartPosition.CenterParent;
		accountSettings.Shown += (_, _) => MessageBox.Show(accountSettings, "Add your Audible account(s), then save.", "Add an Account");
		MainForm.Invoke(() => accountSettings.ShowDialog(MainForm));
		return true;
	}

	private async Task<bool> ShowSettingsDialog()
	{
		if (!ProceedMessageBox("Next, adjust Libation's settings", "Change Settings"))
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

			if (!selectedTab.Visible || !settingTabMessages.ContainsKey(selectedTab.Text)) return;

			MessageBox.Show(selectedTab, settingTabMessages[selectedTab.Text], selectedTab.Text + " Tab", MessageBoxButtons.OK);

			settingTabMessages.Remove(selectedTab.Text);
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
			MainForm.Invoke(() => MessageBox.Show(MainForm, "Add an Audible account, then sync your library through the 'Import' menu.", "Add an Audible Account", MessageBoxButtons.OK, MessageBoxIcon.Information));
			return true;
		}

		var accounts = count > 1 ? "accounts" :"account";
		var library = count > 1 ? "libraries" : "library";
		if (!ProceedMessageBox($"Finally, scan your Audible {accounts} to sync your {library} with Libation.\r\n\r\nIf this is your first time scanning an account, you'll be prompted to enter your account's password to log into your Audible account.", $"Scan {accounts}"))
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

		if (!ProceedMessageBox("You can filter the grid entries by searching", "Searching"))
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

		MessageBox.Show(MainForm, "Libation provides a built-in cheat sheet for its query language", "Search Cheat Sheet");

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

		if (!ProceedMessageBox("Queries that you perform regularly can be added to 'Quick Filters'", "Quick Filters"))
			return false;

		MainForm.Invoke(() => MainForm.filterSearchTb.Text = firstAuthor);

		await Task.Delay(750);
		await displayControlAsync(MainForm.addQuickFilterBtn);
		MainForm.Invoke(MainForm.addQuickFilterBtn.PerformClick);
		await displayControlAsync(MainForm.quickFiltersToolStripMenuItem);
		await displayControlAsync(MainForm.editQuickFiltersToolStripMenuItem);

		var editQuickFilters = MainForm.Invoke(() => new EditQuickFilters());
		editQuickFilters.Shown += (_, _) => MessageBox.Show(editQuickFilters, "From here you can edit, delete, and change the order of Quick Filters", "Editing Quick Filters");
		MainForm.Invoke(editQuickFilters.ShowDialog);

		return true;
	}

	private Task<bool> ShowTourComplete()
	{
		MessageBox.Show(MainForm, "You're now ready to begin using Libation.\r\n\r\nEnjoy!", "Tour Finished");
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
