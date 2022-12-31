using ApplicationServices;
using System;
using System.Linq;
using Avalonia.Threading;
using System.Threading.Tasks;

namespace LibationAvalonia.Views
{
	public partial class MainWindow
	{
		private Task updateCountsTask;
		private void Configure_BackupCounts()
		{
			Load += setBackupCounts;
			LibraryCommands.LibrarySizeChanged += setBackupCounts;
			LibraryCommands.BookUserDefinedItemCommitted += setBackupCounts;
		}

		private void setBackupCounts(object _, object __)
		{
			if (updateCountsTask?.IsCompleted is not false)
				updateCountsTask = Dispatcher.UIThread.InvokeAsync(() => _viewModel.LibraryStats = LibraryCommands.GetCounts());		
		}
	}
}
