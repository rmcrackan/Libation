using Avalonia.Controls;
using LibationAvalonia.ViewModels.Settings;
using LibationFileManager;

namespace LibationAvalonia.Controls.Settings;

public partial class DownloadDecrypt : UserControl
{
	public DownloadDecrypt()
	{
		InitializeComponent();
		if (Design.IsDesignMode)
		{
			DataContext = new DownloadDecryptSettingsVM(Configuration.CreateMockInstance());
		}
	}
}
