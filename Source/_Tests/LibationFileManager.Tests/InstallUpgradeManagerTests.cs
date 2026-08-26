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
		StartupLog.ResetForTests();

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

	// Issue #2001: the recovery logged through Serilog, and on an install broken badly enough to need
	// recovering, Serilog itself would not load. The logging call threw from inside the very catch block
	// that was meant to report the problem, which discarded the real exception and left the rollback undone.
	// Logging now goes through one sink that swallows its own failures, so prove a hostile sink is harmless.
	[TestMethod]
	public void RecoverPendingUpgradeIfNeeded_rolls_back_even_when_every_log_call_throws()
	{
		StartupLog.ReplayTo(_ => throw new FileNotFoundException(
			"Could not load file or assembly 'Serilog, Version=4.3.0.0, Culture=neutral, PublicKeyToken=24c2f752a8e58a10'.",
			"Serilog, Version=4.3.0.0, Culture=neutral, PublicKeyToken=24c2f752a8e58a10"));

		WriteInstallFile("LibationUiBase.dll", "old-ui-base");
		WriteInstallFile("LibationFileManager.dll", "old-file-manager");

		var zipPath = CreateUpgradeZip(
			("LibationUiBase.dll", "new-ui-base"),
			("LibationFileManager.dll", "new-file-manager"));

		InstallUpgradeManager.PrepareForUpgrade(_installDir, zipPath, new Version(9, 9, 9));

		// A partial overlay: LibationUiBase.dll was never replaced.
		WriteInstallFile("LibationFileManager.dll", "new-file-manager");

		var recovery = InstallUpgradeManager.RecoverPendingUpgradeIfNeeded(_installDir);

		Assert.IsNotNull(recovery);
		Assert.IsTrue(recovery!.RolledBack);
		Assert.AreEqual("old-file-manager", File.ReadAllText(Path.Combine(_installDir, "LibationFileManager.dll")));
		Assert.IsFalse(File.Exists(InstallUpgradeManager.GetPendingStatePath(_installDir)));
	}

	// Issue #2001: every backed-up file is an assembly, and by the time startup recovery runs this process
	// has already loaded and mapped several of them. File.Copy(overwrite: true) over a mapped file segfaulted
	// the process on Linux and is denied on Windows, so the rollback could never finish.
	[TestMethod]
	public void RecoverPendingUpgradeIfNeeded_restores_a_file_that_is_still_open()
	{
		WriteInstallFile("LibationUiBase.dll", "old-ui-base");
		WriteInstallFile("LibationFileManager.dll", "old-file-manager");

		var zipPath = CreateUpgradeZip(
			("LibationUiBase.dll", "new-ui-base"),
			("LibationFileManager.dll", "new-file-manager"));

		InstallUpgradeManager.PrepareForUpgrade(_installDir, zipPath, new Version(9, 9, 9));

		WriteInstallFile("LibationUiBase.dll", "new-ui-base");
		// LibationFileManager.dll keeps its old contents, so verification fails and a rollback follows.

		var openPath = Path.Combine(_installDir, "LibationUiBase.dll");
		// The sharing mode .NET uses for a loaded assembly: readers and renames allowed, writers denied.
		using (var held = new FileStream(openPath, FileMode.Open, FileAccess.Read, FileShare.Read | FileShare.Delete))
		{
			var recovery = InstallUpgradeManager.RecoverPendingUpgradeIfNeeded(_installDir);

			Assert.IsNotNull(recovery);
			Assert.IsTrue(recovery!.RolledBack);
			Assert.AreEqual("old-ui-base", File.ReadAllText(openPath));

			// The handle still reads the file it opened, which is what keeps a loaded assembly working.
			using var reader = new StreamReader(held);
			Assert.AreEqual("new-ui-base", reader.ReadToEnd());
		}
	}

	// The rollback used to restore, announce "Libation restored your previous install files" and delete the
	// pending marker without ever reading back what it wrote. These three cover the grading that replaced
	// that assumption, since it decides both how firmly the message pushes a reinstall and whether
	// restarting is offered at all.
	[TestMethod]
	public void An_overlay_that_never_started_leaves_the_previous_version_behind()
	{
		WriteInstallFile("LibationUiBase.dll", "old-ui-base");
		WriteInstallFile("LibationFileManager.dll", "old-file-manager");

		var zipPath = CreateUpgradeZip(
			("LibationUiBase.dll", "new-ui-base"),
			("LibationFileManager.dll", "new-file-manager"));

		InstallUpgradeManager.PrepareForUpgrade(_installDir, zipPath, new Version(9, 9, 9));

		// Nothing was replaced: ZipExtractor never ran.
		var recovery = InstallUpgradeManager.RecoverPendingUpgradeIfNeeded(_installDir);

		Assert.IsNotNull(recovery);
		Assert.AreEqual(RollbackConfidence.RestoredToPreviousVersion, recovery!.Confidence);
		Assert.IsTrue(recovery.WorthRestarting);
		StringAssert.Contains(recovery.Message, "the version you were running before");
	}

	[TestMethod]
	public void An_overlay_that_got_partway_leaves_a_mixed_install()
	{
		WriteInstallFile("LibationUiBase.dll", "old-ui-base");
		WriteInstallFile("LibationFileManager.dll", "old-file-manager");

		var zipPath = CreateUpgradeZip(
			("LibationUiBase.dll", "new-ui-base"),
			("LibationFileManager.dll", "new-file-manager"));

		InstallUpgradeManager.PrepareForUpgrade(_installDir, zipPath, new Version(9, 9, 9));

		// One file was replaced and one was not, so the overlay was underway when it stopped. The backup
		// covers a dozen names out of hundreds, so restoring them cannot undo what else it replaced.
		WriteInstallFile("LibationFileManager.dll", "new-file-manager");

		var recovery = InstallUpgradeManager.RecoverPendingUpgradeIfNeeded(_installDir);

		Assert.IsNotNull(recovery);
		Assert.AreEqual(RollbackConfidence.RestoredButInstallIsMixed, recovery!.Confidence);
		Assert.IsTrue(recovery.WorthRestarting, "a mixed install still starts, so restarting stays on offer");
		StringAssert.Contains(recovery.Message, "mixture of both versions");
	}

	[TestMethod]
	public void A_restore_that_cannot_write_every_file_does_not_claim_success()
	{
		WriteInstallFile("LibationUiBase.dll", "old-ui-base");
		WriteInstallFile("LibationFileManager.dll", "old-file-manager");

		var zipPath = CreateUpgradeZip(
			("LibationUiBase.dll", "new-ui-base"),
			("LibationFileManager.dll", "new-file-manager"));

		InstallUpgradeManager.PrepareForUpgrade(_installDir, zipPath, new Version(9, 9, 9));

		// Make one backup file unreadable so its restore fails while the other succeeds.
		var unreadableBackup = Path.Combine(InstallUpgradeManager.GetBackupDirectory(_installDir), "LibationUiBase.dll");
		using (var exclusive = new FileStream(unreadableBackup, FileMode.Open, FileAccess.Read, FileShare.None))
		{
			var recovery = InstallUpgradeManager.RecoverPendingUpgradeIfNeeded(_installDir);

			Assert.IsNotNull(recovery);
			Assert.AreEqual(RollbackConfidence.RestoreIncomplete, recovery!.Confidence);
			Assert.IsFalse(recovery.WorthRestarting, "do not invite the user back into an install we could not finish");
			StringAssert.Contains(recovery.Message, "could not put all of your previous install files back");
			StringAssert.Contains(recovery.Message, "Could not be restored:");

			// The file it could reach was still restored rather than abandoned.
			Assert.AreEqual("old-file-manager", File.ReadAllText(Path.Combine(_installDir, "LibationFileManager.dll")));
		}
	}

	[TestMethod]
	public void A_displaced_file_is_swept_up_on_a_later_startup()
	{
		var displaced = Path.Combine(_installDir, "LibationUiBase.dll" + InstallUpgradeManager.DisplacedFileSuffix);
		File.WriteAllText(displaced, "left behind by an earlier rollback");

		// No pending upgrade, so this is the ordinary startup path.
		Assert.IsNull(InstallUpgradeManager.RecoverPendingUpgradeIfNeeded(_installDir));

		Assert.IsFalse(File.Exists(displaced));
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

	[TestMethod]
	public void GetStartupFailureMessage_names_the_blocked_file_and_explains_the_missing_signature()
	{
		var ex = new FileLoadException(
			"An Application Control policy has blocked this file. (0x800711C7)",
			@"C:\Libation\Serilog.Settings.Configuration.dll");

		Assert.IsTrue(StartupAssemblyBootstrap.IsApplicationControlBlockedAssembly(ex));

		var message = StartupAssemblyBootstrap.GetStartupFailureMessage(ex);

		Assert.IsNotNull(message);
		Assert.AreEqual("Libation blocked by Windows security", message.Title);
		StringAssert.Contains(message.Body, @"C:\Libation\Serilog.Settings.Configuration.dll");
		StringAssert.Contains(message.Body, "Smart App Control");
		StringAssert.Contains(message.Body, "not signed");
	}

	// Unblock-File takes no -Recurse argument, and Smart App Control gates on the signature
	// rather than on Mark-of-the-Web, so telling users to unblock the folder sent them after a
	// command that both fails to run and cannot fix the block. See issue #1967.
	[TestMethod]
	public void GetApplicationControlBlockedMessage_does_not_suggest_unblocking_the_install_folder()
	{
		var body = StartupAssemblyBootstrap.GetApplicationControlBlockedMessage();

		Assert.IsFalse(body.Contains("Unblock-File", StringComparison.OrdinalIgnoreCase));
	}

	[TestMethod]
	public void Startup_messages_link_to_their_own_docs_sections()
	{
		StringAssert.Contains(
			StartupAssemblyBootstrap.GetIncompleteUpgradeFailureMessage(),
			"troubleshoot#windows-incomplete-in-app-upgrade");

		StringAssert.Contains(
			StartupAssemblyBootstrap.GetApplicationControlBlockedMessage(),
			"troubleshoot#windows-smart-app-control-and-in-app-upgrades");
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
