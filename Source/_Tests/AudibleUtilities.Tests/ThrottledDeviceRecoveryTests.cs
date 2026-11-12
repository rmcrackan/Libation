using AssertionHelper;
using AudibleApi;
using AudibleApi.Authorization;
using AudibleApi.Common;
using AudibleApi.Cryptography;
using AudibleUtilities;
using Dinah.Core.Security;
using LibationFileManager;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AudibleUtilities.Tests;

[TestClass]
[DoNotParallelize]
public class ThrottledDeviceRecoveryTests
{
	private const string SamplePrivateKey = @"
-----BEGIN RSA PRIVATE KEY-----
MIIEpgIBAAKCAQEA5nPbGSVDmlEH2tJa6kz/P2HI8IeirhfPHdmi+X/nsb9i3WNf
tmEdZxfK26IValQDXvBH17a1gr0HD6pYse1XsV2w0HxiW1RW+ZnjL8/fzPdkSOb+
4xKlqRopCueBSdDGgAF06spZ3IeHLfEFOJX4dO1Y73pFBUkA0k53LT12L2Tjay/r
buZHJqIzxmwja7/nkiWL0Xo7UySHtQACYsKEatu6yHBS+cPTlGR/qeUpeJTHwDLP
7ZQ7kWzJGY1mfInYekjlZLsMsWswso3pg1vPyHgxzM2BWhY8m6mlXQ9G/USxBTib
MNuMtpR73XsgamneFCc+Uv1cxw7ofZ41YOOAbQIDAQABAoIBAQDIre8HkKm0Aggj
B7df/TjxCsgenR6PF/Cmf9UqC7XJ1W3UeCrq+NrP4aonZJfdhdeBnyAQuuyJMu6p
N6ARISuSKpJEm2xTN7idluJ9yjmLlYtg6LbhKmXUQhGniz3M999DrQERTLDAF80h
tpbjVcWMnPsrX4AnQBFVEjs5zCHU1hD+X463EmUHBWyT975jbZ8Fy7/fTzkdzLnn
qE5lROALr2MCAAwQRFbRE6dd52vnXaBrVcAtRzjATts3WG3+SNi2Fm/OrYqQcY9e
lBexNviT8VcldOAMrO10E2u0d+tvxFzwB3ABMvaVamrEZky4XSfB6aLzpD0JJj1s
UHnIiVwJAoGBAPl8nLll/J9rud/N2HiAX2YkP0MC0HW4yM3KxLtXKyXrP5qBpaci
wTDUmSWEEE3GUJMM1Z4d9tl9Lz2MhU2KqkEvLI3kQ7aUu33PYUBGMVcUzhFQ49lU
Nzz8YB183iqo31o/DKk2Cr5gI7SykQZ0gn/urZkEJeErLzlhPXcyeY5jAoGBAOx4
CGucVdv5MbdXZP8jVzxuvUlSp7BIQJ2phQXDFBNApFKnZn7yBYBx7dqzleymGm+R
INZAurg3SNw4nvbQc3Z2dJ8I+n5ErjFCKp1IedVxx1eMEfecTwrQZuUwLISIyjqF
czSJNwcNqzCx67z397/Cg5K/0pu6uIe0r7xozcbvAoGBAOOvZ9CDVPOg+rdXQvFm
Jqou9lUPonNtOkUlgjl+qfAnK5q0KxvHSgxoWYO1bLOuAybQlbuBmSCPcKd5MMa9
f/eRN9YetfVQ83Mz6YshBDJ22EFRUz+p7eeIY6dFp/PCvmO8Gq/qlA996dglBtmf
RuG+T0vQT0mZgbWaGuBHfkwFAoGBAMOLg1MRxgKRMKavk6pU3EfyP3+J5XemWCDI
1WLtbgV5uClNmzmxBBGypQHs7jbzKPtHpULn5kB+HzdVb0clG8ZDsK7u6s5OF0pO
sBS+oVl7rF/eSeFcFhUYP26ZhsbWo3z/bERuj926VO2AxDPRTsP5o3pQPGZhY0V9
irGgbUJrAoGBAOseS3J4BqYM4R3Hr7cRAhvzSjIkeTcDF1zTOa4FZDHBxZ6g2PNq
8ekhtfn1zPczsPTF1vNuqEISKLxaPkVPiw0mtaZQjVwpF/IOxMNjWVLp6oJf8Mm2
BxlXqPnQ4mG66oqSFQgDEmFdMhRb2of6xL1gYYL62C80G2T7QtmPfSab
-----END RSA PRIVATE KEY-----
";

	private const string SampleAccessToken = "Atna|_CHAR_ACCESS_";
	private const string SampleRefreshToken = "Atnr|_CHAR_REFRESH_";
	private const string SampleAdpToken = "{enc:abcdefg}{key:1234}{iv:56789}{name:QURQVG9rZW5FbmNyeXB0aW9uS2V5}{serial:Mg==}";

	[TestInitialize]
	public void Initialize() => Configuration.CreateMockInstance();

	[TestCleanup]
	public void Cleanup() => Configuration.RestoreSingletonInstance();

	private static Identity CreateIdentity(string deviceType, string? serialNumber)
	{
		var identity = new Identity(Localization.Get("us"));
		identity.Update(
			new PrivateKey(SamplePrivateKey),
			new AdpToken(SampleAdpToken),
			new AccessToken(SampleAccessToken, DateTime.UtcNow.AddHours(1)),
			new RefreshToken(SampleRefreshToken),
			new List<KeyValuePair<string, SecretString>> { new("session-id", new SecretString("cookie-val")) },
			deviceSerialNumber: serialNumber,
			deviceType: deviceType,
			amazonAccountId: "amzn-123",
			deviceName: "dev-name",
			storeAuthenticationCookie: new SecretString("store-cookie"));
		return identity;
	}

	private static ContentLicenseDeniedException CreateDeniedException(bool isThrottled)
	{
		var rejectionReason = isThrottled ? RejectionReason.CustomerThrottled : RejectionReason.GenericError;
		var license = new ContentLicense
		{
			Asin = "B002V5B8OY",
			StatusCode = "Denied",
			LicenseDenialReasons =
			[
				new LicenseDenialReason
				{
					ValidationType = "Ownership",
					RejectionReason = rejectionReason,
					Message = isThrottled ? "Customer is throttled" : "Denied"
				}
			]
		};
		return new ContentLicenseDeniedException(new Uri("https://audible.com/licenserequest"), license);
	}

	[TestMethod]
	public void HasObsoleteDeviceRegistration_returns_false_for_null_tokens()
	{
		var account = new Account("user@example.com");
		account.HasObsoleteDeviceRegistration().Should().BeFalse();
	}

	[TestMethod]
	public void HasObsoleteDeviceRegistration_detects_non_android_device_types()
	{
		var iphoneAccount = new Account("iphone@example.com")
		{
			IdentityTokens = CreateIdentity("A2CZJZGLK2JJVM", "0123456789abcdef0123")
		};
		iphoneAccount.HasObsoleteDeviceRegistration().Should().BeTrue();

		var retailAndroidAccount = new Account("retail@example.com")
		{
			IdentityTokens = CreateIdentity("A2L54JJ0LGZZ85", "0123456789abcdef0123")
		};
		retailAndroidAccount.HasObsoleteDeviceRegistration().Should().BeTrue();
	}

	[TestMethod]
	public void HasObsoleteDeviceRegistration_detects_non_20_char_or_invalid_serials()
	{
		// Old 32-character hex serial
		var oldHexAccount = new Account("old32@example.com")
		{
			IdentityTokens = CreateIdentity(AudibleApi.Resources.DeviceType, "0123456789abcdef0123456789abcdef")
		};
		oldHexAccount.HasObsoleteDeviceRegistration().Should().BeTrue();

		// Old 16-character hex serial
		var old16Account = new Account("old16@example.com")
		{
			IdentityTokens = CreateIdentity(AudibleApi.Resources.DeviceType, "0123456789abcdef")
		};
		old16Account.HasObsoleteDeviceRegistration().Should().BeTrue();

		// Old 40-character hex serial
		var old40Account = new Account("old40@example.com")
		{
			IdentityTokens = CreateIdentity(AudibleApi.Resources.DeviceType, "0123456789abcdef0123456789abcdef01234567")
		};
		old40Account.HasObsoleteDeviceRegistration().Should().BeTrue();

		// Non-hex serial
		var nonHexAccount = new Account("nonhex@example.com")
		{
			IdentityTokens = CreateIdentity(AudibleApi.Resources.DeviceType, "not-a-hex-serial-here")
		};
		nonHexAccount.HasObsoleteDeviceRegistration().Should().BeTrue();

		// Empty serial
		var emptySerialAccount = new Account("empty@example.com")
		{
			IdentityTokens = CreateIdentity(AudibleApi.Resources.DeviceType, "")
		};
		emptySerialAccount.HasObsoleteDeviceRegistration().Should().BeTrue();
	}

	[TestMethod]
	public void HasObsoleteDeviceRegistration_returns_false_for_valid_current_android()
	{
		var modernAccount = new Account("modern@example.com")
		{
			IdentityTokens = CreateIdentity(AudibleApi.Resources.DeviceType, "0123456789abcdef0123")
		};
		modernAccount.HasObsoleteDeviceRegistration().Should().BeFalse();
	}

	[TestMethod]
	public void ShouldRecover_returns_false_when_not_CustomerThrottled()
	{
		var ex = CreateDeniedException(isThrottled: false);
		var account = new Account("user@example.com")
		{
			IdentityTokens = CreateIdentity("A2CZJZGLK2JJVM", "0123456789abcdef0123")
		};
		Configuration.Instance.UseWidevine = false;

		ThrottledDeviceRecovery.ShouldRecover(ex, account, Configuration.Instance).Should().BeFalse();
	}

	[TestMethod]
	public void ShouldRecover_returns_true_when_throttled_and_widevine_disabled()
	{
		var ex = CreateDeniedException(isThrottled: true);
		var modernAccount = new Account("user@example.com")
		{
			IdentityTokens = CreateIdentity(AudibleApi.Resources.DeviceType, "0123456789abcdef0123")
		};
		Configuration.Instance.UseWidevine = false;
		Configuration.Instance.DeviceRegistrationKind = DeviceRegistrationKind.CurrentAndroid;

		ThrottledDeviceRecovery.ShouldRecover(ex, modernAccount, Configuration.Instance).Should().BeTrue();
	}

	[TestMethod]
	public void ShouldRecover_returns_true_when_throttled_and_device_profile_is_iphone()
	{
		var ex = CreateDeniedException(isThrottled: true);
		var modernAccount = new Account("user@example.com")
		{
			IdentityTokens = CreateIdentity(AudibleApi.Resources.DeviceType, "0123456789abcdef0123")
		};
		Configuration.Instance.UseWidevine = true;
		Configuration.Instance.DeviceRegistrationKind = DeviceRegistrationKind.Mkb79IPhone;

		ThrottledDeviceRecovery.ShouldRecover(ex, modernAccount, Configuration.Instance).Should().BeTrue();
	}

	[TestMethod]
	public void ShouldRecover_returns_true_when_throttled_and_account_has_obsolete_registration()
	{
		var ex = CreateDeniedException(isThrottled: true);
		var obsoleteAccount = new Account("user@example.com")
		{
			IdentityTokens = CreateIdentity("A2CZJZGLK2JJVM", "0123456789abcdef0123")
		};
		Configuration.Instance.UseWidevine = true;
		Configuration.Instance.DeviceRegistrationKind = DeviceRegistrationKind.CurrentAndroid;

		ThrottledDeviceRecovery.ShouldRecover(ex, obsoleteAccount, Configuration.Instance).Should().BeTrue();
	}

	[TestMethod]
	public void ShouldRecover_returns_false_for_throttled_modern_widevine_account()
	{
		// True rate-limit on a modern account should not trigger registration thunking
		var ex = CreateDeniedException(isThrottled: true);
		var modernAccount = new Account("user@example.com")
		{
			IdentityTokens = CreateIdentity(AudibleApi.Resources.DeviceType, "0123456789abcdef0123")
		};
		Configuration.Instance.UseWidevine = true;
		Configuration.Instance.DeviceRegistrationKind = DeviceRegistrationKind.CurrentAndroid;

		ThrottledDeviceRecovery.ShouldRecover(ex, modernAccount, Configuration.Instance).Should().BeFalse();
	}

	[TestMethod]
	public void ApplyRecovery_when_widevine_disabled_thunks_settings_and_marks_all_accounts()
	{
		var config = Configuration.Instance;
		config.UseWidevine = false;
		config.DeviceRegistrationKind = DeviceRegistrationKind.Mkb79IPhone;

		var settings = new AccountsSettings();
		var primary = new Account("primary@example.com")
		{
			IdentityTokens = CreateIdentity("A2CZJZGLK2JJVM", "0123456789abcdef0123")
		};
		var second = new Account("second@example.com")
		{
			IdentityTokens = CreateIdentity(AudibleApi.Resources.DeviceType, "0123456789abcdef0123")
		};
		settings.Add(primary);
		settings.Add(second);

		var result = ThrottledDeviceRecovery.ApplyRecovery(settings, config, primary);

		result.WidevineEnabled.Should().BeTrue();
		result.DeviceRegistrationUpdated.Should().BeTrue();
		config.UseWidevine.Should().BeTrue();
		Assert.AreEqual(DeviceRegistrationKind.CurrentAndroid, config.DeviceRegistrationKind);

		// When Widevine is changed from disabled to enabled, ALL accounts must re-login
		primary.RequiresReRegistration.Should().BeTrue();
		second.RequiresReRegistration.Should().BeTrue();
		CollectionAssert.Contains(result.MarkedAccounts.ToList(), primary);
		CollectionAssert.Contains(result.MarkedAccounts.ToList(), second);
	}

	[TestMethod]
	public void ApplyRecovery_when_widevine_already_enabled_only_marks_throttled_and_obsolete_accounts()
	{
		var config = Configuration.Instance;
		config.UseWidevine = true;
		config.DeviceRegistrationKind = DeviceRegistrationKind.CurrentAndroid;

		var settings = new AccountsSettings();
		var throttled = new Account("throttled@example.com")
		{
			IdentityTokens = CreateIdentity("A2CZJZGLK2JJVM", "0123456789abcdef0123")
		};
		var otherObsolete = new Account("other_old@example.com")
		{
			IdentityTokens = CreateIdentity(AudibleApi.Resources.DeviceType, "old-32-char-serial-0123456789012")
		};
		var modern = new Account("modern@example.com")
		{
			IdentityTokens = CreateIdentity(AudibleApi.Resources.DeviceType, "0123456789abcdef0123")
		};
		settings.Add(throttled);
		settings.Add(otherObsolete);
		settings.Add(modern);

		var result = ThrottledDeviceRecovery.ApplyRecovery(settings, config, throttled);

		result.WidevineEnabled.Should().BeFalse();
		result.DeviceRegistrationUpdated.Should().BeFalse();

		throttled.RequiresReRegistration.Should().BeTrue();
		otherObsolete.RequiresReRegistration.Should().BeTrue();
		modern.RequiresReRegistration.Should().BeFalse();

		CollectionAssert.Contains(result.MarkedAccounts.ToList(), throttled);
		CollectionAssert.Contains(result.MarkedAccounts.ToList(), otherObsolete);
		Assert.IsFalse(result.MarkedAccounts.Contains(modern));
	}

	[TestMethod]
	public void MarkAllAccountsForWidevine_marks_all_accounts_and_sets_CurrentAndroid()
	{
		var config = Configuration.Instance;
		config.DeviceRegistrationKind = DeviceRegistrationKind.Mkb79IPhone;

		var settings = new AccountsSettings();
		var acc1 = new Account("one@example.com");
		var acc2 = new Account("two@example.com");
		settings.Add(acc1);
		settings.Add(acc2);

		ThrottledDeviceRecovery.MarkAllAccountsForWidevine(settings, config);

		Assert.AreEqual(DeviceRegistrationKind.CurrentAndroid, config.DeviceRegistrationKind);
		acc1.RequiresReRegistration.Should().BeTrue();
		acc2.RequiresReRegistration.Should().BeTrue();
	}

	[TestMethod]
	public void MarkObsoleteAccounts_only_marks_accounts_with_obsolete_registrations()
	{
		var settings = new AccountsSettings();
		var obsolete = new Account("obsolete@example.com")
		{
			IdentityTokens = CreateIdentity("A2CZJZGLK2JJVM", "0123456789abcdef0123")
		};
		var modern = new Account("modern@example.com")
		{
			IdentityTokens = CreateIdentity(AudibleApi.Resources.DeviceType, "0123456789abcdef0123")
		};
		settings.Add(obsolete);
		settings.Add(modern);

		var marked = ThrottledDeviceRecovery.MarkObsoleteAccounts(settings);

		marked.Should().Be(1);
		obsolete.RequiresReRegistration.Should().BeTrue();
		modern.RequiresReRegistration.Should().BeFalse();
	}
}
