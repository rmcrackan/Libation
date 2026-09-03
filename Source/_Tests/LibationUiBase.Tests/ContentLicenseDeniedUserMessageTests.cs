using LibationFileManager;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LibationUiBase.Tests;

[TestClass]
public class ContentLicenseDeniedUserMessageTests
{
	[TestInitialize]
	public void Initialize() => Configuration.CreateMockInstance();

	[TestCleanup]
	public void Cleanup() => Configuration.RestoreSingletonInstance();

	[TestMethod]
	public void The_throttling_dialog_names_Audible_throttling_and_says_to_wait()
	{
		var body = ContentLicenseDeniedUserMessage.BuildDialogBodyForThrottling("Monster Hunter Alpha");

		StringAssert.Contains(body, "Monster Hunter Alpha");
		StringAssert.Contains(body, "throttled");
		StringAssert.Contains(body, "24 to 48 hours");
		StringAssert.Contains(body, "not a Libation bug");
	}

	[TestMethod]
	public void The_outage_dialog_still_talks_about_a_service_interruption()
	{
		var body = ContentLicenseDeniedUserMessage.BuildDialogBodyForPossibleOutage("Monster Hunter Alpha");

		StringAssert.Contains(body, "temporary interruption of service");
		Assert.IsFalse(body.Contains("account is being throttled", StringComparison.Ordinal));
	}

	[TestMethod]
	public void The_Plus_dialog_still_names_the_Plus_catalog()
	{
		var body = ContentLicenseDeniedUserMessage.BuildDialogBodyForPlusCatalog("Monster Hunter Alpha");

		StringAssert.Contains(body, "Audible Plus catalog");
		Assert.IsFalse(body.Contains("account is being throttled", StringComparison.Ordinal));
	}
}
