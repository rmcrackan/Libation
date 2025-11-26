using ApplicationServices;
using DataLayer;
using LibationFileManager;
using ReactiveUI;
using System.Collections.Generic;
using System.Threading.Tasks;

#nullable enable
namespace LibationAvalonia.ViewModels
{
	partial class MainVM
	{
		private System.ComponentModel.BackgroundWorker updateCountsBw = new();

		/// <summary> The "Begin Book and PDF Backup" menu item header text </summary>
		public string BookBackupsToolStripText { get; private set; } = "Begin Book and PDF Backups: 0";
		/// <summary> The "Begin PDF Only Backup" menu item header text </summary>
		public string PdfBackupsToolStripText { get; private set; } = "Begin PDF Only Backups: 0";

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
			updateCountsBw.RunWorkerCompleted += UpdateCountsBw_Completed; ;
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

		private void UpdateCountsBw_Completed(object? sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
		{
			if (e.Result is  not LibraryCommands.LibraryStats stats)
				return;
			LibraryStats = stats;

			if (Configuration.Instance.AutoDownloadEpisodes
				&& stats.PendingBooks + stats.pdfsNotDownloaded > 0)
				BackupAllBooks(stats.LibraryBooks);
		}
	}
}
