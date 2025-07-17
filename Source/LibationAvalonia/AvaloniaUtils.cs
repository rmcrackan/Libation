using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Media.Imaging;
using Avalonia.VisualTree;
using LibationFileManager;
using LibationUiBase.Forms;
using System.Threading.Tasks;

#nullable enable
namespace LibationAvalonia
{
	internal static class AvaloniaUtils
	{
		public static T DynamicResource<T>(this T control, AvaloniaProperty prop, object resourceKey) where T : Control
		{
			control[!prop] = new DynamicResourceExtension(resourceKey);
			return control;
		}

		public static Task<DialogResult> ShowDialogAsync(this Dialogs.DialogWindow dialogWindow, Window? owner = null)
			=> ((owner ?? App.MainWindow) is Window window)
			? dialogWindow.ShowDialog<DialogResult>(window)
			: Task.FromResult(DialogResult.None);

		public static Task<DialogResult> ShowDialogAsync(this Dialogs.Login.WebLoginDialog dialogWindow, Window? owner = null)
			=> ((owner ?? App.MainWindow) is Window window)
			? dialogWindow.ShowDialog<DialogResult>(window)
			: Task.FromResult(DialogResult.None);

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
