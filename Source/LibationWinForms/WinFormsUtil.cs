using Dinah.Core.WindowsDesktop.Drawing;
using LibationFileManager;
using System.Drawing;

namespace LibationWinForms
{
	internal static class WinFormsUtil
	{
		private static Bitmap defaultImage;
		public static Image TryLoadImageOrDefault(byte[] picture, PictureSize defaultSize = PictureSize.Native)
		{
			try
			{
				return ImageReader.ToImage(picture);
			}
			catch
			{
				using var ms = new System.IO.MemoryStream(PictureStorage.GetDefaultImage(defaultSize));
				return defaultImage ??= new Bitmap(ms);
			}
		}
	}
}
