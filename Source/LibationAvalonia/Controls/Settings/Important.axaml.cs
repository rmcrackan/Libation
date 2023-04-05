using Avalonia.Controls;
using Avalonia.ReactiveUI;
using Dinah.Core;
using FileManager;
using LibationAvalonia.ViewModels;
using LibationAvalonia.ViewModels.Settings;
using LibationFileManager;

namespace LibationAvalonia.Controls.Settings
{
	public partial class Important : UserControl
	{
		public Important()
		{
			InitializeComponent();
			if (Design.IsDesignMode)
			{
				_ = Configuration.Instance.LibationFiles;
				DataContext = new ImportantSettingsVM(Configuration.Instance);
			}
		}

		public void OpenLogFolderButton_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
		{
			Go.To.Folder(((LongPath)Configuration.Instance.LibationFiles).ShortPathName);
		}
	}
}
