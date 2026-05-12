using System;
using System.IO;
using FileManager;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FileManager.Tests;

[TestClass]
public class FolderPickerInitialPathTests
{
	[TestMethod]
	public void GetExistingDirectoryOrNull_NullOrEmpty_ReturnsNull()
	{
		Assert.IsNull(FolderPickerInitialPath.GetExistingDirectoryOrNull(null));
		Assert.IsNull(FolderPickerInitialPath.GetExistingDirectoryOrNull(""));
		Assert.IsNull(FolderPickerInitialPath.GetExistingDirectoryOrNull("   "));
	}

	[TestMethod]
	public void GetExistingDirectoryOrNull_MissingPath_ReturnsNull()
	{
		Assert.IsNull(FolderPickerInitialPath.GetExistingDirectoryOrNull(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "nope")));
	}

	[TestMethod]
	public void GetExistingDirectoryOrNull_ExistingTempDir_ReturnsFullPath()
	{
		var dir = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), "LibationTest_" + Guid.NewGuid().ToString("N"))).FullName;
		try
		{
			var result = FolderPickerInitialPath.GetExistingDirectoryOrNull(dir);
			Assert.IsNotNull(result);
			Assert.IsTrue(Directory.Exists(result));
		}
		finally
		{
			try { Directory.Delete(dir, recursive: true); } catch { /* ignore */ }
		}
	}
}
