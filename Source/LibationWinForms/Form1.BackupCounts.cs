using ApplicationServices;
using Dinah.Core;
using Dinah.Core.Threading;

namespace LibationWinForms
{
	public partial class Form1
	{
		private System.ComponentModel.BackgroundWorker updateCountsBw = new();

		protected void Configure_BackupCounts()
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
			updateCountsBw.RunWorkerCompleted += updateBottomPdfNumbers;
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
			Invoke(() => exportLibraryToolStripMenuItem.Enabled = libraryStats.HasBookResults);
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
			statusStrip1.UIThreadAsync(() => backupsCountsLbl.Text = statusStripText);
		}

		// update 'begin book backups' menu item
		private void update_BeginBookBackups_menuItem(object _, System.ComponentModel.RunWorkerCompletedEventArgs e)
		{
			var libraryStats = e.Result as LibraryCommands.LibraryStats;

			var menuItemText
				= libraryStats.HasPendingBooks
				? $"{libraryStats.PendingBooks} remaining"
				: "All books have been liberated";
			menuStrip1.UIThreadAsync(() =>
			{
				beginBookBackupsToolStripMenuItem.Format(menuItemText);
				beginBookBackupsToolStripMenuItem.Enabled = libraryStats.HasPendingBooks;
			});
		}

		private void updateBottomPdfNumbers(object _, System.ComponentModel.RunWorkerCompletedEventArgs e)
		{
			var libraryStats = e.Result as LibraryCommands.LibraryStats;

			// don't need to assign the output of Format(). It just makes this logic cleaner
			var statusStripText
				= !libraryStats.HasPdfResults ? ""
				: libraryStats.pdfsNotDownloaded > 0 ? pdfsCountsLbl.Format(libraryStats.pdfsNotDownloaded, libraryStats.pdfsDownloaded)
				: $"|  All {libraryStats.pdfsDownloaded} PDFs downloaded";
			statusStrip1.UIThreadAsync(() => pdfsCountsLbl.Text = statusStripText);
		}

		// update 'begin pdf only backups' menu item
		private void udpate_BeginPdfOnlyBackups_menuItem(object _, System.ComponentModel.RunWorkerCompletedEventArgs e)
		{
			var libraryStats = e.Result as LibraryCommands.LibraryStats;

			var menuItemText
				= libraryStats.pdfsNotDownloaded > 0
				? $"{libraryStats.pdfsNotDownloaded} remaining"
				: "All PDFs have been downloaded";
			menuStrip1.UIThreadAsync(() =>
			{
				beginPdfBackupsToolStripMenuItem.Format(menuItemText);
				beginPdfBackupsToolStripMenuItem.Enabled = libraryStats.pdfsNotDownloaded > 0;
			});
		}
    }
}
