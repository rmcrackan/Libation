using Avalonia.Controls;
using LibationAvalonia.ViewModels.Settings;
using LibationFileManager;

namespace LibationAvalonia.Controls.Settings;

public partial class Audiobookshelf : UserControl
{
	public Audiobookshelf()
	{
		InitializeComponent();

		if (Design.IsDesignMode)
		{
			DataContext = new AudiobookshelfSettingsVM(Configuration.CreateMockInstance());
		}
	}
}
