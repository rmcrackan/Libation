using System;
using System.IO;
using System.Windows.Forms;
using Dinah.Core;
using FileManager;
using InternalUtilities;

namespace LibationWinForms.Dialogs
{
	public partial class SettingsDialog : Form
	{
		Configuration config { get; } = Configuration.Instance;
		Func<string, string> desc { get; } = Configuration.GetDescription;

		public SettingsDialog()
		{
			InitializeComponent();
		}

		private void SettingsDialog_Load(object sender, EventArgs e)
		{
			this.booksLocationDescLbl.Text = desc(nameof(config.Books));
			this.downloadsInProgressDescLbl.Text = desc(nameof(config.DownloadsInProgressEnum));
			this.decryptInProgressDescLbl.Text = desc(nameof(config.DecryptInProgressEnum));

			var winTempText = "In your Windows temporary folder\r\n";
			this.downloadsInProgressWinTempRb.Text = $"{winTempText}{Path.Combine(Configuration.WinTemp, "DownloadsInProgress")}";
			this.decryptInProgressWinTempRb.Text = $"{winTempText}{Path.Combine(Configuration.WinTemp, "DecryptInProgress")}";

			var libFileText = "In your Libation Files (ie: program-created files)\r\n";
			this.downloadsInProgressLibationFilesRb.Text = $"{libFileText}{Path.Combine(config.LibationFiles, "DownloadsInProgress")}";
			this.decryptInProgressLibationFilesRb.Text = $"{libFileText}{Path.Combine(config.LibationFiles, "DecryptInProgress")}";

			this.booksLocationTb.Text
				= !string.IsNullOrWhiteSpace(config.Books)
				? config.Books
				: Path.GetDirectoryName(Exe.FileLocationOnDisk);

			downloadChaptersCbox.Checked = config.DownloadChapters;

			switch (config.DownloadsInProgressEnum)
			{
				case "LibationFiles":
					downloadsInProgressLibationFilesRb.Checked = true;
					break;
				case "WinTemp":
				default:
					downloadsInProgressWinTempRb.Checked = true;
					break;
			}

			switch (config.DecryptInProgressEnum)
			{
				case "LibationFiles":
					decryptInProgressLibationFilesRb.Checked = true;
					break;
				case "WinTemp":
				default:
					decryptInProgressWinTempRb.Checked = true;
					break;
			}
		}

		private void booksLocationSearchBtn_Click(object sender, EventArgs e) => selectFolder("Search for books location", this.booksLocationTb);

		private static void selectFolder(string desc, TextBox textbox)
		{
			using var dialog = new FolderBrowserDialog { Description = desc, SelectedPath = "" };
			dialog.ShowDialog();
			if (!string.IsNullOrWhiteSpace(dialog.SelectedPath))
				textbox.Text = dialog.SelectedPath;
		}

		private void saveBtn_Click(object sender, EventArgs e)
		{
			config.DownloadChapters = downloadChaptersCbox.Checked;
			config.DownloadsInProgressEnum = downloadsInProgressLibationFilesRb.Checked ? "LibationFiles" : "WinTemp";
			config.DecryptInProgressEnum = decryptInProgressLibationFilesRb.Checked ? "LibationFiles" : "WinTemp";

			var newBooks = this.booksLocationTb.Text;
			if (!Directory.Exists(newBooks))
			{
				MessageBox.Show($"Not saving change to Books location. This folder does not exist:\r\n{newBooks}");
				return;
			}
			else
				config.Books = newBooks;

			this.DialogResult = DialogResult.OK;
			this.Close();
		}

		private void cancelBtn_Click(object sender, EventArgs e) => this.Close();
	}
}
