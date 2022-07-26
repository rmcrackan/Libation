using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using LibationFileManager;

namespace LibationAvalonia.Dialogs
{
	public partial class SetupDialog : Window
	{
		public bool IsNewUser { get;private set; }
		public bool IsReturningUser { get;private set; }
		public Configuration Config { get; init; }
		public SetupDialog()
		{
			InitializeComponent();

#if DEBUG
			this.AttachDevTools();
#endif
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

		private void InitializeComponent()
		{
			AvaloniaXamlLoader.Load(this);
		}
	}
}
