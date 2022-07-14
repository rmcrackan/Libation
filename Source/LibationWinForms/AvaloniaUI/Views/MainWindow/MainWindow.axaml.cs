using ApplicationServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using LibationWinForms.AvaloniaUI.Controls;
using System;
using System.Linq;
using Avalonia.Threading;
using LibationWinForms.AvaloniaUI.Views.ProductsGrid;
using Avalonia.ReactiveUI;
using LibationWinForms.AvaloniaUI.ViewModels;
using System.Threading.Tasks;
using ReactiveUI;
using LibationWinForms.AvaloniaUI.ViewModels.Dialogs;
using LibationWinForms.AvaloniaUI.Views.Dialogs;
using Avalonia.Media;
using System.Collections.Generic;

namespace LibationWinForms.AvaloniaUI.Views
{
	public partial class MainWindow : ReactiveWindow<MainWindowViewModel>
	{
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
				this.Load += (_, _) => productsDisplay.Display();
				LibraryCommands.LibrarySizeChanged += (_, __) => Dispatcher.UIThread.Post(() => productsDisplay.Display());
			}
		}
		public event EventHandler Load;

		public void OnLoad() => Load?.Invoke(this, EventArgs.Empty);

		public async void ShowMessageBoxButton_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
		{
			await Task.Run(() => DoShowDialogAsync());
		}

		private async Task DoShowDialogAsync()
		{

			string caption = "this is a dialog message";
			string message =
@"The Collatz conjecture is: This process will eventually reach the number 1, regardless of which positive integer is chosen initially.

If the conjecture is false, it can only be because there is some starting number which gives rise to a sequence that does not contain 1. Such a sequence would either enter a repeating cycle that excludes 1, or increase without bound. No such sequence has been found.

The smallest i such that ai < a0 is called the stopping time of n. Similarly, the smallest k such that ak = 1 is called the total stopping time of n.[3] If one of the indexes i or k doesn't exist, we say that the stopping time or the total stopping time, respectively, is infinite.

The Collatz conjecture asserts that the total stopping time of every n is finite. It is also equivalent to saying that every n >= 2 has a finite stopping time.

Since 3n + 1 is even whenever n is odd, one may instead use the shortcut form of the Collatz function:";


			var result = await MessageBox.Show(message, caption);
		}

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


		private void InitializeComponent()
		{
			AvaloniaXamlLoader.Load(this);
		}
	}
}
