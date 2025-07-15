using ApplicationServices;
using LibationFileManager;
using System;
using System.Linq;
using System.Threading.Tasks;
using DataLayer;
using LibationUiBase.Forms;

#nullable enable
namespace LibationAvalonia.ViewModels
{
	partial class MainVM
	{
		public void Configure_Liberate() { }

		public void BackupAllBooks()
		{
			try
			{
				setQueueCollapseState(false);

				Serilog.Log.Logger.Information("Begin backing up all library books");

				ProcessQueue.AddDownloadDecrypt(
					DbContexts
					.GetLibrary_Flat_NoTracking()
					.UnLiberated()
					);
			}
			catch (Exception ex)
			{
				Serilog.Log.Logger.Error(ex, "An error occurred while backing up all library books");
			}
		}

		public void BackupAllPdfs()
		{
			setQueueCollapseState(false);
			ProcessQueue.AddDownloadPdf(DbContexts.GetLibrary_Flat_NoTracking().Where(lb => lb.Book.UserDefinedItem.PdfStatus is LiberatedStatus.NotLiberated));
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
			if (result == DialogResult.Yes)
			{
				setQueueCollapseState(false);
				ProcessQueue.AddConvertMp3(DbContexts.GetLibrary_Flat_NoTracking().Where(lb => lb.Book.UserDefinedItem.BookStatus is LiberatedStatus.Liberated && lb.Book.ContentType is ContentType.Product));
			}
			//Only Queue Liberated books for conversion.  This isn't a perfect filter, but it's better than nothing.
		}

		private void setQueueCollapseState(bool collapsed)
		{
			QueueOpen = !collapsed;
			Configuration.Instance.SetNonString(!collapsed, nameof(QueueOpen));
		}
	}
}
