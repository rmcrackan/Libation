using Avalonia.Controls;
using LibationFileManager;
using LibationUiBase;
using LibationUiBase.Forms;

namespace LibationAvalonia.Dialogs
{
	public partial class SetupDialog : Window, ILibationSetup
	{
		public bool IsNewUser { get; private set; }
		public bool IsReturningUser { get; private set; }
		public ComboBoxItem? SelectedTheme { get; set; }
		public SetupDialog()
		{
			InitializeComponent();
			DataContext = this;
		}

		public void NewUser_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
		{
			IsNewUser = true;
			Close(DialogResult.OK);
		}

		public void ReturningUser_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
		{
			IsReturningUser = true;
			Close(DialogResult.OK);
		}
	}
}
