using AudibleUtilities;
using LibationUiBase;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LibationUiBase.Tests;

[TestClass]
public class AutoScanAuthPromptTests
{
	[TestMethod]
	public void FormatBody_missing_credentials_mentions_not_logged_in()
	{
		var account = new Account("jade@example.com") { AccountName = "Jade" };
		var ex = new AuthenticationRequiredException(account, "missing");

		var body = AutoScanAuthPrompt.FormatBody(ex);

		StringAssert.Contains(body, "Jade");
		StringAssert.Contains(body, "jade@example.com");
		StringAssert.Contains(body, "has not been logged in");
		StringAssert.Contains(body, "Import > Scan Library");
	}

	[TestMethod]
	public void FormatBody_null_account_uses_generic_label()
	{
		var ex = new AuthenticationRequiredException(null, "auth failed");
		var body = AutoScanAuthPrompt.FormatBody(ex);

		StringAssert.Contains(body, "an Audible account");
		StringAssert.Contains(body, "has not been logged in");
	}
}
