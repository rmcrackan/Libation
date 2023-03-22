using Avalonia.Controls;
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

		private readonly CaptchaDialogViewModel _viewModel;
		public CaptchaDialog()
		{
			InitializeComponent();
			passwordBox = this.FindControl<TextBox>(nameof(passwordBox));
			captchaBox = this.FindControl<TextBox>(nameof(captchaBox));
		}

		public CaptchaDialog(string password, byte[] captchaImage) :this()
		{
			//Avalonia doesn't support animated gifs.
			//Deconstruct gifs into frames and manually switch them.
			using var gif = SixLabors.ImageSharp.Image.Load(captchaImage);
			var gifEncoder = new SixLabors.ImageSharp.Formats.Gif.GifEncoder();
			var gifFrames = new Bitmap[gif.Frames.Count];
			var frameDelayMs = new int[gif.Frames.Count];

			for (int i = 0; i < gif.Frames.Count; i++)
			{
				var frameMetadata = gif.Frames[i].Metadata.GetFormatMetadata(SixLabors.ImageSharp.Formats.Gif.GifFormat.Instance);

                using var clonedFrame = gif.Frames.CloneFrame(i);
				using var framems = new MemoryStream();

				clonedFrame.Save(framems, gifEncoder);
				framems.Position = 0;

				gifFrames[i] = new Bitmap(framems);
				frameDelayMs[i] = frameMetadata.FrameDelay * 10;
			}

			DataContext = _viewModel = new(password, gifFrames, frameDelayMs);

			Opened += (_, _) => (string.IsNullOrEmpty(password) ? passwordBox : captchaBox).Focus();
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

		protected override async Task CancelAndCloseAsync()
		{
			await _viewModel.StopAsync();
			await base.CancelAndCloseAsync();	
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

		public CaptchaDialogViewModel(string password, Bitmap[] gifFrames, int[] frameDelayMs)
		{
			Password = password;
			if (gifFrames.Length == 1)
			{
				FrameSwitch = Task.CompletedTask;
				CaptchaImage = gifFrames[0];
			}
			else
			{
				FrameSwitch = SwitchFramesAsync(gifFrames, frameDelayMs);
			}
		}

		public async Task StopAsync()
		{
			keepSwitching = false;
			await FrameSwitch;
		}

		private async Task SwitchFramesAsync(Bitmap[] gifFrames, int[] frameDelayMs)
		{
			int index = 0;
			while(keepSwitching)
			{
				CaptchaImage = gifFrames[index];
				await Task.Delay(frameDelayMs[index++]);

				index %= gifFrames.Length;
			}

			foreach (var frame in gifFrames)
				frame.Dispose();
		}
	}
}
