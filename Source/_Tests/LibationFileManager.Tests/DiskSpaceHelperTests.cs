using LibationFileManager;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;

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
	[OSCondition(OperatingSystems.Windows)]
	public void Windows_NormalizePathForDriveQuery_strips_extended_prefix()
	{
		Assert.AreEqual(@"C:\Audiobooks\Books", DiskSpaceHelper.NormalizePathForDriveQuery(@"\\?\C:\Audiobooks\Books"));
		Assert.AreEqual(@"\\server\share\Books", DiskSpaceHelper.NormalizePathForDriveQuery(@"\\?\UNC\server\share\Books"));
	}

	[TestMethod]
	[OSCondition(OperatingSystems.Windows)]
	public void Windows_GetPathRootForDiskSpaceCheck_strips_extended_prefix()
	{
		Assert.AreEqual(@"C:\", DiskSpaceHelper.GetPathRootForDiskSpaceCheck(@"\\?\C:\Audiobooks\Books"));
	}

	[TestMethod]
	public void FindLongestMountPointPrefix_prefers_var_home_over_root()
	{
		var mounts = new[] { "/", "/boot", "/var/home", "/tmp" };

		Assert.AreEqual(
			"/var/home",
			DiskSpaceHelper.FindLongestMountPointPrefix("/var/home/user/Libation/Books", mounts));
		Assert.AreEqual(
			"/var/home",
			DiskSpaceHelper.FindLongestMountPointPrefix("/var/home", mounts));
		Assert.AreEqual(
			"/tmp",
			DiskSpaceHelper.FindLongestMountPointPrefix("/tmp/Libation-user", mounts));
		Assert.AreEqual(
			"/",
			DiskSpaceHelper.FindLongestMountPointPrefix("/usr/local/bin", mounts));
	}

	[TestMethod]
	public void FindLongestMountPointPrefix_does_not_match_partial_segment()
	{
		// "/var/home2/..." must not match mount "/var/home"
		var mounts = new[] { "/", "/var/home" };

		Assert.AreEqual(
			"/",
			DiskSpaceHelper.FindLongestMountPointPrefix("/var/home2/books", mounts));
	}

	[TestMethod]
	public void FindLongestMountPointPrefix_home_without_symlink_resolution_stays_on_root()
	{
		// Documents the Bazzite trap: /home/... does not prefix-match /var/home until symlinks are resolved.
		var mounts = new[] { "/", "/var/home", "/tmp" };

		Assert.AreEqual(
			"/",
			DiskSpaceHelper.FindLongestMountPointPrefix("/home/user/Libation/Books", mounts));
	}

	[TestMethod]
	public void ResolvePathSymlinks_follows_home_to_var_home_style_link()
	{
		if (OperatingSystem.IsWindows())
			Assert.Inconclusive("Skipped because OS is Windows.");

		var root = Path.Combine(Path.GetTempPath(), "libation-diskspace-" + Guid.NewGuid().ToString("N"));
		var varHome = Path.Combine(root, "var", "home");
		var homeLink = Path.Combine(root, "home");
		var booksUnderHome = Path.Combine(homeLink, "user", "Libation", "Books");

		try
		{
			Directory.CreateDirectory(varHome);
			Directory.CreateSymbolicLink(homeLink, varHome);

			var resolved = DiskSpaceHelper.ResolvePathSymlinks(booksUnderHome);
			var expected = Path.Combine(varHome, "user", "Libation", "Books");

			Assert.AreEqual(expected, resolved);

			var mounts = new[] { "/", varHome, Path.Combine(root, "tmp") };
			Assert.AreEqual(
				varHome,
				DiskSpaceHelper.FindLongestMountPointPrefix(resolved, mounts));
		}
		finally
		{
			try
			{
				if (Directory.Exists(root))
					Directory.Delete(root, recursive: true);
			}
			catch
			{
				// best-effort cleanup
			}
		}
	}

	[TestMethod]
	public void GetPathRootForDiskSpaceCheck_unix_uses_real_mount_not_always_slash()
	{
		if (OperatingSystem.IsWindows())
			Assert.Inconclusive("Skipped because OS is Windows.");

		// Pick any ready non-root mount that exists on this machine (often /tmp or similar).
		var nonRootMount = DriveInfo.GetDrives()
			.Where(d => d.IsReady && d.Name is not "/" and not null)
			.OrderByDescending(d => d.Name.Length)
			.FirstOrDefault();

		if (nonRootMount is null)
			Assert.Inconclusive("No non-root mounts available to assert against.");

		var probePath = Path.Combine(nonRootMount.Name.TrimEnd(Path.DirectorySeparatorChar), "libation-probe");
		var root = DiskSpaceHelper.GetPathRootForDiskSpaceCheck(probePath);

		Assert.AreEqual(nonRootMount.Name, root);
		Assert.AreNotEqual("/", root);
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
