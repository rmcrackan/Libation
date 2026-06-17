using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace LibationFileManager.Tests;

[TestClass]
public class InstallUpgradeManagerTests
{
	private string _tempRoot = null!;
	private string _installDir = null!;

	[TestInitialize]
	public void Setup()
	{
		_tempRoot = Path.Combine(Path.GetTempPath(), "LibationUpgradeTests-" + Guid.NewGuid().ToString("N"));
		_installDir = Path.Combine(_tempRoot, "install");
		Directory.CreateDirectory(_installDir);
	}

	[TestCleanup]
	public void Cleanup()
	{
		try
		{
			if (Directory.Exists(_tempRoot))
				Directory.Delete(_tempRoot, recursive: true);
		}
		catch { /* best effort */ }
	}

	[TestMethod]
	public void PrepareForUpgrade_creates_backup_and_pending_manifest()
	{
		WriteInstallFile("LibationUiBase.dll", "old-ui-base");
		WriteInstallFile("LibationFileManager.dll", "old-file-manager");
		WriteInstallFile("AppScaffolding.dll", "old-app-scaffolding");
		WriteInstallFile("Microsoft.EntityFrameworkCore.Sqlite.dll", "old-ef");

		var zipPath = CreateUpgradeZip(
			("LibationUiBase.dll", "new-ui-base"),
			("LibationFileManager.dll", "new-file-manager"),
			("AppScaffolding.dll", "new-app-scaffolding"),
			("Microsoft.EntityFrameworkCore.Sqlite.dll", "new-ef"));

		InstallUpgradeManager.PrepareForUpgrade(_installDir, zipPath, new Version(9, 9, 9));

		Assert.IsTrue(File.Exists(InstallUpgradeManager.GetPendingStatePath(_installDir)));
		Assert.IsTrue(File.Exists(Path.Combine(InstallUpgradeManager.GetBackupDirectory(_installDir), "LibationUiBase.dll")));
		Assert.AreEqual("old-ui-base", File.ReadAllText(Path.Combine(InstallUpgradeManager.GetBackupDirectory(_installDir), "LibationUiBase.dll")));
	}

	[TestMethod]
	public void RecoverPendingUpgradeIfNeeded_rolls_back_when_install_files_do_not_match_zip()
	{
		WriteInstallFile("LibationUiBase.dll", "old-ui-base");
		WriteInstallFile("LibationFileManager.dll", "old-file-manager");
		WriteInstallFile("AppScaffolding.dll", "old-app-scaffolding");
		WriteInstallFile("Microsoft.EntityFrameworkCore.Sqlite.dll", "old-ef");

		var zipPath = CreateUpgradeZip(
			("LibationUiBase.dll", "new-ui-base"),
			("LibationFileManager.dll", "new-file-manager"),
			("AppScaffolding.dll", "new-app-scaffolding"),
			("Microsoft.EntityFrameworkCore.Sqlite.dll", "new-ef"));

		InstallUpgradeManager.PrepareForUpgrade(_installDir, zipPath, new Version(9, 9, 9));

		// Simulate a partial overlay: only some files updated.
		WriteInstallFile("LibationFileManager.dll", "new-file-manager");
		WriteInstallFile("AppScaffolding.dll", "new-app-scaffolding");
		WriteInstallFile("Microsoft.EntityFrameworkCore.Sqlite.dll", "new-ef");

		var recovery = InstallUpgradeManager.RecoverPendingUpgradeIfNeeded(_installDir);

		Assert.IsNotNull(recovery);
		Assert.IsTrue(recovery!.RolledBack);
		Assert.AreEqual("old-ui-base", File.ReadAllText(Path.Combine(_installDir, "LibationUiBase.dll")));
		Assert.IsFalse(File.Exists(InstallUpgradeManager.GetPendingStatePath(_installDir)));
		var recoveryAlert = InstallUpgradeManager.TakeStartupRecoveryAlert();
		Assert.IsNotNull(recoveryAlert);
		Assert.AreEqual("In-app upgrade failed -- Libation was restored", recoveryAlert.Title);
		StringAssert.Contains(recoveryAlert.Body, "LibationUiBase.dll");
	}

	[TestMethod]
	public void RecoverPendingUpgradeIfNeeded_completes_when_install_matches_zip()
	{
		WriteInstallFile("LibationUiBase.dll", "old-ui-base");
		WriteInstallFile("LibationFileManager.dll", "old-file-manager");
		WriteInstallFile("AppScaffolding.dll", "old-app-scaffolding");
		WriteInstallFile("Microsoft.EntityFrameworkCore.Sqlite.dll", "old-ef");

		var zipPath = CreateUpgradeZip(
			("LibationUiBase.dll", "new-ui-base"),
			("LibationFileManager.dll", "new-file-manager"),
			("AppScaffolding.dll", "new-app-scaffolding"),
			("Microsoft.EntityFrameworkCore.Sqlite.dll", "new-ef"));

		InstallUpgradeManager.PrepareForUpgrade(_installDir, zipPath, new Version(9, 9, 9));

		WriteInstallFile("LibationUiBase.dll", "new-ui-base");
		WriteInstallFile("LibationFileManager.dll", "new-file-manager");
		WriteInstallFile("AppScaffolding.dll", "new-app-scaffolding");
		WriteInstallFile("Microsoft.EntityFrameworkCore.Sqlite.dll", "new-ef");

		var recovery = InstallUpgradeManager.RecoverPendingUpgradeIfNeeded(_installDir);

		Assert.IsNull(recovery);
		Assert.IsFalse(Directory.Exists(InstallUpgradeManager.GetUpgradeStateDirectory(_installDir)));
	}

	[TestMethod]
	public void VerifyInstallMatchesUpgrade_reports_missing_files()
	{
		var zipPath = CreateUpgradeZip(("LibationUiBase.dll", "new-ui-base"));
		InstallUpgradeManager.PrepareForUpgrade(_installDir, zipPath, new Version(1, 2, 3));

		File.Delete(Path.Combine(_installDir, "LibationUiBase.dll"));

		var verification = InstallUpgradeManager.VerifyInstallMatchesUpgrade(_installDir);

		Assert.IsFalse(verification.Success);
		Assert.IsTrue(verification.FailedFiles.Any(f => f.Contains("LibationUiBase.dll", StringComparison.OrdinalIgnoreCase)));
	}

	[TestMethod]
	public void GetFatalStartupMessage_uses_incomplete_upgrade_body_for_LibationUiBase_TypeLoadException()
	{
		var ex = new TypeLoadException("Could not load type 'LibationUiBase.ShowBadBookDialogAsyncDelegate' from assembly 'LibationUiBase'.");
		var message = StartupAssemblyBootstrap.GetFatalStartupMessage(
			ex,
			new FatalStartupMessage("Generic", "Generic body"));
		Assert.AreEqual("In-app upgrade failed", message.Title);
		StringAssert.Contains(message.Body, "LibationUiBase");
	}

	[TestMethod]
	public void GetStartupFailureMessage_detects_LibationUiBase_TypeLoadException()
	{
		var ex = new TypeLoadException("Could not load type 'LibationUiBase.ShowBadBookDialogAsyncDelegate' from assembly 'LibationUiBase'.");
		Assert.IsTrue(StartupAssemblyBootstrap.IsIncompleteUpgradeAssemblyFailure(ex));
		var message = StartupAssemblyBootstrap.GetStartupFailureMessage(ex);
		Assert.IsNotNull(message);
		Assert.AreEqual("In-app upgrade failed", message.Title);
		StringAssert.Contains(message.Body, "LibationUiBase");
	}

	private void WriteInstallFile(string fileName, string contents)
		=> File.WriteAllText(Path.Combine(_installDir, fileName), contents);

	private static string CreateUpgradeZip(params (string FileName, string Contents)[] files)
	{
		var zipPath = Path.Combine(Path.GetTempPath(), "LibationUpgradeTests-" + Guid.NewGuid().ToString("N") + ".zip");
		using var zip = ZipFile.Open(zipPath, ZipArchiveMode.Create);
		foreach (var (fileName, contents) in files)
		{
			var entry = zip.CreateEntry(fileName);
			using var writer = new StreamWriter(entry.Open());
			writer.Write(contents);
		}

		return zipPath;
	}
}
