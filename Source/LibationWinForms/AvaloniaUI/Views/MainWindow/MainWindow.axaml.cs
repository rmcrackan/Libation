using ApplicationServices;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Markup.Xaml;
using DataLayer;
using LibationWinForms.AvaloniaUI.Controls;
using LibationWinForms.AvaloniaUI.ViewModels;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Threading;

namespace LibationWinForms.AvaloniaUI.Views
{
	public partial class MainWindow : Window
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
				LibraryCommands.LibrarySizeChanged += (_, __) => Dispatcher.UIThread.Post(() => productsDisplay.Display());
			}
		}
		/*
		MenuItem importToolStripMenuItem;
		MenuItem autoScanLibraryToolStripMenuItem;
		CheckBox autoScanLibraryToolStripMenuItemCheckbox;
		MenuItem noAccountsYetAddAccountToolStripMenuItem;
		MenuItem scanLibraryToolStripMenuItem;
		MenuItem scanLibraryOfAllAccountsToolStripMenuItem;
		MenuItem scanLibraryOfSomeAccountsToolStripMenuItem;
		MenuItem removeLibraryBooksToolStripMenuItem;
		MenuItem removeAllAccountsToolStripMenuItem;
		MenuItem removeSomeAccountsToolStripMenuItem;
		MenuItem liberateToolStripMenuItem;
		MenuItem beginBookBackupsToolStripMenuItem;
		MenuItem beginPdfBackupsToolStripMenuItem;
		MenuItem convertAllM4bToMp3ToolStripMenuItem;
		MenuItem liberateVisibleToolStripMenuItem_LiberateMenu;
		MenuItem exportToolStripMenuItem;
		MenuItem exportLibraryToolStripMenuItem;
		MenuItem quickFiltersToolStripMenuItem;
		MenuItem firstFilterIsDefaultToolStripMenuItem;
		CheckBox firstFilterIsDefaultToolStripMenuItem_Checkbox;
		MenuItem editQuickFiltersToolStripMenuItem;
		MenuItem visibleBooksToolStripMenuItem;
		MenuItem liberateVisibleToolStripMenuItem_VisibleBooksMenu;
		MenuItem replaceTagsToolStripMenuItem;
		MenuItem setDownloadedToolStripMenuItem;
		MenuItem removeToolStripMenuItem;
		MenuItem settingsToolStripMenuItem;
		MenuItem accountsToolStripMenuItem;
		MenuItem basicSettingsToolStripMenuItem;
		MenuItem aboutToolStripMenuItem;


		StackPanel scanningToolStripMenuItem;
		TextBlock scanningToolStripMenuItem_Text;

		Button filterHelpBtn;
		Button addQuickFilterBtn;
		TextBox filterSearchTb;
		Button filterBtn;
		Button toggleQueueHideBtn;

		StackPanel removeBooksButtonsPanel;
		Button removeBooksBtn;

		SplitView splitContainer1;
		ProductsDisplay2 productsDisplay;
		ProcessQueueControl2 processBookQueue1;
		*/

		private void FindAllControls()
		{
			importToolStripMenuItem = this.FindControl<MenuItem>(nameof(importToolStripMenuItem));
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


			liberateToolStripMenuItem = this.FindControl<MenuItem>(nameof(liberateToolStripMenuItem));
			{
				beginBookBackupsToolStripMenuItem = this.FindControl<FormattableMenuItem>(nameof(beginBookBackupsToolStripMenuItem));
				beginPdfBackupsToolStripMenuItem = this.FindControl<FormattableMenuItem>(nameof(beginPdfBackupsToolStripMenuItem));
				convertAllM4bToMp3ToolStripMenuItem = this.FindControl<MenuItem>(nameof(convertAllM4bToMp3ToolStripMenuItem));
				liberateVisibleToolStripMenuItem_LiberateMenu = this.FindControl<FormattableMenuItem>(nameof(liberateVisibleToolStripMenuItem_LiberateMenu));
			}

			exportToolStripMenuItem = this.FindControl<MenuItem>(nameof(exportToolStripMenuItem));
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
				replaceTagsToolStripMenuItem = this.FindControl<MenuItem>(nameof(replaceTagsToolStripMenuItem));
				setDownloadedToolStripMenuItem = this.FindControl<MenuItem>(nameof(setDownloadedToolStripMenuItem));
				removeToolStripMenuItem = this.FindControl<MenuItem>(nameof(removeToolStripMenuItem));
			}

			settingsToolStripMenuItem = this.FindControl<MenuItem>(nameof(settingsToolStripMenuItem));
			{
				accountsToolStripMenuItem = this.FindControl<MenuItem>(nameof(accountsToolStripMenuItem));
				basicSettingsToolStripMenuItem = this.FindControl<MenuItem>(nameof(basicSettingsToolStripMenuItem));
				aboutToolStripMenuItem = this.FindControl<MenuItem>(nameof(aboutToolStripMenuItem));
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
