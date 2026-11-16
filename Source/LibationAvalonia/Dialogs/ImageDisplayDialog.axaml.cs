using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using LibationUiBase.Forms;
using ReactiveUI;
using System;
using System.ComponentModel;
using LibationUiBase.Resources;

namespace LibationAvalonia.Dialogs;

public partial class ImageDisplayDialog : DialogWindow, INotifyPropertyChanged
{
	public string? PictureFileName { get; set; }
	public string? BookSaveDirectory { get; set; }

	private readonly BitmapHolder _bitmapHolder = new BitmapHolder();

	public ImageDisplayDialog()
	{
		InitializeComponent();
		DataContext = _bitmapHolder;
	}

	public void SetCoverBytes(byte[] cover)
	{
		_bitmapHolder.CoverImage = AvaloniaUtils.TryLoadImageOrDefault(cover);
	}

	public async void SaveImage_Clicked(object sender, Avalonia.Interactivity.RoutedEventArgs e)
	{
		var options = new FilePickerSaveOptions
		{
			Title = Strings.ImageDisplayDialog_SaveSoverImage,
			SuggestedStartLocation = await StorageProvider.TryGetFolderFromPathAsync(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures)),
			SuggestedFileName = PictureFileName,
			DefaultExtension = "jpg",
			ShowOverwritePrompt = true,
			FileTypeChoices = new FilePickerFileType[]
				{
					new(Strings.ImageDisplayDialog_JpegJpg)
					{
						Patterns = new[] { "jpg" },
						AppleUniformTypeIdentifiers = new[] { "public.jpeg" }
					}
				}
		};

		var selectedFile = (await StorageProvider.SaveFilePickerAsync(options))?.TryGetLocalPath();

		if (selectedFile is null) return;

		try
		{
			_bitmapHolder.CoverImage?.Save(selectedFile, JpegBitmapEncoderOptions.Default);
		}
		catch (Exception ex)
		{
			Serilog.Log.Logger.Error(ex, $"Failed to save picture to {selectedFile}");
			await MessageBox.Show(this, string.Format(Strings.ImageDisplayDialog_AnErrorWasEncounteredWhileTrying, ex.Message), Strings.ImageDisplayDialog_FailedToSavePicture, MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
		}
	}

	public class BitmapHolder : ViewModels.ViewModelBase
	{
		public Bitmap? CoverImage { get => field; set => this.RaiseAndSetIfChanged(ref field, value); }
	}
}
