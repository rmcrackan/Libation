using LibationFileManager;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

namespace LibationFileManager.Tests;

[TestClass]
public class DiskSpaceHelperTests
{
	[TestMethod]
	public void IsDiskFullException_detects_message()
	{
		var ex = new IOException("There is not enough space on the disk. : 'C:\\temp\\x.aaxc'.");
		Assert.IsTrue(DiskSpaceHelper.IsDiskFullException(ex));
	}

	[TestMethod]
	public void IsDiskFullException_detects_message_in_aggregate()
	{
		var inner = new IOException("Failed to create file because the disk was full.");
		var ex = new AggregateException(inner);
		Assert.IsTrue(DiskSpaceHelper.IsDiskFullException(ex));
	}

	[TestMethod]
	public void ErrorMessageIndicatesDiskFull_matches_common_phrases()
	{
		Assert.IsTrue(DiskSpaceHelper.ErrorMessageIndicatesDiskFull("There is not enough space on the disk. : 'C:\\temp\\x.aaxc'."));
		Assert.IsTrue(DiskSpaceHelper.ErrorMessageIndicatesDiskFull("Write failed: disk quota has been exceeded."));
		Assert.IsTrue(DiskSpaceHelper.ErrorMessageIndicatesDiskFull("No space left on device"));
		Assert.IsFalse(DiskSpaceHelper.ErrorMessageIndicatesDiskFull("Unable to read beyond the end of the stream."));
	}

	[TestMethod]
	public void NormalizePathForDriveQuery_strips_extended_prefix()
	{
		Assert.AreEqual(@"C:\Audiobooks\Books", DiskSpaceHelper.NormalizePathForDriveQuery(@"\\?\C:\Audiobooks\Books"));
		Assert.AreEqual(@"\\server\share\Books", DiskSpaceHelper.NormalizePathForDriveQuery(@"\\?\UNC\server\share\Books"));
	}

	[TestMethod]
	public void GetPathRootForDiskSpaceCheck_strips_extended_prefix()
	{
		Assert.AreEqual(@"C:\", DiskSpaceHelper.GetPathRootForDiskSpaceCheck(@"\\?\C:\Audiobooks\Books"));
	}

	[TestMethod]
	public void GetRequiredBytesForDriveUsage_uses_batch_for_books_only()
	{
		const int bookCount = 500;
		var expected = bookCount * DiskSpaceHelper.EstimatedBytesPerAudiobookBackup;

		Assert.AreEqual(expected, DiskSpaceHelper.GetRequiredBytesForDriveUsage(DiskSpaceHelper.BackupDriveUsage.Books, bookCount));
		Assert.AreEqual(
			expected,
			DiskSpaceHelper.GetRequiredBytesForDriveUsage(DiskSpaceHelper.BackupDriveUsage.Books | DiskSpaceHelper.BackupDriveUsage.InProgress, bookCount));
	}

	[TestMethod]
	public void GetRequiredBytesForDriveUsage_uses_single_title_for_in_progress_only()
	{
		Assert.AreEqual(
			DiskSpaceHelper.EstimatedBytesPerAudiobookBackup,
			DiskSpaceHelper.GetRequiredBytesForDriveUsage(DiskSpaceHelper.BackupDriveUsage.InProgress, 500));
	}

	[TestMethod]
	public void GetCriticalFreeBytesForDriveUsage_in_progress_requires_one_title_plus_margin()
	{
		Assert.AreEqual(
			DiskSpaceHelper.EstimatedBytesPerAudiobookBackup + DiskSpaceHelper.InProgressPreflightMarginBytes,
			DiskSpaceHelper.GetCriticalFreeBytesForDriveUsage(DiskSpaceHelper.BackupDriveUsage.InProgress));

		Assert.AreEqual(
			DiskSpaceHelper.CriticalFreeBytes,
			DiskSpaceHelper.GetCriticalFreeBytesForDriveUsage(DiskSpaceHelper.BackupDriveUsage.Books));
	}

	[TestMethod]
	public void HasSufficientSpaceForBulkBackup_in_progress_not_judged_by_batch_estimate()
	{
		var drives = new[]
		{
			new DiskSpaceHelper.BackupDriveSpace(
				@"D:\",
				[@"D:\Books"],
				500L * 1024 * 1024 * 1024,
				500L * DiskSpaceHelper.EstimatedBytesPerAudiobookBackup,
				DiskSpaceHelper.BackupDriveUsage.Books),
			new DiskSpaceHelper.BackupDriveSpace(
				@"C:\",
				[@"C:\Temp\Libation-user"],
				600L * 1024 * 1024,
				DiskSpaceHelper.EstimatedBytesPerAudiobookBackup,
				DiskSpaceHelper.BackupDriveUsage.InProgress),
		};

		Assert.IsTrue(DiskSpaceHelper.HasSufficientSpaceForBulkBackup(drives));
	}

	[TestMethod]
	public void AnyDriveCriticallyLow_in_progress_uses_single_title_threshold()
	{
		var drives = new[]
		{
			new DiskSpaceHelper.BackupDriveSpace(
				@"C:\",
				[@"C:\Temp\Libation-user"],
				200L * 1024 * 1024,
				DiskSpaceHelper.EstimatedBytesPerAudiobookBackup,
				DiskSpaceHelper.BackupDriveUsage.InProgress),
		};

		Assert.IsTrue(DiskSpaceHelper.AnyDriveCriticallyLow(drives));
		Assert.IsFalse(DiskSpaceHelper.HasSufficientSpaceForBulkBackup(drives));
	}
}
