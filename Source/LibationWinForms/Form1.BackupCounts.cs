using ApplicationServices;
using Dinah.Core;
using Dinah.Core.Threading;

namespace LibationWinForms
{
	public partial class Form1
	{
		protected void Configure_BackupCounts()
		{
			// init formattable
			beginBookBackupsToolStripMenuItem.Format(0);
			beginPdfBackupsToolStripMenuItem.Format(0);
			pdfsCountsLbl.Text = "|  [Calculating backed up PDFs]";

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

		// this cannot be cleanly be FormattableToolStripMenuItem because of the optional "Errors" text
		private const string backupsCountsLbl_Format = "BACKUPS: No progress: {0}  In process: {1}  Fully backed up: {2}";

		private void setBookBackupCounts(LibraryCommands.LibraryStats libraryStats)
		{
			var pending = libraryStats.booksNoProgress + libraryStats.booksDownloadedOnly;
			var hasResults = 0 < (libraryStats.booksFullyBackedUp + libraryStats.booksDownloadedOnly + libraryStats.booksNoProgress + libraryStats.booksError);

			// enable/disable export
			{
				exportLibraryToolStripMenuItem.Enabled = hasResults;
			}

			// update bottom numbers
			{
				var formatString
					= !hasResults ? "No books. Begin by importing your library"
					: libraryStats.booksError > 0 ? backupsCountsLbl_Format + "  Errors: {3}"
					: pending > 0 ? backupsCountsLbl_Format
					: $"All {"book".PluralizeWithCount(libraryStats.booksFullyBackedUp)} backed up";
				var statusStripText = string.Format(formatString,
					libraryStats.booksNoProgress,
					libraryStats.booksDownloadedOnly,
					libraryStats.booksFullyBackedUp,
					libraryStats.booksError);
				statusStrip1.UIThreadAsync(() => backupsCountsLbl.Text = statusStripText);
			}

			// update 'begin book backups' menu item
			{
				var menuItemText
					= pending > 0
					? $"{pending} remaining"
					: "All books have been liberated";
				menuStrip1.UIThreadAsync(() =>
				{
					beginBookBackupsToolStripMenuItem.Format(menuItemText);
					beginBookBackupsToolStripMenuItem.Enabled = pending > 0;
				});
			}
		}
		private void setPdfBackupCounts(LibraryCommands.LibraryStats libraryStats)
		{
			// update bottom numbers
			{
				var hasResults = 0 < (libraryStats.pdfsNotDownloaded + libraryStats.pdfsDownloaded);
				// don't need to assign the output of Format(). It just makes this logic cleaner
				var statusStripText
					= !hasResults ? ""
					: libraryStats.pdfsNotDownloaded > 0 ? pdfsCountsLbl.Format(libraryStats.pdfsNotDownloaded, libraryStats.pdfsDownloaded)
					: $"|  All {libraryStats.pdfsDownloaded} PDFs downloaded";
				statusStrip1.UIThreadAsync(() => pdfsCountsLbl.Text = statusStripText);
			}

			// update 'begin pdf only backups' menu item
			{
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
}
