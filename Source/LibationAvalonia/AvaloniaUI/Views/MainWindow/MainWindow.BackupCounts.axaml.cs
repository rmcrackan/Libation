using ApplicationServices;
using System;
using System.Linq;
using Avalonia.Threading;
using Dinah.Core;

namespace LibationAvalonia.AvaloniaUI.Views
{
	//DONE
	public partial class MainWindow
	{
		private System.ComponentModel.BackgroundWorker updateCountsBw = new();
		private void Configure_BackupCounts()
		{
			Load += setBackupCounts;
			LibraryCommands.LibrarySizeChanged += setBackupCounts;
			LibraryCommands.BookUserDefinedItemCommitted += setBackupCounts;

			updateCountsBw.DoWork += UpdateCountsBw_DoWork;
			updateCountsBw.RunWorkerCompleted += updateBottomNumbersAsync;
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

		private void updateBottomNumbersAsync(object _, System.ComponentModel.RunWorkerCompletedEventArgs e)
		{
			_viewModel.LibraryStats = e.Result as LibraryCommands.LibraryStats;
		}
	}
}
