using LibationFileManager;
using System;

namespace LibationUiBase
{
	public static class BaseUtil
	{
		/// <summary>A delegate that loads image bytes into the the UI framework's image format.</summary>
		public static Func<byte[], PictureSize, object> LoadImage { get; private set; }
		public static void SetLoadImageDelegate(Func<byte[], PictureSize, object> tryLoadImage)
			=> LoadImage = tryLoadImage;
	}
}
