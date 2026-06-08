using LibationFileManager;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;

namespace WindowsConfigApp;

/// <summary>
/// Keeps Add/Remove Programs metadata in sync after in-app zip upgrades (Inno only writes these at install time).
/// </summary>
internal static class WindowsUninstallRegistrySync
{
	private const string UninstallRegistryPath = @"Software\Microsoft\Windows\CurrentVersion\Uninstall";

	public static void TrySync(string? processPath = null)
	{
		try
		{
			processPath ??= Environment.ProcessPath;
			if (string.IsNullOrWhiteSpace(processPath))
				return;

			var installDir = Path.GetDirectoryName(processPath);
			if (string.IsNullOrWhiteSpace(installDir))
				return;

			var displayVersion = getDisplayVersion(processPath);
			var displayName = getDisplayName(installDir);
			if (displayVersion is null && displayName is null)
				return;

			using var uninstallKey = Registry.CurrentUser.OpenSubKey(UninstallRegistryPath, writable: false);
			if (uninstallKey is null)
				return;

			foreach (var subKeyName in uninstallKey.GetSubKeyNames())
			{
				using var subKey = uninstallKey.OpenSubKey(subKeyName);
				var installLocation = subKey?.GetValue("InstallLocation") as string;
				if (!pathsEqual(installLocation, installDir))
					continue;

				using var writable = uninstallKey.OpenSubKey(subKeyName, writable: true);
				if (writable is null)
					return;

				var changed = false;

				if (displayVersion is not null)
				{
					var currentVersion = writable.GetValue("DisplayVersion") as string;
					if (!string.Equals(currentVersion, displayVersion, StringComparison.OrdinalIgnoreCase))
					{
						writable.SetValue("DisplayVersion", displayVersion);
						changed = true;
					}
				}

				if (displayName is not null)
				{
					var currentName = writable.GetValue("DisplayName") as string;
					if (!string.Equals(currentName, displayName, StringComparison.Ordinal))
					{
						writable.SetValue("DisplayName", displayName);
						changed = true;
					}
				}

				if (changed)
					Serilog.Log.Logger.Information(
						"Synced Windows Add/Remove Programs entry: {DisplayName} {DisplayVersion}",
						displayName ?? writable.GetValue("DisplayName"),
						displayVersion ?? writable.GetValue("DisplayVersion"));

				return;
			}
		}
		catch (Exception ex)
		{
			Serilog.Log.Logger.Warning(ex, "Could not sync Windows Add/Remove Programs version");
		}
	}

	private static string? getDisplayVersion(string processPath)
	{
		var fileVersion = FileVersionInfo.GetVersionInfo(processPath);
		var raw = fileVersion.ProductVersion ?? fileVersion.FileVersion;
		if (string.IsNullOrWhiteSpace(raw))
			return null;

		raw = raw.Split('+')[0].Split('-')[0].Trim();
		return Version.TryParse(raw, out var version) ? version.ToVersionString() : null;
	}

	private static string? getDisplayName(string installDir)
	{
		var folder = Path.GetFileName(installDir.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
		return folder switch
		{
			"Libation-Classic" => "Libation (Classic)",
			"Libation" => "Libation (Chardonnay)",
			_ => null,
		};
	}

	private static bool pathsEqual(string? registryPath, string installDir)
	{
		if (string.IsNullOrWhiteSpace(registryPath))
			return false;

		try
		{
			var normalizedRegistry = Path.GetFullPath(registryPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
			var normalizedInstall = Path.GetFullPath(installDir.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
			return string.Equals(normalizedRegistry, normalizedInstall, StringComparison.OrdinalIgnoreCase);
		}
		catch
		{
			return false;
		}
	}
}
