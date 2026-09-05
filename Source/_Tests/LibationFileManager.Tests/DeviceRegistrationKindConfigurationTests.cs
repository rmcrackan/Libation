using AssertionHelper;
using AudibleApi;
using FileManager;
using LibationFileManager;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System;

namespace DeviceRegistrationKindConfigurationTests;

[TestClass]
[DoNotParallelize]
public class DeviceRegistrationKindConfigurationTests
{
	[TestCleanup]
	public void Cleanup()
	{
		Configuration.RestoreSingletonInstance();
	}

	[TestMethod]
	public void Default_when_missing_is_CurrentAndroid()
	{
		var config = Configuration.CreateMockInstance();

		config.Exists(nameof(Configuration.DeviceRegistrationKind)).Should().BeFalse();
		Assert.AreEqual(DeviceRegistrationKind.CurrentAndroid, config.DeviceRegistrationKind);
		Assert.AreEqual(DeviceRegistrationKind.CurrentAndroid, config.GetDeviceRegistrationProfile().Kind);
	}

	[TestMethod]
	public void Round_trips_each_kind()
	{
		var config = Configuration.CreateMockInstance();

		foreach (var kind in Enum.GetValues<DeviceRegistrationKind>())
		{
			config.DeviceRegistrationKind = kind;
			Assert.AreEqual(kind, config.DeviceRegistrationKind);
			Assert.AreEqual(kind, config.CreateEphemeralCopy().DeviceRegistrationKind);
			Assert.AreEqual(kind, config.GetDeviceRegistrationProfile().Kind);
		}
	}

	[TestMethod]
	public void Unknown_enum_value_throws_InvalidConfigurationValueException()
	{
		var ex = Assert.ThrowsExactly<InvalidConfigurationValueException>(
			() => IJsonBackedDictionary.UpCast<DeviceRegistrationKind>(new JValue("NotARealProfile"), nameof(Configuration.DeviceRegistrationKind)));

		StringAssert.Contains(ex.Message, "DeviceRegistrationKind");
		StringAssert.Contains(ex.Message, "NotARealProfile");
		StringAssert.Contains(ex.Message, "CurrentAndroid");
	}

	[TestMethod]
	public void ValidateEnumSettings_throws_for_invalid_DeviceRegistrationKind()
	{
		var config = Configuration.CreateMockInstance();
		config.SetNonString("NotARealProfile", nameof(Configuration.DeviceRegistrationKind));

		Assert.ThrowsExactly<InvalidConfigurationValueException>(config.ValidateEnumSettings);
	}
}
