using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System.Threading.Tasks;

namespace LibationAvalonia.Dialogs.Login
{
	public partial class _2faCodeDialog : DialogWindow
	{
		public string Code { get; set; }
		public string Prompt { get; }


		public _2faCodeDialog() => InitializeComponent();

		public _2faCodeDialog(string prompt) : this()
		{
			Prompt = prompt;
			DataContext = this;
		}

		private void InitializeComponent()
		{
			AvaloniaXamlLoader.Load(this);
		}

		protected override Task SaveAndCloseAsync()
		{
			Serilog.Log.Logger.Information("Submit button clicked: {@DebugInfo}", new { Code });

			return base.SaveAndCloseAsync();
		}

		public async void Submit_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
			=> await SaveAndCloseAsync();
	}
}
