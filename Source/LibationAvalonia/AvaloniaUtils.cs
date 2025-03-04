using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.VisualTree;
using LibationAvalonia.Dialogs;
using LibationFileManager;
using System.Threading.Tasks;

#nullable enable
namespace LibationAvalonia
{
	internal static class AvaloniaUtils
	{
		public static IBrush GetBrushFromResources(string name)
			=> GetBrushFromResources(name, Brushes.Transparent);
		public static IBrush GetBrushFromResources(string name, IBrush defaultBrush)
		{
			if ((App.Current?.TryGetResource(name, App.Current.ActualThemeVariant, out var value) ?? false) && value is IBrush brush)
				return brush;
			return defaultBrush;
		}

		public static Task<DialogResult> ShowDialogAsync(this DialogWindow dialogWindow, Window? owner = null)
			=> dialogWindow.ShowDialog<DialogResult>(owner ?? App.MainWindow);

		public static Window? GetParentWindow(this Control control) => control.GetVisualRoot() as Window;


		private static Bitmap? defaultImage;
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
