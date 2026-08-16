using AudibleApi;
using AudibleApi.Authorization;
using AudibleApi.Cryptography;
using AudibleUtilities;
using LibationUiBase;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace LibationUiBase.Tests;

[TestClass]
public class AutoScanAuthPromptTests
{
	private static Account registeredAccount()
	{
		var identity = new Identity(Localization.Get("us"));
		identity.Update(
			new PrivateKey(RSA.Create(2048).ExportRSAPrivateKeyPem()),
			new AdpToken("{enc:abcdefg}{key:1234}{iv:56789}{name:QURQVG9rZW5FbmNyeXB0aW9uS2V5}{serial:Mg==}"),
			new AccessToken("Atna|_CHAR_ACCESS_", new DateTime(2200, 1, 1, 12, 0, 0, DateTimeKind.Utc)),
			new RefreshToken("Atnr|_CHAR_REFRESH_"),
			new List<KeyValuePair<string, string?>> { new("session-id", "cookie-value") },
			deviceSerialNumber: "device-serial",
			deviceType: "device-type",
			amazonAccountId: "amzn-account",
			deviceName: "device-name",
			storeAuthenticationCookie: "store-auth-cookie");

		return new Account("jade@example.com") { AccountName = "Jade", IdentityTokens = identity };
	}

	/// <summary>
	/// The reporter's log paused auto-scan on a second account that had never been logged in, while the dialog
	/// blamed an expired session and named no account at all.
	/// </summary>
	[TestMethod]
	public void an_account_that_was_never_logged_in_is_named_and_explained()
	{
		var account = new Account("jade@example.com") { AccountName = "Jade" };

		var body = AutoScanAuthPrompt.FormatBody(new AuthenticationRequiredException(account, "missing"));

		StringAssert.Contains(body, "'Jade' (jade@example.com)");
		StringAssert.Contains(body, "never been logged in");
		StringAssert.Contains(body, "Import > Scan Library");
	}

	[TestMethod]
	public void an_account_with_stored_tokens_is_told_its_login_expired()
	{
		var body = AutoScanAuthPrompt.FormatBody(new AuthenticationRequiredException(registeredAccount(), "expired"));

		StringAssert.Contains(body, "'Jade' (jade@example.com)");
		StringAssert.Contains(body, "expired");
	}

	[TestMethod]
	public void an_unattributed_failure_still_reads_sensibly()
	{
		var body = AutoScanAuthPrompt.FormatBody(new AuthenticationRequiredException(account: null, "auth failed"));

		StringAssert.Contains(body, "an Audible account");
		StringAssert.Contains(body, "Import > Scan Library");
	}

	[TestMethod]
	public void a_missing_exception_is_rejected()
		=> Assert.ThrowsExactly<ArgumentNullException>(() => AutoScanAuthPrompt.FormatBody(null!));
}
