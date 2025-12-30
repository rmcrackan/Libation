using System;
using System.IO;
using AssertionHelper;
using AudibleApi;
using AudibleApi.Authorization;
using AudibleUtilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

[assembly: Parallelize]

namespace AccountsTests
{
#pragma warning disable CS8981 // The type name only contains lower-cased ascii characters. Such names may become reserved for the language.
	public class AccountsTestBase
    {
        protected string EMPTY_FILE { get; } = "{\r\n  \"Accounts\": [],\r\n  \"Cdm\": null\r\n}".Replace("\r\n", Environment.NewLine);

        protected string TestFile;
        protected Locale usLocale => Localization.Get("us");
        protected Locale ukLocale => Localization.Get("uk");

        protected void WriteToTestFile(string contents)
            => File.WriteAllText(TestFile, contents);

        [TestInitialize]
        public void TestInit()
            => TestFile = Guid.NewGuid() + ".txt";

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
                acct0.Locale.CountryCode.Should().Be("us");

                var acct1 = p.AccountsSettings.Accounts[1];
                acct1.AccountName.Should().Be("n1");
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
                acct0.Locale.CountryCode.Should().Be("us");
                var acct1 = p.AccountsSettings.Accounts[1];
                acct1.AccountName.Should().Be("n1");
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
                acct0.Locale.CountryCode.Should().Be("uk");

                // still here
                acct0.AccountName.Should().Be("n0");
                var acct1 = p.AccountsSettings.Accounts[1];
                acct1.AccountName.Should().Be("n1");
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
                p.AccountsSettings.Accounts[0]
                    .IdentityTokens
                    .Update(new AccessToken("Atna|_NEW_", DateTime.Now.AddDays(1)));
            }

            // re-load file. ensure both accounts still exist
            using (var p = new AccountsSettingsPersister(TestFile))
            {
                p.AccountsSettings.Accounts.Count.Should().Be(2);

                var acct0 = p.AccountsSettings.Accounts[0];
                // new
                acct0.IdentityTokens.ExistingAccessToken.TokenValue.Should().Be("Atna|_NEW_");

                // still here
                acct0.AccountName.Should().Be("n0");
                acct0.Locale.CountryCode.Should().Be("us");
                var acct1 = p.AccountsSettings.Accounts[1];
                acct1.AccountName.Should().Be("n1");
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

            accountsSettings.GetAccount("cng", "uk").AccountName.Should().Be("bar");
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
            accountsSettings.GetAccount("cng", "us").AccountId.Should().Be("cng");
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
            void update(object sender, EventArgs e) => i++;

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
#pragma warning restore CS8981
}
