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
using System.Security.Cryptography;

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

	/// <summary>
	/// Set to "1" to also run the tests that talk to the real OS secret store.
	/// </summary>
	private const string OsSecretStoreTestsEnvVar = "LIBATION_TEST_OS_SECRET_STORE";

	/// <summary>
	/// Reading the OS secret store blocks until the desktop unlock prompt is answered, which can be
	/// forever on a headless or locked-keyring machine. Probing availability first does not help: the
	/// probe is the blocking call. Tests that need the real store are therefore opt-in.
	/// </summary>
	private static void SkipUnlessOsSecretStoreTestsEnabled()
	{
		if (Environment.GetEnvironmentVariable(OsSecretStoreTestsEnvVar) != "1")
			Assert.Inconclusive($"Set {OsSecretStoreTestsEnvVar}=1 to exercise the real OS secret store.");
	}

	/// <summary>
	/// Resolve the master key from a temp key file so <see cref="IdentityTokenStorageWiring.ResolveSecretStore"/>
	/// short-circuits before it reaches the OS secret store. Tests about which write method gets configured
	/// have no business depending on a desktop keyring, and must not mint a key into the real Libation folder.
	/// </summary>
	private void UsePortableMasterKey()
	{
		Environment.SetEnvironmentVariable(LibationFiles.LIBATION_FILES_DIR, _tempDir);
		Environment.SetEnvironmentVariable(IdentityTokenStorageWiring.MasterKeyFileEnvVar, WriteTempMasterKeyFile());
	}

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
		Environment.SetEnvironmentVariable(IdentityTokenStorageWiring.MasterKeyFileEnvVar, null);
		Environment.SetEnvironmentVariable(IdentityTokenStorageWiring.MasterKeyEnvVar, null);
		Environment.SetEnvironmentVariable(LibationFiles.LIBATION_FILES_DIR, null);

		if (_tempDir is not null && Directory.Exists(_tempDir))
		{
			try { Directory.Delete(_tempDir, recursive: true); }
			catch { /* best effort */ }
		}
	}

	[TestMethod]
	public void Apply_Encrypted_configures_write_method_and_protector()
	{
		UsePortableMasterKey();
		var config = Configuration.CreateMockInstance();
		config.TokenStorageMethod = TokenStorageMethod.Encrypted;

		IdentityTokenStorageWiring.Apply(config);

		Assert.AreEqual(TokenStorageMethod.Encrypted, IdentityTokenStorage.WriteMethod);
		Assert.IsNotNull(IdentityTokenStorage.Protector);
	}

	[TestMethod]
	public void ResolveSecretStore_uses_master_key_file_env_before_os_store()
	{
		var keyPath = WriteTempMasterKeyFile();
		Environment.SetEnvironmentVariable(IdentityTokenStorageWiring.MasterKeyFileEnvVar, keyPath);
		Environment.SetEnvironmentVariable(LibationFiles.LIBATION_FILES_DIR, _tempDir);

		var config = Configuration.CreateMockInstance();
		var store = IdentityTokenStorageWiring.ResolveSecretStore(config);

		store.Name.Should().Be("Memory");
		store.TryGet(AesGcmSecretProtector.DefaultMasterKeyName, out var key).Should().BeTrue();
		key.Length.Should().Be(AesGcmSecretProtector.KeySizeBytes);

		IdentityTokenStorageWiring.ConfigureFrom(config);
		Assert.IsNotNull(IdentityTokenStorage.Protector);
		var payload = IdentityTokenStorage.Protector!.Protect("portable-secret", "aad");
		IdentityTokenStorage.Protector.Unprotect(payload, "aad").Should().Be("portable-secret");
	}

	[TestMethod]
	public void ResolveSecretStore_uses_default_libation_master_key_file()
	{
		Environment.SetEnvironmentVariable(LibationFiles.LIBATION_FILES_DIR, _tempDir);
		var config = Configuration.CreateMockInstance();
		var defaultPath = Path.Combine(config.LibationFiles.Location, IdentityTokenStorageWiring.DefaultMasterKeyFileName);
		WriteMasterKeyFile(defaultPath);

		var store = IdentityTokenStorageWiring.ResolveSecretStore(config);
		store.Name.Should().Be("Memory");
		store.TryGet(AesGcmSecretProtector.DefaultMasterKeyName, out _).Should().BeTrue();
	}

	[TestMethod]
	public void ResolveSecretStore_uses_master_key_env_base64()
	{
		var key = new byte[AesGcmSecretProtector.KeySizeBytes];
		RandomNumberGenerator.Fill(key);
		Environment.SetEnvironmentVariable(IdentityTokenStorageWiring.MasterKeyEnvVar, Convert.ToBase64String(key));
		Environment.SetEnvironmentVariable(LibationFiles.LIBATION_FILES_DIR, _tempDir);

		var config = Configuration.CreateMockInstance();
		var store = IdentityTokenStorageWiring.ResolveSecretStore(config);

		store.Name.Should().Be("Memory");
		store.TryGet(AesGcmSecretProtector.DefaultMasterKeyName, out var loaded).Should().BeTrue();
		CollectionAssert.AreEqual(key, loaded);
	}

	[TestMethod]
	public void ResolveSecretStore_invalid_master_key_env_fails_closed_without_falling_through()
	{
		Environment.SetEnvironmentVariable(IdentityTokenStorageWiring.MasterKeyEnvVar, "not-valid-base64!!!");
		Environment.SetEnvironmentVariable(LibationFiles.LIBATION_FILES_DIR, _tempDir);

		var config = Configuration.CreateMockInstance();
		var store = IdentityTokenStorageWiring.ResolveSecretStore(config);

		store.IsAvailable.Should().BeFalse();
		store.Name.Should().Be("Portable master key env");

		config.TokenStorageMethod = TokenStorageMethod.Encrypted;
		IdentityTokenStorageWiring.ConfigureFrom(config);
		Assert.IsNull(IdentityTokenStorage.Protector);
	}

	[TestMethod]
	public void ResolveSecretStore_missing_master_key_file_env_fails_closed()
	{
		var missing = Path.Combine(_tempDir!, "missing-master.key");
		Environment.SetEnvironmentVariable(IdentityTokenStorageWiring.MasterKeyFileEnvVar, missing);
		Environment.SetEnvironmentVariable(LibationFiles.LIBATION_FILES_DIR, _tempDir);

		var config = Configuration.CreateMockInstance();
		var store = IdentityTokenStorageWiring.ResolveSecretStore(config);

		store.IsAvailable.Should().BeFalse();
		IdentityTokenStorageWiring.IsEncryptionKeyAvailable(config, out var reason).Should().BeFalse();
		StringAssert.Contains(reason, "unusable");
	}

	[TestMethod]
	public void Apply_Plaintext_configures_plaintext_writes()
	{
		UsePortableMasterKey();
		var config = Configuration.CreateMockInstance();
		config.TokenStorageMethod = TokenStorageMethod.Plaintext;

		IdentityTokenStorageWiring.Apply(config);

		Assert.AreEqual(TokenStorageMethod.Plaintext, IdentityTokenStorage.WriteMethod);
	}

	[TestMethod]
	public void Changing_preference_updates_write_method_without_converting_existing_tokens()
	{
		UsePortableMasterKey();
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
			new List<KeyValuePair<string, SecretString>> { new(SampleCookieName, SampleCookieValue) },
			deviceSerialNumber: "device-serial",
			deviceType: "device-type",
			amazonAccountId: "amzn-account",
			deviceName: "device-name",
			storeAuthenticationCookie: SampleStoreAuthCookie);
		return identity;
	}

	[TestMethod]
	public void ResolveSecretStore_uses_os_store_when_available_without_last_resort_key_file()
	{
		SkipUnlessOsSecretStoreTestsEnabled();
		if (!IdentityTokenStorageWiring.IsOsSecretStoreAvailable(out var reason))
			Assert.Inconclusive("OS secret store unavailable: " + reason);

		Environment.SetEnvironmentVariable(LibationFiles.LIBATION_FILES_DIR, _tempDir);
		var config = Configuration.CreateMockInstance();
		var defaultPath = Path.Combine(config.LibationFiles.Location, IdentityTokenStorageWiring.DefaultMasterKeyFileName);

		var store = IdentityTokenStorageWiring.ResolveSecretStore(config);

		store.IsAvailable.Should().BeTrue();
		Assert.IsFalse(File.Exists(defaultPath), "OS store available => must not mint last-resort key file");
	}

	[TestMethod]
	public void TryCreateLastResortPortableMasterKeyStore_mints_default_key_file_and_loads_protector()
	{
		Environment.SetEnvironmentVariable(LibationFiles.LIBATION_FILES_DIR, _tempDir);
		var config = Configuration.CreateMockInstance();
		var defaultPath = Path.Combine(config.LibationFiles.Location, IdentityTokenStorageWiring.DefaultMasterKeyFileName);
		File.Exists(defaultPath).Should().BeFalse();

		var store = IdentityTokenStorageWiring.TryCreateLastResortPortableMasterKeyStore(config);

		store.IsAvailable.Should().BeTrue();
		store.Name.Should().Be("Memory");
		File.Exists(defaultPath).Should().BeTrue();
		File.ReadAllBytes(defaultPath).Length.Should().Be(AesGcmSecretProtector.KeySizeBytes);
		var noticePath = Path.Combine(config.LibationFiles.Location, IdentityTokenStorageWiring.LastResortMasterKeyNoticeFileName);
		File.Exists(noticePath).Should().BeTrue();
		var notice = File.ReadAllText(noticePath);
		StringAssert.Contains(notice, "LAST-RESORT");
		StringAssert.Contains(notice, "Plaintext");
		StringAssert.Contains(notice, IdentityTokenStorageWiring.MasterKeyFileEnvVar);
		store.TryGet(AesGcmSecretProtector.DefaultMasterKeyName, out var key).Should().BeTrue();
		key.Length.Should().Be(AesGcmSecretProtector.KeySizeBytes);

		// Second call must reuse the file, not mint a different key.
		var store2 = IdentityTokenStorageWiring.TryCreateLastResortPortableMasterKeyStore(config);
		store2.TryGet(AesGcmSecretProtector.DefaultMasterKeyName, out var key2).Should().BeTrue();
		CollectionAssert.AreEqual(key, key2);

		config.TokenStorageMethod = TokenStorageMethod.Encrypted;
		IdentityTokenStorageWiring.ConfigureFrom(config);
		Assert.IsNotNull(IdentityTokenStorage.Protector);
		var payload = IdentityTokenStorage.Protector!.Protect("last-resort-secret", "aad");
		IdentityTokenStorage.Protector.Unprotect(payload, "aad").Should().Be("last-resort-secret");
	}

	[TestMethod]
	public void AnnounceLastResortPortableMasterKey_writes_notice_beside_key()
	{
		var keyPath = Path.Combine(_tempDir!, IdentityTokenStorageWiring.DefaultMasterKeyFileName);
		File.WriteAllBytes(keyPath, new byte[AesGcmSecretProtector.KeySizeBytes]);

		IdentityTokenStorageWiring.AnnounceLastResortPortableMasterKey(keyPath);

		var noticePath = Path.Combine(_tempDir!, IdentityTokenStorageWiring.LastResortMasterKeyNoticeFileName);
		File.Exists(noticePath).Should().BeTrue();
		var notice = File.ReadAllText(noticePath);
		StringAssert.Contains(notice, "LAST-RESORT");
		StringAssert.Contains(notice, keyPath);
		StringAssert.Contains(notice, "compatibility fallback");
	}

	[TestMethod]
	public void MasterKeyExport_writes_key_file_when_os_store_has_key()
	{
		SkipUnlessOsSecretStoreTestsEnabled();
		if (!IdentityTokenStorageWiring.IsOsSecretStoreAvailable(out var reason))
			Assert.Inconclusive("OS secret store unavailable: " + reason);

		Environment.SetEnvironmentVariable(LibationFiles.LIBATION_FILES_DIR, _tempDir);
		var config = Configuration.CreateMockInstance();
		config.TokenStorageMethod = TokenStorageMethod.Encrypted;
		IdentityTokenStorageWiring.ConfigureFrom(config);

		Assert.IsNotNull(IdentityTokenStorage.Protector);
		_ = IdentityTokenStorage.Protector!.Protect("seed-master-key");

		var exportPath = Path.Combine(_tempDir!, "libation-master.key");
		MasterKeyExport.ExportToFile(exportPath);

		File.Exists(exportPath).Should().BeTrue();
		File.ReadAllBytes(exportPath).Length.Should().Be(AesGcmSecretProtector.KeySizeBytes);

		// Portable load of the exported file can decrypt ciphertext from the OS-backed protector.
		var payload = IdentityTokenStorage.Protector.Protect("roundtrip-secret", "aad");
		Environment.SetEnvironmentVariable(IdentityTokenStorageWiring.MasterKeyFileEnvVar, exportPath);
		IdentityTokenStorageWiring.ConfigureFrom(config);
		IdentityTokenStorage.Protector!.Unprotect(payload, "aad").Should().Be("roundtrip-secret");
	}

	private string WriteTempMasterKeyFile()
	{
		var path = Path.Combine(_tempDir!, "exported-master.key");
		WriteMasterKeyFile(path);
		return path;
	}

	private static void WriteMasterKeyFile(string path)
	{
		var key = new byte[AesGcmSecretProtector.KeySizeBytes];
		RandomNumberGenerator.Fill(key);
		Directory.CreateDirectory(Path.GetDirectoryName(path)!);
		File.WriteAllBytes(path, key);
	}
}
