using LibationUiBase;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LibationUiBase.Tests;

[TestClass]
public class UpgradeCapabilityTests
{
	[TestMethod]
	public void An_ordinary_windows_install_can_still_upgrade_itself()
	{
		var capability = UpgraderBase.ResolveUpgradeCapability(platformCanUpgrade: true, applicationControlEnforcing: false);

		Assert.IsTrue(capability.CapUpgrade);
		Assert.IsNull(capability.Reason);
		Assert.IsNull(capability.Summary);
	}

	// Overlaying new files while Application Control enforces is what leaves Libation unable to
	// start, so the in-app upgrade has to be withdrawn rather than merely discouraged.
	[TestMethod]
	public void Application_control_withdraws_the_in_app_upgrade_and_explains_why()
	{
		var capability = UpgraderBase.ResolveUpgradeCapability(platformCanUpgrade: true, applicationControlEnforcing: true);

		Assert.IsFalse(capability.CapUpgrade);
		Assert.IsNotNull(capability.Reason);
		StringAssert.Contains(capability.Reason, "Smart App Control");
		StringAssert.Contains(capability.Reason, "Download the release below");
		StringAssert.Contains(capability.Reason, "troubleshoot#windows-smart-app-control-and-in-app-upgrades");
	}

	// Classic's dialog has one line above the release notes, so a summary that wraps would sit on
	// top of them.
	[TestMethod]
	public void The_summary_for_classic_stays_on_one_line()
	{
		var capability = UpgraderBase.ResolveUpgradeCapability(platformCanUpgrade: true, applicationControlEnforcing: true);

		Assert.IsNotNull(capability.Summary);
		Assert.IsFalse(capability.Summary.Contains('\n'));
		Assert.IsTrue(capability.Summary.Length <= 75, $"Summary is {capability.Summary.Length} characters and will wrap: {capability.Summary}");
	}

	[TestMethod]
	public void A_platform_that_cannot_upgrade_is_unchanged_and_gets_no_windows_explanation()
	{
		var capability = UpgraderBase.ResolveUpgradeCapability(platformCanUpgrade: false, applicationControlEnforcing: false);

		Assert.IsFalse(capability.CapUpgrade);
		Assert.IsNull(capability.Reason);
		Assert.IsNull(capability.Summary);
	}
}
