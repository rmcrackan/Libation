using System;
using System.Windows.Forms;
using FileManager;

namespace LibationWinForms.Dialogs
{
	public partial class LibationFilesDialog : Form
	{
		Configuration config { get; } = Configuration.Instance;
		Func<string, string> desc { get; } = Configuration.GetDescription;

		public LibationFilesDialog()
		{
			InitializeComponent();

			this.libationFilesCustomTb.TextChanged += (_, __) =>
			{
				if (!string.IsNullOrWhiteSpace(libationFilesCustomTb.Text))
					this.libationFilesCustomRb.Checked = true;
			};
		}

		private void LibationFilesDialog_Load(object sender, EventArgs e)
		{
			if (this.DesignMode)
				return;

			libationFilesDescLbl.Text = desc(nameof(config.LibationFiles));
			this.libationFilesRootRb.Text = "In the same folder that Libation is running from\r\n" + Configuration.AppDir_Relative;
			this.libationFilesMyDocsRb.Text = "In My Documents\r\n" + Configuration.MyDocs;
			if (config.LibationFiles == Configuration.AppDir_Relative)
				libationFilesRootRb.Checked = true;
			else if (config.LibationFiles == Configuration.MyDocs)
				libationFilesMyDocsRb.Checked = true;
			else
			{
				libationFilesCustomRb.Checked = true;
				libationFilesCustomTb.Text = config.LibationFiles;
			}
		}

		private void libationFilesCustomBtn_Click(object sender, EventArgs e) => selectFolder("Search for Libation Files location", this.libationFilesCustomTb);

		private static void selectFolder(string desc, TextBox textbox)
		{
			using var dialog = new FolderBrowserDialog { Description = desc, SelectedPath = "" };
			dialog.ShowDialog();
			if (!string.IsNullOrWhiteSpace(dialog.SelectedPath))
				textbox.Text = dialog.SelectedPath;
		}

		private void saveBtn_Click(object sender, EventArgs e)
		{
			var libationDir
				= libationFilesRootRb.Checked ? Configuration.AppDir_Relative
				: libationFilesMyDocsRb.Checked ? Configuration.MyDocs
				: libationFilesCustomTb.Text;
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
