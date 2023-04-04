using Avalonia.Controls;
using Avalonia.ReactiveUI;
using LibationAvalonia.ViewModels.Settings;
using LibationFileManager;

namespace LibationAvalonia.Controls.Settings
{
	public partial class Import : ReactiveUserControl<ImportSettingsVM>
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
