using ApplicationServices;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.Styling;
using Dinah.Core;
using Dinah.Core.StepRunner;
using LibationAvalonia.Dialogs;
using LibationAvalonia.Views;
using LibationFileManager;
using LibationUiBase;
using LibationUiBase.Forms;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LibationAvalonia;

internal class Walkthrough
{
	private readonly HashSet<string> shownSettingTabs = [];
	private static readonly IBrush FlashColor = Brushes.DodgerBlue;
	private readonly MainWindow MainForm;
	private readonly AsyncStepSequence sequence = new();
	private readonly bool AutoScan;
	public Walkthrough(MainWindow mainForm)
	{
		AutoScan = Configuration.Instance.AutoScan;
		Configuration.Instance.AutoScan = false;
		MainForm = mainForm;
		var uiDispatcher = Avalonia.Threading.Dispatcher.UIThread;
		sequence[nameof(ShowAccountDialog)] = () => uiDispatcher.InvokeAsync(ShowAccountDialog);
		sequence[nameof(ShowSettingsDialog)] = () => uiDispatcher.InvokeAsync(ShowSettingsDialog);
		sequence[nameof(ShowAccountScanning)] = () => uiDispatcher.InvokeAsync(ShowAccountScanning);
		sequence[nameof(ShowSearching)] = () => uiDispatcher.InvokeAsync(ShowSearching);
		sequence[nameof(ShowQuickFilters)] = () => uiDispatcher.InvokeAsync(ShowQuickFilters);
		sequence[nameof(ShowTourComplete)] = () => uiDispatcher.InvokeAsync(ShowTourComplete);
	}

	public async Task RunAsync()
	{
		await sequence.RunAsync();
		Configuration.Instance.AutoScan = AutoScan;
	}

	private async Task<bool> ShowAccountDialog()
	{
		var proceed = WalkthroughMessages.AddAccountsProceed;
		if (!await ProceedMessageBox(proceed.Message, proceed.Title))
			return false;

		await Task.Delay(WalkthroughHelpers.Timing.BeforeHighlightMs);
		await displayControlAsync(MainForm.settingsToolStripMenuItem);
		await displayControlAsync(MainForm.accountsToolStripMenuItem);

		var accountSettings = new AccountsDialog();
		var onDialog = WalkthroughMessages.AddAccountOnDialog;
		accountSettings.Opened += async (_, _) => await MessageBox.Show(accountSettings, onDialog.Message, onDialog.Title);
		await accountSettings.ShowDialog(MainForm);
		return true;
	}

	private async Task<bool> ShowSettingsDialog()
	{
		var proceed = WalkthroughMessages.ChangeSettingsProceed;
		if (!await ProceedMessageBox(proceed.Message, proceed.Title))
			return false;

		await Task.Delay(WalkthroughHelpers.Timing.BeforeHighlightMs);
		await displayControlAsync(MainForm.settingsToolStripMenuItem);
		await displayControlAsync(MainForm.basicSettingsToolStripMenuItem);

		var settingsDialog = new SettingsDialog();

		var tabsToVisit = settingsDialog.tabControl.Items.OfType<TabItem>().ToList();

		foreach (var tab in tabsToVisit)
			tab.PropertyChanged += TabControl_PropertyChanged;

		settingsDialog.Opened += SettingsDialog_Opened;
		settingsDialog.Closing += SettingsDialog_FormClosing;
		settingsDialog.saveBtn.Content = "Next Tab";

		await settingsDialog.ShowDialog(MainForm);

		return true;

		async Task ShowTabPageMessageBoxAsync(TabItem? selectedTab)
		{
			if (selectedTab is null)
				return;
			tabsToVisit.Remove(selectedTab);

			if (!selectedTab.IsVisible
				|| selectedTab.Header is not TextBlock header
				|| header.Text is not string text
				|| !WalkthroughMessages.TryGetSettingsTab(text, out var message))
				return;

			if (tabsToVisit.Count == 0)
				settingsDialog.saveBtn.Content = "Save";

			if (!shownSettingTabs.Add(text))
				return;

			await MessageBox.Show(settingsDialog, message.Message, message.Title, MessageBoxButtons.OK);
		}

		async void SettingsDialog_Opened(object? sender, System.EventArgs e)
		{
			await ShowTabPageMessageBoxAsync(tabsToVisit[0]);
		}

		async void TabControl_PropertyChanged(object? sender, Avalonia.AvaloniaPropertyChangedEventArgs e)
		{
			if (e.Property == TabItem.IsSelectedProperty && settingsDialog.IsLoaded)
			{
				await ShowTabPageMessageBoxAsync(sender as TabItem);
			}
		}

		void SettingsDialog_FormClosing(object? sender, WindowClosingEventArgs e)
		{
			if (tabsToVisit.Count > 0)
			{
				settingsDialog.tabControl.SelectedItem = tabsToVisit[0];
				e.Cancel = true;
			}
		}
	}

	private async Task<bool> ShowAccountScanning()
	{
		var count = WalkthroughHelpers.GetConfiguredAccountCount();

		if (count < 1)
		{
			var noAccounts = WalkthroughMessages.NoAccountsYet;
			await MessageBox.Show(MainForm, noAccounts.Message, noAccounts.Title, MessageBoxButtons.OK, MessageBoxIcon.Information);
			return false;
		}

		var proceed = WalkthroughMessages.ScanProceed(count);
		if (!await ProceedMessageBox(proceed.Message, proceed.Title))
			return false;

		var scanItem = count > 1 ? MainForm.scanLibraryOfAllAccountsToolStripMenuItem : MainForm.scanLibraryToolStripMenuItem;

		await Task.Delay(WalkthroughHelpers.Timing.BeforeHighlightMs);
		await displayControlAsync(MainForm.importToolStripMenuItem);
		await displayControlAsync(scanItem);

		scanItem.Command?.Execute(null);
		MainForm.importToolStripMenuItem.Close();

		var tcs = new TaskCompletionSource();
		LibraryCommands.ScanEnd += LibraryCommands_ScanEnd;
		await tcs.Task;
		LibraryCommands.ScanEnd -= LibraryCommands_ScanEnd;
		MainForm.ViewModel?.ProductsDisplay.VisibleCountChanged -= productsDisplay_VisibleCountChanged;

		return true;

		void LibraryCommands_ScanEnd(object? sender, int newCount)
		{
			//if we imported new books, wait for the grid to update before proceeding.
			if (newCount > 0)
				Avalonia.Threading.Dispatcher.UIThread.Invoke(() =>
					MainForm.ViewModel?.ProductsDisplay.VisibleCountChanged += productsDisplay_VisibleCountChanged);
			else
				tcs.SetResult();
		}
		void productsDisplay_VisibleCountChanged(object? sender, int e) => tcs.SetResult();
	}

	private async Task<bool> ShowSearching()
	{
		var books = DbContexts.GetLibrary_Flat_NoTracking();
		if (books.Count == 0)
			return true;

		var firstAuthor = WalkthroughHelpers.GetFirstAuthorName()?.SurroundWithQuotes();
		if (firstAuthor is null)
			return true;

		var proceed = WalkthroughMessages.SearchingProceed;
		if (!await ProceedMessageBox(proceed.Message, proceed.Title))
			return false;

		await displayControlAsync(MainForm.filterSearchTb);

		MainForm.filterSearchTb.Text = string.Empty;
		foreach (var c in firstAuthor)
		{
			MainForm.filterSearchTb.Text += c;
			await Task.Delay(WalkthroughHelpers.Timing.TypeCharDelayMs);
		}

		await displayControlAsync(MainForm.filterBtn);

		MainForm.filterBtn.Command?.Execute(firstAuthor);

		await Task.Delay(WalkthroughHelpers.Timing.AfterFilterMs);

		var cheatSheet = WalkthroughMessages.SearchCheatSheet;
		await MessageBox.Show(MainForm, cheatSheet.Message, cheatSheet.Title);

		await displayControlAsync(MainForm.filterHelpBtn);

		var searchDialog = MainForm.ShowSearchSyntaxDialog();
		var tcs = new TaskCompletionSource();
		searchDialog.Closed += (_, _) => tcs.SetResult();
		await tcs.Task;
		return true;
	}

	private async Task<bool> ShowQuickFilters()
	{
		var firstAuthor = WalkthroughHelpers.GetFirstAuthorName()?.SurroundWithQuotes();
		if (firstAuthor is null)
			return true;

		var proceed = WalkthroughMessages.QuickFiltersProceed;
		if (!await ProceedMessageBox(proceed.Message, proceed.Title))
			return false;

		MainForm.filterSearchTb.Text = firstAuthor;

		var editQuickFiltersToolStripMenuItem = MainForm.quickFiltersToolStripMenuItem.ItemsSource?.OfType<MenuItem>().ElementAt(1);

		await Task.Delay(WalkthroughHelpers.Timing.BeforeHighlightMs);
		await displayControlAsync(MainForm.addQuickFilterBtn);
		MainForm.addQuickFilterBtn.Command?.Execute(firstAuthor);
		await displayControlAsync(MainForm.quickFiltersToolStripMenuItem);
		await displayControlAsync(editQuickFiltersToolStripMenuItem);

		var editQuickFilters = new EditQuickFilters();
		var editMsg = WalkthroughMessages.EditQuickFilters;
		editQuickFilters.Opened += async (_, _) => await MessageBox.Show(editQuickFilters, editMsg.Message, editMsg.Title);
		await editQuickFilters.ShowDialog(MainForm);

		return true;
	}

	private async Task<bool> ShowTourComplete()
	{
		var finished = WalkthroughMessages.TourFinished;
		await MessageBox.Show(MainForm, finished.Message, finished.Title);
		return true;
	}

	private async Task displayControlAsync(TemplatedControl? control)
	{
		if (control is null)
			return;
		control.IsEnabled = false;
		MainForm.productsDisplay.Focus();
		await flashControlAsync(control);
		if (control is MenuItem menuItem) menuItem.Open();
		await Task.Delay(WalkthroughHelpers.Timing.AfterHighlightMs);
		control.IsEnabled = true;
	}

	private static async Task flashControlAsync(TemplatedControl control, int flashCount = WalkthroughHelpers.Timing.FlashCount)
	{
		for (int i = 0; i < flashCount; i++)
		{
			control.Styles.Add(disabledStyle);
			control.Styles.Add(disabledStyle2);
			await Task.Delay(WalkthroughHelpers.Timing.FlashIntervalMs);
			control.Styles.Remove(disabledStyle);
			control.Styles.Remove(disabledStyle2);
			control.Styles.Add(enabedStyle);
			control.Styles.Add(enabedStyle2);
			control.InvalidateVisual();
			await Task.Delay(WalkthroughHelpers.Timing.FlashIntervalMs);
			control.Styles.Remove(enabedStyle);
			control.Styles.Remove(enabedStyle2);
		}
	}

	private async Task<bool> ProceedMessageBox(string message, string caption)
		=> await MessageBox.Show(MainForm, message, caption, MessageBoxButtons.OKCancel) is DialogResult.OK;

	private static readonly Setter HighlightSetter = new Setter(Border.BackgroundProperty, FlashColor);
	private static readonly Setter HighlightSetter2 = new Setter(ContentPresenter.BackgroundProperty, FlashColor);
	private static readonly Setter TransparentSetter = new Setter(Border.BackgroundProperty, Brushes.Transparent);
	private static readonly Setter TransparentSetter2 = new Setter(ContentPresenter.BackgroundProperty, Brushes.Transparent);

	private static readonly Selector TemplateSelector = Selectors.Is<TemplatedControl>(null).PropertyEquals(Avalonia.Input.InputElement.IsEnabledProperty, false).Template();
	private static readonly Selector ContentPresenterSelector = TemplateSelector.Is<ContentPresenter>();
	private static readonly Selector BorderSelector = TemplateSelector.Is<Border>();

	private static readonly Style disabledStyle = new Style(_ => BorderSelector);
	private static readonly Style disabledStyle2 = new Style(_ => ContentPresenterSelector);
	private static readonly Style enabedStyle = new Style(_ => BorderSelector);
	private static readonly Style enabedStyle2 = new Style(_ => ContentPresenterSelector);

	static Walkthrough()
	{
		disabledStyle.Setters.Add(HighlightSetter);
		disabledStyle2.Setters.Add(HighlightSetter2);
		enabedStyle.Setters.Add(TransparentSetter);
		enabedStyle2.Setters.Add(TransparentSetter2);
	}
}
