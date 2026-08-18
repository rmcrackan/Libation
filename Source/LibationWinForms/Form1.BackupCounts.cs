using ApplicationServices;
using DataLayer;
using Dinah.Core.Threading;
using System.Collections.Generic;

namespace LibationWinForms;

public partial class Form1
{
	private readonly System.ComponentModel.BackgroundWorker updateCountsBw = new();

	protected void Configure_BackupCounts()
	{
		// init formattable
		beginBookBackupsToolStripMenuItem.Format(0);
		beginPdfBackupsToolStripMenuItem.Format(0);

		LibraryCommands.LibrarySizeChanged += setBackupCounts;
		LibraryCommands.LibrarySizeChanged += (_, _) => refreshBooksInTrash();
		//Pass null to the runner to get the whole library.
		LibraryCommands.BookUserDefinedItemCommitted += (_, _)
			=> setBackupCounts(null, null);

		trashBinLbl.Text = "";
		trashBinLbl.Visible = false;
		trashBinLbl.ToolTipText = LibationUiBase.TrashBinUi.StatusToolTip;
		refreshBooksInTrash();

		updateCountsBw.DoWork += UpdateCountsBw_DoWork;
		// Register the error logger first so a failed count is logged exactly once, before the
		// display handlers below run and (safely) treat the stats as unavailable.
		updateCountsBw.RunWorkerCompleted += logBackupCountsError;
		updateCountsBw.RunWorkerCompleted += exportMenuEnable;
		updateCountsBw.RunWorkerCompleted += updateBottomStats;
		updateCountsBw.RunWorkerCompleted += update_BeginBookBackups_menuItem;
		updateCountsBw.RunWorkerCompleted += udpate_BeginPdfOnlyBackups_menuItem;
	}

	/// <summary>
	/// Re-read the trash count and show it only when there is something in there. Kept off
	/// <see cref="LibraryCommands.GetCounts"/> on purpose: that also runs against the visible subset on
	/// every filter change, where a database round trip would be wasted work.
	/// </summary>
	private async void refreshBooksInTrash()
	{
		int booksInTrash;
		try
		{
			booksInTrash = await System.Threading.Tasks.Task.Run(DbContexts.GetTrashedBookCount);
		}
		catch (System.Exception ex)
		{
			//A stale count must not take down the window that displays it.
			Serilog.Log.Logger.Error(ex, "Failed to count books in the trash");
			return;
		}

		statusStrip1.UIThreadAsync(() =>
		{
			trashBinLbl.Text = LibationUiBase.TrashBinUi.StatusText(booksInTrash);
			trashBinLbl.Visible = LibationUiBase.TrashBinUi.ShowStatus(booksInTrash);
			openTrashBinToolStripMenuItem.Text = LibationUiBase.TrashBinUi.MenuText(booksInTrash);
		});
	}

	/// <summary>
	/// Safely extract the completed <see cref="LibraryCommands.LibraryStats"/>. Returns null when the
	/// background count was cancelled or faulted. Reading <see cref="System.ComponentModel.RunWorkerCompletedEventArgs.Result"/>
	/// directly rethrows any <see cref="System.ComponentModel.RunWorkerCompletedEventArgs.Error"/>, which would otherwise
	/// crash the app; callers must degrade gracefully on null.
	/// </summary>
	private static LibraryCommands.LibraryStats? getLibraryStats(System.ComponentModel.RunWorkerCompletedEventArgs e)
		=> e.Cancelled || e.Error is not null ? null : e.Result as LibraryCommands.LibraryStats;

	private static void logBackupCountsError(object? _, System.ComponentModel.RunWorkerCompletedEventArgs e)
	{
		if (e.Error is not null)
			Serilog.Log.Logger.Error(e.Error, "Failed to update backup counts");
	}

	private bool runBackupCountsAgain;

	private void setBackupCounts(object? _, List<LibraryBook>? libraryBooks)
	{
		runBackupCountsAgain = true;

		if (!updateCountsBw.IsBusy)
			updateCountsBw.RunWorkerAsync(libraryBooks);
	}

	private void UpdateCountsBw_DoWork(object? sender, System.ComponentModel.DoWorkEventArgs e)
	{
		while (runBackupCountsAgain)
		{
			runBackupCountsAgain = false;
			e.Result = LibraryCommands.GetCounts(e.Argument as IEnumerable<LibraryBook>);
		}
	}

	private void exportMenuEnable(object? _, System.ComponentModel.RunWorkerCompletedEventArgs e)
	{
		var libraryStats = getLibraryStats(e);
		Invoke(() => exportLibraryToolStripMenuItem.Enabled = libraryStats?.HasBookResults is true);
	}

	private void updateBottomStats(object? _, System.ComponentModel.RunWorkerCompletedEventArgs e)
	{
		var libraryStats = getLibraryStats(e);
		statusStrip1.UIThreadAsync(() => backupsCountsLbl.Text = libraryStats?.StatusString ?? "ERROR GETTING STATUS");
	}

	// update 'begin book and pdf backups' menu item
	private void update_BeginBookBackups_menuItem(object? _, System.ComponentModel.RunWorkerCompletedEventArgs e)
	{
		var libraryStats = getLibraryStats(e);

		var menuItemText
			= libraryStats?.HasPendingBooks is true
			? $"{libraryStats.PendingBooks} remaining"
			: "All books have been liberated";
		menuStrip1.UIThreadAsync(() =>
		{
			beginBookBackupsToolStripMenuItem.Format(menuItemText);
			beginBookBackupsToolStripMenuItem.Enabled = libraryStats?.HasPendingBooks is true;
		});
	}

	// update 'begin pdf only backups' menu item
	private void udpate_BeginPdfOnlyBackups_menuItem(object? _, System.ComponentModel.RunWorkerCompletedEventArgs e)
	{
		var libraryStats = getLibraryStats(e);

		var menuItemText
			= libraryStats?.pdfsNotDownloaded > 0
			? $"{libraryStats.pdfsNotDownloaded} remaining"
			: "All PDFs have been downloaded";
		menuStrip1.UIThreadAsync(() =>
		{
			beginPdfBackupsToolStripMenuItem.Format(menuItemText);
			beginPdfBackupsToolStripMenuItem.Enabled = libraryStats?.pdfsNotDownloaded > 0;
		});
	}
}
