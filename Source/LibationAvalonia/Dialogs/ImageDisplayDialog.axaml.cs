using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using System;
using System.ComponentModel;
using ReactiveUI;
using Avalonia.Platform.Storage;

namespace LibationAvalonia.Dialogs
{
	public partial class ImageDisplayDialog : DialogWindow, INotifyPropertyChanged
	{
		public string PictureFileName { get; set; }
		public string BookSaveDirectory { get; set; }

		private readonly BitmapHolder _bitmapHolder = new BitmapHolder();

		public ImageDisplayDialog()
		{
			InitializeComponent();
			DataContext = _bitmapHolder;
		}


		private void InitializeComponent()
		{
			AvaloniaXamlLoader.Load(this);
		}

		public void SetCoverBytes(byte[] cover)
		{
			_bitmapHolder.CoverImage = AvaloniaUtils.TryLoadImageOrDefault(cover);
		}

		public async void SaveImage_Clicked(object sender, Avalonia.Interactivity.RoutedEventArgs e)
		{
			var options = new FilePickerSaveOptions
			{
				Title = $"Save Sover Image",
				SuggestedStartLocation = new Avalonia.Platform.Storage.FileIO.BclStorageFolder(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures)),
				SuggestedFileName = PictureFileName,
				DefaultExtension = "jpg",
				ShowOverwritePrompt = true,
				FileTypeChoices = new FilePickerFileType[]
					{
						new("Jpeg (*.jpg)")
						{
							Patterns = new[] { "jpg" },
							AppleUniformTypeIdentifiers = new[] { "public.jpeg" }
						}
					}
			};

			var selectedFile = await StorageProvider.SaveFilePickerAsync(options);

			if (selectedFile?.TryGetUri(out var uri) is not true) return;

			try
			{
				_bitmapHolder.CoverImage.Save(uri.LocalPath);
			}
			catch (Exception ex)
			{
				Serilog.Log.Logger.Error(ex, $"Failed to save picture to {uri.LocalPath}");
				await MessageBox.Show(this, $"An error was encountered while trying to save the picture\r\n\r\n{ex.Message}", "Failed to save picture", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
			}
		}

		public class BitmapHolder : ViewModels.ViewModelBase
		{
			private Bitmap _coverImage;
			public Bitmap CoverImage
			{
				get => _coverImage;
				set
				{
					this.RaiseAndSetIfChanged(ref _coverImage, value);
				}
			}
		}
	}
}
