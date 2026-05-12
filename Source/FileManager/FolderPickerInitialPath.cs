using System;
using System.IO;
using System.Runtime.InteropServices;

namespace FileManager;

/// <summary>
/// Normalizes stored paths for OS folder picker APIs that are stricter than <see cref="Directory"/> (for example WinForms <c>FolderBrowserDialog</c> / shell <c>SHCreateItemFromParsingName</c>).
/// </summary>
public static class FolderPickerInitialPath
{
	private const string WinLongPathPrefix = @"\\?\";
	private const string WinLongUncPrefix = @"\\?\UNC\";

	/// <summary>
	/// Returns a directory path suitable as a folder picker's starting location, or <c>null</c> to let the OS use its default.
	/// Verifies the directory exists using the same path rules as <see cref="LongPath"/> before returning a shell-oriented string.
	/// </summary>
	public static string? GetExistingDirectoryOrNull(string? path)
	{
		if (string.IsNullOrWhiteSpace(path))
			return null;

		path = path.Trim();
		try
		{
			LongPath longPath = path;
			if (!Directory.Exists(longPath))
				return null;

			var forPicker = ToOsFolderPickerPath(longPath.Path);
			if (string.IsNullOrWhiteSpace(forPicker))
				return null;

			if (!Directory.Exists(forPicker))
				return null;

			try
			{
				var full = Path.GetFullPath(forPicker);
				if (Directory.Exists(full))
					return full;
			}
			catch (ArgumentException)
			{
				// Path.GetFullPath rejects some edge cases; fall through
			}
			catch (NotSupportedException)
			{
			}
			catch (PathTooLongException)
			{
			}

			return forPicker;
		}
		catch (ArgumentException)
		{
			return null;
		}
		catch (NotSupportedException)
		{
			return null;
		}
		catch (PathTooLongException)
		{
			return null;
		}
	}

	private static string? ToOsFolderPickerPath(string absolutePathFromLongPath)
	{
		if (string.IsNullOrWhiteSpace(absolutePathFromLongPath))
			return null;

		if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			return absolutePathFromLongPath;

		if (absolutePathFromLongPath.StartsWith(WinLongUncPrefix, StringComparison.OrdinalIgnoreCase))
			return @"\\" + absolutePathFromLongPath.Substring(WinLongUncPrefix.Length);

		if (absolutePathFromLongPath.StartsWith(WinLongPathPrefix, StringComparison.Ordinal))
			return absolutePathFromLongPath.Substring(WinLongPathPrefix.Length);

		return absolutePathFromLongPath;
	}
}
