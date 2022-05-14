using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LibationWinForms
{
    public partial class Form1
	{
		private void Configure_Liberate() { }

		//GetLibrary_Flat_NoTracking() may take a long time on a hugh library. so run in new thread 
		private void beginBookBackupsToolStripMenuItem_Click(object sender, EventArgs e)
			=> Task.Run(() => processBookQueue1.AddDownloadDecrypt(ApplicationServices.DbContexts.GetLibrary_Flat_NoTracking()
				.Where(lb => lb.Book.UserDefinedItem.PdfStatus is DataLayer.LiberatedStatus.NotLiberated || lb.Book.UserDefinedItem.BookStatus is DataLayer.LiberatedStatus.NotLiberated)));

		private void beginPdfBackupsToolStripMenuItem_Click(object sender, EventArgs e)
			=> Task.Run(() => processBookQueue1.AddDownloadPdf(ApplicationServices.DbContexts.GetLibrary_Flat_NoTracking()
				.Where(lb => lb.Book.UserDefinedItem.PdfStatus is DataLayer.LiberatedStatus.NotLiberated)));

		private async void convertAllM4bToMp3ToolStripMenuItem_Click(object sender, EventArgs e)
		{
			var result = MessageBox.Show(
				"This converts all m4b titles in your library to mp3 files. Original files are not deleted."
				+ "\r\nFor large libraries this will take a long time and will take up more disk space."
				+ "\r\n\r\nContinue?"
				+ "\r\n\r\n(To always download titles as mp3 instead of m4b, go to Settings: Download my books as .MP3 files)",
				"Convert all M4b => Mp3?",
				MessageBoxButtons.YesNo,
				MessageBoxIcon.Warning);
			if (result == DialogResult.Yes)
				await Task.Run(() => processBookQueue1.AddConvertMp3(ApplicationServices.DbContexts.GetLibrary_Flat_NoTracking()));
		}
	}
}
