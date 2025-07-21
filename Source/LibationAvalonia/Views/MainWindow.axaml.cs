using AudibleUtilities;
using Avalonia.Input;
using Avalonia.ReactiveUI;
using Avalonia.Threading;
using DataLayer;
using FileManager;
using LibationAvalonia.Dialogs;
using LibationAvalonia.ViewModels;
using LibationFileManager;
using LibationUiBase.Forms;
using LibationUiBase.GridView;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LibationAvalonia.Views
{
	public partial class MainWindow : ReactiveWindow<MainVM>
	{
		public MainWindow()
		{
			DataContext = new MainVM(this);
			ApiExtended.LoginChoiceFactory = account => new Dialogs.Login.AvaloniaLoginChoiceEager(account);

			AudibleApiStorage.LoadError += AudibleApiStorage_LoadError;
			InitializeComponent();
			Configure_Upgrade();

			Opened += MainWindow_Opened;
			Closing += MainWindow_Closing;

			KeyBindings.Add(new KeyBinding { Command = ReactiveCommand.Create(selectAndFocusSearchBox), Gesture = new KeyGesture(Key.F, Configuration.IsMacOs ? KeyModifiers.Meta : KeyModifiers.Control) });

			if (!Configuration.IsMacOs)
			{
				KeyBindings.Add(new KeyBinding { Command = ReactiveCommand.Create(ViewModel.ShowSettingsAsync), Gesture = new KeyGesture(Key.P, KeyModifiers.Control) });
				KeyBindings.Add(new KeyBinding { Command = ReactiveCommand.Create(ViewModel.ShowAccountsAsync), Gesture = new KeyGesture(Key.A, KeyModifiers.Control | KeyModifiers.Shift) });
				KeyBindings.Add(new KeyBinding { Command = ReactiveCommand.Create(ViewModel.ExportLibraryAsync), Gesture = new KeyGesture(Key.S, KeyModifiers.Control) });
			}
		}

		private void AudibleApiStorage_LoadError(object sender, AccountSettingsLoadErrorEventArgs e)
		{
			try
			{
				//Backup AccountSettings.json and create a new, empty file.
				var backupFile =
					FileUtility.SaferMoveToValidPath(
						e.SettingsFilePath,
						e.SettingsFilePath,
						ReplacementCharacters.Barebones,
						"bak");
				AudibleApiStorage.EnsureAccountsSettingsFileExists();
				e.Handled = true;

				showAccountSettingsRecoveredMessage(backupFile);
			}
			catch
			{
				showAccountSettingsUnrecoveredMessage();
			}

			async void showAccountSettingsRecoveredMessage(LongPath backupFile)
			=> await MessageBox.Show(this, $"""
				Libation could not load your account settings, so it had created a new, empty account settings file.

				You will need to re-add you Audible account(s) before scanning or downloading.

				The old account settings file has been archived at '{backupFile.PathWithoutPrefix}'

				{e.GetException().ToString()}
				""",
				"Error Loading Account Settings",
				MessageBoxButtons.OK,
				MessageBoxIcon.Warning);

			void showAccountSettingsUnrecoveredMessage()
			{
				var messageBoxWindow = MessageBox.Show(this, $"""
				Libation could not load your account settings. The file may be corrupted, but Libation is unable to delete it.

				Please move or delete the account settings file '{e.SettingsFilePath}'

				{e.GetException().ToString()}
				""",
				"Error Loading Account Settings",
				MessageBoxButtons.OK);

				//Force the message box to show synchronously because we're not handling the exception
				//and libation will crash after the event handler returns
				var frame = new DispatcherFrame();
				_ = messageBoxWindow.ContinueWith(static (_, s) => ((DispatcherFrame)s).Continue = false, frame);
				Dispatcher.UIThread.PushFrame(frame);
				messageBoxWindow.GetAwaiter().GetResult();
			}
		}

		private async void MainWindow_Opened(object sender, EventArgs e)
		{
			if (Configuration.Instance.FirstLaunch)
			{
				var result = await MessageBox.Show(this, "Would you like a guided tour to get started?", "Libation Walkthrough", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);

				if (result is DialogResult.Yes)
				{
					await new Walkthrough(this).RunAsync();
				}

				Configuration.Instance.FirstLaunch = false;
			}
		}
		
		private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			productsDisplay?.CloseImageDisplay();
			this.SaveSizeAndLocation(Configuration.Instance);
		}

		private void selectAndFocusSearchBox()
		{
			filterSearchTb.SelectAll();
			filterSearchTb.Focus();
		}

		public async Task OnLibraryLoadedAsync(List<LibraryBook> initialLibrary)
		{
			//Get the ViewModel before crossing the await boundary
			var vm = ViewModel;
			if (QuickFilters.UseDefault)
				await vm.PerformFilter(QuickFilters.Filters.FirstOrDefault());

			await Task.WhenAll(
				vm.SetBackupCountsAsync(initialLibrary),
				Task.Run(() => vm.ProductsDisplay.BindToGridAsync(initialLibrary)));
		}

		public void ProductsDisplay_LiberateClicked(object _, LibraryBook[] libraryBook) => ViewModel.LiberateClicked(libraryBook);
		public void ProductsDisplay_LiberateSeriesClicked(object _, SeriesEntry series) => ViewModel.LiberateSeriesClicked(series);
		public void ProductsDisplay_ConvertToMp3Clicked(object _, LibraryBook[] libraryBook) => ViewModel.ConvertToMp3Clicked(libraryBook);

		BookDetailsDialog bookDetailsForm;
		public void ProductsDisplay_TagsButtonClicked(object _, LibraryBook libraryBook)
		{
			if (bookDetailsForm is null || !bookDetailsForm.IsVisible)
			{
				bookDetailsForm = new BookDetailsDialog(libraryBook);
				bookDetailsForm.Show(this);
			}
			else
				bookDetailsForm.LibraryBook = libraryBook;
		}

		public async void filterSearchTb_KeyPress(object _, KeyEventArgs e)
		{
			if (e.Key == Key.Return)
			{
				await ViewModel.PerformFilter(ViewModel.SelectedNamedFilter);

				// silence the 'ding'
				e.Handled = true;
			}
		}

		private void Configure_Upgrade()
		{
			setProgressVisible(false);
#pragma warning disable CS8321 // Local function is declared but never used
			async Task upgradeAvailable(LibationUiBase.UpgradeEventArgs e)
			{
				var notificationResult = await new UpgradeNotificationDialog(e.UpgradeProperties, e.CapUpgrade).ShowDialogAsync(this);

				e.Ignore = notificationResult == DialogResult.Ignore;
				e.InstallUpgrade = notificationResult == DialogResult.OK;
			}
#pragma warning restore CS8321 // Local function is declared but never used

			var upgrader = new LibationUiBase.Upgrader();
			upgrader.DownloadProgress += async (_, e) => await Dispatcher.UIThread.InvokeAsync(() => ViewModel.DownloadProgress = e.ProgressPercentage);
			upgrader.DownloadBegin += async (_, _) => await Dispatcher.UIThread.InvokeAsync(() => setProgressVisible(true));
			upgrader.DownloadCompleted += async (_, _) => await Dispatcher.UIThread.InvokeAsync(() => setProgressVisible(false));
			upgrader.UpgradeFailed += async (_, message) => await Dispatcher.UIThread.InvokeAsync(() => { setProgressVisible(false); MessageBox.Show(this, message, "Upgrade Failed", MessageBoxButtons.OK, MessageBoxIcon.Error); });

#if !DEBUG
			Opened += async (_, _) => await upgrader.CheckForUpgradeAsync(upgradeAvailable);
#endif
		}

		private void setProgressVisible(bool visible) => ViewModel.DownloadProgress = visible ? 0 : null;
	}
}
