using AssertionHelper;
using Dinah.Core.Security;
using AudibleApi;
using AudibleApi.Authorization;
using AudibleApi.Cryptography;
using AudibleUtilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

[assembly: Parallelize]

namespace AccountsTests;

#pragma warning disable CS8981 // The type name only contains lower-cased ascii characters. Such names may become reserved for the language.
public class AccountsTestBase
{
	protected string EMPTY_FILE { get; } = "{\r\n  \"Accounts\": [],\r\n  \"Cdm\": null\r\n}".Replace("\r\n", Environment.NewLine);

	private string? _testFile;
	protected Locale usLocale => Localization.Get("us");
	protected Locale ukLocale => Localization.Get("uk");

	protected void WriteToTestFile(string contents)
		=> File.WriteAllText(TestFile, contents);

	protected string TestFile => _testFile ??= Guid.NewGuid() + ".txt";

	[TestInitialize]
	public void TestInit()
			=> _ = TestFile;

	[TestCleanup]
	public void TestCleanup()
	{
		if (File.Exists(TestFile))
			File.Delete(TestFile);
	}
}

[TestClass]
public class FromJson : AccountsTestBase
{
	[TestMethod]
	public void _0_accounts()
	{
		var accountsSettings = AccountsSettings.FromJson(EMPTY_FILE);
		accountsSettings.BeNotNull();
		accountsSettings.Accounts.Count.Should().Be(0);
	}

	[TestMethod]
	public void _1_account_new()
	{
		var json = @"
{
  ""Accounts"": [
	{
      ""AccountId"": ""cng"",
      ""AccountName"": ""my main login"",
      ""DecryptKey"": ""asdfasdf"",
      ""IdentityTokens"": null
    }
  ]
}
".Trim();
		var accountsSettings = AccountsSettings.FromJson(json);
		accountsSettings.BeNotNull();
		accountsSettings.Accounts.Count.Should().Be(1);
		accountsSettings.Accounts[0].AccountId.Should().Be("cng");
		accountsSettings.Accounts[0].IdentityTokens.Should().BeNull();
	}
}

[TestClass]
public class ctor : AccountsTestBase
{
	[TestMethod]
	public void create_file()
	{
		File.Exists(TestFile).Should().BeFalse();
		var accountsSettings = new AccountsSettings();
		_ = new AccountsSettingsPersister(accountsSettings, TestFile);
		File.Exists(TestFile).Should().BeTrue();
		File.ReadAllText(TestFile).Should().Be(EMPTY_FILE);
	}

	[TestMethod]
	public void overwrite_existing_file()
	{
		File.Exists(TestFile).Should().BeFalse();
		WriteToTestFile("foo");
		File.Exists(TestFile).Should().BeTrue();

		var accountsSettings = new AccountsSettings();
		_ = new AccountsSettingsPersister(accountsSettings, TestFile);
		File.Exists(TestFile).Should().BeTrue();
		File.ReadAllText(TestFile).Should().Be(EMPTY_FILE);
	}

	[TestMethod]
	public void save_multiple_children()
	{
		var accountsSettings = new AccountsSettings();
		accountsSettings.Add(new Account("a0") { AccountName = "n0" });
		accountsSettings.Add(new Account("a1") { AccountName = "n1" });

		// dispose to cease auto-updates
		using (var p = new AccountsSettingsPersister(accountsSettings, TestFile)) { }

		var persister = new AccountsSettingsPersister(TestFile);
		persister.AccountsSettings.Accounts.Count.Should().Be(2);
		persister.AccountsSettings.Accounts[1].AccountName.Should().Be("n1");
	}

	[TestMethod]
	public void save_with_identity()
	{
		var id = new Identity(usLocale);
		var idJson = JsonConvert.SerializeObject(id, Identity.GetJsonSerializerSettings());

		var accountsSettings = new AccountsSettings();
		accountsSettings.Add(new Account("a0") { AccountName = "n0", IdentityTokens = id });

		// dispose to cease auto-updates
		using (var p = new AccountsSettingsPersister(accountsSettings, TestFile)) { }

		var persister = new AccountsSettingsPersister(TestFile);
		var acct = persister.AccountsSettings.Accounts[0];
		acct.AccountName.Should().Be("n0");
		acct.Locale.BeNotNull();
		acct.Locale.CountryCode.Should().Be("us");
	}
}

[TestClass]
public class save : AccountsTestBase
{
	// add/save account after file creation
	[TestMethod]
	public void save_1_account()
	{
		// create initial file
		using (var p = new AccountsSettingsPersister(new AccountsSettings(), TestFile)) { }

		// load file. create account
		using (var p = new AccountsSettingsPersister(TestFile))
		{
			var idIn = new Identity(usLocale);
			var acctIn = new Account("a0") { AccountName = "n0", IdentityTokens = idIn };

			p.AccountsSettings.Add(acctIn);
		}

		// re-load file. ensure account still exists
		using (var p = new AccountsSettingsPersister(TestFile))
		{
			p.AccountsSettings.Accounts.Count.Should().Be(1);
			var acct0 = p.AccountsSettings.Accounts[0];
			acct0.AccountName.Should().Be("n0");
			acct0.Locale.BeNotNull();
			acct0.Locale.CountryCode.Should().Be("us");
		}
	}

	// add/save mult accounts after file creation
	// separately create 2 accounts. ensure both still exist in the end
	[TestMethod]
	public void save_2_accounts()
	{
		// create initial file
		using (var p = new AccountsSettingsPersister(new AccountsSettings(), TestFile)) { }

		// load file. create account 0
		using (var p = new AccountsSettingsPersister(TestFile))
		{
			var idIn = new Identity(usLocale);
			var acctIn = new Account("a0") { AccountName = "n0", IdentityTokens = idIn };

			p.AccountsSettings.Add(acctIn);
		}

		// re-load file. ensure account still exists
		using (var p = new AccountsSettingsPersister(TestFile))
		{
			p.AccountsSettings.Accounts.Count.Should().Be(1);

			var acct0 = p.AccountsSettings.Accounts[0];
			acct0.AccountName.Should().Be("n0");
			acct0.Locale.BeNotNull();
			acct0.Locale.CountryCode.Should().Be("us");
		}

		// load file. create account 1
		using (var p = new AccountsSettingsPersister(TestFile))
		{
			var idIn = new Identity(ukLocale);
			var acctIn = new Account("a1") { AccountName = "n1", IdentityTokens = idIn };

			p.AccountsSettings.Add(acctIn);
		}

		// re-load file. ensure both accounts still exist
		using (var p = new AccountsSettingsPersister(TestFile))
		{
			p.AccountsSettings.Accounts.Count.Should().Be(2);

			var acct0 = p.AccountsSettings.Accounts[0];
			acct0.AccountName.Should().Be("n0");
			acct0.Locale.BeNotNull();
			acct0.Locale.CountryCode.Should().Be("us");

			var acct1 = p.AccountsSettings.Accounts[1];
			acct1.AccountName.Should().Be("n1");
			acct1.Locale.BeNotNull();
			acct1.Locale.CountryCode.Should().Be("uk");
		}
	}

	[TestMethod]
	public void update_Account_field_just_added()
	{
		// create initial file
		using (var p = new AccountsSettingsPersister(new AccountsSettings(), TestFile)) { }

		// load file. create 2 accounts
		using (var p = new AccountsSettingsPersister(TestFile))
		{
			var id1 = new Identity(usLocale);
			var acct1 = new Account("a0") { AccountName = "n0", IdentityTokens = id1 };
			p.AccountsSettings.Add(acct1);

			// update just-added item. note: this is different than the subscription which happens on initial collection load. ensure this works also
			acct1.AccountName = "new";
		}

		// verify save property
		using (var p = new AccountsSettingsPersister(TestFile))
		{
			var acct0 = p.AccountsSettings.Accounts[0];
			acct0.AccountName.Should().Be("new");
		}
	}

	// update Account property. must be non-destructive to all other data
	[TestMethod]
	public void update_Account_field()
	{
		// create initial file
		using (var p = new AccountsSettingsPersister(new AccountsSettings(), TestFile)) { }

		// load file. create 2 accounts
		using (var p = new AccountsSettingsPersister(TestFile))
		{
			var id1 = new Identity(usLocale);
			var acct1 = new Account("a0") { AccountName = "n0", IdentityTokens = id1 };
			p.AccountsSettings.Add(acct1);

			var id2 = new Identity(ukLocale);
			var acct2 = new Account("a1") { AccountName = "n1", IdentityTokens = id2 };

			p.AccountsSettings.Add(acct2);
		}

		// update AccountName on existing file
		using (var p = new AccountsSettingsPersister(TestFile))
		{
			var acct0 = p.AccountsSettings.Accounts[0];
			acct0.AccountName = "new";
		}

		// re-load file. ensure both accounts still exist
		using (var p = new AccountsSettingsPersister(TestFile))
		{
			p.AccountsSettings.Accounts.Count.Should().Be(2);

			var acct0 = p.AccountsSettings.Accounts[0];
			// new
			acct0.AccountName.Should().Be("new");

			// still here
			acct0.Locale.BeNotNull();
			acct0.Locale.CountryCode.Should().Be("us");
			var acct1 = p.AccountsSettings.Accounts[1];
			acct1.AccountName.Should().Be("n1");
			acct1.Locale.BeNotNull();
			acct1.Locale.CountryCode.Should().Be("uk");
		}
	}

	// update identity. must be non-destructive to all other data
	[TestMethod]
	public void replace_identity()
	{
		// create initial file
		using (var p = new AccountsSettingsPersister(new AccountsSettings(), TestFile)) { }

		// load file. create 2 accounts
		using (var p = new AccountsSettingsPersister(TestFile))
		{
			var id1 = new Identity(usLocale);
			var acct1 = new Account("a0") { AccountName = "n0", IdentityTokens = id1 };
			p.AccountsSettings.Add(acct1);

			var id2 = new Identity(ukLocale);
			var acct2 = new Account("a1") { AccountName = "n1", IdentityTokens = id2 };

			p.AccountsSettings.Add(acct2);
		}

		// update identity on existing file
		using (var p = new AccountsSettingsPersister(TestFile))
		{
			var id = new Identity(ukLocale);

			var acct0 = p.AccountsSettings.Accounts[0];
			acct0.IdentityTokens = id;
		}

		// re-load file. ensure both accounts still exist
		using (var p = new AccountsSettingsPersister(TestFile))
		{
			p.AccountsSettings.Accounts.Count.Should().Be(2);

			var acct0 = p.AccountsSettings.Accounts[0];
			// new
			acct0.Locale.BeNotNull();
			acct0.Locale.CountryCode.Should().Be("uk");

			// still here
			acct0.AccountName.Should().Be("n0");
			var acct1 = p.AccountsSettings.Accounts[1];
			acct1.AccountName.Should().Be("n1");
			acct1.Locale.BeNotNull();
			acct1.Locale.CountryCode.Should().Be("uk");
		}
	}

	// multi-level subscribe => update
	// edit field of existing identity. must be non-destructive to all other data
	[TestMethod]
	public void update_identity_field()
	{
		// create initial file
		using (var p = new AccountsSettingsPersister(new AccountsSettings(), TestFile)) { }

		// load file. create 2 accounts
		using (var p = new AccountsSettingsPersister(TestFile))
		{
			var id1 = new Identity(usLocale);
			var acct1 = new Account("a0") { AccountName = "n0", IdentityTokens = id1 };
			p.AccountsSettings.Add(acct1);

			var id2 = new Identity(ukLocale);
			var acct2 = new Account("a1") { AccountName = "n1", IdentityTokens = id2 };

			p.AccountsSettings.Add(acct2);
		}

		// update identity on existing file
		using (var p = new AccountsSettingsPersister(TestFile))
		{
			var acct0 = p.AccountsSettings.Accounts[0];

			acct0.IdentityTokens.BeNotNull();
			acct0.IdentityTokens
				.Update(new AccessToken("Atna|_NEW_", DateTime.Now.AddDays(1)));
		}

		// re-load file. ensure both accounts still exist
		using (var p = new AccountsSettingsPersister(TestFile))
		{
			p.AccountsSettings.Accounts.Count.Should().Be(2);

			var acct0 = p.AccountsSettings.Accounts[0];
			// new
			acct0.IdentityTokens.BeNotNull();
			acct0.IdentityTokens.ExistingAccessToken.TokenValue.Should().Be("Atna|_NEW_");

			// still here
			acct0.AccountName.Should().Be("n0");
			acct0.Locale.BeNotNull();
			acct0.Locale.CountryCode.Should().Be("us");
			var acct1 = p.AccountsSettings.Accounts[1];
			acct1.AccountName.Should().Be("n1");
			acct1.Locale.BeNotNull();
			acct1.Locale.CountryCode.Should().Be("uk");
		}
	}
}

[TestClass]
public class retrieve : AccountsTestBase
{
	[TestMethod]
	public void get_where()
	{
		var idUs = new Identity(usLocale);
		var acct1 = new Account("cng") { IdentityTokens = idUs, AccountName = "foo" };

		var idUk = new Identity(ukLocale);
		var acct2 = new Account("cng") { IdentityTokens = idUk, AccountName = "bar" };

		var accountsSettings = new AccountsSettings();
		accountsSettings.Add(acct1);
		accountsSettings.Add(acct2);

		var acct = accountsSettings.GetAccount("cng", "uk");
		acct.BeNotNull();
		acct.AccountName.Should().Be("bar");
	}

	[TestMethod]
	public void get_account_id_is_case_insensitive()
	{
		var id = new Identity(usLocale);
		var acct = new Account("HopefulRN2016@Gmail.com") { IdentityTokens = id, AccountName = "foo" };

		var accountsSettings = new AccountsSettings();
		accountsSettings.Add(acct);

		// stored id differs only by letter case; lookup must still succeed (issue #1931)
		var found = accountsSettings.GetAccount("hopefulrn2016@gmail.com", "us");
		found.BeNotNull();
		found.AccountName.Should().Be("foo");
	}
}

[TestClass]
public class upsert : AccountsTestBase
{
	[TestMethod]
	public void upsert_new()
	{
		var accountsSettings = new AccountsSettings();
		accountsSettings.Accounts.Count.Should().Be(0);

		accountsSettings.Upsert("cng", "us");

		accountsSettings.Accounts.Count.Should().Be(1);
		var acct = accountsSettings.GetAccount("cng", "us");
		acct.BeNotNull();
		acct.AccountId.Should().Be("cng");
		acct.LibraryScan.Should().BeTrue();
	}

	[TestMethod]
	public void upsert_exists()
	{
		var accountsSettings = new AccountsSettings();
		var orig = accountsSettings.Upsert("cng", "us");
		orig.AccountName = "foo";

		var exists = accountsSettings.Upsert("cng", "us");
		exists.AccountName.Should().Be("foo");

		orig.Should().BeSameAs(exists);
	}
}

[TestClass]
public class delete : AccountsTestBase
{
	[TestMethod]
	public void delete_account()
	{
		var accountsSettings = new AccountsSettings();
		var acct = accountsSettings.Upsert("cng", "us");
		accountsSettings.Accounts.Count.Should().Be(1);

		var removed = accountsSettings.Delete(acct);
		removed.Should().BeTrue();

		accountsSettings.Accounts.Count.Should().Be(0);
	}

	[TestMethod]
	public void delete_where()
	{
		var accountsSettings = new AccountsSettings();
		_ = accountsSettings.Upsert("cng", "us");
		accountsSettings.Accounts.Count.Should().Be(1);

		accountsSettings.Delete("baz", "baz").Should().BeFalse();
		accountsSettings.Accounts.Count.Should().Be(1);

		accountsSettings.Delete("cng", "us").Should().BeTrue();
		accountsSettings.Accounts.Count.Should().Be(0);
	}

	[TestMethod]
	public void delete_updates()
	{
		var i = 0;
		void update(object? sender, EventArgs e) => i++;

		var accountsSettings = new AccountsSettings();
		accountsSettings.Updated += update;

		accountsSettings.Accounts.Count.Should().Be(0);
		i.Should().Be(0);

		_ = accountsSettings.Upsert("cng", "us");
		accountsSettings.Accounts.Count.Should().Be(1);
		i.Should().Be(1);

		accountsSettings.Delete("baz", "baz").Should().BeFalse();
		accountsSettings.Accounts.Count.Should().Be(1);
		i.Should().Be(1);

		accountsSettings.Delete("cng", "us").Should().BeTrue();
		accountsSettings.Accounts.Count.Should().Be(0);
		i.Should().Be(2); // <== this is the one being tested
	}

	[TestMethod]
	public void deleted_account_should_not_persist_file()
	{
		Account acct;

		using (var p = new AccountsSettingsPersister(new AccountsSettings(), TestFile))
		{
			acct = p.AccountsSettings.Upsert("foo", "us");
			p.AccountsSettings.Accounts.Count.Should().Be(1);
			acct.AccountName = "old";
		}

		using (var p = new AccountsSettingsPersister(new AccountsSettings(), TestFile))
		{
			p.AccountsSettings.Delete(acct);
			p.AccountsSettings.Accounts.Count.Should().Be(0);
		}

		using (var p = new AccountsSettingsPersister(new AccountsSettings(), TestFile))
		{
			File.ReadAllText(TestFile).Should().Be(EMPTY_FILE);

			acct.AccountName = "new";

			File.ReadAllText(TestFile).Should().Be(EMPTY_FILE);
		}
	}
}

/// <summary>
/// One login can hold a library in more than one marketplace - a title bought while an Amazon address was
/// briefly set to another country lives there permanently. The account keeps a single registration and simply
/// records the extra marketplaces it should also read.
/// </summary>
[TestClass]
public class additional_marketplaces : AccountsTestBase
{
	private static Account registeredIn(string localeName, string accountId = "user@example.com")
		=> new(accountId) { IdentityTokens = new Identity(Localization.Get(localeName)) };

	[TestMethod]
	public void an_account_starts_with_only_its_own_marketplace()
	{
		var account = registeredIn("ca");

		account.AdditionalLocales.Should().HaveCount(0);
		account.ScanLocales.Select(l => l.Name).Should().BeEquivalentTo(["canada"]);
		account.HasMarketplace("ca").Should().BeTrue();
		account.HasMarketplace("us").Should().BeFalse();
	}

	[TestMethod]
	public void a_scan_reads_the_registered_marketplace_first()
	{
		// the registered marketplace is where a login happens, so it has to be the one scanned first
		var account = registeredIn("ca");
		account.AddMarketplace("us");

		CollectionAssert.AreEqual(
			new[] { "canada", "us" },
			account.ScanLocales.Select(l => l.Name).ToArray());
	}

	[TestMethod]
	public void the_registered_marketplace_cannot_be_added_again()
	{
		var account = registeredIn("ca");

		account.AddMarketplace("ca").Should().BeFalse();
		account.ScanLocales.Count.Should().Be(1);
	}

	[TestMethod]
	public void a_marketplace_cannot_be_added_twice()
	{
		var account = registeredIn("ca");

		account.AddMarketplace("us").Should().BeTrue();
		account.AddMarketplace("us").Should().BeFalse();
		account.AdditionalLocales.Count.Should().Be(1);
	}

	[TestMethod]
	public void a_country_code_and_a_locale_name_are_the_same_marketplace()
	{
		// 'de' and 'germany' name one marketplace; storing both would scan it twice
		var account = registeredIn("us");

		account.AddMarketplace("de").Should().BeTrue();
		account.AddMarketplace("germany").Should().BeFalse();
		account.AdditionalLocales.Single().Name.Should().Be("germany");
	}

	[TestMethod]
	public void an_unknown_marketplace_is_refused()
	{
		var account = registeredIn("us");

		account.AddMarketplace("atlantis").Should().BeFalse();
		account.AddMarketplace("").Should().BeFalse();
		account.AddMarketplace(null).Should().BeFalse();
		account.AdditionalLocales.Should().HaveCount(0);
	}

	[TestMethod]
	public void removing_a_marketplace_leaves_the_registered_one_alone()
	{
		var account = registeredIn("ca");
		account.AddMarketplace("us");

		account.RemoveMarketplace("us").Should().BeTrue();
		account.RemoveMarketplace("ca").Should().BeFalse();
		account.ScanLocales.Select(l => l.Name).Should().BeEquivalentTo(["canada"]);
	}

	[TestMethod]
	public void adding_a_marketplace_marks_the_settings_dirty()
	{
		var account = registeredIn("ca");
		var updates = 0;
		account.Updated += (_, _) => updates++;

		account.AddMarketplace("us");
		updates.Should().Be(1);

		// no change, no save
		account.AddMarketplace("us");
		updates.Should().Be(1);
	}

	[TestMethod]
	public void a_download_finds_the_account_by_the_marketplace_its_book_came_from()
	{
		// this is the lookup FileLiberator uses: (account id, book's marketplace) -> credentials
		var settings = new AccountsSettings();
		var account = registeredIn("ca");
		settings.Add(account);
		account.AddMarketplace("us");

		settings.GetAccount("user@example.com", "us").Should().BeSameAs(account);
		settings.GetAccount("user@example.com", "ca").Should().BeSameAs(account);
		settings.GetAccount("user@example.com", "uk").Should().BeNull();
	}

	[TestMethod]
	public void the_registered_account_wins_over_one_holding_the_marketplace_as_an_extra()
	{
		var settings = new AccountsSettings();
		var mine = registeredIn("us", "mine@example.com");
		var theirs = registeredIn("ca", "theirs@example.com");
		settings.Add(mine);
		settings.Add(theirs);
		theirs.AddMarketplace("uk");

		settings.GetAccount("mine@example.com", "us").Should().BeSameAs(mine);
		settings.GetAccount("theirs@example.com", "uk").Should().BeSameAs(theirs);
	}

	[TestMethod]
	public void two_rows_for_one_login_cannot_scan_the_same_marketplace()
	{
		// the older way to hold two marketplaces was two rows for one login. adding a marketplace that another
		// row already scans would import it twice and leave a download two accounts to choose from.
		var settings = new AccountsSettings();
		var ca = registeredIn("ca");
		var us = registeredIn("us");
		settings.Add(ca);
		settings.Add(us);

		Assert.ThrowsExactly<InvalidOperationException>(() => ca.AddMarketplace("us"));
	}

	[TestMethod]
	public void a_different_login_may_scan_the_same_marketplace()
	{
		var settings = new AccountsSettings();
		var mine = registeredIn("ca", "mine@example.com");
		var theirs = registeredIn("uk", "theirs@example.com");
		settings.Add(mine);
		settings.Add(theirs);

		mine.AddMarketplace("us");
		theirs.AddMarketplace("us");

		mine.HasMarketplace("us").Should().BeTrue();
		theirs.HasMarketplace("us").Should().BeTrue();
	}

	[TestMethod]
	public void a_row_registered_with_a_marketplace_blocks_another_row_from_adding_it()
	{
		var settings = new AccountsSettings();
		var ca = registeredIn("ca");
		settings.Add(ca);
		ca.AddMarketplace("us");

		// the extra marketplace is a claim as real as a registration
		Assert.ThrowsExactly<InvalidOperationException>(() => settings.Add(registeredIn("us")));
	}

	[TestMethod]
	public void the_account_already_scanning_a_marketplace_can_be_named()
	{
		var settings = new AccountsSettings();
		var ca = registeredIn("ca");
		ca.AccountName = "Canada";
		settings.Add(ca);

		settings.GetAccountClaimingMarketplace("user@example.com", "ca").Should().BeSameAs(ca);
		settings.GetAccountClaimingMarketplace("user@example.com", "ca", excluding: ca).Should().BeNull();
		settings.GetAccountClaimingMarketplace("user@example.com", "us").Should().BeNull();
	}
}

[TestClass]
public class additional_marketplaces_persistence : AccountsTestBase
{
	/// <summary>An account file exactly as it was written before additional marketplaces existed.</summary>
	private static string OneMarketplaceFile()
	{
		var settings = new AccountsSettings();
		settings.Add(new Account("user@example.com")
		{
			AccountName = "Main",
			IdentityTokens = new Identity(Localization.Get("ca"))
		});
		return settings.ToJson();
	}

	[TestMethod]
	public void a_file_written_before_this_feature_loads_as_a_single_marketplace_account()
	{
		var json = OneMarketplaceFile();
		JObject.Parse(json)["Accounts"]![0]!["AdditionalLocaleNames"].Should().BeNull();

		var loaded = AccountsSettings.FromJson(json);
		loaded.BeNotNull();

		var account = loaded.Accounts.Single();
		account.AdditionalLocales.Should().HaveCount(0);
		account.ScanLocales.Select(l => l.Name).Should().BeEquivalentTo(["canada"]);

		// and saving it back does not introduce the property
		JObject.Parse(loaded.ToJson())["Accounts"]![0]!["AdditionalLocaleNames"].Should().BeNull();
	}

	[TestMethod]
	public void additional_marketplaces_survive_a_round_trip()
	{
		var settings = new AccountsSettings();
		var account = new Account("user@example.com") { IdentityTokens = new Identity(Localization.Get("ca")) };
		settings.Add(account);
		account.AddMarketplace("us");
		account.AddMarketplace("uk");

		var reloaded = AccountsSettings.FromJson(settings.ToJson());
		reloaded.BeNotNull();

		CollectionAssert.AreEqual(
			new[] { "canada", "us", "uk" },
			reloaded.Accounts.Single().ScanLocales.Select(l => l.Name).ToArray());
	}

	[TestMethod]
	public void the_registered_marketplace_is_never_duplicated_into_the_extras_by_a_hand_edited_file()
	{
		var jo = JObject.Parse(OneMarketplaceFile());
		jo["Accounts"]![0]!["AdditionalLocaleNames"]
			= new JArray("ca", "canada", "us", "us", "atlantis");

		var account = AccountsSettings.FromJson(jo.ToString())!.Accounts.Single();

		account.ScanLocales.Select(l => l.Name).Should().BeEquivalentTo(["canada", "us"]);
	}

	/// <summary>
	/// Encrypted tokens are bound to the marketplace they were registered with: the AES-GCM associated data is
	/// built from Identity.LocaleName. Extra marketplaces must therefore stay out of the identity, or every
	/// stored token would need re-encrypting.
	/// </summary>
	[TestMethod]
	public void adding_a_marketplace_leaves_the_stored_identity_untouched()
	{
		var settings = new AccountsSettings();
		var account = new Account("user@example.com") { IdentityTokens = new Identity(Localization.Get("ca")) };
		settings.Add(account);

		var identityBefore = JObject.Parse(settings.ToJson())["Accounts"]![0]!["IdentityTokens"]!.ToString();

		account.AddMarketplace("us");

		JObject.Parse(settings.ToJson())["Accounts"]![0]!["IdentityTokens"]!.ToString()
			.Should().Be(identityBefore);
		account.Locale!.Name.Should().Be("canada");
	}

	/// <summary>
	/// The identity is located in the file by account id and the identity's own LocaleName. A scan of an extra
	/// marketplace loads the very same tokens, so the path must not follow the marketplace being read.
	/// </summary>
	[TestMethod]
	public void the_identity_json_path_follows_the_registered_marketplace()
	{
		var account = new Account("user@example.com") { IdentityTokens = new Identity(Localization.Get("ca")) };
		account.AddMarketplace("us");

		account.GetIdentityTokensJsonPath()
			.Should().Be(AudibleApiStorage.GetIdentityTokensJsonPath("user@example.com", "canada"));
	}
}

// account.Id + Locale.Name -- must be unique
[TestClass]
public class validate : AccountsTestBase
{
	[TestMethod]
	public void violate_validation()
	{
		var accountsSettings = new AccountsSettings();

		var idIn = new Identity(usLocale);

		var a1 = new Account("a") { AccountName = "one", IdentityTokens = idIn };
		accountsSettings.Add(a1);

		var a2 = new Account("a") { AccountName = "two", IdentityTokens = idIn };

		// violation: validate()
		Assert.ThrowsExactly<InvalidOperationException>(() => accountsSettings.Add(a2));
	}

	[TestMethod]
	public void identity_violate_validation()
	{
		var accountsSettings = new AccountsSettings();

		var idIn = new Identity(usLocale);

		var a1 = new Account("a") { AccountName = "one", IdentityTokens = idIn };
		accountsSettings.Add(a1);

		var a2 = new Account("a") { AccountName = "two" };
		accountsSettings.Add(a2);

		// violation: GetAccount.SingleOrDefault
		Assert.ThrowsExactly<InvalidOperationException>(() => a2.IdentityTokens = idIn);
	}
}

[TestClass]
public class transactions : AccountsTestBase
{
	[TestMethod]
	public void atomic_update_at_end()
	{
		var p = new AccountsSettingsPersister(new AccountsSettings(), TestFile);
		p.BeginTransation();

		// upserted account will not persist until CommitTransation
		var acct = p.AccountsSettings.Upsert("cng", "us");
		acct.AccountName = "foo";

		File.ReadAllText(TestFile).Should().Be(EMPTY_FILE);
		p.IsInTransaction.Should().BeTrue();

		p.CommitTransation();
		p.IsInTransaction.Should().BeFalse();
	}

	[TestMethod]
	public void abandoned_transaction()
	{
		var p = new AccountsSettingsPersister(new AccountsSettings(), TestFile);
		try
		{
			p.BeginTransation();

			var acct = p.AccountsSettings.Upsert("cng", "us");
			acct.AccountName = "foo";
			throw new Exception();
		}
		catch { }
		finally
		{
			File.ReadAllText(TestFile).Should().Be(EMPTY_FILE);
			p.IsInTransaction.Should().BeTrue();
		}
	}
}

/// <summary>
/// Characterization: AccountsSettings.json currently persists Identity tokens as plaintext
/// with no IsEncrypted metadata. Baseline before token-storage encryption work.
/// </summary>
[TestClass]
public class LegacyPlaintextIdentityTokens : AccountsTestBase
{
	const string SampleAccessToken = "Atna|_CHAR_ACCESS_";
	const string SampleRefreshToken = "Atnr|_CHAR_REFRESH_";
	const string SampleAdpToken = "{enc:abcdefg}{key:1234}{iv:56789}{name:QURQVG9rZW5FbmNyeXB0aW9uS2V5}{serial:Mg==}";
	const string SampleStoreAuthCookie = "store-auth-cookie-value";
	static readonly DateTime SampleExpires = new(2200, 1, 1, 12, 0, 0, DateTimeKind.Utc);

	const string SamplePrivateKey = @"
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

	static Identity CreateRegisteredIdentity()
	{
		var identity = new Identity(Localization.Get("us"));
		identity.Update(
			new PrivateKey(SamplePrivateKey),
			new AdpToken(SampleAdpToken),
			new AccessToken(SampleAccessToken, SampleExpires),
			new RefreshToken(SampleRefreshToken),
			new List<KeyValuePair<string, SecretString>> { new("session-id", "cookie-secret-value") },
			deviceSerialNumber: "device-serial",
			deviceType: "device-type",
			amazonAccountId: "amzn-account",
			deviceName: "device-name",
			storeAuthenticationCookie: SampleStoreAuthCookie);
		return identity;
	}

	[TestMethod]
	public void toJson_registered_account_has_no_IsEncrypted_and_plaintext_secrets()
	{
		var settings = new AccountsSettings();
		settings.Add(new Account("user@example.com")
		{
			AccountName = "Main",
			IdentityTokens = CreateRegisteredIdentity()
		});

		var json = settings.ToJson();
		var jo = JObject.Parse(json);

		jo.SelectTokens("$..IsEncrypted").Should().HaveCount(0);
		var tokens = jo["Accounts"]![0]!["IdentityTokens"]!;
		tokens["ExistingAccessToken"]!["TokenValue"]!.Value<string>().Should().Be(SampleAccessToken);
		tokens["RefreshToken"]!["Value"]!.Value<string>().Should().Be(SampleRefreshToken);
		tokens["AdpToken"]!["Value"]!.Value<string>().Should().Be(SampleAdpToken);
		tokens["PrivateKey"]!["Value"]!.Value<string>().Should().Be(SamplePrivateKey);
		tokens["StoreAuthenticationCookie"]!.Value<string>().Should().Be(SampleStoreAuthCookie);
		tokens["Cookies"]![0]!["Value"]!.Value<string>().Should().Be("cookie-secret-value");
	}

	[TestMethod]
	public void fromJson_legacy_file_loads_usable_plaintext_tokens()
	{
		var settings = new AccountsSettings();
		settings.Add(new Account("user@example.com")
		{
			AccountName = "Main",
			DecryptKey = "activation-bytes-ok-as-plaintext",
			IdentityTokens = CreateRegisteredIdentity()
		});
		var legacyJson = settings.ToJson();
		JObject.Parse(legacyJson).SelectTokens("$..IsEncrypted").Should().HaveCount(0);

		var loaded = AccountsSettings.FromJson(legacyJson);
		loaded.BeNotNull();
		loaded.Accounts.Count.Should().Be(1);

		var account = loaded.Accounts[0];
		account.AccountId.Should().Be("user@example.com");
		account.DecryptKey.Should().Be("activation-bytes-ok-as-plaintext");
		account.IdentityTokens.BeNotNull();
		account.IdentityTokens.IsValid.Should().BeTrue();
		account.IdentityTokens.ExistingAccessToken.TokenValue.Should().Be(SampleAccessToken);
		account.IdentityTokens.RefreshToken.BeNotNull();
		account.IdentityTokens.RefreshToken.Value.Should().Be(SampleRefreshToken);
		account.IdentityTokens.AdpToken.BeNotNull();
		account.IdentityTokens.AdpToken.Value.Should().Be(SampleAdpToken);
		account.IdentityTokens.PrivateKey.BeNotNull();
		account.IdentityTokens.PrivateKey.Value.Should().Be(SamplePrivateKey);
		account.IdentityTokens.StoreAuthenticationCookie.Should().Be(SampleStoreAuthCookie);
		account.IdentityTokens.Cookies.Single().Value.Should().Be("cookie-secret-value");
	}

	[TestMethod]
	public void persister_roundtrip_keeps_plaintext_tokens_without_IsEncrypted()
	{
		using (var p = new AccountsSettingsPersister(new AccountsSettings(), TestFile))
		{
			p.AccountsSettings.Add(new Account("user@example.com")
			{
				AccountName = "Main",
				IdentityTokens = CreateRegisteredIdentity()
			});
		}

		var onDisk = File.ReadAllText(TestFile);
		var jo = JObject.Parse(onDisk);
		jo.SelectTokens("$..IsEncrypted").Should().HaveCount(0);
		jo["Accounts"]![0]!["IdentityTokens"]!["RefreshToken"]!["Value"]!.Value<string>()
			.Should().Be(SampleRefreshToken);

		using var loaded = new AccountsSettingsPersister(TestFile);
		var tokens = loaded.AccountsSettings.Accounts[0].IdentityTokens;
		tokens.BeNotNull();
		tokens.IsValid.Should().BeTrue();
		tokens.ExistingAccessToken.TokenValue.Should().Be(SampleAccessToken);
		tokens.RefreshToken.BeNotNull();
		tokens.RefreshToken.Value.Should().Be(SampleRefreshToken);
	}
}

/// <summary>
/// AccountsSettings.json should hold an account's state and nothing derived from it. A computed property
/// without <see cref="JsonIgnore"/> is written out silently, and then sits in the file going stale.
/// </summary>
[TestClass]
public class SerializedShape : AccountsTestBase
{
	private static JObject SerializeOneAccount()
	{
		var settings = new AccountsSettings();
		settings.Add(new Account("user@example.com") { AccountName = "Main" });

		return (JObject)JObject.Parse(settings.ToJson())["Accounts"]![0]!;
	}

	[TestMethod]
	public void MaskedLogEntry_is_not_persisted()
		=> SerializeOneAccount().ContainsKey(nameof(Account.MaskedLogEntry)).Should().BeFalse();

	[TestMethod]
	public void Locale_is_not_persisted()
		=> SerializeOneAccount().ContainsKey(nameof(Account.Locale)).Should().BeFalse();

	[TestMethod]
	public void only_an_accounts_own_state_is_persisted()
	{
		// Fails when a property is added without deciding whether it belongs in the file.
		CollectionAssert.AreEquivalent(
			new[] { "AccountId", "AccountName", "LibraryScan", "DecryptKey", "IdentityTokens" },
			SerializeOneAccount().Properties().Select(p => p.Name).ToArray());
	}

	[TestMethod]
	public void additional_marketplaces_are_written_only_when_there_are_some()
	{
		// An account with one marketplace - nearly every account - must serialize exactly as it did before
		// additional marketplaces existed, so that nothing about those files changes.
		var settings = new AccountsSettings();
		var account = new Account("user@example.com")
		{
			AccountName = "Main",
			IdentityTokens = new Identity(Localization.Get("ca"))
		};
		settings.Add(account);

		((JObject)JObject.Parse(settings.ToJson())["Accounts"]![0]!)
			.ContainsKey("AdditionalLocaleNames").Should().BeFalse();

		account.AddMarketplace("us");

		JObject.Parse(settings.ToJson())["Accounts"]![0]!["AdditionalLocaleNames"]!
			.Values<string>().Should().BeEquivalentTo(["us"]);
	}

	[TestMethod]
	public void a_stray_property_from_an_older_file_is_dropped_on_the_next_save()
	{
		// Files written before MaskedLogEntry was ignored still carry it. Loading must not choke on it,
		// and saving must not preserve it.
		var withStray = """
			{
			  "Accounts": [
			    {
			      "AccountId": "user@example.com",
			      "AccountName": "Main",
			      "LibraryScan": true,
			      "DecryptKey": "",
			      "IdentityTokens": null,
			      "MaskedLogEntry": "AccountId=u**r|AccountName=M**n|Locale=[empty]"
			    }
			  ],
			  "Cdm": null
			}
			""";

		var loaded = AccountsSettings.FromJson(withStray);
		loaded.BeNotNull();
		loaded.Accounts.Count.Should().Be(1);
		loaded.Accounts[0].AccountId.Should().Be("user@example.com");

		JObject.Parse(loaded.ToJson())["Accounts"]![0]!["MaskedLogEntry"].Should().BeNull();
	}
}
#pragma warning restore CS8981

