using AudibleUtilities;
using Avalonia.Controls;
using Avalonia.Threading;
using Dinah.Core.StepRunner;
using LibationAvalonia.Dialogs;
using LibationAvalonia.Views;
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
			sequence[nameof(ScanAccounts)] = ScanAccounts;
		}

		public async Task RunAsync() => await sequence.RunAsync();

		private async Task<bool> ShowAccountDialog()
		{
			await Dispatcher.UIThread.InvokeAsync(() => MessageBox.Show(MainForm, "First, add you Audible account(s).", "Add Accounts"));

			await Task.Delay(750);
			await Dispatcher.UIThread.InvokeAsync(MainForm.settingsToolStripMenuItem.Open);
			await Task.Delay(500);
			await Dispatcher.UIThread.InvokeAsync(() => MainForm.accountsToolStripMenuItem.IsSelected = true);
			await Task.Delay(1000);
			var accountSettings = await Dispatcher.UIThread.InvokeAsync(() => new AccountsDialog());
			accountSettings.Opened += (_, _) => MessageBox.Show(accountSettings, "Add your Audible account(s), then save.", "Add an Account");
			await Dispatcher.UIThread.InvokeAsync(() => accountSettings.ShowDialog(MainForm));
			return true;
		}

		private async Task<bool> ShowSettingsDialog()
		{
			await Dispatcher.UIThread.InvokeAsync(() => MessageBox.Show(MainForm, "Next, adjust Libation's settings", "Change Settings"));

			await Task.Delay(750);
			await Dispatcher.UIThread.InvokeAsync(MainForm.settingsToolStripMenuItem.Open);
			await Task.Delay(500);
			await Dispatcher.UIThread.InvokeAsync(() => MainForm.basicSettingsToolStripMenuItem.IsSelected = true);
			await Task.Delay(1000);

			var settingsDialog = await Dispatcher.UIThread.InvokeAsync(() => new SettingsDialog());

			var tabsToVisit = settingsDialog.tabControl.Items.OfType<TabItem>().ToList();

			foreach (var tab in tabsToVisit)
				tab.PropertyChanged += TabControl_PropertyChanged;

			settingsDialog.Closing += SettingsDialog_FormClosing;

			await Dispatcher.UIThread.InvokeAsync(() => settingsDialog.ShowDialog(MainForm));

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

		private async Task<bool> ScanAccounts()
		{
			using var persister = AudibleApiStorage.GetAccountsSettingsPersister();
			var count = persister.AccountsSettings.Accounts.Count;

			if (count < 1)
			{
				await Dispatcher.UIThread.InvokeAsync(() => MessageBox.Show(MainForm, "Add an Audible account, then sync your library through the \"Import\" menu", "Add an Audible Account", MessageBoxButtons.OK, MessageBoxIcon.Information));
				return false;
			}

			var accounts = count > 1 ? "accounts" : "account";
			var library = count > 1 ? "libraries" : "library";
			await Dispatcher.UIThread.InvokeAsync(() => MessageBox.Show(MainForm, $"Finally, scan your Audible {accounts} to sync your {library} with Libation", $"Scan {accounts}"));

			await Task.Delay(750);
			await Dispatcher.UIThread.InvokeAsync(MainForm.importToolStripMenuItem.Open);
			await Task.Delay(500);
			await Dispatcher.UIThread.InvokeAsync(() => (count > 1 ? MainForm.scanLibraryOfAllAccountsToolStripMenuItem : MainForm.scanLibraryToolStripMenuItem).IsSelected = true);
			await Task.Delay(1000);
			await Dispatcher.UIThread.InvokeAsync(() => (count > 1 ? MainForm.scanLibraryOfAllAccountsToolStripMenuItem : MainForm.scanLibraryToolStripMenuItem).RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(MenuItem.ClickEvent)));

			return true;
		}
	}
}
