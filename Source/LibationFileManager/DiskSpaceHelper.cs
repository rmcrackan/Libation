using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LibationFileManager;

/// <summary>
/// Detects disk-full I/O failures and reports free space for Libation backup paths.
/// Preflight uses <see cref="DriveInfo"/> when available; runtime detection uses actual write failures
/// (works even when free space cannot be queried, e.g. some UNC/SMB shares).
/// </summary>
public static class DiskSpaceHelper
{
	/// <summary>Conservative per-title estimate (download + decrypt temp + final file) for bulk preflight.</summary>
	public const long EstimatedBytesPerAudiobookBackup = 400_000_000L;

	/// <summary>Below this free space on a relevant drive, bulk backup is blocked (no Continue).</summary>
	public const long CriticalFreeBytes = 100_000_000L;

	private const int HResultDiskFull = unchecked((int)0x80070070);

	public static bool IsDiskFullException(Exception? ex)
	{
		for (var current = ex; current is not null; current = current.InnerException)
		{
			if (current is IOException && current.HResult == HResultDiskFull)
				return true;

			if (ErrorMessageIndicatesDiskFull(current.Message))
				return true;
		}

		return false;
	}

	/// <summary>
	/// Matches Windows-style disk-full text from logs and StatusHandler errors.
	/// Does not cover quota-specific wording from some NAS/cloud providers; those fall through to normal retry UI.
	/// </summary>
	public static bool ErrorMessageIndicatesDiskFull(string? message)
	{
		if (string.IsNullOrWhiteSpace(message))
			return false;

		return message.Contains("not enough space on the disk", StringComparison.OrdinalIgnoreCase)
			|| message.Contains("disk was full", StringComparison.OrdinalIgnoreCase)
			|| message.Contains("there is not enough space on the disk", StringComparison.OrdinalIgnoreCase);
	}

	/// <summary>
	/// Returns free bytes for the volume containing <paramref name="path"/>, or null if unknown.
	/// Null means preflight cannot warn/block on that root (writable shares with no capacity API, offline drive, bad path).
	/// On Windows, <see cref="Path.GetPathRoot"/> yields drive letters (C:\) or UNC roots (\\server\share\).
	/// </summary>
	public static long? TryGetAvailableFreeBytes(string? path)
	{
		if (string.IsNullOrWhiteSpace(path))
			return null;

		try
		{
			var fullPath = Path.GetFullPath(path);
			var root = Path.GetPathRoot(fullPath);
			if (string.IsNullOrWhiteSpace(root))
				return null;

			var drive = new DriveInfo(root);
			// IsReady is false for disconnected network drives; AvailableFreeSpace may be wrong on some NAS reporting.
			return drive.IsReady ? drive.AvailableFreeSpace : null;
		}
		catch
		{
			// DriveInfo can throw for invalid roots; treat as unknown rather than failing backup setup.
			return null;
		}
	}

	public static IReadOnlyList<BackupDriveSpace> GetBackupDriveSpaces(Configuration config, int bookCount)
	{
		var requiredBytes = Math.Max(0, bookCount) * EstimatedBytesPerAudiobookBackup;
		var pathsByRoot = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

		void addPath(string? path)
		{
			if (string.IsNullOrWhiteSpace(path))
				return;

			string fullPath;
			try
			{
				fullPath = Path.GetFullPath(path);
			}
			catch
			{
				return;
			}

			var root = Path.GetPathRoot(fullPath);
			if (string.IsNullOrWhiteSpace(root))
				return;

			// Same physical share via Z: vs \\server\share appears as two roots; each gets the full estimate (conservative).
			if (!pathsByRoot.TryGetValue(root, out var list))
			{
				list = [];
				pathsByRoot[root] = list;
			}

			if (!list.Contains(fullPath, StringComparer.OrdinalIgnoreCase))
				list.Add(fullPath);
		}

		addPath(config.Books?.Path);
		addPath(config.InProgress);

		return pathsByRoot
			.Select(kvp => new BackupDriveSpace(
				kvp.Key,
				kvp.Value,
				TryGetAvailableFreeBytes(kvp.Key),
				requiredBytes))
			.ToList();
	}

	/// <summary>
	/// True when every root is unknown or has enough reported space. All-unknown => no preflight dialog.
	/// </summary>
	public static bool HasSufficientSpaceForBulkBackup(IReadOnlyList<BackupDriveSpace> drives)
		=> drives.All(d => d.AvailableBytes is null || d.AvailableBytes >= d.RequiredBytes);

	/// <summary>
	/// Only applies when free space was read successfully; unknown (null) never hard-blocks.
	/// </summary>
	public static bool AnyDriveCriticallyLow(IReadOnlyList<BackupDriveSpace> drives)
		=> drives.Any(d => d.AvailableBytes is not null && d.AvailableBytes < CriticalFreeBytes);

	public readonly record struct BackupDriveSpace(
		/// <summary>Path root from <see cref="Path.GetPathRoot"/> (e.g. C:\ or \\nas\library\).</summary>
		string DriveRoot,
		IReadOnlyList<string> Paths,
		/// <summary>Null when <see cref="TryGetAvailableFreeBytes"/> could not query this root.</summary>
		long? AvailableBytes,
		long RequiredBytes);
}
