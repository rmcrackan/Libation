using LibationFileManager;
using LibationUiBase;
using LibationUiBase.Forms;
using System.Collections.Generic;
using System.IO;

namespace LibationAvalonia.Dialogs
{
	public partial class LibationFilesDialog : DialogWindow, ILibationInstallLocation
	{
		private class DirSelectOptions
		{
			public List<Configuration.KnownDirectories> KnownDirectories { get; } = new()
			{
				Configuration.KnownDirectories.UserProfile,
				Configuration.KnownDirectories.AppDir,
				Configuration.KnownDirectories.MyDocs
			};

			public string? Directory { get; set; }
		}

		private readonly DirSelectOptions dirSelectOptions;
		public string? SelectedDirectory => dirSelectOptions.Directory;

		public LibationFilesDialog() : base(saveAndRestorePosition: false)
		{
			InitializeComponent();

			DataContext = dirSelectOptions = new();
		}

		public LibationFilesDialog(string initialDir)
		{
			dirSelectOptions = new();
			dirSelectOptions.Directory = Directory.Exists(initialDir) ? initialDir : Configuration.GetKnownDirectoryPath(Configuration.KnownDirectories.UserProfile);
			InitializeComponent();

			DataContext = dirSelectOptions;
		}

		public async void Save_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
		{

			if (SelectedDirectory is not null && !Directory.Exists(SelectedDirectory))
			{
				try
				{
					Directory.CreateDirectory(SelectedDirectory);
				}
				catch
				{
					await MessageBox.Show("Not saving change to Libation Files location. This folder does not exist:\r\n" + SelectedDirectory, "Folder does not exist", MessageBoxButtons.OK, MessageBoxIcon.Error, saveAndRestorePosition: false);
					return;
				}
			}

			await SaveAndCloseAsync();
		}
	}
}
