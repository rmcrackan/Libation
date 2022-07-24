using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using LibationFileManager;
using LibationAvalonia.AvaloniaUI.Controls;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LibationAvalonia.AvaloniaUI.Views.Dialogs
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
			
			public string Directory { get; set; } = Configuration.Instance.LibationFiles;
		}
		private DirSelectOptions dirSelectOptions;
		public string SelectedDirectory => dirSelectOptions.Directory;
		public LibationFilesDialog()
		{
			InitializeComponent();
			DataContext = dirSelectOptions = new();
		}

		protected override async Task SaveAndCloseAsync()
		{
			var libationDir = dirSelectOptions.Directory;

			if (!System.IO.Directory.Exists(libationDir))
			{
				MessageBox.Show("Not saving change to Libation Files location. This folder does not exist:\r\n" + libationDir, "Folder does not exist", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			await base.SaveAndCloseAsync();
		}

		private void InitializeComponent()
		{
			AvaloniaXamlLoader.Load(this);
		}
	}
}
