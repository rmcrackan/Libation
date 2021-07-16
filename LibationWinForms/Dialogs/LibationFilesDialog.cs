using System;
using System.Windows.Forms;
using FileManager;

namespace LibationWinForms.Dialogs
{
	public partial class LibationFilesDialog : Form
	{
		private Configuration config { get; } = Configuration.Instance;
		private Func<string, string> desc { get; } = Configuration.GetDescription;

		public LibationFilesDialog() => InitializeComponent();

		private void LibationFilesDialog_Load(object sender, EventArgs e)
		{
			if (this.DesignMode)
				return;

			libationFilesDescLbl.Text = desc(nameof(config.LibationFiles));

			directoryOrCustomSelectControl.SetSearchTitle("Libation Files");
			directoryOrCustomSelectControl.SetDirectoryItems(new()
			{
				Configuration.KnownDirectories.UserProfile,
				Configuration.KnownDirectories.AppDir,
				Configuration.KnownDirectories.MyDocs
			}, Configuration.KnownDirectories.UserProfile);
			directoryOrCustomSelectControl.SelectDirectory(config.LibationFiles);
		}

		private void saveBtn_Click(object sender, EventArgs e)
		{
			var libationDir = directoryOrCustomSelectControl.SelectedDirectory;
			if (!config.TrySetLibationFiles(libationDir))
			{
				MessageBox.Show("Not saving change to Libation Files location. This folder does not exist:\r\n" + libationDir);
				return;
			}

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
