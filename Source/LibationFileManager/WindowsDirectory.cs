using System;
using System.Threading;

namespace LibationFileManager;

public static class WindowsDirectory
{
	const int FolderIconMaxAttempts = 5;

	public static void SetCoverAsFolderIcon(string? pictureId, string directory, CancellationToken cancellationToken)
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

        for (var attempt = 1; attempt <= FolderIconMaxAttempts; attempt++)
		{
			cancellationToken.ThrowIfCancellationRequested();

			try
			{
				var jpegBytes = PictureStorage.GetPictureSynchronously(new(pictureId, PictureSize._300x300), cancellationToken);
				if (jpegBytes.Length == 0)
				{
					if (attempt < FolderIconMaxAttempts)
					{
						Serilog.Log.Logger.Debug("Folder icon: empty 300x300 image on attempt {Attempt}/{Max}; retrying after delay. {@DebugInfo}", attempt, FolderIconMaxAttempts, new { directory, pictureId });
						DelayBetweenFolderIconRetries(cancellationToken, attempt);
						continue;
					}

					Serilog.Log.Logger.Warning(
						"Could not set Explorer folder icon after {MaxAttempts} attempts: the 300x300 cover image never became available (empty or missing). The audiobook download itself is unaffected. Check your network to Amazon images, disk space under Libation's Images folder, or try liberating again. {@DebugInfo}",
						FolderIconMaxAttempts, new { directory, pictureId });
					TryDeleteFolderIcon(directory);
					return;
				}

				InteropFactory.Create().SetFolderIcon(imageJpegBytes: jpegBytes, directory: directory);
				return;
			}
			catch (Exception ex)
			{
				if (attempt < FolderIconMaxAttempts)
				{
					Serilog.Log.Logger.Debug(ex, "Folder icon: attempt {Attempt}/{Max} failed; retrying after delay. {@DebugInfo}", attempt, FolderIconMaxAttempts, new { directory, pictureId });
					DelayBetweenFolderIconRetries(cancellationToken, attempt);
					continue;
				}

				Serilog.Log.Logger.Error(ex,
					"Could not set Explorer folder icon after {MaxAttempts} attempts (decode, ICO conversion, or writing desktop.ini/Icon.ico failed). The audiobook download itself should still be fine; try liberating again, or check folder permissions if the library is on removable media. {@DebugInfo}",
					FolderIconMaxAttempts, new { directory, pictureId });
				TryDeleteFolderIcon(directory);
				return;
			}
		}
	}

	static void DelayBetweenFolderIconRetries(CancellationToken cancellationToken, int attemptAfterFailure)
	{
		// 100, 200, 400, 800 ms; bounded backoff without Task.Delay allocation on hot path
		var ms = 100 * (1 << (attemptAfterFailure - 1));
		if (ms > 0)
			cancellationToken.WaitHandle.WaitOne(ms);
	}

	static void TryDeleteFolderIcon(string directory)
	{
		try
		{
			InteropFactory.Create().DeleteFolderIcon(directory);
		}
		catch (Exception ex)
		{
			Serilog.Log.Logger.Error(ex, "Error rolling back folder icon files. {@DebugInfo}", new { directory });
		}
	}
}
