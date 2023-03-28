using ApplicationServices;
using AudibleUtilities;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using Dinah.Core.StepRunner;
using LibationAvalonia.Dialogs;
using LibationAvalonia.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LibationAvalonia
{
	internal class Walkthrough
	{
		private static Dictionary<string, string> settingTabMessages = new()
		{
			{ "Important Settings", "Change where liberated books are stored."},
			{ "Import Library", "Change how your library is scanned, imported, and liberated."},
			{ "Download/Decrypt", "Control how liberated files and folders are named and stored."},
			{ "Audio File Settings", "Control how audio files are decrypted, including audio format and metadata handling."},
		};

		private readonly MainWindow MainForm;
		private readonly AsyncStepSequence sequence = new();
		public Walkthrough(MainWindow mainForm)
		{
			MainForm = mainForm;
			sequence[nameof(ShowAccountDialog)] = ShowAccountDialog;
			sequence[nameof(ShowSettingsDialog)] = ShowSettingsDialog;
			sequence[nameof(ShowAccountScanning)] = ShowAccountScanning;
			sequence[nameof(ShowSearching)] = ShowSearching;
			sequence[nameof(ShowQuickFilters)] = ShowQuickFilters;
		}

		public async Task RunAsync() => await sequence.RunAsync();

		private async Task<bool> ShowAccountDialog()
		{
			if (await OkCancelMessageBox("First, add your Audible account(s).", "Add Accounts") is not DialogResult.OK) return false;

			await Task.Delay(750);
			await flashControlAsync(MainForm.settingsToolStripMenuItem);
			await InvokeAsync(MainForm.settingsToolStripMenuItem.Open);
			await Task.Delay(500);
			
			await flashControlAsync(MainForm.accountsToolStripMenuItem);
			await InvokeAsync(() => MainForm.accountsToolStripMenuItem.IsSelected = true);
			await Task.Delay(500);

			var accountSettings = await InvokeAsync(() => new AccountsDialog());
			accountSettings.Opened += async (_, _) => await MessageBox.Show(accountSettings, "Add your Audible account(s), then save.", "Add an Account");
			await InvokeAsync(() => accountSettings.ShowDialog(MainForm));
			return true;
		}

		private async Task<bool> ShowSettingsDialog()
		{
			if (await OkCancelMessageBox("Next, adjust Libation's settings", "Change Settings") is not DialogResult.OK) return false;

			await Task.Delay(750);
			await flashControlAsync(MainForm.settingsToolStripMenuItem);
			await InvokeAsync(MainForm.settingsToolStripMenuItem.Open);
			await Task.Delay(500);

			await flashControlAsync(MainForm.basicSettingsToolStripMenuItem);
			await InvokeAsync(() => MainForm.basicSettingsToolStripMenuItem.IsSelected = true);
			await Task.Delay(500);

			var settingsDialog = await InvokeAsync(() => new SettingsDialog());

			var tabsToVisit = settingsDialog.tabControl.Items.OfType<TabItem>().ToList();

			foreach (var tab in tabsToVisit)
				tab.PropertyChanged += TabControl_PropertyChanged;

			settingsDialog.Closing += SettingsDialog_FormClosing;

			await InvokeAsync(() => settingsDialog.ShowDialog(MainForm));

			return true;

			async void TabControl_PropertyChanged(object sender, Avalonia.AvaloniaPropertyChangedEventArgs e)
			{
				if (e.Property == TabItem.IsSelectedProperty)
				{
					var selectedTab = sender as TabItem;

					tabsToVisit.Remove(selectedTab);

					if (!selectedTab.IsVisible || !(selectedTab.Header is TextBlock header && settingTabMessages.ContainsKey(header.Text))) return;

					await MessageBox.Show(settingsDialog, settingTabMessages[header.Text], header.Text + " Tab", MessageBoxButtons.OK);

					settingTabMessages.Remove(header.Text);
				}
			}

			void SettingsDialog_FormClosing(object sender, WindowClosingEventArgs e)
			{
				if (tabsToVisit.Count > 0)
				{
					var nextTab = tabsToVisit[0];
					tabsToVisit.RemoveAt(0);

					settingsDialog.tabControl.SelectedItem = nextTab;
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
				await InvokeAsync(() => MessageBox.Show(MainForm, "Add an Audible account, then sync your library through the \"Import\" menu", "Add an Audible Account", MessageBoxButtons.OK, MessageBoxIcon.Information));
				return false;
			}

			var accounts = count > 1 ? "accounts" : "account";
			var library = count > 1 ? "libraries" : "library";
			if (await OkCancelMessageBox($"Finally, scan your Audible {accounts} to sync your {library} with Libation", $"Scan {accounts}") is not DialogResult.OK) return false;

			var scanItem = count > 1 ? MainForm.scanLibraryOfAllAccountsToolStripMenuItem : MainForm.scanLibraryToolStripMenuItem;

			await Task.Delay(750);
			await flashControlAsync(MainForm.importToolStripMenuItem);
			await InvokeAsync(MainForm.importToolStripMenuItem.Open);
			await Task.Delay(500);

			await flashControlAsync(scanItem);
			await InvokeAsync(() => scanItem.IsSelected = true);
			await Task.Delay(500);

			await InvokeAsync(() => scanItem.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent)));
			await InvokeAsync(MainForm.importToolStripMenuItem.Close);

			var tcs = new TaskCompletionSource();
			LibraryCommands.ScanEnd += LibraryCommands_ScanEnd;
			await tcs.Task;
			LibraryCommands.ScanEnd -= LibraryCommands_ScanEnd;
			MainForm.ViewModel.ProductsDisplay.VisibleCountChanged -= productsDisplay_VisibleCountChanged;

			return true;

			void LibraryCommands_ScanEnd(object sender, int newCount)
			{
				//if we imported new books, wait for the grid to update before proceeding.
				if (newCount > 0)
					MainForm.ViewModel.ProductsDisplay.VisibleCountChanged += productsDisplay_VisibleCountChanged;
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

			if (await OkCancelMessageBox("You can filter the grid entries by searching", "Searching") is not DialogResult.OK) return false;

			await flashControlAsync(MainForm.filterSearchTb);

			await InvokeAsync(() => MainForm.filterSearchTb.Text = string.Empty);
			foreach (var c in firstAuthor)
			{
				await InvokeAsync(() => MainForm.filterSearchTb.Text += c);
				await Task.Delay(200);
			}

			await flashControlAsync(MainForm.filterBtn);
			await InvokeAsync(MainForm.filterBtn.Focus);
			await Task.Delay(500);

			await InvokeAsync(() => MainForm.filterBtn.RaiseEvent(new RoutedEventArgs(Button.ClickEvent)));
			await Task.Delay(1000);

			await MessageBox.Show(MainForm, "Libation provides a built-in cheat sheet for its query language", "Search Cheat Sheet");

			await flashControlAsync(MainForm.filterHelpBtn);
			var filterHelp = await InvokeAsync(() => new SearchSyntaxDialog());
			await InvokeAsync(() => filterHelp.ShowDialog(MainForm));

			return true;
		}
		private async Task<bool> ShowQuickFilters()
		{
			var firstAuthor = getFirstAuthor();

			if (firstAuthor == null) return true;

			if (await OkCancelMessageBox("Queries that you perform regularly can be added to 'Quick Filters'", "Quick Filters") is not DialogResult.OK) return false;

			await InvokeAsync(() => MainForm.filterSearchTb.Text = firstAuthor);

			await Task.Delay(750);
			await flashControlAsync(MainForm.addQuickFilterBtn);
			await InvokeAsync(() => MainForm.addQuickFilterBtn.RaiseEvent(new RoutedEventArgs(Button.ClickEvent)));
			await Task.Delay(750);

			await flashControlAsync(MainForm.quickFiltersToolStripMenuItem);
			await InvokeAsync(MainForm.quickFiltersToolStripMenuItem.Open);
			await Task.Delay(500);

			var editQuickFiltersToolStripMenuItem = MainForm.quickFiltersToolStripMenuItem.ItemsSource.OfType<MenuItem>().ElementAt(1);

			await flashControlAsync(editQuickFiltersToolStripMenuItem);
			await InvokeAsync(() => editQuickFiltersToolStripMenuItem.IsSelected = true);
			await Task.Delay(500);

			var editQuickFilters = await InvokeAsync(() => new EditQuickFilters());
			editQuickFilters.Opened += async (_, _) => await MessageBox.Show(editQuickFilters, "From here you can edit, delete, and change the order of Quick Filters", "Editing Quick Filters");
			await InvokeAsync(() => editQuickFilters.ShowDialog(MainForm));

			return true;
		}

		private string getFirstAuthor()
		{
			var books = DbContexts.GetLibrary_Flat_NoTracking();
			return books.SelectMany(lb => lb.Book.Authors).FirstOrDefault(a => !string.IsNullOrWhiteSpace(a.Name))?.Name;
		}

		private async Task flashControlAsync(TemplatedControl control, int flashCount = 3)
		{
			var backColor = await InvokeAsync(() => control.Background);
			for (int i = 0; i < flashCount; i++)
			{
				await InvokeAsync(() => control.Background = Brushes.Firebrick);
				await Task.Delay(200);
				await InvokeAsync(() => control.Background = backColor);
				await Task.Delay(200);
			}
		}

		private Task<T> InvokeAsync<T>(Func<T> func) => Dispatcher.UIThread.InvokeAsync(func);
		private Task<T> InvokeAsync<T>(Func<Task<T>> func) => Dispatcher.UIThread.InvokeAsync(func);
		private Task InvokeAsync(Func<Task> action) => Dispatcher.UIThread.InvokeAsync(action);
		private Task InvokeAsync(Action action) => Dispatcher.UIThread.InvokeAsync(action);

		private Task<DialogResult> OkCancelMessageBox(string message, string caption)
			=> InvokeAsync(() => MessageBox.Show(MainForm, message, caption, MessageBoxButtons.OKCancel));
	}
}
