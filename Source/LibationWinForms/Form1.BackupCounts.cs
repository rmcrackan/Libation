using ApplicationServices;
using Dinah.Core;
using Dinah.Core.Threading;

namespace LibationWinForms
{
	public partial class Form1
	{
		private string beginBookBackupsToolStripMenuItem_format;
		private string beginPdfBackupsToolStripMenuItem_format;

		protected void Configure_BackupCounts()
        {
            // back up string formats
            beginBookBackupsToolStripMenuItem_format = beginBookBackupsToolStripMenuItem.Text;
            beginPdfBackupsToolStripMenuItem_format = beginPdfBackupsToolStripMenuItem.Text;

            Load += setBackupCounts;
            LibraryCommands.LibrarySizeChanged += setBackupCounts;
            LibraryCommands.BookUserDefinedItemCommitted += setBackupCounts;
        }

		private System.ComponentModel.BackgroundWorker updateCountsBw;
		private bool runBackupCountsAgain;

		private void setBackupCounts(object _, object __)
		{
			runBackupCountsAgain = true;

			if (updateCountsBw is not null)
				return;

			updateCountsBw = new System.ComponentModel.BackgroundWorker();
			updateCountsBw.DoWork += UpdateCountsBw_DoWork;
			updateCountsBw.RunWorkerCompleted += UpdateCountsBw_RunWorkerCompleted;
			updateCountsBw.RunWorkerAsync();
		}

		private void UpdateCountsBw_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
		{
			while (runBackupCountsAgain)
			{
				runBackupCountsAgain = false;

				var libraryStats = LibraryCommands.GetCounts();
				e.Result = libraryStats;
			}
			updateCountsBw = null;
		}

		private void UpdateCountsBw_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
		{
			var libraryStats = e.Result as LibraryCommands.LibraryStats;

			setBookBackupCounts(libraryStats);
			setPdfBackupCounts(libraryStats);
		}

		private void setBookBackupCounts(LibraryCommands.LibraryStats libraryStats)
		{
			var backupsCountsLbl_Format = "BACKUPS: No progress: {0}  In process: {1}  Fully backed up: {2}";

			// enable/disable export
			var hasResults = 0 < (libraryStats.booksFullyBackedUp + libraryStats.booksDownloadedOnly + libraryStats.booksNoProgress + libraryStats.booksError);
			exportLibraryToolStripMenuItem.Enabled = hasResults;

			// update bottom numbers
			var pending = libraryStats.booksNoProgress + libraryStats.booksDownloadedOnly;
			var statusStripText
				= !hasResults ? "No books. Begin by importing your library"
				: libraryStats.booksError > 0 ? string.Format(backupsCountsLbl_Format + "  Errors: {3}", libraryStats.booksNoProgress, libraryStats.booksDownloadedOnly, libraryStats.booksFullyBackedUp, libraryStats.booksError)
				: pending > 0 ? string.Format(backupsCountsLbl_Format, libraryStats.booksNoProgress, libraryStats.booksDownloadedOnly, libraryStats.booksFullyBackedUp)
				: $"All {"book".PluralizeWithCount(libraryStats.booksFullyBackedUp)} backed up";

			// update menu item
			var menuItemText
				= pending > 0
				? $"{pending} remaining"
				: "All books have been liberated";

			// update UI
			statusStrip1.UIThreadAsync(() => backupsCountsLbl.Text = statusStripText);
			menuStrip1.UIThreadAsync(() => beginBookBackupsToolStripMenuItem.Enabled = pending > 0);
			menuStrip1.UIThreadAsync(() => beginBookBackupsToolStripMenuItem.Text = string.Format(beginBookBackupsToolStripMenuItem_format, menuItemText));
		}
		private void setPdfBackupCounts(LibraryCommands.LibraryStats libraryStats)
		{
			var pdfsCountsLbl_Format = "|  PDFs: NOT d/l\'ed: {0}  Downloaded: {1}";

			// update bottom numbers
			var hasResults = 0 < (libraryStats.pdfsNotDownloaded + libraryStats.pdfsDownloaded);
			var statusStripText
				= !hasResults ? ""
				: libraryStats.pdfsNotDownloaded > 0 ? string.Format(pdfsCountsLbl_Format, libraryStats.pdfsNotDownloaded, libraryStats.pdfsDownloaded)
				: $"|  All {libraryStats.pdfsDownloaded} PDFs downloaded";

			// update menu item
			var menuItemText
				= libraryStats.pdfsNotDownloaded > 0
				? $"{libraryStats.pdfsNotDownloaded} remaining"
				: "All PDFs have been downloaded";

			// update UI
			statusStrip1.UIThreadAsync(() => pdfsCountsLbl.Text = statusStripText);
			menuStrip1.UIThreadAsync(() => beginPdfBackupsToolStripMenuItem.Enabled = libraryStats.pdfsNotDownloaded > 0);
			menuStrip1.UIThreadAsync(() => beginPdfBackupsToolStripMenuItem.Text = string.Format(beginPdfBackupsToolStripMenuItem_format, menuItemText));
		}
    }
}
