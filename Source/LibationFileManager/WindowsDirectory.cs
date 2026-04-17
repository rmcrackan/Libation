using System;
using System.Threading;

namespace LibationFileManager;

public static class WindowsDirectory
{
	public static void SetCoverAsFolderIcon(string? pictureId, string directory, CancellationToken cancellationToken)
	{
		try
		{
			//Currently only works for Windows and macOS
			if (!Configuration.Instance.UseCoverAsFolderIcon)
				return;
			if (string.IsNullOrEmpty(pictureId))
			{
				Serilog.Log.Logger.Warning("No picture ID provided to set cover art as folder icon. {@DebugInfo}", new { directory });
				return;
			}

			// Load JPEG bytes from Images cache (or download). Prefer bytes → ICO so we never depend on a
			// path that might not exist when Amazon omits Content-Length or another downloader left a stale cache entry.
			var jpegBytes = PictureStorage.GetPictureSynchronously(new(pictureId, PictureSize._300x300), cancellationToken);
			if (jpegBytes.Length == 0)
			{
				Serilog.Log.Logger.Warning("Cover art unavailable for folder icon (empty image). {@DebugInfo}", new { directory, pictureId });
				return;
			}

			InteropFactory.Create().SetFolderIcon(imageJpegBytes: jpegBytes, directory: directory);
		}
		catch (Exception ex)
		{
			// Failure to 'set cover as folder icon' should not be considered a failure to download the book
			Serilog.Log.Logger.Error(ex, "Error setting cover art as folder icon. {@DebugInfo}", new { directory });

			try
			{
				InteropFactory.Create().DeleteFolderIcon(directory);
			}
			catch
			{
				Serilog.Log.Logger.Error(ex, "Error rolling back: setting cover art as folder icon. {@DebugInfo}", new { directory });
			}
		}
	}
}
