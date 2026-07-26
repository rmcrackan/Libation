using AssertionHelper;
using AudibleApi;
using AudibleApi.Authorization;
using AudibleApi.Cryptography;
using AudibleUtilities;
using Dinah.Core.Security;
using LibationFileManager;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;

namespace IdentityTokenStorageWiringTests;

[TestClass]
[DoNotParallelize]
public class IdentityTokenStorageWiringTests
{
	// Fixture material only (same shape as AudibleApi characterization tests).
	private const string SampleAccessToken = "Atna|_CHAR_ACCESS_";
	private const string SampleRefreshToken = "Atnr|_CHAR_REFRESH_";
	private const string SampleAdpToken = "{enc:abcdefg}{key:1234}{iv:56789}{name:QURQVG9rZW5FbmNyeXB0aW9uS2V5}{serial:Mg==}";
	private const string SampleStoreAuthCookie = "store-auth-cookie-value";
	private const string SampleCookieName = "session-id";
	private const string SampleCookieValue = "cookie-secret-value";
	private static readonly DateTime SampleExpires = new(2200, 1, 1, 12, 0, 0, DateTimeKind.Utc);
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

	private string? _tempDir;
	private string? _accountsFile;

	[TestInitialize]
	public void Init()
	{
		IdentityTokenStorage.Reset();
		_tempDir = Path.Combine(Path.GetTempPath(), "LibationTokenStorageTests-" + Guid.NewGuid().ToString("N"));
		Directory.CreateDirectory(_tempDir);
		_accountsFile = Path.Combine(_tempDir, "AccountsSettings.json");
	}

	[TestCleanup]
	public void Cleanup()
	{
		IdentityTokenStorage.Reset();
		Configuration.RestoreSingletonInstance();

		if (_tempDir is not null && Directory.Exists(_tempDir))
		{
			try { Directory.Delete(_tempDir, recursive: true); }
			catch { /* best effort */ }
		}
	}

	[TestMethod]
	public void Apply_Encrypted_configures_write_method_and_protector_when_store_available()
	{
		var config = Configuration.CreateMockInstance();
		config.TokenStorageMethod = TokenStorageMethod.Encrypted;

		IdentityTokenStorageWiring.Apply(config);

		Assert.AreEqual(TokenStorageMethod.Encrypted, IdentityTokenStorage.WriteMethod);
		if (IdentityTokenStorageWiring.IsOsSecretStoreAvailable(out _))
			Assert.IsNotNull(IdentityTokenStorage.Protector);
	}

	[TestMethod]
	public void Apply_Plaintext_configures_plaintext_writes()
	{
		var config = Configuration.CreateMockInstance();
		config.TokenStorageMethod = TokenStorageMethod.Plaintext;

		IdentityTokenStorageWiring.Apply(config);

		Assert.AreEqual(TokenStorageMethod.Plaintext, IdentityTokenStorage.WriteMethod);
	}

	[TestMethod]
	public void Changing_preference_updates_write_method_without_converting_existing_tokens()
	{
		WriteLegacyAccountsFile(_accountsFile!);
		var before = File.ReadAllText(_accountsFile!);

		var config = Configuration.CreateMockInstance();
		config.TokenStorageMethod = TokenStorageMethod.Plaintext;
		IdentityTokenStorageWiring.Apply(config);

		config.TokenStorageMethod = TokenStorageMethod.Encrypted;

		Assert.AreEqual(TokenStorageMethod.Encrypted, IdentityTokenStorage.WriteMethod);
		File.ReadAllText(_accountsFile!).Should().Be(before);
		Assert.AreEqual(
			TokenStorageAlignment.SomeMismatch,
			AccountTokenStorage.GetAccountsAlignment(TokenStorageMethod.Encrypted, _accountsFile));
		Assert.AreEqual(
			TokenStorageAlignment.AllMatch,
			AccountTokenStorage.GetAccountsAlignment(TokenStorageMethod.Plaintext, _accountsFile));
	}

	[TestMethod]
	public void New_identity_serialize_uses_configured_write_method()
	{
		var store = new MemoryOsSecretStore();
		var protector = new AesGcmSecretProtector(store, "libation-wiring-tests-master-key");
		IdentityTokenStorage.Configure(TokenStorageMethod.Encrypted, protector);

		var identity = CreateRegisteredIdentity();
		var jo = JObject.Parse(JsonConvert.SerializeObject(identity, Identity.GetJsonSerializerSettings()));

		jo.SelectTokens("$..IsEncrypted").Should().HaveCount(6);
		jo["ExistingAccessToken"]!["IsEncrypted"]!.Value<bool>().Should().BeTrue();
	}

	[TestMethod]
	public void ConvertAllAccounts_is_explicit_and_aligns_file()
	{
		WriteLegacyAccountsFile(_accountsFile!);

		var store = new MemoryOsSecretStore();
		IdentityTokenStorage.Configure(TokenStorageMethod.Encrypted, new AesGcmSecretProtector(store, "libation-convert-tests"));

		var result = AccountTokenStorage.ConvertAllAccounts(TokenStorageMethod.Encrypted, _accountsFile);
		result.Succeeded.Should().BeTrue();
		result.Changed.Should().BeTrue();

		Assert.AreEqual(
			TokenStorageAlignment.AllMatch,
			AccountTokenStorage.GetAccountsAlignment(TokenStorageMethod.Encrypted, _accountsFile));

		var after = JObject.Parse(File.ReadAllText(_accountsFile!));
		after.SelectTokens("$..IsEncrypted").Should().HaveCount(6);
	}

	private static void WriteLegacyAccountsFile(string path)
	{
		var identity = CreateRegisteredIdentity();
		IdentityTokenStorage.Configure(TokenStorageMethod.Plaintext, protector: null);
		var identityJson = JsonConvert.SerializeObject(identity, Identity.GetJsonSerializerSettings());

		var root = new JObject
		{
			["Accounts"] = new JArray
			{
				new JObject
				{
					["AccountId"] = "user@example.com",
					["AccountName"] = "Test",
					["LibraryScan"] = true,
					["DecryptKey"] = "",
					["IdentityTokens"] = JObject.Parse(identityJson)
				}
			},
			["Cdm"] = null
		};
		File.WriteAllText(path, root.ToString(Formatting.Indented));
	}

	private static Identity CreateRegisteredIdentity()
	{
		var identity = new Identity(Localization.Get("us"));
		identity.Update(
			new PrivateKey(SamplePrivateKey),
			new AdpToken(SampleAdpToken),
			new AccessToken(SampleAccessToken, SampleExpires),
			new RefreshToken(SampleRefreshToken),
			new List<KeyValuePair<string, string?>> { new(SampleCookieName, SampleCookieValue) },
			deviceSerialNumber: "device-serial",
			deviceType: "device-type",
			amazonAccountId: "amzn-account",
			deviceName: "device-name",
			storeAuthenticationCookie: SampleStoreAuthCookie);
		return identity;
	}
}
