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

	/// <summary>Single byte formatter for user-facing copy, so free space and download limits read the same way.</summary>
	public static string FormatBytes(long bytes)
	{
		const long gb = 1024L * 1024 * 1024;
		if (bytes >= gb)
			return $"{bytes / (double)gb:F1} GB";
		const long mb = 1024 * 1024;
		return $"{bytes / (double)mb:F0} MB";
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
	/// Returns the volume root used for free-space queries and drive grouping
	/// (e.g. <c>C:\</c>, <c>\\server\share\</c>, or a Unix mount such as <c>/var/home</c>), or null if unknown.
	/// On Unix, <see cref="Path.GetPathRoot"/> is not used: it always returns <c>/</c> for absolute paths and
	/// mis-attributes free space when Books/In progress live on another mount (e.g. Bazzite <c>/var/home</c>).
	/// </summary>
	public static string? GetPathRootForDiskSpaceCheck(string? path)
	{
		if (string.IsNullOrWhiteSpace(path))
			return null;

		try
		{
			var normalized = NormalizePathForDriveQuery(path);
			var fullPath = Path.GetFullPath(normalized);

			if (OperatingSystem.IsWindows())
			{
				var root = Path.GetPathRoot(fullPath);
				return string.IsNullOrWhiteSpace(root) ? null : root;
			}

			return GetUnixMountPoint(fullPath);
		}
		catch
		{
			return null;
		}
	}

	/// <summary>
	/// Resolves symlinks in <paramref name="path"/> component-by-component so that e.g.
	/// <c>/home/user/Books</c> becomes <c>/var/home/user/Books</c> when <c>/home</c> → <c>/var/home</c>.
	/// Non-existent trailing segments are kept so a not-yet-created Books folder still resolves.
	/// </summary>
	public static string ResolvePathSymlinks(string path)
	{
		var fullPath = Path.GetFullPath(path);
		if (OperatingSystem.IsWindows())
			return fullPath;

		var parts = fullPath.Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries);
		var resolved = "/";

		foreach (var part in parts)
		{
			var next = resolved == "/"
				? "/" + part
				: resolved + Path.DirectorySeparatorChar + part;

			try
			{
				var dirInfo = new DirectoryInfo(next);
				if (dirInfo.Exists)
				{
					var target = dirInfo.ResolveLinkTarget(returnFinalTarget: true);
					resolved = target?.FullName ?? dirInfo.FullName;
					continue;
				}

				var fileInfo = new FileInfo(next);
				if (fileInfo.Exists)
				{
					var target = fileInfo.ResolveLinkTarget(returnFinalTarget: true);
					resolved = target?.FullName ?? fileInfo.FullName;
					continue;
				}
			}
			catch
			{
				// Keep syntactic path when link resolution fails for a segment.
			}

			resolved = next;
		}

		return resolved;
	}

	/// <summary>
	/// Pure helper: longest mount-point prefix of <paramref name="fullPath"/> from <paramref name="mountPoints"/>.
	/// Used for Unix volume identity and unit tests (injectable mount list).
	/// </summary>
	public static string? FindLongestMountPointPrefix(string fullPath, IEnumerable<string> mountPoints)
	{
		if (string.IsNullOrWhiteSpace(fullPath))
			return null;

		string? best = null;

		foreach (var mount in mountPoints)
		{
			if (string.IsNullOrWhiteSpace(mount))
				continue;

			if (!IsPathOnMount(fullPath, mount))
				continue;

			if (best is null || mount.Length > best.Length)
				best = mount;
		}

		return best;
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
		var pathComparer = OperatingSystem.IsWindows()
			? StringComparer.OrdinalIgnoreCase
			: StringComparer.Ordinal;
		var pathsByRoot = new Dictionary<string, (List<string> paths, BackupDriveUsage usage)>(pathComparer);

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

			var root = GetPathRootForDiskSpaceCheck(fullPath);
			if (string.IsNullOrWhiteSpace(root))
				return;

			if (!pathsByRoot.TryGetValue(root, out var entry))
				entry = ([], usageFlag);
			else
				entry.usage |= usageFlag;

			if (!entry.paths.Contains(fullPath, pathComparer))
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
		/// <summary>Volume root used for free-space display (e.g. C:\ , \\nas\library\ , or /var/home).</summary>
		string DriveRoot,
		IReadOnlyList<string> Paths,
		/// <summary>Null when <see cref="TryGetAvailableFreeBytes"/> could not query this root.</summary>
		long? AvailableBytes,
		long RequiredBytes,
		BackupDriveUsage Usage);

	private static string? GetUnixMountPoint(string fullPath)
	{
		var resolved = ResolvePathSymlinks(fullPath);
		var mounts = DriveInfo.GetDrives()
			.Where(static d => d.IsReady)
			.Select(static d => d.Name);

		return FindLongestMountPointPrefix(resolved, mounts)
			?? Path.GetPathRoot(resolved);
	}

	/// <summary>
	/// Unix mount paths always use '/'. Do not use <see cref="Path.DirectorySeparatorChar"/> —
	/// on Windows that is '\', which would break pure unit tests and any cross-OS path handling.
	/// </summary>
	private static bool IsPathOnMount(string fullPath, string mount)
	{
		const char unixSep = '/';

		var mountTrimmed = mount.TrimEnd(unixSep);
		if (mountTrimmed.Length == 0)
			mountTrimmed = "/";

		if (fullPath.Equals(mountTrimmed, StringComparison.Ordinal))
			return true;

		// Root mount "/" prefixes every absolute Unix path.
		if (mountTrimmed == "/")
			return fullPath.StartsWith("/", StringComparison.Ordinal);

		var prefix = mount.EndsWith(unixSep) ? mount : mount + unixSep;

		return fullPath.StartsWith(prefix, StringComparison.Ordinal);
	}
}
