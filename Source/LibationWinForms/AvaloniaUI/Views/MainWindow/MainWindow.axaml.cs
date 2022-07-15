using ApplicationServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System;
using LibationWinForms.AvaloniaUI.Views.ProductsGrid;
using Avalonia.ReactiveUI;
using LibationWinForms.AvaloniaUI.ViewModels;
using LibationFileManager;
using DataLayer;
using System.Collections.Generic;
using System.Linq;

namespace LibationWinForms.AvaloniaUI.Views
{
	public partial class MainWindow : ReactiveWindow<MainWindowViewModel>
	{
		public event EventHandler Load;
		public event EventHandler<List<LibraryBook>> LibraryLoaded;
		private MainWindowViewModel _viewModel;

		public MainWindow()
		{
			InitializeComponent();
#if DEBUG
			this.AttachDevTools();
#endif
			this.FindAllControls();

			this.DataContext = _viewModel = new MainWindowViewModel();

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
			Configure_Filter();
			// misc which belongs in winforms app but doesn't have a UI element
			Configure_NonUI();

			_viewModel.ProductsDisplay.InitialLoaded += ProductsDisplay_Initialized;
			_viewModel.ProductsDisplay.RemovableCountChanged += ProductsDisplay_RemovableCountChanged;
			_viewModel.ProductsDisplay.VisibleCountChanged += ProductsDisplay_VisibleCountChanged;

			{
				this.LibraryLoaded += MainWindow_LibraryLoaded;

				LibraryCommands.LibrarySizeChanged += async (_, _) => await _viewModel.ProductsDisplay.DisplayBooks(DbContexts.GetLibrary_Flat_NoTracking(includeParents: true));
				this.Closing += (_,_) => this.SaveSizeAndLocation(Configuration.Instance);
			}
		}

		private void MainWindow_LibraryLoaded(object sender, List<LibraryBook> dbBooks)
		{
			if (Design.IsDesignMode)
				return;

			_viewModel.ProductsDisplay.InitialDisplay(dbBooks, productsDisplay);
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
			productsDisplay = this.FindControl<ProductsDisplay2>(nameof(productsDisplay));
		}

		protected override void OnDataContextChanged(EventArgs e)
		{
			base.OnDataContextChanged(e);
		}
	}
}
