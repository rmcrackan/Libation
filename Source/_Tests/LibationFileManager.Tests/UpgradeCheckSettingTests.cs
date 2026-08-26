using AssertionHelper;
using LibationFileManager;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UpgradeCheckSettingTests;

[TestClass]
[DoNotParallelize]
public class UpgradeCheckSettingTests
{
	[TestCleanup]
	public void Cleanup()
	{
		Configuration.RestoreSingletonInstance();
	}

	// The update check is opt-out, so every install that predates the setting has to keep checking
	// without anything having written the key.
	[TestMethod]
	public void Default_when_missing_is_enabled()
	{
		var config = Configuration.CreateMockInstance();

		config.Exists(nameof(Configuration.CheckForUpgradesAtStartup)).Should().BeFalse();
		Assert.IsTrue(config.CheckForUpgradesAtStartup);
	}

	[TestMethod]
	public void Round_trips_off_and_on()
	{
		var config = Configuration.CreateMockInstance();

		config.CheckForUpgradesAtStartup = false;
		Assert.IsFalse(config.CheckForUpgradesAtStartup);
		Assert.IsFalse(config.CreateEphemeralCopy().CheckForUpgradesAtStartup);

		config.CheckForUpgradesAtStartup = true;
		Assert.IsTrue(config.CheckForUpgradesAtStartup);
		Assert.IsTrue(config.CreateEphemeralCopy().CheckForUpgradesAtStartup);
	}

	// get-setting and the -o override both reflect over Configuration properties carrying
	// [Description], so a missing attribute would silently drop the setting from the CLI.
	[TestMethod]
	public void Has_a_description_so_the_cli_can_see_it()
	{
		var description = Configuration.GetDescription(nameof(Configuration.CheckForUpgradesAtStartup));

		Assert.AreNotEqual($"[{nameof(Configuration.CheckForUpgradesAtStartup)}]", description);
		StringAssert.Contains(description, "startup");
	}
}
