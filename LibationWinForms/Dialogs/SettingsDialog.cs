using System;
using System.IO;
using System.Windows.Forms;
using FileManager;

namespace LibationWinForms.Dialogs
{
	public partial class SettingsDialog : Form
	{
		Configuration config { get; } = Configuration.Instance;
		Func<string, string> desc { get; } = Configuration.GetDescription;

		public SettingsDialog() => InitializeComponent();

		private void SettingsDialog_Load(object sender, EventArgs e)
		{
			if (this.DesignMode)
				return;

			this.booksLocationDescLbl.Text = desc(nameof(config.Books));
			this.inProgressDescLbl.Text = desc(nameof(config.InProgress));

			booksSelectControl.SetSearchTitle("books location");
			booksSelectControl.SetDirectoryItems(
				new()
				{
					Configuration.KnownDirectories.UserProfile,
					Configuration.KnownDirectories.AppDir,
					Configuration.KnownDirectories.MyDocs
				},
				Configuration.KnownDirectories.UserProfile,
				"Books");
			booksSelectControl.SelectDirectory(config.Books);

			allowLibationFixupCbox.Checked = config.AllowLibationFixup;
			convertLosslessRb.Checked = !config.DecryptToLossy;
			convertLossyRb.Checked = config.DecryptToLossy;

			allowLibationFixupCbox_CheckedChanged(this, e);

			inProgressSelectControl.SetDirectoryItems(new()
			{
				Configuration.KnownDirectories.WinTemp,
				Configuration.KnownDirectories.UserProfile,
				Configuration.KnownDirectories.AppDir,
				Configuration.KnownDirectories.MyDocs,
				Configuration.KnownDirectories.LibationFiles
			}, Configuration.KnownDirectories.WinTemp);
			inProgressSelectControl.SelectDirectory(config.InProgress);
		}

		private void saveBtn_Click(object sender, EventArgs e)
		{
			config.AllowLibationFixup = allowLibationFixupCbox.Checked;
			config.DecryptToLossy = convertLossyRb.Checked;

			config.InProgress = inProgressSelectControl.SelectedDirectory;
			
			var newBooks = booksSelectControl.SelectedDirectory;

			if (string.IsNullOrWhiteSpace(newBooks))
			{
				MessageBox.Show("Cannot set Books Location to blank", "Location is blank", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			if (!Directory.Exists(newBooks))
			{
				if (booksSelectControl.SelectedDirectoryIsCustom)
				{
					MessageBox.Show($"Not saving change to Books location. This folder does not exist:\r\n{newBooks}", "Folder does not exist", MessageBoxButtons.OK, MessageBoxIcon.Error);
					return;
				}

				if (booksSelectControl.SelectedDirectoryIsKnown)
					Directory.CreateDirectory(newBooks);
			}
			
			config.Books = newBooks;

			this.DialogResult = DialogResult.OK;
			this.Close();
		}

		private void cancelBtn_Click(object sender, EventArgs e)
		{
			this.DialogResult = DialogResult.Cancel;
			this.Close();
		}

        private void allowLibationFixupCbox_CheckedChanged(object sender, EventArgs e)
        {
			convertLosslessRb.Enabled = allowLibationFixupCbox.Checked;
			convertLossyRb.Enabled = allowLibationFixupCbox.Checked;

			if (!allowLibationFixupCbox.Checked)
            {
				convertLosslessRb.Checked = true;
			}
		}
    }
}
