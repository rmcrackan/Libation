using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;

namespace LibationFileManager.Tests;

/// <summary>
/// The restart offered after a rollback. A process that restarts itself is worth being careful with, so the
/// decision is tested here and only the Process.Start call itself goes untested.
/// </summary>
[TestClass]
[DoNotParallelize]
public class InstallRelauncherTests
{
	private readonly List<string> started = [];
	private Func<string, bool> originalStarter = null!;

	[TestInitialize]
	public void Initialize()
	{
		originalStarter = InstallRelauncher.StartProcess;
		InstallRelauncher.StartProcess = executable =>
		{
			started.Add(executable);
			return true;
		};
	}

	[TestCleanup]
	public void Cleanup()
	{
		InstallRelauncher.StartProcess = originalStarter;
		Environment.SetEnvironmentVariable(InstallRelauncher.RelaunchedEnvironmentVariable, null);
		StartupLog.ResetForTests();
	}

	[TestMethod]
	public void TryRelaunch_starts_this_same_executable()
	{
		Assert.IsTrue(InstallRelauncher.TryRelaunch());

		Assert.AreEqual(1, started.Count);
		Assert.AreEqual(Environment.ProcessPath, started[0]);
	}

	[TestMethod]
	public void A_launcher_that_fails_is_reported_rather_than_thrown()
	{
		InstallRelauncher.StartProcess = _ => throw new InvalidOperationException("no can do");

		Assert.IsFalse(InstallRelauncher.TryRelaunch());
	}

	[TestMethod]
	public void A_launcher_that_declines_is_reported_as_not_started()
	{
		InstallRelauncher.StartProcess = _ => false;

		Assert.IsFalse(InstallRelauncher.TryRelaunch());
	}

	// A rollback deletes the pending marker first, so it normally cannot repeat. But that delete swallows
	// its own failure, and a marker that survives would mean every launch rolls back and offers a restart
	// again. The child carries a marker of its own so the chain stops at one.
	[TestMethod]
	public void A_process_started_by_a_relaunch_knows_it()
	{
		Assert.IsFalse(InstallRelauncher.WasRelaunched);

		Environment.SetEnvironmentVariable(InstallRelauncher.RelaunchedEnvironmentVariable, "1");

		Assert.IsTrue(InstallRelauncher.WasRelaunched);
	}

	[TestMethod]
	public void The_restart_is_offered_for_an_install_that_was_fully_restored()
	{
		Assert.IsTrue(StartupAssemblyBootstrap.ShouldOfferRestart(
			Rollback(RollbackConfidence.RestoredToPreviousVersion),
			wasRelaunched: false));
	}

	[TestMethod]
	public void The_restart_is_still_offered_for_a_mixed_install_because_it_does_run()
	{
		Assert.IsTrue(StartupAssemblyBootstrap.ShouldOfferRestart(
			Rollback(RollbackConfidence.RestoredButInstallIsMixed),
			wasRelaunched: false));
	}

	[TestMethod]
	public void The_restart_is_withheld_when_the_restore_did_not_finish()
	{
		Assert.IsFalse(StartupAssemblyBootstrap.ShouldOfferRestart(
			Rollback(RollbackConfidence.RestoreIncomplete),
			wasRelaunched: false));
	}

	[TestMethod]
	public void The_restart_is_withheld_in_a_process_that_a_restart_started()
	{
		Assert.IsFalse(StartupAssemblyBootstrap.ShouldOfferRestart(
			Rollback(RollbackConfidence.RestoredToPreviousVersion),
			wasRelaunched: true));
	}

	[TestMethod]
	public void Nothing_is_offered_when_no_rollback_happened()
	{
		Assert.IsFalse(StartupAssemblyBootstrap.ShouldOfferRestart(
			new UpgradeRecoveryResult(false, string.Empty, string.Empty, []),
			wasRelaunched: false));
	}

	private static UpgradeRecoveryResult Rollback(RollbackConfidence confidence)
		=> new(true, "title", "body", []) { Confidence = confidence };
}
