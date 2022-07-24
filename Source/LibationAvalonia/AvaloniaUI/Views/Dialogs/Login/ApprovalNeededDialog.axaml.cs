using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System.Threading.Tasks;

namespace LibationAvalonia.AvaloniaUI.Views.Dialogs.Login
{
	public partial class ApprovalNeededDialog : DialogWindow
	{
		public ApprovalNeededDialog()
		{
			InitializeComponent();
		}

		private void InitializeComponent()
		{
			AvaloniaXamlLoader.Load(this);
		}

		protected override Task SaveAndCloseAsync()
		{
			Serilog.Log.Logger.Information("Approve button clicked");

			return base.SaveAndCloseAsync();
		}

		public async void Approve_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
			=> await SaveAndCloseAsync();
	}
}
