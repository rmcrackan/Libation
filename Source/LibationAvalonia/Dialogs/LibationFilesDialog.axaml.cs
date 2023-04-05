using Avalonia.Controls;
using LibationFileManager;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LibationAvalonia.Dialogs
{
	public partial class LibationFilesDialog : Window
	{
		private class DirSelectOptions
		{
			public List<Configuration.KnownDirectories> KnownDirectories { get; } = new()
			{
				Configuration.KnownDirectories.UserProfile,
				Configuration.KnownDirectories.AppDir,
				Configuration.KnownDirectories.MyDocs
			};

			public string Directory { get; set; } = Configuration.GetKnownDirectoryPath(Configuration.KnownDirectories.UserProfile);
		}
		private DirSelectOptions dirSelectOptions;
		public string SelectedDirectory => dirSelectOptions.Directory;
		public DialogResult DialogResult { get; private set; }
		public LibationFilesDialog()
		{
			InitializeComponent();

			DataContext = dirSelectOptions = new();
		}

		public async Task SaveButtonAsync()
		{
			var libationDir = dirSelectOptions.Directory;

			if (!System.IO.Directory.Exists(libationDir))
			{
				await MessageBox.Show("Not saving change to Libation Files location. This folder does not exist:\r\n" + libationDir, "Folder does not exist", MessageBoxButtons.OK, MessageBoxIcon.Error, saveAndRestorePosition: false);
				return;
			}

			DialogResult = DialogResult.OK;
			Close(DialogResult);
		}
	}
}
