using LibationUiBase;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LibationUiBase.Tests;

[TestClass]
public class UpgradeCapabilityTests
{
	[TestMethod]
	public void An_ordinary_windows_install_can_still_upgrade_itself()
	{
		var (capUpgrade, reason) = UpgraderBase.ResolveUpgradeCapability(platformCanUpgrade: true, applicationControlEnforcing: false);

		Assert.IsTrue(capUpgrade);
		Assert.IsNull(reason);
	}

	// Overlaying new files while Application Control enforces is what leaves Libation unable to
	// start, so the in-app upgrade has to be withdrawn rather than merely discouraged.
	[TestMethod]
	public void Application_control_withdraws_the_in_app_upgrade_and_explains_why()
	{
		var (capUpgrade, reason) = UpgraderBase.ResolveUpgradeCapability(platformCanUpgrade: true, applicationControlEnforcing: true);

		Assert.IsFalse(capUpgrade);
		Assert.IsNotNull(reason);
		StringAssert.Contains(reason, "Smart App Control");
		StringAssert.Contains(reason, "Download the release below");
		StringAssert.Contains(reason, "troubleshoot#windows-smart-app-control-and-in-app-upgrades");
	}

	[TestMethod]
	public void A_platform_that_cannot_upgrade_is_unchanged_and_gets_no_windows_explanation()
	{
		var (capUpgrade, reason) = UpgraderBase.ResolveUpgradeCapability(platformCanUpgrade: false, applicationControlEnforcing: false);

		Assert.IsFalse(capUpgrade);
		Assert.IsNull(reason);
	}
}
