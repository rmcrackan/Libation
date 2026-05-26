using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;

namespace LibationFileManager;

public partial class Configuration
{
	public static string ProcessDirectory { get; } = Path.GetDirectoryName(Environment.ProcessPath)!;
	public static string AppDir_Relative => $@".{Path.DirectorySeparatorChar}{LibationFiles.LIBATION_FILES_KEY}";
	public static string AppDir_Absolute => Path.GetFullPath(Path.Combine(ProcessDirectory, LibationFiles.LIBATION_FILES_KEY));
	public static string MyDocs => Path.GetFullPath(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Libation"));
	public static string MyMusic => Path.GetFullPath(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyMusic), "Libation"));
	// Use a per-user subdir so we don't collide with -- or get blocked by -- another local
	// user's leftover dir on shared-/tmp systems (Linux). On Windows and macOS, GetTempPath
	// already returns a per-user location, so this is just a harmless extra path segment.
	public static string WinTemp => Path.GetFullPath(Path.Combine(Path.GetTempPath(), $"Libation-{Environment.UserName}"));
	public static string UserProfile => Path.GetFullPath(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Libation"));
	public static string LocalAppData => Path.GetFullPath(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Libation"));

	public enum KnownDirectories
	{
		None = 0,

		[Description("My Users folder")]
		UserProfile = 1,

		[Description("The same folder that Libation is running from")]
		AppDir = 2,

		[Description("System temporary folder")]
		WinTemp = 3,

		[Description("My Documents")]
		MyDocs = 4,

		[Description("Your settings folder (aka: Libation Files)")]
		LibationFiles = 5,

		[Description("User Application Data Folder")]
		ApplicationData = 6,

		[Description("My Music")]
		MyMusic = 7,
	}
	// use func calls so we always get the latest value of LibationFiles
	private static List<(KnownDirectories directory, Func<string?> getPathFunc)> directoryOptionsPaths { get; } = new()
	{
		(KnownDirectories.None, () => null),
		(KnownDirectories.ApplicationData, () => LocalAppData),
		(KnownDirectories.MyMusic, () => MyMusic),
		(KnownDirectories.UserProfile, () => UserProfile),
		(KnownDirectories.AppDir, () => AppDir_Relative),
		(KnownDirectories.WinTemp, () => WinTemp),
		(KnownDirectories.MyDocs, () => MyDocs),
			// this is important to not let very early calls try to accidentally load LibationFiles too early.
			// also, keep this at bottom of this list
			(KnownDirectories.LibationFiles, () => Instance.LibationFiles.Location)
	};
	public static string? GetKnownDirectoryPath(KnownDirectories directory)
	{
		var dirFunc = directoryOptionsPaths.SingleOrDefault(dirFunc => dirFunc.directory == directory);
		return dirFunc == default ? null : dirFunc.getPathFunc();
	}
	public static KnownDirectories GetKnownDirectory(string directory)
	{
		// especially important so a very early call doesn't match null => LibationFiles
		if (string.IsNullOrWhiteSpace(directory))
			return KnownDirectories.None;

		// 'First' instead of 'Single' because LibationFiles could match other directories. eg: default value of LibationFiles == UserProfile.
		// since it's a list, order matters and non-LibationFiles will be returned first
		var dirFunc = directoryOptionsPaths.FirstOrDefault(dirFunc => dirFunc.getPathFunc() == directory);
		return dirFunc == default ? KnownDirectories.None : dirFunc.directory;
	}

	/// <summary>How a known-directory list is used; affects Flatpak filtering.</summary>
	public enum KnownDirectoryUsage
	{
		/// <summary>Temp paths, decrypt scratch, etc.</summary>
		General,
		/// <summary>Audiobook library location. Under Flatpak, host presets are hidden in favor of portal browse.</summary>
		BooksLocation,
		/// <summary>Libation Files (settings) location. Same Flatpak preset rules as <see cref="BooksLocation"/>.</summary>
		LibationFilesLocation,
	}

	public static List<KnownDirectories> FilterKnownDirectories(IEnumerable<KnownDirectories> source, KnownDirectoryUsage usage)
		=> FilterKnownDirectories(source, usage, IsRunningUnderFlatpak);

	internal static List<KnownDirectories> FilterKnownDirectories(IEnumerable<KnownDirectories> source, KnownDirectoryUsage usage, bool isFlatpak)
	{
		var list = new List<KnownDirectories>();
		foreach (var directory in source)
		{
			if (directory == KnownDirectories.None)
				continue;
			if (isFlatpak && ShouldExcludeKnownDirectoryUnderFlatpak(directory, usage))
				continue;
			list.Add(directory);
		}
		return list;
	}

	internal static bool ShouldExcludeKnownDirectoryUnderFlatpak(KnownDirectories directory, KnownDirectoryUsage usage)
	{
		if (directory == KnownDirectories.AppDir)
			return true;

		if (usage is not (KnownDirectoryUsage.BooksLocation or KnownDirectoryUsage.LibationFilesLocation))
			return false;

		// Host home/documents/music presets resolve inside the Flatpak sandbox unless the user
		// grants filesystem access. Prefer LibationFiles (existing settings) or Browse (portal).
		return directory is KnownDirectories.UserProfile
			or KnownDirectories.MyDocs
			or KnownDirectories.MyMusic
			or KnownDirectories.ApplicationData;
	}
}
