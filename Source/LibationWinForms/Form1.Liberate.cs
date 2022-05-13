using System;
using System.Windows.Forms;

namespace LibationWinForms
{
    public partial class Form1
	{
		private void Configure_Liberate() { }

		private async void beginBookBackupsToolStripMenuItem_Click(object sender, EventArgs e)
			=> await BookLiberation.ProcessorAutomationController.BackupAllBooksAsync();

		private async void beginPdfBackupsToolStripMenuItem_Click(object sender, EventArgs e)
			=> await BookLiberation.ProcessorAutomationController.BackupAllPdfsAsync();

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
				await BookLiberation.ProcessorAutomationController.ConvertAllBooksAsync();
		}
	}
}
