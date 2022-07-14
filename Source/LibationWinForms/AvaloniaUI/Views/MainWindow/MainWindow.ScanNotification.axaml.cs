using ApplicationServices;
using System;
using System.Linq;

namespace LibationWinForms.AvaloniaUI.Views
{
	//DONE
	public partial class MainWindow
	{
		private void Configure_ScanNotification()
		{
			scanningToolStripMenuItem.IsVisible = false;
			LibraryCommands.ScanBegin += LibraryCommands_ScanBegin;
			LibraryCommands.ScanEnd += LibraryCommands_ScanEnd;
		}
		private void LibraryCommands_ScanBegin(object sender, int accountsLength)
		{
			removeLibraryBooksToolStripMenuItem.IsEnabled = false;
			removeAllAccountsToolStripMenuItem.IsEnabled = false;
			removeSomeAccountsToolStripMenuItem.IsEnabled = false;
			scanLibraryToolStripMenuItem.IsEnabled = false;
			scanLibraryOfAllAccountsToolStripMenuItem.IsEnabled = false;
			scanLibraryOfSomeAccountsToolStripMenuItem.IsEnabled = false;

			this.scanningToolStripMenuItem.IsVisible = true;
			this.scanningToolStripMenuItem_Text.Text
				= (accountsLength == 1)
				? "Scanning..."
				: $"Scanning {accountsLength} accounts...";
		}

		private void LibraryCommands_ScanEnd(object sender, EventArgs e)
		{
			removeLibraryBooksToolStripMenuItem.IsEnabled = true;
			removeAllAccountsToolStripMenuItem.IsEnabled = true;
			removeSomeAccountsToolStripMenuItem.IsEnabled = true;
			scanLibraryToolStripMenuItem.IsEnabled = true;
			scanLibraryOfAllAccountsToolStripMenuItem.IsEnabled = true;
			scanLibraryOfSomeAccountsToolStripMenuItem.IsEnabled = true;

			this.scanningToolStripMenuItem.IsVisible = false;
		}
	}
}
