using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using LibationAvalonia.ViewModels;
using ReactiveUI;
using System.IO;
using System.Threading.Tasks;

namespace LibationAvalonia.Dialogs.Login
{
	public partial class CaptchaDialog : DialogWindow
	{
		public string Password => _viewModel.Password;
		public string Answer => _viewModel.Answer;

		private CaptchaDialogViewModel _viewModel;
		public CaptchaDialog()
		{
			InitializeComponent();
		}

		public CaptchaDialog(string password, byte[] captchaImage) :this()
		{
			//Avalonia doesn't support animated gifs.
			//Deconstruct gifs into frames and manually switch them.
			using var gif = SixLabors.ImageSharp.Image.Load(captchaImage);
			var gifEncoder = new SixLabors.ImageSharp.Formats.Gif.GifEncoder();
			var gifFrames = new Bitmap[gif.Frames.Count];

			for (int i = 0; i < gif.Frames.Count; i++)
			{
				using var framems = new MemoryStream();

				using var clonedFrame = gif.Frames.CloneFrame(i);

				clonedFrame.Save(framems, gifEncoder);
				framems.Position = 0;
				gifFrames[i] = new Bitmap(framems);
			}

			DataContext = _viewModel = new(password, gifFrames);
		}

		private void InitializeComponent()
		{
			AvaloniaXamlLoader.Load(this);
		}

		protected override async Task SaveAndCloseAsync()
		{
			if (string.IsNullOrWhiteSpace(_viewModel.Password))
			{
				await MessageBox.Show(this, "Please re-enter your password");
				return;
			}

			Serilog.Log.Logger.Information("Submit button clicked: {@DebugInfo}", new { _viewModel.Answer });

			await _viewModel.StopAsync();
			await base.SaveAndCloseAsync();
		}

		public async void Submit_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
			=> await SaveAndCloseAsync();
	}

	public class CaptchaDialogViewModel : ViewModelBase
	{
		public string Answer { get; set; }
		public string Password { get; set; }
		public Bitmap CaptchaImage { get => _captchaImage; private set => this.RaiseAndSetIfChanged(ref _captchaImage, value); }

		private Bitmap _captchaImage;
		private bool keepSwitching = true;
		private readonly Task FrameSwitch;
		private readonly Bitmap[] GifFrames;
		private const int FRAME_INTERVAL_MS = 100;

		public CaptchaDialogViewModel(string password, Bitmap[] gifFrames)
		{
			Password = password;
			GifFrames = gifFrames;
			FrameSwitch = SwitchFramesAsync();
		}

		public async Task StopAsync()
		{
			keepSwitching = false;
			await FrameSwitch;
		}

		private async Task SwitchFramesAsync()
		{
			int index = 0;
			while(keepSwitching)
			{
				CaptchaImage = GifFrames[index++];

				index %= GifFrames.Length;
				await Task.Delay(FRAME_INTERVAL_MS);
			}
		}
	}
}
