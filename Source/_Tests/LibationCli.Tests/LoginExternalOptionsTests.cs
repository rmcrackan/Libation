using AudibleApi;
using LibationFileManager;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LibationCli.Tests;

[TestClass]
[DoNotParallelize]
public class LoginExternalOptionsTests
{
	[TestInitialize]
	public void Initialize() => Configuration.CreateMockInstance();

	[TestCleanup]
	public void Cleanup() => Configuration.RestoreSingletonInstance();

	[TestMethod]
	public void Omitted_flag_uses_the_Settings_value()
	{
		Configuration.Instance.DeviceRegistrationKind = DeviceRegistrationKind.RetailAndroid;
		var options = new LoginExternalOptions();

		Assert.IsTrue(options.TryResolveRegistrationProfile(out var profile, out var error));
		Assert.AreEqual("", error);
		Assert.AreEqual(DeviceRegistrationKind.RetailAndroid, profile.Kind);
	}

	[TestMethod]
	public void Flag_overrides_Settings()
	{
		Configuration.Instance.DeviceRegistrationKind = DeviceRegistrationKind.CurrentAndroid;
		var options = new LoginExternalOptions { DeviceRegistration = "Mkb79IPhone" };

		Assert.IsTrue(options.TryResolveRegistrationProfile(out var profile, out var error));
		Assert.AreEqual("", error);
		Assert.AreEqual(DeviceRegistrationKind.Mkb79IPhone, profile.Kind);
	}

	[TestMethod]
	public void Unknown_flag_fails()
	{
		var options = new LoginExternalOptions { DeviceRegistration = "WindowsPhone" };

		Assert.IsFalse(options.TryResolveRegistrationProfile(out _, out var error));
		StringAssert.Contains(error, "WindowsPhone");
		StringAssert.Contains(error, "CurrentAndroid");
	}
}
