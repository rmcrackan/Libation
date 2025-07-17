using LibationFileManager;
using LibationUiBase.Forms;
using System.Collections.Generic;

namespace LibationAvalonia.Dialogs
{
	public partial class LibationFilesDialog : DialogWindow
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

		private readonly DirSelectOptions dirSelectOptions;
		public string SelectedDirectory => dirSelectOptions.Directory;

		public LibationFilesDialog() : base(saveAndRestorePosition: false)
		{
			InitializeComponent();

			DataContext = dirSelectOptions = new();
		}

		public async void Save_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
		{
			if (!System.IO.Directory.Exists(SelectedDirectory))
			{
				await MessageBox.Show("Not saving change to Libation Files location. This folder does not exist:\r\n" + SelectedDirectory, "Folder does not exist", MessageBoxButtons.OK, MessageBoxIcon.Error, saveAndRestorePosition: false);
				return;
			}

			await SaveAndCloseAsync();
		}
	}
}
