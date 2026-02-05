using LibationFileManager;
using System;

namespace LibationUiBase
{
	public static class BaseUtil
	{
		/// <summary>A delegate that loads image bytes into the the UI framework's image format.</summary>
		public static Func<byte[]?, PictureSize, object?> LoadImage => s_LoadImage ?? DefaultLoadImageImpl;

		/// <summary>A delegate that loads a named resource into the the UI framework's image format.</summary>
		public static Func<string, object?> LoadResourceImage => s_LoadResourceImage ?? DefaultLoadResourceImageImpl;

		public static void SetLoadImageDelegate(Func<byte[]?, PictureSize, object?> tryLoadImage)
			=> s_LoadImage = tryLoadImage;
		public static void SetLoadResourceImageDelegate(Func<string, object?> tryLoadResourceImage)
			=> s_LoadResourceImage = tryLoadResourceImage;

		private static Func<byte[]?, PictureSize, object?>? s_LoadImage;
		private static Func<string, object?>? s_LoadResourceImage;

		private static object? DefaultLoadImageImpl(byte[]? imageBytes, PictureSize size)
		{
			Serilog.Log.Error("{LoadImage} called without a delegate set. Picture size: {PictureSize}", nameof(LoadImage), size);
			return null;
		}

		private static object? DefaultLoadResourceImageImpl(string resourceName)
		{
			Serilog.Log.Error("{LoadResourceImage} called without a delegate set. Resource name: {ResourceName}", nameof(LoadResourceImage), resourceName);
			return null;
		}
	}
}
