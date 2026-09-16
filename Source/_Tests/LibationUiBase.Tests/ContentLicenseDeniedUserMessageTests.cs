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
		Assert.IsFalse(body.Contains("not a Libation bug"));
		Assert.IsFalse(body.Contains("experimental device registration"));
		Assert.IsFalse(body.Contains("audible-cli"));
		Assert.IsTrue(body.IndexOf("Deregister") < body.IndexOf("24 to 48 hours"));
		AssertSuggestsRemoveSaveReAdd(body);
	}

	[TestMethod]
	[DataRow(false)]
	[DataRow(true)]
	public void All_dialogs_only_include_extra_marketplace_steps_when_applicable(bool hasAdditional)
	{
		var status = AppScaffolding.VersionCheckOutcome.UpToDate;
		foreach (var body in new[] {
			ContentLicenseDeniedUserMessage.BuildDialogBodyForThrottling("Title", null, status, hasAdditional),
			ContentLicenseDeniedUserMessage.BuildDialogBodyForPossibleOutage("Title", null, status, hasAdditional),
			ContentLicenseDeniedUserMessage.BuildDialogBodyForPlusCatalog("Title", null, status, hasAdditional) })
		{
			Assert.AreEqual(hasAdditional, body.Contains("additional marketplaces"));
			Assert.AreEqual(hasAdditional, body.Contains("save both dialogs"));
		}
	}

	[TestMethod]
	public void The_outage_dialog_still_talks_about_a_service_interruption()
	{
		var body = ContentLicenseDeniedUserMessage.BuildDialogBodyForPossibleOutage("Monster Hunter Alpha");

		StringAssert.Contains(body, "temporary interruption of service");
		Assert.IsFalse(body.Contains("account is being throttled", StringComparison.Ordinal));
		AssertSuggestsRemoveSaveReAdd(body);
	}

	[TestMethod]
	public void The_Plus_dialog_still_names_the_Plus_catalog()
	{
		var body = ContentLicenseDeniedUserMessage.BuildDialogBodyForPlusCatalog("Monster Hunter Alpha");

		StringAssert.Contains(body, "Audible Plus catalog");
		Assert.IsFalse(body.Contains("account is being throttled", StringComparison.Ordinal));
		AssertSuggestsRemoveSaveReAdd(body);
	}

	private static void AssertSuggestsRemoveSaveReAdd(string body)
	{
		StringAssert.Contains(body, "remove the affected account", StringComparison.OrdinalIgnoreCase);
		StringAssert.Contains(body, "Save the removal, then close Libation");
		StringAssert.Contains(body, "re-add the account");
	}
}
