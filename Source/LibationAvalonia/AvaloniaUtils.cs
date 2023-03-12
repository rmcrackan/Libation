using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using LibationAvalonia.Dialogs;
using LibationFileManager;
using System.Threading.Tasks;

namespace LibationAvalonia
{
	internal static class AvaloniaUtils
	{
		public static IBrush GetBrushFromResources(string name)
			=> GetBrushFromResources(name, Brushes.Transparent);
		public static IBrush GetBrushFromResources(string name, IBrush defaultBrush)
		{
			if (App.Current.Styles.TryGetResource(name, out var value) && value is IBrush brush)
				return brush;
			return defaultBrush;
		}

		public static Task<DialogResult> ShowDialogAsync(this DialogWindow dialogWindow, Window owner = null)
			=> dialogWindow.ShowDialog<DialogResult>(owner ?? App.MainWindow);

		public static Window GetParentWindow(this IControl control) => control.VisualRoot as Window;


		private static Bitmap defaultImage;
		public static Bitmap TryLoadImageOrDefault(byte[] picture, PictureSize defaultSize = PictureSize.Native)
		{
			try
			{
				using var ms = new System.IO.MemoryStream(picture);
				return new Bitmap(ms);
			}
			catch
			{
				using var ms = new System.IO.MemoryStream(PictureStorage.GetDefaultImage(defaultSize));
				return defaultImage ??= new Bitmap(ms);
			}
		}
	}
}
