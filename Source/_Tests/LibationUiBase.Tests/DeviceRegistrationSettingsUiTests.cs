using AudibleApi;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LibationUiBase.Tests;

[TestClass]
public class DeviceRegistrationSettingsUiTests
{
	[TestMethod]
	public void Options_cover_every_DeviceRegistrationKind()
	{
		var kinds = DeviceRegistrationSettingsUi.Options.Select(o => o.Value).ToArray();
		// RetailAndroid is not a valid option for the setting, so it is excluded from the assertion.
		CollectionAssert.AreEquivalent(Enum.GetValues<DeviceRegistrationKind>().Where(p => p is not DeviceRegistrationKind.RetailAndroid).ToArray(), kinds);
	}

	[TestMethod]
	public void Display_falls_back_to_CurrentAndroid()
	{
		Assert.AreEqual(DeviceRegistrationKind.CurrentAndroid, DeviceRegistrationSettingsUi.Display((DeviceRegistrationKind)99).Value);
	}

	[TestMethod]
	public void Throttling_workaround_names_experimental_relogin_and_audible_cli()
	{
		StringAssert.Contains(DeviceRegistrationSettingsUi.ThrottlingWorkaround, "experimental device registration");
		StringAssert.Contains(DeviceRegistrationSettingsUi.ThrottlingWorkaround, "audible-cli");
		StringAssert.Contains(DeviceRegistrationSettingsUi.ReLoginNote, "does not convert existing accounts");
	}
}
