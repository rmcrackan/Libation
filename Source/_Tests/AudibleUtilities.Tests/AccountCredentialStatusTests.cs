using Dinah.Core.Security;
using AssertionHelper;
using AudibleApi;
using AudibleApi.Authorization;
using AudibleApi.Cryptography;
using AudibleUtilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace AudibleUtilities.Tests;

[TestClass]
public class AccountCredentialStatusTests
{
	private static Account registeredAccount(bool modernDevice = false)
	{
		var identity = new Identity(Localization.Get("us"));
		identity.Update(
			new PrivateKey(RSA.Create(2048).ExportRSAPrivateKeyPem()),
			new AdpToken("{enc:abcdefg}{key:1234}{iv:56789}{name:QURQVG9rZW5FbmNyeXB0aW9uS2V5}{serial:Mg==}"),
			new AccessToken("Atna|_CHAR_ACCESS_", new DateTime(2200, 1, 1, 12, 0, 0, DateTimeKind.Utc)),
			new RefreshToken("Atnr|_CHAR_REFRESH_"),
			new List<KeyValuePair<string, SecretString>> { new("session-id", "cookie-value") },
			deviceSerialNumber: modernDevice ? "0123456789abcdef0123" : "device-serial",
			deviceType: modernDevice ? AudibleApi.Resources.DeviceType : "device-type",
			amazonAccountId: "amzn-account",
			deviceName: "device-name",
			storeAuthenticationCookie: "store-auth-cookie");

		return new Account("user@example.com") { AccountName = "Jade", IdentityTokens = identity };
	}

	[TestMethod]
	public void no_account_looks_like_missing_credentials()
		=> AccountCredentialStatus.LooksLikeMissingCredentials(null).Should().BeTrue();

	[TestMethod]
	public void an_account_that_was_never_logged_in_looks_like_missing_credentials()
		=> AccountCredentialStatus.LooksLikeMissingCredentials(new Account("user@example.com")).Should().BeTrue();

	/// <summary>A bare Identity has a locale but no tokens to renew from, so it needs a first login.</summary>
	[TestMethod]
	public void tokens_without_a_refresh_token_look_like_missing_credentials()
	{
		var account = new Account("user@example.com") { IdentityTokens = new Identity(Localization.Get("us")) };

		AccountCredentialStatus.LooksLikeMissingCredentials(account).Should().BeTrue();
	}

	[TestMethod]
	public void a_registered_account_does_not_look_like_missing_credentials()
		=> AccountCredentialStatus.LooksLikeMissingCredentials(registeredAccount()).Should().BeFalse();

	[TestMethod]
	public void an_account_requiring_re_registration_looks_like_missing_credentials()
	{
		var account = registeredAccount();
		account.RequiresReRegistration = true;
		AccountCredentialStatus.LooksLikeMissingCredentials(account).Should().BeTrue();
	}

	[TestMethod]
	public void modern_account_reports_modern_sign_in_status()
	{
		var account = registeredAccount(modernDevice: true);
		Assert.AreEqual(AccountSignInStatus.Modern, AccountCredentialStatus.GetSignInStatus(account));
		Assert.AreEqual(AccountSignInStatus.Modern, account.SignInStatus);
	}

	[TestMethod]
	public void legacy_account_with_older_device_profile_reports_legacy_status()
	{
		var account = registeredAccount(modernDevice: false);
		Assert.AreEqual(AccountSignInStatus.Legacy, AccountCredentialStatus.GetSignInStatus(account));
		Assert.AreEqual(AccountSignInStatus.Legacy, account.SignInStatus);
	}

	[TestMethod]
	public void unregistered_or_null_account_reports_not_signed_in()
	{
		Assert.AreEqual(AccountSignInStatus.NotSignedIn, AccountCredentialStatus.GetSignInStatus(null));
		Assert.AreEqual(AccountSignInStatus.NotSignedIn, new Account("user@example.com").SignInStatus);
	}

	[TestMethod]
	public void account_requiring_re_registration_reports_not_signed_in()
	{
		var account = registeredAccount(modernDevice: true);
		account.RequiresReRegistration = true;
		Assert.AreEqual(AccountSignInStatus.NotSignedIn, AccountCredentialStatus.GetSignInStatus(account));
		Assert.AreEqual(AccountSignInStatus.NotSignedIn, account.SignInStatus);
	}

	[TestMethod]
	public void the_label_pairs_a_friendly_name_with_the_id()
		=> AccountCredentialStatus.FormatAccountLabel(new Account("user@example.com") { AccountName = "Jade" })
			.Should().Be("'Jade' (user@example.com)");

	[TestMethod]
	public void the_label_falls_back_to_the_id_alone()
		=> AccountCredentialStatus.FormatAccountLabel(new Account("user@example.com"))
			.Should().Be("'user@example.com'");

	/// <summary>Accounts default their name to their id, and "'x' (x)" reads like a mistake.</summary>
	[TestMethod]
	public void the_label_does_not_repeat_an_id_used_as_the_name()
		=> AccountCredentialStatus.FormatAccountLabel(new Account("user@example.com") { AccountName = "USER@example.com" })
			.Should().Be("'user@example.com'");

	[TestMethod]
	public void the_label_stays_readable_without_an_account()
		=> AccountCredentialStatus.FormatAccountLabel(null).Should().Be("an Audible account");
}
