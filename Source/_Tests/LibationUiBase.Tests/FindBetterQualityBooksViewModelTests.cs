using AudibleApi;
using AudibleApi.Authorization;
using AudibleApi.Common;
using AudibleApi.Cryptography;
using AudibleUtilities;
using ApplicationServices;
using DataLayer;
using Dinah.Core.Security;
using LibationFileManager;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace LibationUiBase.Tests;

[TestClass]
[DoNotParallelize]
public class FindBetterQualityBooksViewModelTests
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

	private string tempDir = string.Empty;

	[TestInitialize]
	public void Initialize()
	{
		SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());
		tempDir = Path.Combine(Path.GetTempPath(), $"libation-vm-test-{Guid.NewGuid():N}");
		Directory.CreateDirectory(tempDir);
		Environment.SetEnvironmentVariable(LibationFiles.LIBATION_FILES_DIR, tempDir);
		Configuration.CreateMockInstance();
	}

	[TestCleanup]
	public void Cleanup()
	{
		Configuration.RestoreSingletonInstance();
		Environment.SetEnvironmentVariable(LibationFiles.LIBATION_FILES_DIR, null);
		SynchronizationContext.SetSynchronizationContext(null);
		try
		{
			Directory.Delete(tempDir, recursive: true);
		}
		catch
		{
		}
	}

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

	private static Account CreateAccount(string email, string serialNumber, string? deviceType = null)
	{
		return new Account(email)
		{
			IdentityTokens = CreateIdentity(deviceType ?? AudibleApi.Resources.DeviceType, serialNumber)
		};
	}

	private static LibraryBook InsertBook(string title, LiberatedStatus bookStatus = LiberatedStatus.Liberated)
	{
		var libraryBook = MockLibraryBook.CreateBook(title: title, bookStatus: bookStatus);

		using (var context = DbContexts.GetContext())
		{
			context.LibraryBooks.Add(new LibraryBook(libraryBook.Book, libraryBook.DateAdded, libraryBook.Account));
			context.SaveChanges();
		}

		return DbContexts.GetLibraryBook_Flat_NoTracking(libraryBook.Book.AudibleProductId)!;
	}

	[TestMethod]
	public async Task MarkBooksAsync_NoSignificantBooks_ReturnsZeroAndDoesNotModifyConfig()
	{
		Configuration.Instance.UseWidevine = false;
		Configuration.Instance.Request_xHE_AAC = false;

		var vm = new FindBetterQualityBooksViewModel
		{
			Books = new List<BookDataViewModel>()
		};

		var result = await vm.MarkBooksAsync();

		Assert.AreEqual(0, result.MarkedCount);
		Assert.IsFalse(result.WidevineEnabled);
		Assert.IsFalse(result.XHeAacRequested);
		Assert.IsFalse(result.AccountsMarkedForReRegistration);
		Assert.IsNull(result.GetSummaryMessage());

		Assert.IsFalse(Configuration.Instance.UseWidevine);
		Assert.IsFalse(Configuration.Instance.Request_xHE_AAC);
	}

	[TestMethod]
	public async Task MarkBooksAsync_SignificantWidevineBooks_EnablesWidevineAndMarksAccounts()
	{
		Configuration.Instance.UseWidevine = false;
		Configuration.Instance.Request_xHE_AAC = false;
		AudibleApiStorage.EnsureAccountsSettingsFileExists();

		var account = CreateAccount("test@example.com", "0123456789abcdef0123");
		using (var persister = AudibleApiStorage.GetAccountsSettingsPersister())
		{
			persister.AccountsSettings.Add(account);
		}

		var book = InsertBook("High Quality Book");
		var bookVm = new BookDataViewModel(book)
		{
			Bitrate = 64,
			AvailableBitrate = 128,
			AvailableCodec = "xHE-AAC"
		};
		Assert.IsTrue(bookVm.IsSignificant);

		var vm = new FindBetterQualityBooksViewModel
		{
			Books = new List<BookDataViewModel> { bookVm },
			ScanWidevine = true
		};

		var result = await vm.MarkBooksAsync();

		Assert.AreEqual(1, result.MarkedCount);
		Assert.IsTrue(result.WidevineEnabled);
		Assert.IsTrue(result.XHeAacRequested);
		Assert.IsTrue(result.AccountsMarkedForReRegistration);
		Assert.IsNotNull(result.GetSummaryMessage());
		StringAssert.Contains(result.GetSummaryMessage()!, "Marked 1 book as Download Pending.");
		StringAssert.Contains(result.GetSummaryMessage()!, "Libation has automatically configured your download settings");

		Assert.IsTrue(Configuration.Instance.UseWidevine);
		Assert.IsTrue(Configuration.Instance.Request_xHE_AAC);
		Assert.AreEqual(DeviceRegistrationKind.CurrentAndroid, Configuration.Instance.DeviceRegistrationKind);
		Assert.AreEqual(0, vm.Books.Count);

		using (var persister = AudibleApiStorage.GetAccountsSettingsPersister())
		{
			var reloaded = persister.AccountsSettings.Accounts[0];
			Assert.IsTrue(reloaded.RequiresReRegistration);
		}
	}

	[TestMethod]
	public async Task MarkBooksAsync_AlreadyWidevine_OnlyMarksObsoleteAccounts()
	{
		Configuration.Instance.UseWidevine = true;
		Configuration.Instance.Request_xHE_AAC = true;
		AudibleApiStorage.EnsureAccountsSettingsFileExists();

		var obsoleteAccount = CreateAccount("legacy@example.com", "0123456789abcdef"); // 16 hex: obsolete
		var modernAccount = CreateAccount("modern@example.com", "0123456789abcdef0123"); // 20 hex: modern
		using (var persister = AudibleApiStorage.GetAccountsSettingsPersister())
		{
			persister.AccountsSettings.Add(obsoleteAccount);
			persister.AccountsSettings.Add(modernAccount);
		}

		var book = InsertBook("Another High Quality Book");
		var bookVm = new BookDataViewModel(book)
		{
			Bitrate = 64,
			AvailableBitrate = 128,
			AvailableCodec = "xHE-AAC"
		};

		var vm = new FindBetterQualityBooksViewModel
		{
			Books = new List<BookDataViewModel> { bookVm },
			ScanWidevine = true
		};

		var result = await vm.MarkBooksAsync();

		Assert.AreEqual(1, result.MarkedCount);
		Assert.IsFalse(result.WidevineEnabled); // was already true
		Assert.IsFalse(result.XHeAacRequested); // was already true
		Assert.IsTrue(result.AccountsMarkedForReRegistration);

		using (var persister = AudibleApiStorage.GetAccountsSettingsPersister())
		{
			Assert.IsTrue(persister.AccountsSettings.Accounts[0].RequiresReRegistration);
			Assert.IsFalse(persister.AccountsSettings.Accounts[1].RequiresReRegistration);
		}
	}
}
