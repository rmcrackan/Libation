using System;
using System.IO;

namespace LibationFileManager;

/// <summary>
/// Detects whether a path sits inside a cloud sync root. Sync clients dehydrate files into
/// placeholders, restore old copies, and leave conflict copies behind, which breaks an install
/// folder the upgrader rewrites in place and corrupts the search index.
/// </summary>
public static class CloudSyncedFolders
{
	private static readonly string[] SyncRootVariables =
	[
		"OneDrive",
		"OneDriveConsumer",
		"OneDriveCommercial",
	];

	/// <summary>The sync root containing <paramref name="path"/>, or null when it is not inside one.</summary>
	public static string? FindSyncRootContaining(string? path)
	{
		if (!TryGetFullPath(path, out var fullPath))
			return null;

		foreach (var variable in SyncRootVariables)
		{
			if (!TryGetFullPath(Environment.GetEnvironmentVariable(variable), out var syncRoot))
				continue;

			if (IsSameOrUnder(fullPath, syncRoot))
				return syncRoot;
		}

		return null;
	}

	public static bool IsInsideSyncRoot(string? path) => FindSyncRootContaining(path) is not null;

	private static bool TryGetFullPath(string? path, out string fullPath)
	{
		fullPath = string.Empty;
		if (string.IsNullOrWhiteSpace(path))
			return false;

		try
		{
			fullPath = Path.TrimEndingDirectorySeparator(Path.GetFullPath(path));
			return fullPath.Length > 0;
		}
		catch
		{
			// Unparseable environment values are simply not sync roots.
			return false;
		}
	}

	private static bool IsSameOrUnder(string path, string root)
	{
		var comparison = OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

		if (!path.StartsWith(root, comparison))
			return false;

		// Without this, C:\OneDriveBackup would count as being inside C:\OneDrive.
		return path.Length == root.Length
			|| path[root.Length] == Path.DirectorySeparatorChar
			|| path[root.Length] == Path.AltDirectorySeparatorChar;
	}
}
