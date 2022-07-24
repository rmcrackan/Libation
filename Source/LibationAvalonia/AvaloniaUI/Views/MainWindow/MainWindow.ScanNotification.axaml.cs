using ApplicationServices;
using System;
using System.Linq;

namespace LibationAvalonia.AvaloniaUI.Views
{
	//DONE
	public partial class MainWindow
	{
		private void Configure_ScanNotification()
		{
			_viewModel.NumAccountsScanning = 0;
			LibraryCommands.ScanBegin += LibraryCommands_ScanBegin;
			LibraryCommands.ScanEnd += LibraryCommands_ScanEnd;
		}
		private void LibraryCommands_ScanBegin(object sender, int accountsLength)
		{
			_viewModel.NumAccountsScanning = accountsLength;
		}

		private void LibraryCommands_ScanEnd(object sender, EventArgs e)
		{
			_viewModel.NumAccountsScanning = 0;
		}
	}
}
