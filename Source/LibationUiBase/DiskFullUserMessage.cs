using LibationFileManager;
using System.Collections.Generic;
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

			Download and decrypt use temporary files under your "In progress" folder, then write finished audiobooks to your Books location. Those folders can be on different drives, and each needs enough free space for its role.

			Free disk space (or move Books / In progress to a larger drive in Settings), delete partial files under your In progress folder if needed, then retry your backups in smaller batches.
			""";

	public static string BuildPreflightBlockedBody(IReadOnlyList<DiskSpaceHelper.BackupDriveSpace> drives, int bookCount)
	{
		var sb = new StringBuilder();
		sb.AppendLine($"You are about to back up {bookCount} books, but a drive Libation uses does not have enough free space to continue safely.");
		sb.AppendLine();
		sb.AppendLine("Books is where finished audiobooks are stored. In progress holds temporary files while one title downloads and decrypts (about one book at a time, often on a different drive than Books).");
		sb.AppendLine();
		AppendDriveLines(sb, drives);
		sb.AppendLine();
		sb.Append("Free space or change the Books and In progress locations in Settings, then try again.");
		return sb.ToString();
	}

	public static string BuildPreflightWarningBody(IReadOnlyList<DiskSpaceHelper.BackupDriveSpace> drives, int bookCount)
	{
		var sb = new StringBuilder();
		sb.AppendLine($"You are about to back up {bookCount} books. Libation estimates you may need on the order of {FormatBytes(bookCount * DiskSpaceHelper.EstimatedBytesPerAudiobookBackup)} on your Books drive for the finished files, plus about {FormatBytes(DiskSpaceHelper.EstimatedBytesPerAudiobookBackup)} free on your In progress drive at a time for temporary download/decrypt files.");
		sb.AppendLine();
		sb.AppendLine("Books and In progress can be on different drives. A warning below for your In progress drive does not necessarily mean your Books location is full.");
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
			var free = drive.AvailableBytes is null ? "unknown" : FormatBytes(drive.AvailableBytes.Value);
			var needed = FormatBytes(drive.RequiredBytes);
			var role = DescribeDriveUsage(drive.Usage);
			sb.AppendLine($"{drive.DriveRoot}  ({role})  Free: {free}  Estimated needed: {needed}");
			foreach (var path in drive.Paths)
				sb.AppendLine($"  {path}");
		}
	}

	private static string DescribeDriveUsage(DiskSpaceHelper.BackupDriveUsage usage)
	{
		var hasBooks = usage.HasFlag(DiskSpaceHelper.BackupDriveUsage.Books);
		var hasInProgress = usage.HasFlag(DiskSpaceHelper.BackupDriveUsage.InProgress);

		if (hasBooks && hasInProgress)
			return "Books + In progress";

		if (hasBooks)
			return "Books";

		if (hasInProgress)
			return "In progress";

		return "Libation paths";
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
