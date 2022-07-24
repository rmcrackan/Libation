using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using System.IO;
using System.Threading.Tasks;

namespace LibationAvalonia.AvaloniaUI.Views.Dialogs.Login
{
	public partial class CaptchaDialog : DialogWindow
	{
		public string Answer { get; set; }
		public Bitmap CaptchaImage { get; }
		public CaptchaDialog()
		{
			InitializeComponent();
		}

		public CaptchaDialog(byte[] captchaImage) :this()
		{
			using var ms = new MemoryStream(captchaImage);
			CaptchaImage = new Bitmap(ms);
			DataContext = this;
		}

		private void InitializeComponent()
		{
			AvaloniaXamlLoader.Load(this);
		}


		protected override Task SaveAndCloseAsync()
		{
			Serilog.Log.Logger.Information("Submit button clicked: {@DebugInfo}", new { Answer });

			return base.SaveAndCloseAsync();
		}

		public async void Submit_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
			=> await SaveAndCloseAsync();
	}
}
