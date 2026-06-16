using System;
using System.IO;
using System.Runtime.InteropServices;

namespace LibationFileManager;

/// <summary>
/// Removes Mark-of-the-Web (Zone.Identifier) from install-folder binaries on Windows.
/// Helps after manual downloads and in-app upgrades before Application Control evaluates files.
/// </summary>
public static class InstallFolderUnblock
{
	private static readonly string[] UnblockExtensions = [".dll", ".exe"];

	public static void TryUnblockProcessDirectoryIfWindows()
	{
		if (!OperatingSystem.IsWindows())
			return;

		try
		{
			_ = UnblockDirectory(Configuration.ProcessDirectory);
		}
		catch
		{
			// Best-effort before logging may be available.
		}
	}

	public static int UnblockDirectory(string directory)
	{
		if (!OperatingSystem.IsWindows() || string.IsNullOrEmpty(directory) || !Directory.Exists(directory))
			return 0;

		var unblocked = 0;
		if (TryUnblockFile(directory))
			unblocked++;

		foreach (var file in Directory.EnumerateFiles(directory))
		{
			if (!ShouldUnblock(file))
				continue;

			if (TryUnblockFile(file))
				unblocked++;
		}

		return unblocked;
	}

	private static bool ShouldUnblock(string path)
	{
		var extension = Path.GetExtension(path);
		foreach (var candidate in UnblockExtensions)
		{
			if (extension.Equals(candidate, StringComparison.OrdinalIgnoreCase))
				return true;
		}

		return false;
	}

	private static bool TryUnblockFile(string path)
	{
		var zoneIdentifier = path + ":Zone.Identifier";
		if (!DeleteFile(zoneIdentifier))
		{
			var error = Marshal.GetLastWin32Error();
			// File or stream does not exist.
			if (error is 2 or 3)
				return false;

			throw new IOException($"Could not remove Zone.Identifier from '{path}'. Win32 error {error}.");
		}

		return true;
	}

	[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool DeleteFile(string lpFileName);
}
