using ApplicationServices;
using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Threading;
using Dinah.Core;

namespace LibationWinForms.AvaloniaUI.Views
{
	public partial class MainWindow
	{
		private System.ComponentModel.BackgroundWorker updateCountsBw = new();
		private void Configure_BackupCounts()
		{
			// init formattable
			beginBookBackupsToolStripMenuItem.Format(0);
			beginPdfBackupsToolStripMenuItem.Format(0);
			pdfsCountsLbl.Text = "|  [Calculating backed up PDFs]";

			Load += setBackupCounts;
			LibraryCommands.LibrarySizeChanged += setBackupCounts;
			LibraryCommands.BookUserDefinedItemCommitted += setBackupCounts;

			updateCountsBw.DoWork += UpdateCountsBw_DoWork;
			updateCountsBw.RunWorkerCompleted += exportMenuEnable;
			updateCountsBw.RunWorkerCompleted += updateBottomBookNumbers;
			updateCountsBw.RunWorkerCompleted += update_BeginBookBackups_menuItem;
			updateCountsBw.RunWorkerCompleted += updateBottomPdfNumbersAsync;
			updateCountsBw.RunWorkerCompleted += udpate_BeginPdfOnlyBackups_menuItem;
		}
		private bool runBackupCountsAgain;
		private void setBackupCounts(object _, object __)
		{
			runBackupCountsAgain = true;

			if (!updateCountsBw.IsBusy)
				updateCountsBw.RunWorkerAsync();
		}
		private void UpdateCountsBw_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
		{
			while (runBackupCountsAgain)
			{
				runBackupCountsAgain = false;
				e.Result = LibraryCommands.GetCounts();
			}
		}

		private void exportMenuEnable(object _, System.ComponentModel.RunWorkerCompletedEventArgs e)
		{
			var libraryStats = e.Result as LibraryCommands.LibraryStats;
			Dispatcher.UIThread.Post(() => exportLibraryToolStripMenuItem.IsEnabled = libraryStats.HasBookResults);
		}

		// this cannot be cleanly be FormattableToolStripMenuItem because of the optional "Errors" text
		private const string backupsCountsLbl_Format = "BACKUPS: No progress: {0}  In process: {1}  Fully backed up: {2}";

		private void updateBottomBookNumbers(object _, System.ComponentModel.RunWorkerCompletedEventArgs e)
		{
			var libraryStats = e.Result as LibraryCommands.LibraryStats;

			var formatString
				= !libraryStats.HasBookResults ? "No books. Begin by importing your library"
				: libraryStats.booksError > 0 ? backupsCountsLbl_Format + "  Errors: {3}"
				: libraryStats.HasPendingBooks ? backupsCountsLbl_Format
				: $"All {"book".PluralizeWithCount(libraryStats.booksFullyBackedUp)} backed up";
			var statusStripText = string.Format(formatString,
				libraryStats.booksNoProgress,
				libraryStats.booksDownloadedOnly,
				libraryStats.booksFullyBackedUp,
				libraryStats.booksError);
			Dispatcher.UIThread.InvokeAsync(() => backupsCountsLbl.Text = statusStripText);
		}

		// update 'begin book backups' menu item
		private void update_BeginBookBackups_menuItem(object _, System.ComponentModel.RunWorkerCompletedEventArgs e)
		{
			var libraryStats = e.Result as LibraryCommands.LibraryStats;

			var menuItemText
				= libraryStats.HasPendingBooks
				? $"{libraryStats.PendingBooks} remaining"
				: "All books have been liberated";
			Dispatcher.UIThread.InvokeAsync(() =>
			{
				beginBookBackupsToolStripMenuItem.Format(menuItemText);
				beginBookBackupsToolStripMenuItem.IsEnabled = libraryStats.HasPendingBooks;
			});
		}

		private async void updateBottomPdfNumbersAsync(object _, System.ComponentModel.RunWorkerCompletedEventArgs e)
		{
			var libraryStats = e.Result as LibraryCommands.LibraryStats;

			// don't need to assign the output of Format(). It just makes this logic cleaner
			var statusStripText
				= !libraryStats.HasPdfResults ? ""
				: libraryStats.pdfsNotDownloaded > 0 ? await Dispatcher.UIThread.InvokeAsync(()=> pdfsCountsLbl.Format(libraryStats.pdfsNotDownloaded, libraryStats.pdfsDownloaded))
				: $"  |  All {libraryStats.pdfsDownloaded} PDFs downloaded";
			await Dispatcher.UIThread.InvokeAsync(() => pdfsCountsLbl.Text = statusStripText);
		}

		// update 'begin pdf only backups' menu item
		private void udpate_BeginPdfOnlyBackups_menuItem(object _, System.ComponentModel.RunWorkerCompletedEventArgs e)
		{
			var libraryStats = e.Result as LibraryCommands.LibraryStats;

			var menuItemText
				= libraryStats.pdfsNotDownloaded > 0
				? $"{libraryStats.pdfsNotDownloaded} remaining"
				: "All PDFs have been downloaded";
			Dispatcher.UIThread.InvokeAsync(() =>
			{
				beginPdfBackupsToolStripMenuItem.Format(menuItemText);
				beginPdfBackupsToolStripMenuItem.IsEnabled = libraryStats.pdfsNotDownloaded > 0;
			});
		}
	}
}
