using DataLayer;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace LibationWinForms.AvaloniaUI.Views
{
	//DONE
	public partial class MainWindow
	{
		private void Configure_Liberate() { }

		//GetLibrary_Flat_NoTracking() may take a long time on a hugh library. so run in new thread 
		public void beginBookBackupsToolStripMenuItem_Click(object _ = null, Avalonia.Interactivity.RoutedEventArgs __ = null)
		{
			try
			{
				SetQueueCollapseState(false);

				Serilog.Log.Logger.Information("Begin backing up all library books");

				processBookQueue1.AddDownloadDecrypt(
					ApplicationServices.DbContexts
					.GetLibrary_Flat_NoTracking()
					.UnLiberated()
					);
			}
			catch (Exception ex)
			{
				Serilog.Log.Logger.Error(ex, "An error occurred while backing up all library books");
			}
		}

		public async void beginPdfBackupsToolStripMenuItem_Click(object sender, Avalonia.Interactivity.RoutedEventArgs args)
		{
			SetQueueCollapseState(false);
			await Task.Run(() => processBookQueue1.AddDownloadPdf(ApplicationServices.DbContexts.GetLibrary_Flat_NoTracking()
				  .Where(lb => lb.Book.UserDefinedItem.PdfStatus is DataLayer.LiberatedStatus.NotLiberated)));
		}

		public async void convertAllM4bToMp3ToolStripMenuItem_Click(object sender, Avalonia.Interactivity.RoutedEventArgs args)
		{
			var result = System.Windows.Forms.MessageBox.Show(
				"This converts all m4b titles in your library to mp3 files. Original files are not deleted."
				+ "\r\nFor large libraries this will take a long time and will take up more disk space."
				+ "\r\n\r\nContinue?"
				+ "\r\n\r\n(To always download titles as mp3 instead of m4b, go to Settings: Download my books as .MP3 files)",
				"Convert all M4b => Mp3?",
				System.Windows.Forms.MessageBoxButtons.YesNo,
				System.Windows.Forms.MessageBoxIcon.Warning);
			if (result == System.Windows.Forms.DialogResult.Yes)
			{
				SetQueueCollapseState(false);
				await Task.Run(() => processBookQueue1.AddConvertMp3(ApplicationServices.DbContexts.GetLibrary_Flat_NoTracking()
					.Where(lb => lb.Book.UserDefinedItem.BookStatus is DataLayer.LiberatedStatus.Liberated && lb.Book.ContentType is DataLayer.ContentType.Product)));
			}
			//Only Queue Liberated books for conversion.  This isn't a perfect filter, but it's better than nothing.
		}
	}
}
