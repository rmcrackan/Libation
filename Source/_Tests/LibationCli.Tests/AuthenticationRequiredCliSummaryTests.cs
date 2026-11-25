using AudibleApi;
using AudibleUtilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace LibationCli.Tests;

[TestClass]
public class AuthenticationRequiredCliSummaryTests
{
	private static Account MakeAccount(string accountId, string localeName, string? accountName = null)
		=> new(accountId)
		{
			AccountName = accountName,
			IdentityTokens = new AudibleApi.Authorization.Identity(Localization.Get(localeName))
		};

	[TestMethod]
	public void WriteSummary_uses_account_name_and_masks_email_on_stdout_and_gives_commands_on_stderr()
	{
		var named = new AuthenticationRequiredException(MakeAccount("wtanksleyjr@gmail.com", "us", "wm.us"));
		var unnamed = new AuthenticationRequiredException(MakeAccount("briantanksley@gmail.com", "us"));
		var duplicate = new AuthenticationRequiredException(MakeAccount("wtanksleyjr@gmail.com", "us", "wm.us"));

		using var stdout = new StringWriter();
		using var stderr = new StringWriter();

		AuthenticationRequiredCliSummary.WriteSummary([named, unnamed, duplicate], stdout, stderr);

		var outText = stdout.ToString();
		var errText = stderr.ToString();

		StringAssert.Contains(outText, "2 accounts were skipped because they need Audible sign-in:");
		StringAssert.Contains(outText, "'wm.us'");
		StringAssert.Contains(outText, "(us)");
		Assert.IsFalse(outText.Contains("wtanksleyjr@gmail.com"), "Unmasked email must never be written to stdout.");
		Assert.IsFalse(outText.Contains("briantanksley@gmail.com"), "Unmasked email must never be written to stdout.");

		StringAssert.Contains(errText, "libationcli login-external --account wtanksleyjr@gmail.com --locale us");
		StringAssert.Contains(errText, "libationcli login-external --account briantanksley@gmail.com --locale us");
		StringAssert.Contains(errText, "Libation GUI");
	}

	[TestMethod]
	public void WriteSummary_handles_null_account_info_gracefully()
	{
		var authEx = new AuthenticationRequiredException(account: null, message: "missing");

		using var stdout = new StringWriter();
		using var stderr = new StringWriter();

		AuthenticationRequiredCliSummary.WriteSummary([authEx], stdout, stderr);

		StringAssert.Contains(stdout.ToString(), "One or more accounts were skipped because they need Audible sign-in.");
		StringAssert.Contains(stderr.ToString(), "libationcli login-external");
	}
}
