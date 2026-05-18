using LibationFileManager;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibationUiBase;

/// <summary>
/// User-facing copy when backups fail or are blocked due to insufficient disk space.
/// </summary>
public static class DiskFullUserMessage
{
	public const string DialogCaption = "Not enough disk space";

	public static string BuildQueueStoppedBody()
		=> """
			Libation stopped the backup queue because the disk ran out of free space.

			Download and decrypt use temporary files under your "In progress" folder, then write finished audiobooks to your Books location. Both need enough free space on their drives.

			Free disk space (or move Books / In progress to a larger drive in Settings), delete partial files under your In progress folder if needed, then retry your backups in smaller batches.
			""";

	public static string BuildPreflightBlockedBody(IReadOnlyList<DiskSpaceHelper.BackupDriveSpace> drives, int bookCount)
	{
		var sb = new StringBuilder();
		sb.AppendLine($"You are about to back up {bookCount} books, but a drive Libation uses does not have enough free space to continue safely.");
		sb.AppendLine();
		AppendDriveLines(sb, drives);
		sb.AppendLine();
		sb.Append("Free space or change the Books and In progress locations in Settings, then try again.");
		return sb.ToString();
	}

	public static string BuildPreflightWarningBody(IReadOnlyList<DiskSpaceHelper.BackupDriveSpace> drives, int bookCount)
	{
		var sb = new StringBuilder();
		sb.AppendLine($"You are about to back up {bookCount} books. Libation estimates you may need on the order of {FormatBytes(bookCount * DiskSpaceHelper.EstimatedBytesPerAudiobookBackup)} total, plus extra room for temporary files during each download.");
		sb.AppendLine();
		AppendDriveLines(sb, drives);
		sb.AppendLine();
		sb.Append("Continue anyway?");
		return sb.ToString();
	}

	private static void AppendDriveLines(StringBuilder sb, IReadOnlyList<DiskSpaceHelper.BackupDriveSpace> drives)
	{
		foreach (var drive in drives)
		{
			// "unknown" when DriveInfo could not report space (typical for some network paths); user may still have chosen Continue.
			var free = drive.AvailableBytes is null ? "unknown" : FormatBytes(drive.AvailableBytes.Value);
			var needed = FormatBytes(drive.RequiredBytes);
			sb.AppendLine($"{drive.DriveRoot}  Free: {free}  Estimated needed: {needed}");
			foreach (var path in drive.Paths)
				sb.AppendLine($"  {path}");
		}
	}

	private static string FormatBytes(long bytes)
	{
		const long gb = 1024L * 1024 * 1024;
		if (bytes >= gb)
			return $"{bytes / (double)gb:F1} GB";
		const long mb = 1024 * 1024;
		return $"{bytes / (double)mb:F0} MB";
	}
}
