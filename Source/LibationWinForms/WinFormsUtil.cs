using Dinah.Core.WindowsDesktop.Drawing;
using LibationFileManager;
using System.Drawing;
using System.Windows.Forms;

namespace LibationWinForms;

internal static class WinFormsUtil
{
	private const float BaseDpi = 96;

	private static Bitmap? defaultImage;
	public static Image TryLoadImageOrDefault(byte[]? picture, PictureSize defaultSize = PictureSize.Native)
	{
		if (picture?.Length is null or 0)
			return getDefaultImage();

		try
		{
			return ImageReader.ToImage(picture);
		}
		catch
		{
			return getDefaultImage();
		}

		Image getDefaultImage()
		{
			if (defaultImage is null)
			{
				using var ms = new System.IO.MemoryStream(PictureStorage.GetDefaultImage(defaultSize));
				defaultImage = new Bitmap(ms);
			}
			return defaultImage;
		}
	}

	public static int DpiScale(this Control control, int value, float additionalScaleFactor = 1)
		=> (int)float.Round(control.DeviceDpi / BaseDpi * value * additionalScaleFactor);

	public static int DpiUnscale(this Control control, int value)
		=> (int)float.Round(BaseDpi / control.DeviceDpi * value);

	public static int ScaleX(this Graphics control, int value)
		=> (int)float.Round(control.DpiX / BaseDpi * value);
	public static int ScaleY(this Graphics control, int value)
		=> (int)float.Round(control.DpiY / BaseDpi * value);
}
