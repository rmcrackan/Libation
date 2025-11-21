using ApplicationServices;
using Avalonia.Threading;
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
		private Task<LibraryCommands.LibraryStats>? updateCountsTask;

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
		}

		public async Task SetBackupCountsAsync(IEnumerable<LibraryBook>? libraryBooks)
		{
			if (updateCountsTask?.IsCompleted ?? true)
			{
				updateCountsTask = Task.Run(() => LibraryCommands.GetCounts(libraryBooks));
				var stats = await updateCountsTask;
				await Dispatcher.UIThread.InvokeAsync(() => LibraryStats = stats);

				if (Configuration.Instance.AutoDownloadEpisodes
					&& stats.PendingBooks + stats.pdfsNotDownloaded > 0)
					await Dispatcher.UIThread.InvokeAsync(BackupAllBooks);
			}
		}
	}
}
