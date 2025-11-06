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
				DataContext = new ImportSettingsVM(Configuration.CreateMockInstance());
			}
		}
	}
}
