using ApplicationServices;

namespace LibationWinForms;

// This is for the Scanning notification in the upper right. This shown for manual scanning and auto-scan
public partial class Form1
{
	private void Configure_ScanNotification()
	{
		LibraryCommands.ScanBegin += LibraryCommands_ScanBegin;
		LibraryCommands.ScanEnd += LibraryCommands_ScanEnd;
	}

	private void LibraryCommands_ScanBegin(object? sender, int accountsLength)
	{
		// ImportAccountAsync (incl. auto-scan) runs on a worker thread; ToolStrip must be touched on the UI thread.
		if (InvokeRequired)
		{
			BeginInvoke(LibraryCommands_ScanBegin, sender, accountsLength);
			return;
		}

		removeLibraryBooksToolStripMenuItem.Enabled = false;
		removeAllAccountsToolStripMenuItem.Enabled = false;
		removeSomeAccountsToolStripMenuItem.Enabled = false;
		scanLibraryToolStripMenuItem.Enabled = false;
		scanLibraryOfAllAccountsToolStripMenuItem.Enabled = false;
		scanLibraryOfSomeAccountsToolStripMenuItem.Enabled = false;

		this.scanningToolStripMenuItem.Image = System.Windows.Forms.Application.IsDarkModeEnabled ? Properties.Resources.import_16x16_dark : Properties.Resources.import_16x16;
		this.scanningToolStripMenuItem.Visible = true;
		this.scanningToolStripMenuItem.Text
			= (accountsLength == 1)
			? "Scanning..."
			: $"Scanning {accountsLength} accounts...";
	}

	private void LibraryCommands_ScanEnd(object? sender, int newCount)
	{
		if (InvokeRequired)
		{
			BeginInvoke(LibraryCommands_ScanEnd, sender, newCount);
			return;
		}

		removeLibraryBooksToolStripMenuItem.Enabled = true;
		removeAllAccountsToolStripMenuItem.Enabled = true;
		removeSomeAccountsToolStripMenuItem.Enabled = true;
		scanLibraryToolStripMenuItem.Enabled = true;
		scanLibraryOfAllAccountsToolStripMenuItem.Enabled = true;
		scanLibraryOfSomeAccountsToolStripMenuItem.Enabled = true;

		this.scanningToolStripMenuItem.Visible = false;
	}
}
