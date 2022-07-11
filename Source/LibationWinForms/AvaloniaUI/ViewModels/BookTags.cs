using Avalonia.Controls;
using Avalonia.Media.Imaging;
using System;
using System.ComponentModel;

namespace LibationWinForms.AvaloniaUI.ViewModels
{
	public class BookTags
	{
		private static Bitmap _buttonImage;

		static BookTags()
		{
			var memoryStream = new System.IO.MemoryStream();

			Properties.Resources.edit_25x25.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);
			memoryStream.Position = 0;
			_buttonImage = new Bitmap(memoryStream);

		}

		public string Tags { get; init; }
		public bool IsSeries { get; init; }

		public Control Control
		{
			get
			{
				if (IsSeries)
					return null;

				if (string.IsNullOrEmpty(Tags))
				{
					return new Image
					{
						Stretch = Avalonia.Media.Stretch.None,
						Source = _buttonImage
					};
				}
				else
				{
					return new TextBlock
					{
						Text = Tags,
						Margin = new Avalonia.Thickness(0, 0),
						TextWrapping = Avalonia.Media.TextWrapping.WrapWithOverflow
					};
				}
			}
		}
	}
}
