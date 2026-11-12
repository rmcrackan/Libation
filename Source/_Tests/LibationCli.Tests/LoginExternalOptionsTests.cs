using AudibleApi;
using AudibleApi.Authorization;
using AudibleApi.Common;
using AudibleApi.Cryptography;
using AudibleUtilities;
using LibationFileManager;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace LibationCli.Tests;

[TestClass]
[DoNotParallelize]
public class LoginExternalOptionsTests
{
	[TestInitialize]
	public void Initialize() => Configuration.CreateMockInstance();

	[TestCleanup]
	public void Cleanup() => Configuration.RestoreSingletonInstance();

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
	private const string SampleAdpToken = "{enc:abcdefg}{key:1234}{iv:56789}{name:QURQVG9rZW5FbmNyeXB0aW9uS2V5}{serial:Mg==}";

	private static Identity CreateValidIdentity(string? deviceType = null, string? deviceSerialNumber = null)
	{
		var identity = new Identity(Localization.Get("us"));
		identity.Update(
			new PrivateKey(SamplePrivateKey),
			new AdpToken(SampleAdpToken),
			new AccessToken("Atna|dummy-access-token", new DateTime(2200, 1, 1)),
			new RefreshToken("Atnr|dummy-refresh-token"),
			null,
			deviceSerialNumber: deviceSerialNumber ?? "0123456789ABCDEF0123",
			deviceType: deviceType ?? AudibleApi.Resources.DeviceType);
		return identity;
	}

	[TestMethod]
	public void EvaluateLoginRequirement_null_or_invalid_tokens_returns_NotAuthenticated()
	{
		Assert.AreEqual(
			LoginExternalOptions.LoginRequiredReason.NotAuthenticated,
			LoginExternalOptions.EvaluateLoginRequirement(null, ApiAuthenticationMethod.AdpSignature, force: false));

		var account = new Account("test@example.com");
		Assert.AreEqual(
			LoginExternalOptions.LoginRequiredReason.NotAuthenticated,
			LoginExternalOptions.EvaluateLoginRequirement(account, ApiAuthenticationMethod.AdpSignature, force: false));

		account.IdentityTokens = new Identity(Localization.Get("us"));
		Assert.AreEqual(
			LoginExternalOptions.LoginRequiredReason.NotAuthenticated,
			LoginExternalOptions.EvaluateLoginRequirement(account, ApiAuthenticationMethod.AdpSignature, force: false));
	}

	[TestMethod]
	public void EvaluateLoginRequirement_valid_tokens_without_force_returns_AlreadyAuthenticated()
	{
		var account = new Account("test@example.com")
		{
			IdentityTokens = CreateValidIdentity()
		};

		Assert.AreEqual(
			LoginExternalOptions.LoginRequiredReason.AlreadyAuthenticated,
			LoginExternalOptions.EvaluateLoginRequirement(account, ApiAuthenticationMethod.AdpSignature | ApiAuthenticationMethod.BearerToken, force: false));
	}

	[TestMethod]
	public void EvaluateLoginRequirement_valid_tokens_with_force_returns_Forced()
	{
		var account = new Account("test@example.com")
		{
			IdentityTokens = CreateValidIdentity()
		};

		Assert.AreEqual(
			LoginExternalOptions.LoginRequiredReason.Forced,
			LoginExternalOptions.EvaluateLoginRequirement(account, ApiAuthenticationMethod.AdpSignature | ApiAuthenticationMethod.BearerToken, force: true));
	}

	[TestMethod]
	public void EvaluateLoginRequirement_missing_required_tokens_returns_MissingRequiredTokens()
	{
		var account = new Account("test@example.com")
		{
			IdentityTokens = CreateValidIdentity()
		};

		// When an additional or missing method is required but missing
		var unavailableMethod = (ApiAuthenticationMethod)8;
		Assert.AreEqual(
			LoginExternalOptions.LoginRequiredReason.MissingRequiredTokens,
			LoginExternalOptions.EvaluateLoginRequirement(account, unavailableMethod, force: false));

		// Missing token takes precedence even if force is true
		Assert.AreEqual(
			LoginExternalOptions.LoginRequiredReason.MissingRequiredTokens,
			LoginExternalOptions.EvaluateLoginRequirement(account, ApiAuthenticationMethod.AdpSignature | unavailableMethod, force: true));
	}

	[TestMethod]
	public void EvaluateLoginRequirement_marked_re_registration_returns_RequiresReRegistration()
	{
		var account = new Account("test@example.com")
		{
			IdentityTokens = CreateValidIdentity(),
			RequiresReRegistration = true
		};

		Assert.AreEqual(
			LoginExternalOptions.LoginRequiredReason.RequiresReRegistration,
			LoginExternalOptions.EvaluateLoginRequirement(account, ApiAuthenticationMethod.AdpSignature | ApiAuthenticationMethod.BearerToken, force: false));

		// Takes precedence over force flag
		Assert.AreEqual(
			LoginExternalOptions.LoginRequiredReason.RequiresReRegistration,
			LoginExternalOptions.EvaluateLoginRequirement(account, ApiAuthenticationMethod.AdpSignature | ApiAuthenticationMethod.BearerToken, force: true));
	}

	[TestMethod]
	public void EvaluateLoginRequirement_obsolete_device_registration_returns_ObsoleteDeviceRegistration()
	{
		var account = new Account("test@example.com")
		{
			IdentityTokens = CreateValidIdentity(deviceType: "A1MPSLFC7L5AFK"),
			RequiresReRegistration = false
		};

		Assert.AreEqual(
			LoginExternalOptions.LoginRequiredReason.ObsoleteDeviceRegistration,
			LoginExternalOptions.EvaluateLoginRequirement(account, ApiAuthenticationMethod.AdpSignature | ApiAuthenticationMethod.BearerToken, force: false));

		// Takes precedence over force flag
		Assert.AreEqual(
			LoginExternalOptions.LoginRequiredReason.ObsoleteDeviceRegistration,
			LoginExternalOptions.EvaluateLoginRequirement(account, ApiAuthenticationMethod.AdpSignature | ApiAuthenticationMethod.BearerToken, force: true));
	}

	[TestMethod]
	public void CommandLine_parsing_supports_force_flag()
	{
		var parser = new CommandLine.Parser(with => with.CaseInsensitiveEnumValues = true);

		var resultShort = parser.ParseArguments<LoginExternalOptions>(new[] { "-a", "user@example.com", "-l", "us", "-f" });
		Assert.IsInstanceOfType<CommandLine.Parsed<LoginExternalOptions>>(resultShort);
		Assert.IsTrue(((CommandLine.Parsed<LoginExternalOptions>)resultShort).Value.Force);

		var resultLong = parser.ParseArguments<LoginExternalOptions>(new[] { "--account", "user@example.com", "--locale", "us", "--force" });
		Assert.IsInstanceOfType<CommandLine.Parsed<LoginExternalOptions>>(resultLong);
		Assert.IsTrue(((CommandLine.Parsed<LoginExternalOptions>)resultLong).Value.Force);

		var resultDefault = parser.ParseArguments<LoginExternalOptions>(new[] { "-a", "user@example.com", "-l", "us" });
		Assert.IsInstanceOfType<CommandLine.Parsed<LoginExternalOptions>>(resultDefault);
		Assert.IsFalse(((CommandLine.Parsed<LoginExternalOptions>)resultDefault).Value.Force);
	}

	[TestMethod]
	public void Omitted_flag_uses_the_Settings_value()
	{
		Configuration.Instance.DeviceRegistrationKind = DeviceRegistrationKind.RetailAndroid;
		var options = new LoginExternalOptions();

		Assert.IsTrue(options.TryResolveRegistrationProfile(out var profile, out var error));
		Assert.AreEqual("", error);
		Assert.AreEqual(DeviceRegistrationKind.CurrentAndroid, profile.Kind);
	}

	[TestMethod]
	public void Flag_overrides_Settings()
	{
		Configuration.Instance.DeviceRegistrationKind = DeviceRegistrationKind.CurrentAndroid;
		var options = new LoginExternalOptions { DeviceRegistration = "Mkb79IPhone" };

		Assert.IsTrue(options.TryResolveRegistrationProfile(out var profile, out var error));
		Assert.AreEqual("", error);
		Assert.AreEqual(DeviceRegistrationKind.Mkb79IPhone, profile.Kind);
	}

	[TestMethod]
	public void Unknown_flag_fails()
	{
		var options = new LoginExternalOptions { DeviceRegistration = "WindowsPhone" };

		Assert.IsFalse(options.TryResolveRegistrationProfile(out _, out var error));
		StringAssert.Contains(error, "WindowsPhone");
		StringAssert.Contains(error, "CurrentAndroid");
	}
}


