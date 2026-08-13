using LibationFileManager;
using System;

namespace LibationUiBase;

public static class BaseUtil
{
	/// <summary>A delegate that loads image bytes into the the UI framework's image format.</summary>
	public static Func<byte[]?, PictureSize, object?> LoadImage => s_LoadImage ?? DefaultLoadImageImpl;

	/// <summary>A delegate that reports whether the UI framework is currently using a dark theme.</summary>
	public static Func<bool> IsDarkMode => s_IsDarkMode ?? DefaultIsDarkModeImpl;

	public static void SetLoadImageDelegate(Func<byte[]?, PictureSize, object?> tryLoadImage)
		=> s_LoadImage = tryLoadImage;
	public static void SetIsDarkModeDelegate(Func<bool> isDarkMode)
		=> s_IsDarkMode = isDarkMode;

	private static Func<byte[]?, PictureSize, object?>? s_LoadImage;
	private static Func<bool>? s_IsDarkMode;

	private static object? DefaultLoadImageImpl(byte[]? imageBytes, PictureSize size)
	{
		Serilog.Log.Error("{LoadImage} called without a delegate set. Picture size: {PictureSize}", nameof(LoadImage), size);
		return null;
	}

	/// <summary>Light is the safe assumption for hosts without a theme, like the CLI and tests.</summary>
	private static bool DefaultIsDarkModeImpl() => false;
}
