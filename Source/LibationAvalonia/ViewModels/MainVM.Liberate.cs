using ApplicationServices;
using LibationFileManager;
using System;
using System.Linq;
using System.Threading.Tasks;
using DataLayer;
using LibationUiBase.Forms;
using LibationUiBase;
using System.Collections.Generic;

#nullable enable
namespace LibationAvalonia.ViewModels
{
	partial class MainVM
	{
		public void Configure_Liberate() { }

		public async Task BackupAllBooks()
		{
			try
			{
				var unliberated = await Task.Run(() => DbContexts.GetLibrary_Flat_NoTracking().UnLiberated().ToArray());

				if (ProcessQueue.QueueDownloadDecrypt(unliberated))
					setQueueCollapseState(false);
			}
			catch (Exception ex)
			{
				Serilog.Log.Logger.Error(ex, "An error occurred while backing up all library books");
			}
		}

		public async Task BackupAllPdfs()
		{
			if (ProcessQueue.QueueDownloadPdf(await Task.Run(() => DbContexts.GetLibrary_Flat_NoTracking())))
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
