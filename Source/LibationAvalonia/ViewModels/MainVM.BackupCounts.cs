using ApplicationServices;
using Avalonia.Threading;
using DataLayer;
using LibationFileManager;
using ReactiveUI;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LibationAvalonia.ViewModels
{
	partial class MainVM
	{
		private Task<LibraryCommands.LibraryStats> updateCountsTask;
		private LibraryCommands.LibraryStats _libraryStats;

		/// <summary> The "Begin Book and PDF Backup" menu item header text </summary>
		public string BookBackupsToolStripText { get; private set; } = "Begin Book and PDF Backups: 0";
		/// <summary> The "Begin PDF Only Backup" menu item header text </summary>
		public string PdfBackupsToolStripText { get; private set; } = "Begin PDF Only Backups: 0";

		/// <summary> The user's library statistics </summary>
		public LibraryCommands.LibraryStats LibraryStats
		{
			get => _libraryStats;
			set
			{
				this.RaiseAndSetIfChanged(ref _libraryStats, value);

				BookBackupsToolStripText
					= LibraryStats.HasPendingBooks
					? "Begin " + menufyText($"Book and PDF Backups: {LibraryStats.PendingBooks} remaining")
					: "All books have been liberated";

				PdfBackupsToolStripText
					= LibraryStats.pdfsNotDownloaded > 0
					? "Begin " + menufyText($"PDF Only Backups: {LibraryStats.pdfsNotDownloaded} remaining")
					: "All PDFs have been downloaded";

				this.RaisePropertyChanged(nameof(BookBackupsToolStripText));
				this.RaisePropertyChanged(nameof(PdfBackupsToolStripText));
			}
		}

		private void Configure_BackupCounts()
		{
			MainWindow.LibraryLoaded += (_, e) => setBackupCounts(e.Where(l => !l.Book.IsEpisodeParent()));
			LibraryCommands.LibrarySizeChanged += (_,_) => setBackupCounts();
			LibraryCommands.BookUserDefinedItemCommitted += (_, _) => setBackupCounts();
		}

		private async void setBackupCounts(IEnumerable<LibraryBook> libraryBooks = null)
		{
			if (updateCountsTask?.IsCompleted ?? true)
			{
				libraryBooks ??= DbContexts.GetLibrary_Flat_NoTracking();
				updateCountsTask = Task.Run(() => LibraryCommands.GetCounts(libraryBooks));
				var stats = await updateCountsTask;
				await Dispatcher.UIThread.InvokeAsync(() => LibraryStats = stats);
				
				if (Configuration.Instance.AutoDownloadEpisodes
					&& stats.booksNoProgress + stats.pdfsNotDownloaded > 0)
					await Dispatcher.UIThread.InvokeAsync(BackupAllBooks);
			}
		}
	}
}
