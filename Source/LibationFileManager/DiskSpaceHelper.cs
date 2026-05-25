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

	/// <summary>Below this free space on a Books drive, bulk backup is blocked (no Continue).</summary>
	public const long CriticalFreeBytes = 100_000_000L;

	/// <summary>Extra headroom required on the In progress drive beyond one active title.</summary>
	public const long InProgressPreflightMarginBytes = 50_000_000L;

	private const int HResultDiskFull = unchecked((int)0x80070070);
	private const string WinLongPathPrefix = @"\\?\";

	[Flags]
	public enum BackupDriveUsage
	{
		InProgress = 1,
		Books = 2,
	}

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
	/// Matches disk-full and common quota-exceeded text from logs and StatusHandler errors.
	/// </summary>
	public static bool ErrorMessageIndicatesDiskFull(string? message)
	{
		if (string.IsNullOrWhiteSpace(message))
			return false;

		return message.Contains("not enough space on the disk", StringComparison.OrdinalIgnoreCase)
			|| message.Contains("disk was full", StringComparison.OrdinalIgnoreCase)
			|| message.Contains("there is not enough space on the disk", StringComparison.OrdinalIgnoreCase)
			|| message.Contains("no space left on device", StringComparison.OrdinalIgnoreCase)
			|| message.Contains("disk quota", StringComparison.OrdinalIgnoreCase)
			|| message.Contains("quota exceeded", StringComparison.OrdinalIgnoreCase)
			|| message.Contains("storage quota", StringComparison.OrdinalIgnoreCase);
	}

	/// <summary>
	/// Strips the Win32 extended-length prefix so <see cref="DriveInfo"/> and path APIs see a normal root.
	/// No-op on non-Windows platforms (Libation only uses the prefix there).
	/// </summary>
	public static string NormalizePathForDriveQuery(string path)
	{
		if (!OperatingSystem.IsWindows() || !path.StartsWith(WinLongPathPrefix, StringComparison.Ordinal))
			return path;

		var stripped = path[WinLongPathPrefix.Length..];
		if (stripped.StartsWith(@"UNC\", StringComparison.OrdinalIgnoreCase))
			return @"\\" + stripped[4..];

		return stripped;
	}

	/// <summary>
	/// Returns the volume root used for free-space queries (e.g. C:\ or \\server\share\), or null if unknown.
	/// </summary>
	public static string? GetPathRootForDiskSpaceCheck(string? path)
	{
		if (string.IsNullOrWhiteSpace(path))
			return null;

		try
		{
			var normalized = NormalizePathForDriveQuery(path);
			var fullPath = Path.GetFullPath(normalized);
			var root = Path.GetPathRoot(fullPath);
			return string.IsNullOrWhiteSpace(root) ? null : root;
		}
		catch
		{
			return null;
		}
	}

	/// <summary>
	/// Returns free bytes for the volume containing <paramref name="path"/>, or null if unknown.
	/// Null means preflight cannot warn/block on that root (writable shares with no capacity API, offline drive, bad path).
	/// </summary>
	public static long? TryGetAvailableFreeBytes(string? path)
	{
		var root = GetPathRootForDiskSpaceCheck(path);
		if (root is null)
			return null;

		try
		{
			var drive = new DriveInfo(root);
			return drive.IsReady ? drive.AvailableFreeSpace : null;
		}
		catch
		{
			return null;
		}
	}

	public static long GetRequiredBytesForDriveUsage(BackupDriveUsage usage, int bookCount)
	{
		var hasBooks = usage.HasFlag(BackupDriveUsage.Books);
		var hasInProgress = usage.HasFlag(BackupDriveUsage.InProgress);

		if (hasBooks)
			return Math.Max(0, bookCount) * EstimatedBytesPerAudiobookBackup;

		if (hasInProgress)
			return EstimatedBytesPerAudiobookBackup;

		return 0;
	}

	public static long GetCriticalFreeBytesForDriveUsage(BackupDriveUsage usage)
	{
		if (usage.HasFlag(BackupDriveUsage.Books))
			return CriticalFreeBytes;

		if (usage.HasFlag(BackupDriveUsage.InProgress))
			return EstimatedBytesPerAudiobookBackup + InProgressPreflightMarginBytes;

		return CriticalFreeBytes;
	}

	public static IReadOnlyList<BackupDriveSpace> GetBackupDriveSpaces(Configuration config, int bookCount)
	{
		var pathsByRoot = new Dictionary<string, (List<string> paths, BackupDriveUsage usage)>(StringComparer.OrdinalIgnoreCase);

		void addPath(string? path, BackupDriveUsage usageFlag)
		{
			if (string.IsNullOrWhiteSpace(path))
				return;

			string fullPath;
			try
			{
				fullPath = Path.GetFullPath(NormalizePathForDriveQuery(path));
			}
			catch
			{
				return;
			}

			var root = Path.GetPathRoot(fullPath);
			if (string.IsNullOrWhiteSpace(root))
				return;

			if (!pathsByRoot.TryGetValue(root, out var entry))
				entry = ([], usageFlag);
			else
				entry.usage |= usageFlag;

			if (!entry.paths.Contains(fullPath, StringComparer.OrdinalIgnoreCase))
				entry.paths.Add(fullPath);

			pathsByRoot[root] = entry;
		}

		addPath(config.Books?.Path, BackupDriveUsage.Books);
		addPath(config.InProgress, BackupDriveUsage.InProgress);

		return pathsByRoot
			.Select(kvp =>
			{
				var usage = kvp.Value.usage;
				var required = GetRequiredBytesForDriveUsage(usage, bookCount);
				return new BackupDriveSpace(
					kvp.Key,
					kvp.Value.paths,
					TryGetAvailableFreeBytes(kvp.Key),
					required,
					usage);
			})
			.ToList();
	}

	/// <summary>
	/// True when every root is unknown or has enough reported space for its role. All-unknown => no preflight dialog.
	/// </summary>
	public static bool HasSufficientSpaceForBulkBackup(IReadOnlyList<BackupDriveSpace> drives)
		=> drives.All(d => d.AvailableBytes is null || d.AvailableBytes >= d.RequiredBytes);

	/// <summary>
	/// Only applies when free space was read successfully; unknown (null) never hard-blocks.
	/// </summary>
	public static bool AnyDriveCriticallyLow(IReadOnlyList<BackupDriveSpace> drives)
		=> drives.Any(d => d.AvailableBytes is not null && d.AvailableBytes < GetCriticalFreeBytesForDriveUsage(d.Usage));

	public readonly record struct BackupDriveSpace(
		/// <summary>Volume root used for free-space display (e.g. C:\ or \\nas\library\).</summary>
		string DriveRoot,
		IReadOnlyList<string> Paths,
		/// <summary>Null when <see cref="TryGetAvailableFreeBytes"/> could not query this root.</summary>
		long? AvailableBytes,
		long RequiredBytes,
		BackupDriveUsage Usage);
}
