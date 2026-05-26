using LibationFileManager;
using LibationUiBase;
using System;
using System.Windows.Forms;

namespace LibationWinForms.Dialogs;

public partial class LibationFilesDialog : Form, ILibationInstallLocation
{
	public string? SelectedDirectory { get; private set; }

	public LibationFilesDialog() => InitializeComponent();

	private void LibationFilesDialog_Load(object sender, EventArgs e)
	{
		if (this.DesignMode)
			return;

		var config = Configuration.Instance;

		libationFilesDescLbl.Text = Configuration.GetDescription(nameof(config.LibationFiles));

		libationFilesSelectControl.SetSearchTitle("Libation Files");
		var knownDirectories = Configuration.FilterKnownDirectories(
		[
			Configuration.KnownDirectories.UserProfile,
			Configuration.KnownDirectories.AppDir,
			Configuration.KnownDirectories.MyDocs
		], Configuration.KnownDirectoryUsage.LibationFilesLocation);
		libationFilesSelectControl.SetDirectoryItems(
			knownDirectories,
			knownDirectories.Count > 0 ? knownDirectories[0] : null);

		var selectedDir = System.IO.Directory.Exists(Configuration.Instance.LibationFiles.Location.PathWithoutPrefix)
			? Configuration.Instance.LibationFiles.Location.PathWithoutPrefix
			: LibationFiles.DefaultLibationFilesDirectory;

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
