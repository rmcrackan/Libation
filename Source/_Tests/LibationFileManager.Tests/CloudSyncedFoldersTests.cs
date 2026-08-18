using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

namespace LibationFileManager.Tests;

[TestClass]
public class CloudSyncedFoldersTests
{
	// Sync roots are a Windows concept and the answer comes from cldapi.dll, so there is nothing
	// here for CI on Linux to verify beyond the guards. The Windows path needs a machine with a
	// sync client installed, which CI does not have either.
	[TestMethod]
	public void Nothing_is_synced_off_windows()
	{
		if (OperatingSystem.IsWindows())
			Assert.Inconclusive("Skipped because the OS is Windows, where a real sync root may exist.");

		var status = CloudSyncedFolders.GetSyncStatus(Path.GetTempPath());

		Assert.IsFalse(status.IsSynced);
		Assert.IsNull(status.ProviderName);
	}

	[TestMethod]
	[DataRow(null)]
	[DataRow("")]
	[DataRow("   ")]
	public void A_missing_path_is_not_synced(string? path)
		=> Assert.IsFalse(CloudSyncedFolders.GetSyncStatus(path).IsSynced);

	[TestMethod]
	public void The_description_names_the_provider_when_windows_reported_one()
		=> Assert.AreEqual("OneDrive", new CloudSyncStatus(true, "OneDrive").Description);

	// Windows reports the fact of syncing through one info class and the provider name through
	// another, so the name can be missing while the folder really is synced.
	[TestMethod]
	[DataRow(null)]
	[DataRow("")]
	public void The_description_falls_back_when_no_provider_name_came_back(string? providerName)
		=> Assert.AreEqual("a cloud sync folder", new CloudSyncStatus(true, providerName).Description);

	[TestMethod]
	public void An_unsynced_install_adds_nothing_to_the_upgrade_failure_message()
		=> Assert.AreEqual(string.Empty, StartupAssemblyBootstrap.DescribeCloudSyncedInstall(CloudSyncStatus.NotSynced));

	// The paragraph carries its own blank lines so the message around it reads the same either way.
	[TestMethod]
	public void A_synced_install_names_the_provider_and_keeps_its_own_spacing()
	{
		var paragraph = StartupAssemblyBootstrap.DescribeCloudSyncedInstall(new CloudSyncStatus(true, "Dropbox"));

		StringAssert.Contains(paragraph, "inside Dropbox.");
		StringAssert.StartsWith(paragraph, Environment.NewLine);
		StringAssert.EndsWith(paragraph, Environment.NewLine);
	}
}
