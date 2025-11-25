using ApplicationServices;
using LibationFileManager;
using System;
using System.Linq;
using System.Threading.Tasks;
using DataLayer;
using LibationUiBase.Forms;
using LibationUiBase;
using System.Collections.Generic;
using Avalonia.Threading;

#nullable enable
namespace LibationAvalonia.ViewModels
{
	partial class MainVM
	{
		public void Configure_Liberate() { }

		/// <summary> This gets called by the "Begin Book and PDF Backups" menu item. </summary>
		public async Task BackupAllBooks()
		{
			var books = await Task.Run(() => DbContexts.GetLibrary_Flat_NoTracking());
			BackupAllBooks(books);
		}

		private void BackupAllBooks(IEnumerable<LibraryBook> books)
		{
			try
			{
				var unliberated = books.UnLiberated().ToArray();

				Dispatcher.UIThread.Invoke(() =>
				{
					if (ProcessQueue.QueueDownloadDecrypt(unliberated))
						setQueueCollapseState(false);
				});
			}
			catch (Exception ex)
			{
				Serilog.Log.Logger.Error(ex, "An error occurred while backing up all library books");
			}
		}

		/// <summary> This gets called by the "Begin PDF Only Backups" menu item. </summary>
		public async Task BackupAllPdfs()
		{
			var books = await Task.Run(() => DbContexts.GetLibrary_Flat_NoTracking());
			if (ProcessQueue.QueueDownloadPdf(books))
				setQueueCollapseState(false);
		}

		public async Task ConvertAllToMp3Async()
		{
			var result = await MessageBox.Show(MainWindow,
				"This converts all m4b titles in your library to mp3 files. Original files are not deleted."
				+ "\r\nFor large libraries this will take a long time and will take up more disk space."
				+ "\r\n\r\nContinue?"
				+ "\r\n\r\n(To always download titles as mp3 instead of m4b, go to Settings: Download my books as .MP3 files)",
				"Convert all M4b => Mp3?",
				MessageBoxButtons.YesNo,
				MessageBoxIcon.Warning);
			if (result == DialogResult.Yes && ProcessQueue.QueueConvertToMp3(await Task.Run(() => DbContexts.GetLibrary_Flat_NoTracking())))
				setQueueCollapseState(false);
		}

		private void setQueueCollapseState(bool collapsed)
		{
			QueueOpen = !collapsed;
			Configuration.Instance.SetNonString(!collapsed, nameof(QueueOpen));
		}
	}
}
