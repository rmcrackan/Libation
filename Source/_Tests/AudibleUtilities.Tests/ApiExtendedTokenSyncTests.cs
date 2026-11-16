using AudibleApi;
using AudibleApi.Authorization;
using AudibleApi.Common;
using AudibleApi.Cryptography;
using Dinah.Core.Security;
using LibationFileManager;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;

namespace AudibleUtilities.Tests;

[TestClass]
[DoNotParallelize]
public class ApiExtendedTokenSyncTests
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

	private const string OldAdp = "{enc:OLD}{key:1234}{iv:56789}{name:QURQVG9rZW5FbmNyeXB0aW9uS2V5}{serial:Mg==}";
	private const string NewAdp = "{enc:NEW}{key:1234}{iv:56789}{name:QURQVG9rZW5FbmNyeXB0aW9uS2V5}{serial:Mg==}";

	[TestInitialize]
	public void Initialize() => Configuration.CreateMockInstance();

	[TestMethod]
	public void SyncAccountFromDiskAfterLogin_preserves_new_tokens_when_caller_persister_is_open()
	{
		var file = Path.Combine(Path.GetTempPath(), $"libation_sync_test_{Guid.NewGuid():N}.json");
		try
		{
			var seed = new AccountsSettings();
			seed.Add(new Account("u@x.com") { IdentityTokens = MakeIdentity(OldAdp), RequiresReRegistration = true });
			using (new AccountsSettingsPersister(seed, file)) { }

			// Caller (e.g. UtilityExtensions.GetApiAsync) holds this persister open for the entire CreateAsync call.
			using var callerPersister = new AccountsSettingsPersister(file);
			var account = callerPersister.AccountsSettings.GetAccount("u@x.com", "us")!;

			// CreateAsync invalidates both in-memory and on-disk tokens when RequiresReRegistration is true.
			account.IdentityTokens?.Invalidate();
			using (var p = new AccountsSettingsPersister(file))
				p.AccountsSettings.GetAccount(account.AccountId, "us")?.IdentityTokens?.Invalidate();

			// Interactive EzApiCreator.GetApiAsync writes a new Identity at the account's JSONPath on disk.
			_ = new IdentityPersister(MakeIdentity(NewAdp), file, account.GetIdentityTokensJsonPath());

			// Post-login sync must load the new tokens before clearing RequiresReRegistration.
			ApiExtended.SyncAccountFromDiskAfterLogin(account, file);

			Assert.IsFalse(account.RequiresReRegistration);
			Assert.IsNotNull(account.IdentityTokens?.AdpToken);

			using var verifyPersister = new AccountsSettingsPersister(file);
			var persisted = verifyPersister.AccountsSettings.GetAccount(account.AccountId, "us")!;
			Assert.IsFalse(persisted.RequiresReRegistration);
			Assert.IsNotNull(persisted.IdentityTokens?.AdpToken);
		}
		finally
		{
			if (File.Exists(file))
				File.Delete(file);
		}
	}

	private static Identity MakeIdentity(string adp)
	{
		var id = new Identity(Localization.Get("us"));
		id.Update(
			new PrivateKey(SamplePrivateKey),
			new AdpToken(adp),
			new AccessToken("Atna|x", DateTime.UtcNow.AddHours(1)),
			new RefreshToken("Atnr|y"),
			new List<KeyValuePair<string, SecretString>> { new("session-id", new SecretString("c")) },
			deviceSerialNumber: "0123456789abcdef0123",
			deviceType: AudibleApi.Resources.DeviceType,
			amazonAccountId: "amzn",
			deviceName: "dev",
			storeAuthenticationCookie: new SecretString("s"));
		return id;
	}
}
