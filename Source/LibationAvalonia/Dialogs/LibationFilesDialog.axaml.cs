using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using LibationFileManager;
using LibationAvalonia.Controls;
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
		public LibationFilesDialog()
		{
			InitializeComponent();

#if DEBUG
			this.AttachDevTools();
#endif
			DataContext = dirSelectOptions = new();
		}

		public async void SaveButton_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
		{

			var libationDir = dirSelectOptions.Directory;

			if (!System.IO.Directory.Exists(libationDir))
			{
				await MessageBox.Show("Not saving change to Libation Files location. This folder does not exist:\r\n" + libationDir, "Folder does not exist", MessageBoxButtons.OK, MessageBoxIcon.Error, saveAndRestorePosition: false);
				return;
			}

			Close(DialogResult.OK);
		}

		private void InitializeComponent()
		{
			AvaloniaXamlLoader.Load(this);
		}
	}
}
