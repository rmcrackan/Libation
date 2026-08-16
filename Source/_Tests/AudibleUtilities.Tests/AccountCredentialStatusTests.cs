using AudibleUtilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AudibleUtilities.Tests;

[TestClass]
public class AccountCredentialStatusTests
{
	[TestMethod]
	public void LooksLikeMissingCredentials_null_account()
	{
		Assert.IsTrue(AccountCredentialStatus.LooksLikeMissingCredentials(null));
	}

	[TestMethod]
	public void LooksLikeMissingCredentials_null_tokens()
	{
		var account = new Account("user@example.com") { AccountName = "User" };
		Assert.IsTrue(AccountCredentialStatus.LooksLikeMissingCredentials(account));
	}

	[TestMethod]
	public void FormatAccountLabel_uses_name_and_id()
	{
		var account = new Account("user@example.com") { AccountName = "Jade" };
		Assert.AreEqual("'Jade' (user@example.com)", AccountCredentialStatus.FormatAccountLabel(account));
	}

	[TestMethod]
	public void FormatAccountLabel_falls_back_to_id()
	{
		var account = new Account("user@example.com");
		Assert.AreEqual("'user@example.com'", AccountCredentialStatus.FormatAccountLabel(account));
	}

	[TestMethod]
	public void FormatAccountLabel_null_account()
	{
		Assert.AreEqual("an Audible account", AccountCredentialStatus.FormatAccountLabel(null));
	}
}
