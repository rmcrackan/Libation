using System;
using System.Collections.Generic;
using System.Linq;
using ApplicationServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using DataLayer;
using LibationAvalonia.ViewModels;
using LibationFileManager;

namespace LibationAvalonia.Views
{
	public partial class MainWindow : ReactiveWindow<MainWindowViewModel>
	{
		public event EventHandler Load;
		public event EventHandler<List<LibraryBook>> LibraryLoaded;
		private readonly MainWindowViewModel _viewModel;

		public MainWindow()
		{
			this.DataContext = _viewModel = new MainWindowViewModel();

			InitializeComponent();
#if DEBUG
			this.AttachDevTools();
#endif
			FindAllControls();

			// eg: if one of these init'd productsGrid, then another can't reliably subscribe to it
			Configure_BackupCounts();
			Configure_ScanAuto();
			Configure_ScanNotification();
			Configure_VisibleBooks();
			Configure_QuickFilters();
			Configure_ScanManual();
			Configure_RemoveBooks();
			Configure_Liberate();
			Configure_Export();
			Configure_Settings();
			Configure_ProcessQueue();
			Configure_Upgrade();
			Configure_Filter();
			// misc which belongs in winforms app but doesn't have a UI element
			Configure_NonUI();

			_viewModel.ProductsDisplay.RemovableCountChanged += ProductsDisplay_RemovableCountChanged;
			_viewModel.ProductsDisplay.VisibleCountChanged += ProductsDisplay_VisibleCountChanged;

			{
				this.LibraryLoaded += MainWindow_LibraryLoaded;

				LibraryCommands.LibrarySizeChanged += async (_, _) => await _viewModel.ProductsDisplay.UpdateGridAsync(DbContexts.GetLibrary_Flat_NoTracking(includeParents: true));
				Closing += (_, _) => this.SaveSizeAndLocation(Configuration.Instance);
			}
			Closing += MainWindow_Closing;
		}

		private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			productsDisplay?.CloseImageDisplay();
		}

		private async void MainWindow_LibraryLoaded(object sender, List<LibraryBook> dbBooks)
		{
			if (QuickFilters.UseDefault)
				await performFilter(QuickFilters.Filters.FirstOrDefault());

			_viewModel.ProductsDisplay.BindToGrid(dbBooks);
		}

		private void InitializeComponent()
		{
			AvaloniaXamlLoader.Load(this);
		}

		public void OnLoad() => Load?.Invoke(this, EventArgs.Empty);
		public void OnLibraryLoaded(List<LibraryBook> initialLibrary) => LibraryLoaded?.Invoke(this, initialLibrary);

		private void FindAllControls()
		{
			quickFiltersToolStripMenuItem = this.FindControl<MenuItem>(nameof(quickFiltersToolStripMenuItem));
			productsDisplay = this.FindControl<ProductsDisplay>(nameof(productsDisplay));
		}
	}
}
