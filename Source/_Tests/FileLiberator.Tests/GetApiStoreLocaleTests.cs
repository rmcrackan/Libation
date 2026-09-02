using AssertionHelper;
using AudibleApi;
using AudibleApi.Authorization;
using AudibleUtilities;
using DataLayer;
using LibationFileManager;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Threading.Tasks;

namespace FileLiberator.Tests;

/// <summary>
/// A download must speak to the marketplace the book was scanned from, not only the one the account logged
/// into. Issue #2020: a Germany-registered account with UK ticked as an extra marketplace scanned UK titles
/// correctly, then asked <c>api.audible.de</c> for their licenses and got NotFound.
/// </summary>
[TestClass]
[DoNotParallelize]
public class GetApiStoreLocaleTests
{
	private string tempLibationFiles = string.Empty;

	[TestInitialize]
	public void Initialize()
	{
		tempLibationFiles = Path.Combine(Path.GetTempPath(), $"libation-getapi-tests-{Guid.NewGuid():N}");
		Directory.CreateDirectory(tempLibationFiles);

		Environment.SetEnvironmentVariable(LibationFiles.LIBATION_FILES_DIR, tempLibationFiles);
		Configuration.CreateMockInstance();
		AudibleApiStorage.EnsureAccountsSettingsFileExists();
	}

	[TestCleanup]
	public void Cleanup()
	{
		UtilityExtensions.ApiExtendedFunc = null;
		Configuration.RestoreSingletonInstance();
		Environment.SetEnvironmentVariable(LibationFiles.LIBATION_FILES_DIR, null);

		try
		{
			Directory.Delete(tempLibationFiles, recursive: true);
		}
		catch (IOException)
		{
			// A leftover temp directory is not worth failing a test over.
		}
	}

	private static void SeedAccount(string registeredLocale, params string[] extraLocales)
	{
		using var persister = AudibleApiStorage.GetAccountsSettingsPersister();
		var account = new Account("user@example.com")
		{
			AccountName = "Main",
			IdentityTokens = new Identity(Localization.Get(registeredLocale)),
			LibraryScan = true
		};
		persister.AccountsSettings.Add(account);
		foreach (var extra in extraLocales)
			account.AddMarketplace(extra);
	}

	private static async Task<(Account Account, Locale? StoreLocale)> CaptureCreateAsync(LibraryBook libraryBook)
	{
		Account? capturedAccount = null;
		Locale? capturedStore = null;

		UtilityExtensions.ApiExtendedFunc = (account, storeLocale) =>
		{
			capturedAccount = account;
			capturedStore = storeLocale;
			throw new InvalidOperationException("stop before talking to Audible");
		};

		try
		{
			await libraryBook.GetApiAsync();
		}
		catch (InvalidOperationException ex) when (ex.Message == "stop before talking to Audible")
		{
		}

		capturedAccount.BeNotNull();
		return (capturedAccount, capturedStore);
	}

	[TestMethod]
	public async Task A_book_from_an_extra_marketplace_is_licensed_there_not_at_the_home_store()
	{
		SeedAccount("germany", "uk");
		var libraryBook = MockLibraryBook.CreateBook(
			account: "user@example.com",
			localeName: "uk",
			title: "Kill Box");

		var (account, storeLocale) = await CaptureCreateAsync(libraryBook);

		account.Locale!.Name.Should().Be("germany");
		storeLocale.BeNotNull();
		storeLocale.Name.Should().Be("uk");
	}

	[TestMethod]
	public async Task A_book_from_the_registered_marketplace_is_still_licensed_there()
	{
		SeedAccount("germany", "uk");
		var libraryBook = MockLibraryBook.CreateBook(
			account: "user@example.com",
			localeName: "germany",
			title: "Home Store Title");

		var (account, storeLocale) = await CaptureCreateAsync(libraryBook);

		account.Locale!.Name.Should().Be("germany");
		storeLocale.BeNotNull();
		storeLocale.Name.Should().Be("germany");
	}
}
