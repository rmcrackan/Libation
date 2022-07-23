using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using System;
using System.ComponentModel;
using System.IO;
using ReactiveUI;

namespace LibationWinForms.AvaloniaUI.Views.Dialogs
{
	public partial class ImageDisplayDialog : DialogWindow, INotifyPropertyChanged
	{
		public string PictureFileName { get; set; }
		public string BookSaveDirectory { get; set; }

		private byte[] _coverBytes;
		public byte[] CoverBytes
		{
			get => _coverBytes;
			set
			{
				_coverBytes = value;
				var ms = new MemoryStream(_coverBytes);
				ms.Position = 0;
				_bitmapHolder.CoverImage = new Bitmap(ms);
			}
		}


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

		public async void SaveImage_Clicked(object sender, Avalonia.Interactivity.RoutedEventArgs e)
		{

			SaveFileDialog saveFileDialog = new();
			saveFileDialog.Filters.Add(new FileDialogFilter { Name = "Jpeg", Extensions = new System.Collections.Generic.List<string>() { "jpg" } });
			saveFileDialog.Directory = Directory.Exists(BookSaveDirectory) ? BookSaveDirectory : Path.GetDirectoryName(BookSaveDirectory);
			saveFileDialog.InitialFileName = PictureFileName;

			var fileName = await saveFileDialog.ShowAsync(this);

			if (fileName is null)
				return;

			try
			{
				File.WriteAllBytes(fileName, CoverBytes);
			}
			catch (Exception ex)
			{
				Serilog.Log.Logger.Error(ex, $"Failed to save picture to {fileName}");
				MessageBox.Show(this, $"An error was encountered while trying to save the picture\r\n\r\n{ex.Message}", "Failed to save picture", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
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
