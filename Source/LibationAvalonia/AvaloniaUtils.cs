using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Media.Imaging;
using Avalonia.VisualTree;
using LibationFileManager;
using LibationUiBase.Forms;
using System;
using System.Threading.Tasks;

namespace LibationAvalonia;

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

	public static Window GetParentWindow(this Control control)
		=> control.GetVisualRoot() as Window ?? App.MainWindow
		?? throw new InvalidOperationException("Cannot find parent window.");


	private static Bitmap? defaultImage;
	public static Bitmap TryLoadImageOrDefault(byte[]? picture, PictureSize defaultSize = PictureSize.Native)
	{
		if (picture is null || picture.Length == 0)
			return getDefaultImage();

		try
		{
			using var ms = new System.IO.MemoryStream(picture);
			return new Bitmap(ms);
		}
		catch
		{
			using var ms = new System.IO.MemoryStream(PictureStorage.GetDefaultImage(defaultSize));
			return getDefaultImage();
		}

		Bitmap getDefaultImage()
		{
			if (defaultImage is null)
			{
				using var ms = new System.IO.MemoryStream(PictureStorage.GetDefaultImage(defaultSize));
				defaultImage = new Bitmap(ms);
			}
			return defaultImage;
		}
	}
}
