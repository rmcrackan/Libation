using LibationFileManager;
using System.Collections.Generic;
using System.Text;
using LibationUiBase.Resources;

namespace LibationUiBase;

/// <summary>
/// User-facing copy when backups fail or are blocked due to insufficient disk space.
/// </summary>
public static class DiskFullUserMessage
{
	public static string DialogCaption => Strings.DiskFullUserMessage_NotEnoughDiskSpace;

	public static string BuildQueueStoppedBody()
		=> Strings.DiskFullUserMessage_LibationStoppedTheBackupQueueBecause;

	public static string BuildPreflightBlockedBody(IReadOnlyList<DiskSpaceHelper.BackupDriveSpace> drives, int bookCount)
	{
		var sb = new StringBuilder();
		sb.AppendLine(string.Format(Strings.DiskFullUserMessage_YouAreAboutToBackUp, bookCount));
		sb.AppendLine();
		sb.AppendLine(Strings.DiskFullUserMessage_BooksIsWhereFinishedAudiobooksAre);
		sb.AppendLine();
		AppendDriveLines(sb, drives);
		sb.AppendLine();
		sb.Append(Strings.DiskFullUserMessage_FreeSpaceOrChangeTheBooks);
		return sb.ToString();
	}

	public static string BuildPreflightWarningBody(IReadOnlyList<DiskSpaceHelper.BackupDriveSpace> drives, int bookCount)
	{
		var sb = new StringBuilder();
		sb.AppendLine(string.Format(Strings.DiskFullUserMessage_YouAreAboutToBackUp2, bookCount, FormatBytes(bookCount * DiskSpaceHelper.EstimatedBytesPerAudiobookBackup), FormatBytes(DiskSpaceHelper.EstimatedBytesPerAudiobookBackup)));
		sb.AppendLine();
		sb.AppendLine(Strings.DiskFullUserMessage_BooksAndInProgressCanBe);
		sb.AppendLine();
		AppendDriveLines(sb, drives);
		sb.AppendLine();
		sb.Append(Strings.DiskFullUserMessage_ContinueAnyway);
		return sb.ToString();
	}

	private static void AppendDriveLines(StringBuilder sb, IReadOnlyList<DiskSpaceHelper.BackupDriveSpace> drives)
	{
		foreach (var drive in drives)
		{
			var free = drive.AvailableBytes is null ? Strings.DiskFullUserMessage_Unknown : FormatBytes(drive.AvailableBytes.Value);
			var needed = FormatBytes(drive.RequiredBytes);
			var role = DescribeDriveUsage(drive.Usage);
			sb.AppendLine(string.Format(Strings.DiskFullUserMessage_FreeEstimatedNeeded, drive.DriveRoot, role, free, needed));
			foreach (var path in drive.Paths)
				sb.AppendLine($"  {path}");
		}
	}

	private static string DescribeDriveUsage(DiskSpaceHelper.BackupDriveUsage usage)
	{
		var hasBooks = usage.HasFlag(DiskSpaceHelper.BackupDriveUsage.Books);
		var hasInProgress = usage.HasFlag(DiskSpaceHelper.BackupDriveUsage.InProgress);

		if (hasBooks && hasInProgress)
			return Strings.DiskFullUserMessage_BooksInProgress;

		if (hasBooks)
			return Strings.DiskFullUserMessage_Books;

		if (hasInProgress)
			return Strings.DiskFullUserMessage_InProgress;

		return Strings.DiskFullUserMessage_LibationPaths;
	}

	private static string FormatBytes(long bytes) => DiskSpaceHelper.FormatBytes(bytes);
}
