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
			booksSelectControl.SetDirectoryItems(new()
			{
				Configuration.KnownDirectories.UserProfile,
				Configuration.KnownDirectories.AppDir,
				Configuration.KnownDirectories.MyDocs
			}, Configuration.KnownDirectories.UserProfile);
			booksSelectControl.SelectDirectory(config.Books);

			allowLibationFixupCbox.Checked = config.AllowLibationFixup;

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

			config.InProgress = inProgressSelectControl.SelectedDirectory;
			
			var newBooks = booksSelectControl.SelectedDirectory;
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

		private void cancelBtn_Click(object sender, EventArgs e)
		{
			this.DialogResult = DialogResult.Cancel;
			this.Close();
		}
	}
}
