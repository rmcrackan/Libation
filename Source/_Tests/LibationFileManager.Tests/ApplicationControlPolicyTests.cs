using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace LibationFileManager.Tests;

[TestClass]
public class ApplicationControlPolicyTests
{
	[TestMethod]
	[DataRow(0, ApplicationControlState.Off)]
	[DataRow(1, ApplicationControlState.Enforcing)]
	[DataRow(2, ApplicationControlState.Evaluation)]
	public void FromPolicyValue_maps_the_documented_values(int policyValue, ApplicationControlState expected)
		=> Assert.AreEqual(expected, ApplicationControlPolicy.FromPolicyValue(policyValue));

	// The CI\Policy key exists without this value on Windows builds that never shipped Smart App
	// Control. Reading that as enforcement tells people to make an irreversible change to a PC that
	// was never blocking anything, which is how ludus shipped a false positive (ludus issue #410).
	[TestMethod]
	[DataRow(null)]
	[DataRow(3)]
	[DataRow(-1)]
	public void FromPolicyValue_treats_anything_unrecognised_as_unknown(int? policyValue)
	{
		Assert.AreEqual(ApplicationControlState.Unknown, ApplicationControlPolicy.FromPolicyValue(policyValue));
		Assert.AreNotEqual(ApplicationControlState.Enforcing, ApplicationControlPolicy.FromPolicyValue(policyValue));
	}

	[TestMethod]
	public void GetState_is_unknown_and_not_enforcing_off_windows()
	{
		if (OperatingSystem.IsWindows())
			Assert.Inconclusive("Skipped because the OS is Windows, where a real policy state is expected.");

		Assert.AreEqual(ApplicationControlState.Unknown, ApplicationControlPolicy.GetState());
		Assert.IsFalse(ApplicationControlPolicy.IsEnforcing);
	}

	[TestMethod]
	public void Only_the_enforcing_message_tells_the_user_to_turn_smart_app_control_off()
	{
		var enforcing = StartupAssemblyBootstrap.DescribeApplicationControlState(ApplicationControlState.Enforcing);
		StringAssert.Contains(enforcing, "On for this PC");
		StringAssert.Contains(enforcing, "cannot turn it back on");

		// Turning Smart App Control off cannot be undone, so a state that is not blocking must never
		// be answered by suggesting it.
		foreach (var state in new[] { ApplicationControlState.Off, ApplicationControlState.Evaluation })
		{
			var message = StartupAssemblyBootstrap.DescribeApplicationControlState(state);
			StringAssert.Contains(message, "another Application Control policy");
			Assert.IsFalse(message.Contains("turn", StringComparison.OrdinalIgnoreCase), $"The {state} message must not suggest changing the setting: {message}");
		}
	}

	[TestMethod]
	public void The_unknown_message_explains_how_to_look_the_setting_up()
	{
		var message = StartupAssemblyBootstrap.DescribeApplicationControlState(ApplicationControlState.Unknown);

		StringAssert.Contains(message, "Smart App Control settings");
		StringAssert.Contains(message, "Evaluation observes without blocking");
	}
}
