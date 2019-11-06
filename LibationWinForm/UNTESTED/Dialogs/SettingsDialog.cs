using System;
using System.IO;
using System.Windows.Forms;
using Dinah.Core;
using FileManager;

namespace LibationWinForm
{
	public partial class SettingsDialog : Form
	{
		Configuration config { get; } = Configuration.Instance;
		Func<string, string> desc { get; } = Configuration.GetDescription;
		string exeRoot { get; }
		string myDocs { get; }

		bool isFirstLoad;

		public SettingsDialog()
		{
			InitializeComponent();
			audibleLocaleCb.SelectedIndex = 0;

			exeRoot = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(Exe.FileLocationOnDisk), "Libation"));
			myDocs = Path.GetFullPath(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Libation"));
		}

		private void SettingsDialog_Load(object sender, EventArgs e)
		{
			isFirstLoad = string.IsNullOrWhiteSpace(config.Books);

			this.settingsFileTb.Text = config.Filepath;
			this.settingsFileDescLbl.Text = desc(nameof(config.Filepath));

			this.decryptKeyTb.Text = config.DecryptKey;
			this.decryptKeyDescLbl.Text = desc(nameof(config.DecryptKey));

			this.booksLocationTb.Text = config.Books;
			this.booksLocationDescLbl.Text = desc(nameof(config.Books));

			this.audibleLocaleCb.Text = config.LocaleCountryCode;

			libationFilesDescLbl.Text = desc(nameof(config.LibationFiles));
			this.libationFilesRootRb.Text = "In the same folder that Libation is running from\r\n" + exeRoot;
			this.libationFilesMyDocsRb.Text = "In My Documents\r\n" + myDocs;
			if (config.LibationFiles == exeRoot)
				libationFilesRootRb.Checked = true;
			else if (config.LibationFiles == myDocs)
				libationFilesMyDocsRb.Checked = true;
			else
			{
				libationFilesCustomRb.Checked = true;
				libationFilesCustomTb.Text = config.LibationFiles;
			}

			this.downloadsInProgressDescLbl.Text = desc(nameof(config.DownloadsInProgressEnum));
			var winTempDownloadsInProgress = Path.Combine(config.WinTemp, "DownloadsInProgress");
			this.downloadsInProgressWinTempRb.Text = "In your Windows temporary folder\r\n" + winTempDownloadsInProgress;
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

			this.decryptInProgressDescLbl.Text = desc(nameof(config.DecryptInProgressEnum));
			var winTempDecryptInProgress = Path.Combine(config.WinTemp, "DecryptInProgress");
			this.decryptInProgressWinTempRb.Text = "In your Windows temporary folder\r\n" + winTempDecryptInProgress;
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

			libationFiles_Changed(this, null);
		}

		private void libationFiles_Changed(object sender, EventArgs e)
		{
			Check_libationFilesCustom_RadioButton();

			var libationFilesDir
				= libationFilesRootRb.Checked ? exeRoot
				: libationFilesMyDocsRb.Checked ? myDocs
				: libationFilesCustomTb.Text;

			var downloadsInProgress = Path.Combine(libationFilesDir, "DownloadsInProgress");
			this.downloadsInProgressLibationFilesRb.Text = $"In your Libation Files (ie: program-created files)\r\n{downloadsInProgress}";

			var decryptInProgress = Path.Combine(libationFilesDir, "DecryptInProgress");
			this.decryptInProgressLibationFilesRb.Text = $"In your Libation Files (ie: program-created files)\r\n{decryptInProgress}";
		}

		private void booksLocationSearchBtn_Click(object sender, EventArgs e) => selectFolder("Search for books location", this.booksLocationTb);

		private void libationFilesCustomBtn_Click(object sender, EventArgs e)
		{
			Check_libationFilesCustom_RadioButton();
			selectFolder("Search for Libation Files location", this.libationFilesCustomTb);
		}
		private void libationFilesCustomRb_CheckedChanged(object sender, EventArgs e) => Check_libationFilesCustom_RadioButton();

		private void Check_libationFilesCustom_RadioButton() => this.libationFilesCustomRb.Checked = true;

		private static void selectFolder(string desc, TextBox textbox)
        {
            using var dialog = new FolderBrowserDialog { Description = desc, SelectedPath = "" };
            dialog.ShowDialog();
            if (!string.IsNullOrWhiteSpace(dialog.SelectedPath))
                textbox.Text = dialog.SelectedPath;
        }

        private void saveBtn_Click(object sender, EventArgs e)
        {
            config.DecryptKey = this.decryptKeyTb.Text;

            var pathsChanged = false;

            if (!Directory.Exists(this.booksLocationTb.Text))
                MessageBox.Show("Not saving change to Books location. This folder does not exist:\r\n" + this.booksLocationTb.Text);
            else if (config.Books != this.booksLocationTb.Text)
            {
                pathsChanged = true;
                config.Books = this.booksLocationTb.Text;
            }

			config.LocaleCountryCode = this.audibleLocaleCb.Text;

            var libationDir
                = libationFilesRootRb.Checked ? exeRoot
                : libationFilesMyDocsRb.Checked ? myDocs
                : libationFilesCustomTb.Text;
            if (!Directory.Exists(libationDir))
                MessageBox.Show("Not saving change to Libation Files location. This folder does not exist:\r\n" + libationDir);
            else if (config.LibationFiles != libationDir)
            {
                pathsChanged = true;
                config.LibationFiles = libationDir;
            }

            config.DownloadsInProgressEnum = downloadsInProgressLibationFilesRb.Checked ? "LibationFiles" : "WinTemp";
            config.DecryptInProgressEnum = decryptInProgressLibationFilesRb.Checked ? "LibationFiles" : "WinTemp";

            if (!isFirstLoad && pathsChanged)
            {
                var shutdownResult = MessageBox.Show(
                    "You have changed a file path important for this program. All files will remain in their original location; nothing will be moved. It is highly recommended that you restart this program so these changes are handled correctly."
                    + "\r\n"
                    + "\r\nClose program?",
                    "Restart program",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Exclamation,
                    MessageBoxDefaultButton.Button1);
                if (shutdownResult == DialogResult.Yes)
                {
                    Application.Exit();
                }
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void cancelBtn_Click(object sender, EventArgs e) => this.Close();
	}
}
