using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.ReactiveUI;
using DataLayer;
using LibationAvalonia.ViewModels;
using LibationFileManager;
using LibationUiBase.GridView;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LibationAvalonia.Views
{
	public partial class MainWindow : ReactiveWindow<MainVM>
	{
		public event EventHandler<List<LibraryBook>> LibraryLoaded;
		public MainWindow()
		{
			DataContext = new MainVM(this);

			InitializeComponent();
			Configure_Upgrade();

			Loaded += MainWindow_Loaded;
			Closing += MainWindow_Closing;
			LibraryLoaded += MainWindow_LibraryLoaded;
		}

		private async void MainWindow_Loaded(object sender, EventArgs e)
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

		private async void MainWindow_LibraryLoaded(object sender, List<LibraryBook> dbBooks)
		{
			if (QuickFilters.UseDefault)
				await ViewModel.PerformFilter(QuickFilters.Filters.FirstOrDefault());

			ViewModel.ProductsDisplay.BindToGrid(dbBooks);
		}

		public void OnLibraryLoaded(List<LibraryBook> initialLibrary) => LibraryLoaded?.Invoke(this, initialLibrary);
		public void ProductsDisplay_LiberateClicked(object _, LibraryBook libraryBook) => ViewModel.LiberateClicked(libraryBook);
		public void ProductsDisplay_LiberateSeriesClicked(object _, ISeriesEntry series) => ViewModel.LiberateSeriesClicked(series);
		public void ProductsDisplay_ConvertToMp3Clicked(object _, LibraryBook libraryBook) => ViewModel.ConvertToMp3Clicked(libraryBook);

		public async void filterSearchTb_KeyPress(object _, KeyEventArgs e)
		{
			if (e.Key == Key.Return)
			{
				await ViewModel.PerformFilter(ViewModel.FilterString);

				// silence the 'ding'
				e.Handled = true;
			}
		}

		private void QuickFiltersMenuItem_KeyDown(object _, KeyEventArgs e)
		{
			int keyNum = (int)e.Key - 34;

			if (keyNum <= 9 && keyNum >= 1)
			{
				ViewModel.QuickFilterMenuItems
					.OfType<MenuItem>()
					.FirstOrDefault(i => i.Header is string h && h.StartsWith($"_{keyNum}"))
					?.Command
					?.Execute(null);
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
