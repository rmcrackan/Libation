using Avalonia.Input;
using Avalonia.ReactiveUI;
using DataLayer;
using LibationAvalonia.ViewModels;
using LibationFileManager;
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
			if (QuickFilters.UseDefault)
				await ViewModel.PerformFilter(QuickFilters.Filters.FirstOrDefault());

			await Task.WhenAll(
				ViewModel.SetBackupCountsAsync(initialLibrary),
				ViewModel.ProductsDisplay.BindToGridAsync(initialLibrary));
		}

		public void ProductsDisplay_LiberateClicked(object _, LibraryBook libraryBook) => ViewModel.LiberateClicked(libraryBook);
		public void ProductsDisplay_LiberateSeriesClicked(object _, ISeriesEntry series) => ViewModel.LiberateSeriesClicked(series);
		public void ProductsDisplay_ConvertToMp3Clicked(object _, LibraryBook libraryBook) => ViewModel.ConvertToMp3Clicked(libraryBook);

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
#if !DEBUG
			async System.Threading.Tasks.Task upgradeAvailable(LibationUiBase.UpgradeEventArgs e)
			{
				var notificationResult = await new Dialogs.UpgradeNotificationDialog(e.UpgradeProperties, e.CapUpgrade).ShowDialogAsync(this);

				e.Ignore = notificationResult == DialogResult.Ignore;
				e.InstallUpgrade = notificationResult == DialogResult.OK;
			}

			var upgrader = new LibationUiBase.Upgrader();
			upgrader.DownloadProgress += async (_, e) => await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() => ViewModel.DownloadProgress = e.ProgressPercentage);
			upgrader.DownloadBegin += async (_, _) => await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() => setProgressVisible(true));
			upgrader.DownloadCompleted += async (_, _) => await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() => setProgressVisible(false));

			Opened += async (_, _) => await upgrader.CheckForUpgradeAsync(upgradeAvailable);
#endif
		}

		private void setProgressVisible(bool visible) => ViewModel.DownloadProgress = visible ? 0 : null;
	}
}
