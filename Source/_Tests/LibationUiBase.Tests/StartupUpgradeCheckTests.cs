using LibationFileManager;

namespace LibationUiBase.Tests;

/// <summary>
/// Covers the one thing the CheckForUpgradesAtStartup setting has to do: keep the startup check off
/// the network when it is off, and leave it alone when it is on.
/// <para>
/// <see cref="MockUpgrader"/> is the seam. With <see cref="MockUpgrader.CheckForUpgradeSucceeds"/>
/// false, the check reports its own distinctive failure and the flow returns before reaching
/// <c>InteropFactory</c>, whose <c>CanUpgrade</c> throws off-platform. That failure message is
/// therefore proof that the check ran, and its absence proof that it did not.
/// </para>
/// </summary>
[TestClass]
[DoNotParallelize]
public class StartupUpgradeCheckTests
{
	private const string CheckRanMessage = "Mock Check For Upgrade Failed";

	[TestCleanup]
	public void Cleanup()
	{
		Configuration.RestoreSingletonInstance();
	}

	[TestMethod]
	public async Task Startup_checks_when_the_setting_is_on()
	{
		var config = Configuration.CreateMockInstance();
		config.CheckForUpgradesAtStartup = true;

		var (upgrader, failures) = BuildUpgraderThatFailsItsCheck();
		await upgrader.CheckForUpgradeAtStartupAsync(ShouldNotBeCalled);

		Assert.AreEqual(1, failures.Count);
		StringAssert.Contains(failures[0], CheckRanMessage);
	}

	[TestMethod]
	public async Task Startup_does_not_check_when_the_setting_is_off()
	{
		var config = Configuration.CreateMockInstance();
		config.CheckForUpgradesAtStartup = false;

		var (upgrader, failures) = BuildUpgraderThatFailsItsCheck();
		await upgrader.CheckForUpgradeAtStartupAsync(ShouldNotBeCalled);

		Assert.AreEqual(0, failures.Count);
	}

	// Turning the automatic check off must not disable the About window's "Check for Upgrade"
	// button, which calls CheckForUpgradeAsync directly.
	[TestMethod]
	public async Task A_requested_check_still_runs_when_the_setting_is_off()
	{
		var config = Configuration.CreateMockInstance();
		config.CheckForUpgradesAtStartup = false;

		var (upgrader, failures) = BuildUpgraderThatFailsItsCheck();
		var result = await upgrader.CheckForUpgradeAsync(ShouldNotBeCalled);

		Assert.AreEqual(AppScaffolding.VersionCheckOutcome.UnableToDetermine, result.Outcome);
		Assert.AreEqual(1, failures.Count);
		StringAssert.Contains(failures[0], CheckRanMessage);
	}

	private static (MockUpgrader Upgrader, List<string> Failures) BuildUpgraderThatFailsItsCheck()
	{
		var upgrader = new MockUpgrader { CheckForUpgradeSucceeds = false };
		List<string> failures = [];
		upgrader.UpgradeFailed += (_, message) => failures.Add(message);
		return (upgrader, failures);
	}

	private static Task ShouldNotBeCalled(UpgradeEventArgs e)
	{
		Assert.Fail("The upgrade-available handler ran, but the check was supposed to fail before an update could be offered.");
		return Task.CompletedTask;
	}
}
