using LibationFileManager;
using LibationUiBase;
using System;
using System.Windows.Forms;

namespace LibationWinForms.Dialogs
{
	public partial class LibationFilesDialog : Form, ILibationInstallLocation
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

			var selectedDir = System.IO.Directory.Exists(Configuration.Instance.LibationFiles.Location.PathWithoutPrefix)
				? Configuration.Instance.LibationFiles.Location.PathWithoutPrefix
				: Configuration.GetKnownDirectoryPath(Configuration.KnownDirectories.UserProfile);

			libationFilesSelectControl.SelectDirectory(selectedDir);
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
