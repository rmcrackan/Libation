using Avalonia.Controls;
using LibationAvalonia.ViewModels.Settings;
using LibationFileManager;

namespace LibationAvalonia.Controls.Settings
{
	public partial class Import : UserControl
	{
		public Import()
		{
			InitializeComponent();

			if (Design.IsDesignMode)
			{
				_ = Configuration.Instance.LibationFiles;
				DataContext = new ImportSettingsVM(Configuration.Instance);
			}
		}
	}
}
