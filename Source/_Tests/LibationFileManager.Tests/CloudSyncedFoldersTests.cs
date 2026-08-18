using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

namespace LibationFileManager.Tests;

[TestClass]
public class CloudSyncedFoldersTests
{
	private const string SyncRootVariable = "OneDrive";
	private string? _originalSyncRoot;
	private string _syncRoot = null!;

	[TestInitialize]
	public void Setup()
	{
		_originalSyncRoot = Environment.GetEnvironmentVariable(SyncRootVariable);
		_syncRoot = Path.Combine(Path.GetTempPath(), "LibationSyncTests-" + Guid.NewGuid().ToString("N"));
		Environment.SetEnvironmentVariable(SyncRootVariable, _syncRoot);
	}

	[TestCleanup]
	public void Cleanup() => Environment.SetEnvironmentVariable(SyncRootVariable, _originalSyncRoot);

	[TestMethod]
	public void Finds_the_sync_root_containing_an_install()
	{
		var install = Path.Combine(_syncRoot, "Documents", "Books", "Libation");

		Assert.AreEqual(_syncRoot, CloudSyncedFolders.FindSyncRootContaining(install));
		Assert.IsTrue(CloudSyncedFolders.IsInsideSyncRoot(install));
	}

	[TestMethod]
	public void The_sync_root_itself_counts_as_inside()
		=> Assert.IsTrue(CloudSyncedFolders.IsInsideSyncRoot(_syncRoot));

	[TestMethod]
	public void A_folder_merely_sharing_the_prefix_is_not_inside()
	{
		// Without a separator check, _syncRoot + "Backup" would look like a child of _syncRoot.
		Assert.IsFalse(CloudSyncedFolders.IsInsideSyncRoot(_syncRoot + "Backup"));
		Assert.IsFalse(CloudSyncedFolders.IsInsideSyncRoot(Path.Combine(_syncRoot + "Backup", "Libation")));
	}

	[TestMethod]
	public void A_path_outside_every_sync_root_is_not_inside()
		=> Assert.IsFalse(CloudSyncedFolders.IsInsideSyncRoot(Path.Combine(Path.GetTempPath(), "Libation")));

	[TestMethod]
	public void Nothing_is_inside_a_sync_root_that_is_not_configured()
	{
		Environment.SetEnvironmentVariable(SyncRootVariable, null);

		Assert.IsNull(CloudSyncedFolders.FindSyncRootContaining(Path.Combine(_syncRoot, "Libation")));
	}

	[TestMethod]
	[DataRow(null)]
	[DataRow("")]
	[DataRow("   ")]
	public void A_missing_path_is_not_inside(string? path)
		=> Assert.IsFalse(CloudSyncedFolders.IsInsideSyncRoot(path));
}
