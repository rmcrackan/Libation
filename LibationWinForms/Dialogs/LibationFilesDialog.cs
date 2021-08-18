using FileManager;
using System;
using System.Linq;
using System.Windows.Forms;

namespace LibationWinForms.Dialogs
{
	public partial class LibationFilesDialog : Form
	{
		public string SelectedDirectory { get; private set; }

		public LibationFilesDialog() => InitializeComponent();

		private void LibationFilesDialog_Load(object sender, EventArgs e)
		{
			if (this.DesignMode)
				return;

			var config = Configuration.Instance;

			libationFilesDescLbl.Text = Configuration.GetDescription(nameof(config.LibationFiles));

			libationFilesSelectControl.SetSearchTitle("Libation Files");
			libationFilesSelectControl.SetDirectoryItems(new()
			{
				Configuration.KnownDirectories.UserProfile,
				Configuration.KnownDirectories.AppDir,
				Configuration.KnownDirectories.MyDocs
			}, Configuration.KnownDirectories.UserProfile);
			libationFilesSelectControl.SelectDirectory(config.LibationFiles);
		}

		private void saveBtn_Click(object sender, EventArgs e)
		{
			var libationDir = libationFilesSelectControl.SelectedDirectory;

			if (!System.IO.Directory.Exists(libationDir))
			{
				MessageBox.Show("Not saving change to Libation Files location. This folder does not exist:\r\n" + libationDir, "Folder does not exist", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			SelectedDirectory = libationDir;

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
