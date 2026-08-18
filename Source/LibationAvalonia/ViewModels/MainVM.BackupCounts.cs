using ApplicationServices;
using Avalonia.Threading;
using DataLayer;
using LibationFileManager;
using LibationUiBase;
using ReactiveUI;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LibationAvalonia.ViewModels;

partial class MainVM
{
	private readonly System.ComponentModel.BackgroundWorker updateCountsBw = new();

	/// <summary> The "Begin Book and PDF Backup" menu item header text </summary>
	public string BookBackupsToolStripText { get; private set; } = "Begin Book and PDF Backups: 0";
	/// <summary> The "Begin PDF Only Backup" menu item header text </summary>
	public string PdfBackupsToolStripText { get; private set; } = "Begin PDF Only Backups: 0";

	/// <summary> How many books are in the trash, where nothing else on screen would reveal them </summary>
	public int BooksInTrash
	{
		get => field;
		private set
		{
			this.RaiseAndSetIfChanged(ref field, value);
			this.RaisePropertyChanged(nameof(TrashBinMenuText));
			this.RaisePropertyChanged(nameof(TrashBinStatusText));
			this.RaisePropertyChanged(nameof(TrashBinStatusVisible));
		}
	}

	public string TrashBinMenuText => TrashBinUi.MenuText(BooksInTrash);
	public string TrashBinStatusText => TrashBinUi.StatusText(BooksInTrash);
	public bool TrashBinStatusVisible => TrashBinUi.ShowStatus(BooksInTrash);
	public string TrashBinStatusToolTip => TrashBinUi.StatusToolTip;

	/// <summary>
	/// Re-read the trash count. Kept off <see cref="LibraryCommands.GetCounts"/> on purpose: that also runs
	/// against the visible subset on every filter change, where a database round trip would be wasted work.
	/// </summary>
	public async Task RefreshBooksInTrashAsync()
	{
		try
		{
			var count = await Task.Run(DbContexts.GetTrashedBookCount);
			await Dispatcher.UIThread.InvokeAsync(() => BooksInTrash = count);
		}
		catch (System.Exception ex)
		{
			//A stale count must not take down the window that displays it.
			Serilog.Log.Logger.Error(ex, "Failed to count books in the trash");
		}
	}

	/// <summary> The user's library statistics </summary>
	public LibraryCommands.LibraryStats? LibraryStats
	{
		get => field;
		set
		{
			this.RaiseAndSetIfChanged(ref field, value);

			BookBackupsToolStripText
				= LibraryStats?.HasPendingBooks ?? false
				? "Begin " + menufyText($"Book and PDF Backups: {LibraryStats.PendingBooks} remaining")
				: "All books have been liberated";

			PdfBackupsToolStripText
				= LibraryStats?.pdfsNotDownloaded > 0
				? "Begin " + menufyText($"PDF Only Backups: {LibraryStats.pdfsNotDownloaded} remaining")
				: "All PDFs have been downloaded";

			this.RaisePropertyChanged(nameof(BookBackupsToolStripText));
			this.RaisePropertyChanged(nameof(PdfBackupsToolStripText));
		}
	}

	private void Configure_BackupCounts()
	{
		//Pass null to the setup count to get the whole library.
		LibraryCommands.BookUserDefinedItemCommitted += async (_, _)
			=> await SetBackupCountsAsync(null);

		updateCountsBw.DoWork += UpdateCountsBw_DoWork;
		updateCountsBw.RunWorkerCompleted += UpdateCountsBw_CompletedAsync;
	}


	private bool runBackupCountsAgain;

	public async Task SetBackupCountsAsync(IEnumerable<LibraryBook>? libraryBooks)
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

	private async void UpdateCountsBw_CompletedAsync(object? sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
	{
		// Reading e.Result rethrows any exception from UpdateCountsBw_DoWork. Because this is an
		// async void handler with no SynchronizationContext, that would become an unhandled
		// exception and crash the app. Check e.Error/e.Cancelled first and degrade gracefully.
		if (e.Cancelled)
			return;
		if (e.Error is not null)
		{
			Serilog.Log.Logger.Error(e.Error, "Failed to update backup counts");
			return;
		}
		if (e.Result is not LibraryCommands.LibraryStats stats)
			return;
		LibraryStats = stats;

		if (Configuration.Instance.AutoDownloadEpisodes
			&& stats.PendingBooks + stats.pdfsNotDownloaded > 0)
		{
			// RunWorkerCompleted has no SynchronizationContext; queue items require the UI thread.
			await Dispatcher.UIThread.InvokeAsync(async () => await BackupAllBooksAsync(stats.LibraryBooks, notifyIfNothingQueued: false));
		}
	}
}
