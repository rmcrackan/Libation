using System;
using System.Collections.Generic;
using System.IO;

namespace LibationFileManager;

public partial class Configuration
{
	/// <summary>
	/// True when <paramref name="path"/> is a known-directory preset that is misleading under Flatpak
	/// (resolves inside the sandbox instead of the host filesystem).
	/// </summary>
	public static bool IsMisleadingFlatpakPresetPath(string? path)
		=> IsRunningUnderFlatpak && IsMisleadingFlatpakPresetPath(path, isFlatpak: true);

	internal static bool IsMisleadingFlatpakPresetPath(string? path, bool isFlatpak)
	{
		if (!isFlatpak || string.IsNullOrWhiteSpace(path))
			return false;

		try
		{
			var fullPath = Path.GetFullPath(path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));

			if (PathsEqual(fullPath, AppDir_Absolute))
				return true;

			if (PathsEqual(fullPath, AppDir_Relative) || path == AppDir_Relative)
				return true;

			var known = GetKnownDirectory(fullPath);
			if (known != KnownDirectories.None && known != KnownDirectories.LibationFiles)
				return ShouldExcludeKnownDirectoryUnderFlatpak(known, KnownDirectoryUsage.BooksLocation);

			foreach (var preset in GetFlatpakMisleadingPresetRoots())
			{
				if (PathsEqual(fullPath, preset))
					return true;
				if (PathsEqual(fullPath, Path.Combine(preset, "Books")))
					return true;
			}
		}
		catch
		{
			return false;
		}

		return false;
	}

	/// <summary>
	/// Under Flatpak, rewrites misleading preset paths to persisted defaults.
	/// </summary>
	public static string NormalizeFlatpakLibationFilesPath(string path)
		=> NormalizeFlatpakLibationFilesPath(path, IsRunningUnderFlatpak);

	internal static string NormalizeFlatpakLibationFilesPath(string path, bool isFlatpak)
		=> isFlatpak && IsMisleadingFlatpakPresetPath(path, isFlatpak: true)
			? LibationFiles.DefaultLibationFilesDirectory
			: path;

	/// <summary>
	/// Under Flatpak, rewrites misleading Books paths to a folder under Libation Files.
	/// </summary>
	public static string NormalizeFlatpakBooksPath(string? booksPath, string libationFilesLocation)
		=> NormalizeFlatpakBooksPath(booksPath, libationFilesLocation, IsRunningUnderFlatpak);

	internal static string NormalizeFlatpakBooksPath(string? booksPath, string libationFilesLocation, bool isFlatpak)
	{
		if (!isFlatpak || string.IsNullOrWhiteSpace(booksPath))
			return booksPath ?? "";

		if (!IsMisleadingFlatpakPresetPath(booksPath, isFlatpak: true))
			return booksPath;

		return Path.Combine(libationFilesLocation, nameof(Books));
	}

	internal static IEnumerable<string> GetFlatpakMisleadingPresetRoots()
	{
		yield return UserProfile;
		yield return MyDocs;
		yield return MyMusic;
	}

	internal static bool PathsEqual(string a, string b)
	{
		try
		{
			return string.Equals(
				Path.GetFullPath(a.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)),
				Path.GetFullPath(b.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)),
				StringComparison.OrdinalIgnoreCase);
		}
		catch
		{
			return false;
		}
	}
}
