using ApplicationServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using LibationWinForms.AvaloniaUI.Controls;
using System;
using LibationWinForms.AvaloniaUI.Views.ProductsGrid;
using Avalonia.ReactiveUI;
using LibationWinForms.AvaloniaUI.ViewModels;
using LibationFileManager;
using DataLayer;
using System.Collections.Generic;

namespace LibationWinForms.AvaloniaUI.Views
{
	public partial class MainWindow : ReactiveWindow<MainWindowViewModel>
	{
		public event EventHandler Load;
		public event EventHandler<List<LibraryBook>> LibraryLoaded;

		public MainWindow()
		{
			InitializeComponent();
#if DEBUG
			this.AttachDevTools();
#endif
			this.FindAllControls();

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

			{
				this.LibraryLoaded += (_, dbBooks) => productsDisplay.Display(dbBooks);
				LibraryCommands.LibrarySizeChanged += (_, _) => productsDisplay.Display(DbContexts.GetLibrary_Flat_NoTracking(includeParents: true));
				this.Closing += (_,_) => this.SaveSizeAndLocation(Configuration.Instance);
			}
		}

		private void InitializeComponent()
		{
			AvaloniaXamlLoader.Load(this);
		}

		public void OnLoad() => Load?.Invoke(this, EventArgs.Empty);
		public void OnLibraryLoaded(List<LibraryBook> initialLibrary) => LibraryLoaded?.Invoke(this, initialLibrary);

		private void FindAllControls()
		{
			{
				autoScanLibraryToolStripMenuItem = this.FindControl<MenuItem>(nameof(autoScanLibraryToolStripMenuItem));
				autoScanLibraryToolStripMenuItemCheckbox = this.FindControl<CheckBox>(nameof(autoScanLibraryToolStripMenuItemCheckbox));
				noAccountsYetAddAccountToolStripMenuItem = this.FindControl<MenuItem>(nameof(noAccountsYetAddAccountToolStripMenuItem));
				scanLibraryToolStripMenuItem = this.FindControl<MenuItem>(nameof(scanLibraryToolStripMenuItem));
				scanLibraryOfAllAccountsToolStripMenuItem = this.FindControl<MenuItem>(nameof(scanLibraryOfAllAccountsToolStripMenuItem));
				scanLibraryOfSomeAccountsToolStripMenuItem = this.FindControl<MenuItem>(nameof(scanLibraryOfSomeAccountsToolStripMenuItem));
				removeLibraryBooksToolStripMenuItem = this.FindControl<MenuItem>(nameof(removeLibraryBooksToolStripMenuItem));
				{
					removeAllAccountsToolStripMenuItem = this.FindControl<MenuItem>(nameof(removeAllAccountsToolStripMenuItem));
					removeSomeAccountsToolStripMenuItem = this.FindControl<MenuItem>(nameof(removeSomeAccountsToolStripMenuItem));
				}
			}

			{
				beginBookBackupsToolStripMenuItem = this.FindControl<FormattableMenuItem>(nameof(beginBookBackupsToolStripMenuItem));
				beginPdfBackupsToolStripMenuItem = this.FindControl<FormattableMenuItem>(nameof(beginPdfBackupsToolStripMenuItem));
				liberateVisibleToolStripMenuItem_LiberateMenu = this.FindControl<FormattableMenuItem>(nameof(liberateVisibleToolStripMenuItem_LiberateMenu));
			}

			{
				exportLibraryToolStripMenuItem = this.FindControl<MenuItem>(nameof(exportLibraryToolStripMenuItem));
			}

			quickFiltersToolStripMenuItem = this.FindControl<MenuItem>(nameof(quickFiltersToolStripMenuItem));
			{
				firstFilterIsDefaultToolStripMenuItem = this.FindControl<MenuItem>(nameof(firstFilterIsDefaultToolStripMenuItem));
				firstFilterIsDefaultToolStripMenuItem_Checkbox = this.FindControl<CheckBox>(nameof(firstFilterIsDefaultToolStripMenuItem_Checkbox));
				editQuickFiltersToolStripMenuItem = this.FindControl<MenuItem>(nameof(editQuickFiltersToolStripMenuItem));
			}

			visibleBooksToolStripMenuItem = this.FindControl<FormattableMenuItem>(nameof(visibleBooksToolStripMenuItem));
			{
				liberateVisibleToolStripMenuItem_VisibleBooksMenu = this.FindControl<FormattableMenuItem>(nameof(liberateVisibleToolStripMenuItem_VisibleBooksMenu));
				setDownloadedToolStripMenuItem = this.FindControl<MenuItem>(nameof(setDownloadedToolStripMenuItem));
				removeToolStripMenuItem = this.FindControl<MenuItem>(nameof(removeToolStripMenuItem));
			}

			scanningToolStripMenuItem = this.FindControl<StackPanel>(nameof(scanningToolStripMenuItem));
			scanningToolStripMenuItem_Text = this.FindControl<TextBlock>(nameof(scanningToolStripMenuItem_Text));


			filterHelpBtn = this.FindControl<Button>(nameof(filterHelpBtn));
			addQuickFilterBtn = this.FindControl<Button>(nameof(addQuickFilterBtn));
			filterSearchTb = this.FindControl<TextBox>(nameof(filterSearchTb));
			filterBtn = this.FindControl<Button>(nameof(filterBtn));
			toggleQueueHideBtn = this.FindControl<Button>(nameof(toggleQueueHideBtn));

			removeBooksBtn = this.FindControl<Button>(nameof(removeBooksBtn));
			doneRemovingBtn = this.FindControl<Button>(nameof(doneRemovingBtn));

			splitContainer1 = this.FindControl<SplitView>(nameof(splitContainer1));
			productsDisplay = this.FindControl<ProductsDisplay2>(nameof(productsDisplay));
			processBookQueue1 = this.FindControl<ProcessQueueControl2>(nameof(processBookQueue1));

			visibleCountLbl = this.FindControl<FormattableTextBlock>(nameof(visibleCountLbl));
			backupsCountsLbl = this.FindControl<TextBlock>(nameof(backupsCountsLbl));
			pdfsCountsLbl = this.FindControl<FormattableTextBlock>(nameof(pdfsCountsLbl));

		}

		protected override void OnDataContextChanged(EventArgs e)
		{
			base.OnDataContextChanged(e);
		}
	}
}
