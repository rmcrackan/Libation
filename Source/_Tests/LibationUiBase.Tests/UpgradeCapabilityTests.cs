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

	[TestMethod]
	public void An_accepted_upgrade_installs_when_the_platform_can()
		=> Assert.IsTrue(UpgraderBase.MayInstallUpgrade(userAccepted: true, capUpgrade: true));

	// Chardonnay's prompt relabels its button to "OK" when Libation cannot install the upgrade
	// itself, but still reported acceptance, so acknowledging a download-link notice began a
	// download and an install that was never on offer and could only fail.
	[TestMethod]
	public void Acceptance_is_not_enough_when_libation_cannot_install_the_upgrade()
		=> Assert.IsFalse(UpgraderBase.MayInstallUpgrade(userAccepted: true, capUpgrade: false));

	// InstallUpgrade defaults to true, so a UI that never answers must not be taken as a yes on a
	// platform that cannot install.
	[TestMethod]
	public void Declining_never_installs()
	{
		Assert.IsFalse(UpgraderBase.MayInstallUpgrade(userAccepted: false, capUpgrade: true));
		Assert.IsFalse(UpgraderBase.MayInstallUpgrade(userAccepted: false, capUpgrade: false));
	}
}
