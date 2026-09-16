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
	public void Registration_setting_explains_how_to_persist_a_new_registration()
	{
		StringAssert.Contains(DeviceRegistrationSettingsUi.ReLoginNote, "does not convert existing accounts");
		StringAssert.Contains(DeviceRegistrationSettingsUi.ReLoginNote, "save or close the Accounts dialog");
		StringAssert.Contains(DeviceRegistrationSettingsUi.RemoveSaveReAddAccountSteps, "Remove the account");
		StringAssert.Contains(DeviceRegistrationSettingsUi.RemoveSaveReAddAccountSteps, "save or close the Accounts dialog");
		StringAssert.Contains(DeviceRegistrationSettingsUi.RemoveSaveReAddAccountSteps, "re-add the account");
	}
}
