using System.IO;
using LibationFileManager;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LibationFileManager.Tests;

[TestClass]
public class SandboxKnownDirectoriesTests
{
	[TestMethod]
	public void FilterKnownDirectories_NotFlatpak_OnlyRemovesNone()
	{
		var source = new[]
		{
			Configuration.KnownDirectories.UserProfile,
			Configuration.KnownDirectories.AppDir,
			Configuration.KnownDirectories.MyDocs,
		};

		var filtered = Configuration.FilterKnownDirectories(source, Configuration.KnownDirectoryUsage.BooksLocation, isFlatpak: false);

		CollectionAssert.AreEquivalent(source, filtered);
	}

	[TestMethod]
	public void FilterKnownDirectories_FlatpakBooksLocation_HidesHostPresetsAndAppDir()
	{
		var source = new[]
		{
			Configuration.KnownDirectories.LibationFiles,
			Configuration.KnownDirectories.MyMusic,
			Configuration.KnownDirectories.MyDocs,
			Configuration.KnownDirectories.AppDir,
			Configuration.KnownDirectories.UserProfile,
			Configuration.KnownDirectories.ApplicationData,
		};

		var filtered = Configuration.FilterKnownDirectories(source, Configuration.KnownDirectoryUsage.BooksLocation, isFlatpak: true);

		CollectionAssert.AreEqual(
			new[] { Configuration.KnownDirectories.LibationFiles },
			filtered);
	}

	[TestMethod]
	public void FilterKnownDirectories_FlatpakGeneral_OnlyHidesAppDir()
	{
		var source = new[]
		{
			Configuration.KnownDirectories.WinTemp,
			Configuration.KnownDirectories.UserProfile,
			Configuration.KnownDirectories.AppDir,
			Configuration.KnownDirectories.LibationFiles,
		};

		var filtered = Configuration.FilterKnownDirectories(source, Configuration.KnownDirectoryUsage.General, isFlatpak: true);

		CollectionAssert.AreEqual(
			new[]
			{
				Configuration.KnownDirectories.WinTemp,
				Configuration.KnownDirectories.UserProfile,
				Configuration.KnownDirectories.LibationFiles,
			},
			filtered);
	}

	[TestMethod]
	public void ShouldExcludeKnownDirectoryUnderFlatpak_AppDir_AlwaysExcluded()
	{
		Assert.IsTrue(Configuration.ShouldExcludeKnownDirectoryUnderFlatpak(Configuration.KnownDirectories.AppDir, Configuration.KnownDirectoryUsage.General));
		Assert.IsTrue(Configuration.ShouldExcludeKnownDirectoryUnderFlatpak(Configuration.KnownDirectories.AppDir, Configuration.KnownDirectoryUsage.BooksLocation));
	}

	[TestMethod]
	public void FilterKnownDirectories_FlatpakLibationFilesLocation_MatchesBooksLocation()
	{
		var source = new[]
		{
			Configuration.KnownDirectories.UserProfile,
			Configuration.KnownDirectories.AppDir,
			Configuration.KnownDirectories.MyDocs,
		};

		var filtered = Configuration.FilterKnownDirectories(source, Configuration.KnownDirectoryUsage.LibationFilesLocation, isFlatpak: true);

		Assert.AreEqual(0, filtered.Count);
	}

	[TestMethod]
	public void IsMisleadingFlatpakPresetPath_NotFlatpak_ReturnsFalse()
	{
		Assert.IsFalse(Configuration.IsMisleadingFlatpakPresetPath(Configuration.UserProfile, isFlatpak: false));
	}

	[TestMethod]
	public void IsMisleadingFlatpakPresetPath_Flatpak_UserProfilePreset_ReturnsTrue()
	{
		Assert.IsTrue(Configuration.IsMisleadingFlatpakPresetPath(Configuration.UserProfile, isFlatpak: true));
		Assert.IsTrue(Configuration.IsMisleadingFlatpakPresetPath(Path.Combine(Configuration.UserProfile, "Books"), isFlatpak: true));
	}

	[TestMethod]
	public void IsMisleadingFlatpakPresetPath_Flatpak_PortalStylePath_ReturnsFalse()
	{
		Assert.IsFalse(Configuration.IsMisleadingFlatpakPresetPath("/run/user/1000/doc/abc123", isFlatpak: true));
	}

	[TestMethod]
	public void NormalizeFlatpakBooksPath_RewritesMisleadingPreset()
	{
		var libationFiles = "/var/lib/flatpak/libation";
		var normalized = Configuration.NormalizeFlatpakBooksPath(Configuration.MyMusic, libationFiles, isFlatpak: true);
		Assert.AreEqual(Path.Combine(libationFiles, "Books"), normalized);
	}

	[TestMethod]
	public void NormalizeFlatpakLibationFilesPath_RewritesMisleadingPreset()
	{
		var normalized = Configuration.NormalizeFlatpakLibationFilesPath(Configuration.MyDocs, isFlatpak: true);
		Assert.AreEqual(LibationFiles.DefaultLibationFilesDirectory, normalized);
		Assert.IsFalse(Configuration.PathsEqual(Configuration.MyDocs, normalized));
	}
}
