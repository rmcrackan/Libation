using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using LibationUiBase.Forms;
using ReactiveUI;
using System;
using System.ComponentModel;

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
			Title = $"Save Sover Image",
			SuggestedStartLocation = await StorageProvider.TryGetFolderFromPathAsync(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures)),
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

		var selectedFile = (await StorageProvider.SaveFilePickerAsync(options))?.TryGetLocalPath();

		if (selectedFile is null) return;

		try
		{
			_bitmapHolder.CoverImage?.Save(selectedFile);
		}
		catch (Exception ex)
		{
			Serilog.Log.Logger.Error(ex, $"Failed to save picture to {selectedFile}");
			await MessageBox.Show(this, $"An error was encountered while trying to save the picture\r\n\r\n{ex.Message}", "Failed to save picture", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
		}
	}

	public class BitmapHolder : ViewModels.ViewModelBase
	{
		public Bitmap? CoverImage { get => field; set => this.RaiseAndSetIfChanged(ref field, value); }
	}
}
