using ApplicationServices;
using AudibleUtilities;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Styling;
using Dinah.Core.StepRunner;
using LibationAvalonia.Dialogs;
using LibationAvalonia.Views;
using LibationFileManager;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Avalonia.Threading.Dispatcher;

namespace LibationAvalonia
{
	internal class Walkthrough
	{
		private Dictionary<string, string> settingTabMessages = new()
		{
			{ "Important Settings", "From here you can change where liberated books are stored and how detailed Libation's logs are.\r\n\r\nIf you experience a problem and need help, you'll be asked to provide your log file. In certain circumstances we may need you to reproduce the error with a higher level of logging detail."},
			{ "Import Library", "In this tab you can change how your library is scanned and imported into Libation, as well as automatic liberation."},
			{ "Download/Decrypt", "These settings allow you to control how liberated files and folders are named and stored.\r\nYou can customize the 'Naming Templates' to use any number of the audiobook's properties to build a customized file and folder naming format. Learn more about the syntax from the wiki at\r\n\r\nhttps://github.com/rmcrackan/Libation/blob/master/Documentation/NamingTemplates.md"},
			{ "Audio File Settings", "Control how audio files are decrypted, including audio format and metadata handling.\r\n\r\nIf you choose to split your audiobook into multiple files by chapter marker, you may edit the chapter file 'Naming Template' to control how each chapter file is named."},
		};

		private static readonly IBrush FlashColor = Brushes.DodgerBlue;
		private readonly MainWindow MainForm;
		private readonly AsyncStepSequence sequence = new();
		public Walkthrough(MainWindow mainForm)
		{
			var autoscan = Configuration.Instance.AutoScan;
			Configuration.Instance.AutoScan = false;
			MainForm = mainForm;
			sequence[nameof(ShowAccountDialog)] = () => UIThread.InvokeAsync(ShowAccountDialog);
			sequence[nameof(ShowSettingsDialog)] = () => UIThread.InvokeAsync(ShowSettingsDialog);
			sequence[nameof(ShowAccountScanning)] = () => UIThread.InvokeAsync(ShowAccountScanning);
			sequence[nameof(ShowSearching)] = () => UIThread.InvokeAsync(ShowSearching);
			sequence[nameof(ShowQuickFilters)] = () => UIThread.InvokeAsync(ShowQuickFilters);
			sequence[nameof(ShowTourComplete)] = () => UIThread.InvokeAsync(ShowTourComplete);
			Configuration.Instance.AutoScan = autoscan;
		}

		public async Task RunAsync() => await sequence.RunAsync();

		private async Task<bool> ShowAccountDialog()
		{
			if (!await ProceedMessageBox("First, add your Audible account(s).", "Add Accounts"))
				return false;

			await Task.Delay(750);
			await displayControlAsync(MainForm.settingsToolStripMenuItem);
			await displayControlAsync(MainForm.accountsToolStripMenuItem);

			var accountSettings = new AccountsDialog();
			accountSettings.Loaded += async (_, _) => await MessageBox.Show(accountSettings, "Add your Audible account(s), then save.", "Add an Account");
			await accountSettings.ShowDialog(MainForm);
			return true;
		}

		private async Task<bool> ShowSettingsDialog()
		{
			if (!await ProceedMessageBox("Next, adjust Libation's settings", "Change Settings"))
				return false;

			await Task.Delay(750);
			await displayControlAsync(MainForm.settingsToolStripMenuItem);
			await displayControlAsync(MainForm.basicSettingsToolStripMenuItem);

			var settingsDialog = await UIThread.InvokeAsync(() => new SettingsDialog());

			var tabsToVisit = settingsDialog.tabControl.Items.OfType<TabItem>().ToList();

			foreach (var tab in tabsToVisit)
				tab.PropertyChanged += TabControl_PropertyChanged;

			settingsDialog.Loaded += SettingsDialog_Loaded;
			settingsDialog.Closing += SettingsDialog_FormClosing;
			settingsDialog.saveBtn.Content = "Next Tab";

			await settingsDialog.ShowDialog(MainForm);

			return true;

			async Task ShowTabPageMessageBoxAsync(TabItem selectedTab)
			{
				tabsToVisit.Remove(selectedTab);

				if (!selectedTab.IsVisible || !(selectedTab.Header is TextBlock header && settingTabMessages.ContainsKey(header.Text))) return;

				if (tabsToVisit.Count == 0)
					settingsDialog.saveBtn.Content = "Save";

				await MessageBox.Show(settingsDialog, settingTabMessages[header.Text], header.Text + " Tab", MessageBoxButtons.OK);

				settingTabMessages.Remove(header.Text);
			}

			async void SettingsDialog_Loaded(object sender, RoutedEventArgs e)
			{
				await ShowTabPageMessageBoxAsync(tabsToVisit[0]);
			}

			async void TabControl_PropertyChanged(object sender, Avalonia.AvaloniaPropertyChangedEventArgs e)
			{
				if (e.Property == TabItem.IsSelectedProperty && settingsDialog.IsLoaded)
				{
					await ShowTabPageMessageBoxAsync(sender as TabItem);
				}
			}

			void SettingsDialog_FormClosing(object sender, WindowClosingEventArgs e)
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
			var persister = AudibleApiStorage.GetAccountsSettingsPersister();
			var count = persister.AccountsSettings.Accounts.Count;
			persister.Dispose();

			if (count < 1)
			{
				await MessageBox.Show(MainForm, "Add an Audible account, then sync your library through the 'Import' menu", "Add an Audible Account", MessageBoxButtons.OK, MessageBoxIcon.Information);
				return false;
			}

			var accounts = count > 1 ? "accounts" : "account";
			var library = count > 1 ? "libraries" : "library";
			if(! await ProceedMessageBox($"Finally, scan your Audible {accounts} to sync your {library} with Libation.\r\n\r\nIf this is your first time scanning an account, you'll be prompted to enter your account's password to log into your Audible account.", $"Scan {accounts}"))
				return false;

			var scanItem = count > 1 ? MainForm.scanLibraryOfAllAccountsToolStripMenuItem : MainForm.scanLibraryToolStripMenuItem;

			await Task.Delay(750);
			await displayControlAsync(MainForm.importToolStripMenuItem);
			await displayControlAsync(scanItem);

			scanItem.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
			MainForm.importToolStripMenuItem.Close();

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

			if (!await ProceedMessageBox("You can filter the grid entries by searching", "Searching"))
				return false;

			await displayControlAsync(MainForm.filterSearchTb);

			MainForm.filterSearchTb.Text = string.Empty;
			foreach (var c in firstAuthor)
			{
				MainForm.filterSearchTb.Text += c;
				await Task.Delay(150);
			}

			await displayControlAsync(MainForm.filterBtn);

			MainForm.filterBtn.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

			await Task.Delay(1000);

			await MessageBox.Show(MainForm, "Libation provides a built-in cheat sheet for its query language", "Search Cheat Sheet");
			
			await displayControlAsync(MainForm.filterHelpBtn);

			var filterHelp = new SearchSyntaxDialog();
			await filterHelp.ShowDialog(MainForm);

			return true;
		}

		private async Task<bool> ShowQuickFilters()
		{
			var firstAuthor = getFirstAuthor();

			if (firstAuthor == null) return true;

			if (!await ProceedMessageBox("Queries that you perform regularly can be added to 'Quick Filters'", "Quick Filters"))
				return false;

			MainForm.filterSearchTb.Text = firstAuthor;

			var editQuickFiltersToolStripMenuItem = MainForm.quickFiltersToolStripMenuItem.ItemsSource.OfType<MenuItem>().ElementAt(1);

			await Task.Delay(750);
			await displayControlAsync(MainForm.addQuickFilterBtn);
			MainForm.addQuickFilterBtn.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
			await displayControlAsync(MainForm.quickFiltersToolStripMenuItem);
			await displayControlAsync(editQuickFiltersToolStripMenuItem);

			var editQuickFilters = new EditQuickFilters();
			editQuickFilters.Loaded += async (_, _) => await MessageBox.Show(editQuickFilters, "From here you can edit, delete, and change the order of Quick Filters", "Editing Quick Filters");
			await editQuickFilters.ShowDialog(MainForm);

			return true;
		}

		private async Task<bool> ShowTourComplete()
		{
			await MessageBox.Show(MainForm, "You're now ready to begin using Libation.\r\n\r\nEnjoy!", "Tour Finished");
			return true;
		}

		private string getFirstAuthor()
		{
			var books = DbContexts.GetLibrary_Flat_NoTracking();
			return books.SelectMany(lb => lb.Book.Authors).FirstOrDefault(a => !string.IsNullOrWhiteSpace(a.Name))?.Name;
		}

		private async Task displayControlAsync(TemplatedControl control)
		{
			await UIThread.InvokeAsync(() => control.IsEnabled = false);
			await UIThread.InvokeAsync(MainForm.productsDisplay.Focus);
			await UIThread.InvokeAsync(() => flashControlAsync(control));
			if (control is MenuItem menuItem) await UIThread.InvokeAsync(menuItem.Open);
			await Task.Delay(500);
			await UIThread.InvokeAsync(() => control.IsEnabled = true);
		}

		private static async Task flashControlAsync(TemplatedControl control, int flashCount = 3)
		{
			for (int i = 0; i < flashCount; i++)
			{
				control.Styles.Add(disabledStyle);
				control.Styles.Add(disabledStyle2);
				await Task.Delay(200);
				control.Styles.Remove(disabledStyle);
				control.Styles.Remove(disabledStyle2);
				control.Styles.Add(enabedStyle);
				control.Styles.Add(enabedStyle2);
				control.InvalidateVisual();
				await Task.Delay(200);
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

		private static Selector TemplateSelector = Selectors.Is<TemplatedControl>(null).PropertyEquals(Avalonia.Input.InputElement.IsEnabledProperty, false).Template();
		private static Selector ContentPresenterSelector = TemplateSelector.Is<ContentPresenter>();
		private static Selector BorderSelector = TemplateSelector.Is<Border>();

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
}
