using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AudibleApi;
using AudibleApi.Authorization;
using Dinah.Core;
using FluentAssertions;
using InternalUtilities;
using Microsoft.VisualStudio.TestPlatform.Common.Filtering;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TestAudibleApiCommon;
using TestCommon;
using static AuthorizationShared.Shared;
using static AuthorizationShared.Shared.AccessTokenTemporality;
using static TestAudibleApiCommon.ComputedTestValues;

namespace AccountsTests
{
    [TestClass]
    public class FromJson
    {
        [TestMethod]
        public void _0_accounts()
        {
            var json = @"
{
  ""AccountsSettings"": []
}
".Trim();
            var accounts = Accounts.FromJson(json);
            accounts.AccountsSettings.Count.Should().Be(0);
        }

        [TestMethod]
        public void _1_account_new()
        {
            var json = @"
{
  ""AccountsSettings"": [
	{
      ""AccountId"": ""cng"",
      ""AccountName"": ""my main login"",
      ""DecryptKey"": ""asdfasdf"",
      ""IdentityTokens"": null
    }
  ]
}
".Trim();
            var accounts = Accounts.FromJson(json);
            accounts.AccountsSettings.Count.Should().Be(1);
            accounts.AccountsSettings[0].AccountId.Should().Be("cng");
            accounts.AccountsSettings[0].IdentityTokens.Should().BeNull();
        }

        [TestMethod]
        public void _1_account_populated()
        {
            var id = GetIdentityJson(Future);

            var json = $@"
{{
  ""AccountsSettings"": [
	{{
      ""AccountId"": ""cng"",
      ""AccountName"": ""my main login"",
      ""DecryptKey"": ""asdfasdf"",
      ""IdentityTokens"": {id}
    }}
  ]
}}
".Trim();
            var accounts = Accounts.FromJson(json);
            accounts.AccountsSettings.Count.Should().Be(1);
            accounts.AccountsSettings[0].AccountId.Should().Be("cng");
            accounts.AccountsSettings[0].IdentityTokens.Should().NotBeNull();
            accounts.AccountsSettings[0].IdentityTokens.ExistingAccessToken.TokenValue.Should().Be(AccessTokenValue);
        }
    }

    [TestClass]
    public class ToJson
    {
        [TestMethod]
        public void serialize()
        {
            var id = JsonConvert.SerializeObject(Identity.Empty, Identity.GetJsonSerializerSettings());
            var jsonIn = $@"
{{
  ""AccountsSettings"": [
	{{
      ""AccountId"": ""cng"",
      ""AccountName"": ""my main login"",
      ""DecryptKey"": ""asdfasdf"",
      ""IdentityTokens"": {id}
    }}
  ]
}}
".Trim();
            var accounts = Accounts.FromJson(jsonIn);

            var jsonOut = accounts.ToJson();
            jsonOut.Should().Be(@"
{
  ""AccountsSettings"": [
    {
      ""AccountId"": ""cng"",
      ""AccountName"": ""my main login"",
      ""DecryptKey"": ""asdfasdf"",
      ""IdentityTokens"": {
        ""LocaleName"": ""[empty]"",
        ""ExistingAccessToken"": {
          ""TokenValue"": ""Atna|"",
          ""Expires"": ""9999-12-31T23:59:59.9999999""
        },
        ""PrivateKey"": null,
        ""AdpToken"": null,
        ""RefreshToken"": null,
        ""Cookies"": []
      }
    }
  ]
}
".Trim());
        }
    }

    public class AccountsPersisterTestBase
    {
        protected string TestFile;

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
    public class ctor : AccountsPersisterTestBase
    {
        [TestMethod]
        public void create_file()
		{
            File.Exists(TestFile).Should().BeFalse();
            var accounts = new Accounts();
            _ = new AccountsPersister(accounts, TestFile);
            File.Exists(TestFile).Should().BeTrue();
            File.ReadAllText(TestFile).Should().Be(@"
{
  ""AccountsSettings"": []
}
".Trim());
        }

        [TestMethod]
        public void overwrite_existing_file()
        {
            File.Exists(TestFile).Should().BeFalse();
            WriteToTestFile("foo");
            File.Exists(TestFile).Should().BeTrue();
            
            var accounts = new Accounts();
            _ = new AccountsPersister(accounts, TestFile);
            File.Exists(TestFile).Should().BeTrue();
            File.ReadAllText(TestFile).Should().Be(@"
{
  ""AccountsSettings"": []
}
".Trim());
        }

        [TestMethod]
        public void save_multiple_children()
        {
            var accounts = new Accounts();
            accounts.UNITTEST_Seed(new Account("a0") { AccountName = "n0" });
            accounts.UNITTEST_Seed(new Account("a1") { AccountName = "n1" });

            // dispose to cease auto-updates
            using (var p = new AccountsPersister(accounts, TestFile)) { }

            var persister = new AccountsPersister(TestFile);
            persister.Accounts.AccountsSettings.Count.Should().Be(2);
            persister.Accounts.AccountsSettings[1].AccountName.Should().Be("n1");
        }

        [TestMethod]
        public void save_with_identity()
        {
            var usLocale = Localization.Locales.Single(l => l.Name == "us");
            var id = new Identity(usLocale);
            var idJson = JsonConvert.SerializeObject(id, Identity.GetJsonSerializerSettings());

            var accounts = new Accounts();
            accounts.UNITTEST_Seed(new Account("a0") { AccountName = "n0", IdentityTokens = id });

            // dispose to cease auto-updates
            using (var p = new AccountsPersister(accounts, TestFile)) { }

            var persister = new AccountsPersister(TestFile);
            var acct = persister.Accounts.AccountsSettings[0];
            acct.AccountName.Should().Be("n0");
            acct.Locale.CountryCode.Should().Be("us");
        }
    }

    [TestClass]
    public class save : AccountsPersisterTestBase
    {
        // add/save account after file creation
        [TestMethod]
        public void save_1_account()
        {
            // create initial file
            using (var p = new AccountsPersister(new Accounts(), TestFile)) { }

            // load file. create account
            using (var p = new AccountsPersister(TestFile))
            {
                var localeIn = Localization.Locales.Single(l => l.Name == "us");
                var idIn = new Identity(localeIn);
                var acctIn = new Account("a0") { AccountName = "n0", IdentityTokens = idIn };

                p.Accounts.UNITTEST_Seed(acctIn);
            }

            // re-load file. ensure account still exists
            using (var p = new AccountsPersister(TestFile))
            {
                p.Accounts.AccountsSettings.Count.Should().Be(1);
                var acct0 = p.Accounts.AccountsSettings[0];
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
            using (var p = new AccountsPersister(new Accounts(), TestFile)) { }

            // load file. create account 0
            using (var p = new AccountsPersister(TestFile))
            {
                var localeIn = Localization.Locales.Single(l => l.Name == "us");
                var idIn = new Identity(localeIn);
                var acctIn = new Account("a0") { AccountName = "n0", IdentityTokens = idIn };

                p.Accounts.UNITTEST_Seed(acctIn);
            }

            // re-load file. ensure account still exists
            using (var p = new AccountsPersister(TestFile))
            {
                p.Accounts.AccountsSettings.Count.Should().Be(1);

                var acct0 = p.Accounts.AccountsSettings[0];
                acct0.AccountName.Should().Be("n0");
                acct0.Locale.CountryCode.Should().Be("us");
            }

            // load file. create account 1
            using (var p = new AccountsPersister(TestFile))
            {
                var localeIn = Localization.Locales.Single(l => l.Name == "uk");
                var idIn = new Identity(localeIn);
                var acctIn = new Account("a1") { AccountName = "n1", IdentityTokens = idIn };

                p.Accounts.UNITTEST_Seed(acctIn);
            }

            // re-load file. ensure both accounts still exist
            using (var p = new AccountsPersister(TestFile))
            {
                p.Accounts.AccountsSettings.Count.Should().Be(2);

                var acct0 = p.Accounts.AccountsSettings[0];
                acct0.AccountName.Should().Be("n0");
                acct0.Locale.CountryCode.Should().Be("us");

                var acct1 = p.Accounts.AccountsSettings[1];
                acct1.AccountName.Should().Be("n1");
                acct1.Locale.CountryCode.Should().Be("uk");
            }
        }

        // update Account property. must be non-destructive to all other data
        [TestMethod]
        public void update_Account_field()
        {
            // create initial file
            using (var p = new AccountsPersister(new Accounts(), TestFile)) { }

            // load file. create 2 accounts
            using (var p = new AccountsPersister(TestFile))
            {
                var locale1 = Localization.Locales.Single(l => l.Name == "us");
                var id1 = new Identity(locale1);
                var acct1 = new Account("a0") { AccountName = "n0", IdentityTokens = id1 };
                p.Accounts.UNITTEST_Seed(acct1);

                var locale2 = Localization.Locales.Single(l => l.Name == "uk");
                var id2 = new Identity(locale2);
                var acct2 = new Account("a1") { AccountName = "n1", IdentityTokens = id2 };

                p.Accounts.UNITTEST_Seed(acct2);
            }

            // update AccountName on existing file
            using (var p = new AccountsPersister(TestFile))
            {
                var acct0 = p.Accounts.AccountsSettings[0];
                acct0.AccountName = "new";
            }

            // re-load file. ensure both accounts still exist
            using (var p = new AccountsPersister(TestFile))
            {
                p.Accounts.AccountsSettings.Count.Should().Be(2);

                var acct0 = p.Accounts.AccountsSettings[0];
                // new
                acct0.AccountName.Should().Be("new");

                // still here
                acct0.Locale.CountryCode.Should().Be("us");
                var acct1 = p.Accounts.AccountsSettings[1];
                acct1.AccountName.Should().Be("n1");
                acct1.Locale.CountryCode.Should().Be("uk");
            }
        }

        // update identity. must be non-destructive to all other data
        [TestMethod]
        public void replace_identity()
        {
            // create initial file
            using (var p = new AccountsPersister(new Accounts(), TestFile)) { }

            // load file. create 2 accounts
            using (var p = new AccountsPersister(TestFile))
            {
                var locale1 = Localization.Locales.Single(l => l.Name == "us");
                var id1 = new Identity(locale1);
                var acct1 = new Account("a0") { AccountName = "n0", IdentityTokens = id1 };
                p.Accounts.UNITTEST_Seed(acct1);

                var locale2 = Localization.Locales.Single(l => l.Name == "uk");
                var id2 = new Identity(locale2);
                var acct2 = new Account("a1") { AccountName = "n1", IdentityTokens = id2 };

                p.Accounts.UNITTEST_Seed(acct2);
            }

            // update identity on existing file
            using (var p = new AccountsPersister(TestFile))
            {
                var locale = Localization.Locales.Single(l => l.Name == "uk");
                var id = new Identity(locale);

                var acct0 = p.Accounts.AccountsSettings[0];
                acct0.IdentityTokens = id;
            }

            // re-load file. ensure both accounts still exist
            using (var p = new AccountsPersister(TestFile))
            {
                p.Accounts.AccountsSettings.Count.Should().Be(2);

                var acct0 = p.Accounts.AccountsSettings[0];
                // new
                acct0.Locale.CountryCode.Should().Be("uk");

                // still here
                acct0.AccountName.Should().Be("n0");
                var acct1 = p.Accounts.AccountsSettings[1];
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
            using (var p = new AccountsPersister(new Accounts(), TestFile)) { }

            // load file. create 2 accounts
            using (var p = new AccountsPersister(TestFile))
            {
                var locale1 = Localization.Locales.Single(l => l.Name == "us");
                var id1 = new Identity(locale1);
                var acct1 = new Account("a0") { AccountName = "n0", IdentityTokens = id1 };
                p.Accounts.UNITTEST_Seed(acct1);

                var locale2 = Localization.Locales.Single(l => l.Name == "uk");
                var id2 = new Identity(locale2);
                var acct2 = new Account("a1") { AccountName = "n1", IdentityTokens = id2 };

                p.Accounts.UNITTEST_Seed(acct2);
            }

            // update identity on existing file
            using (var p = new AccountsPersister(TestFile))
            {
                p.Accounts.AccountsSettings[0]
                    .IdentityTokens
                    .Update(new AccessToken("Atna|_NEW_", DateTime.Now.AddDays(1)));
            }

            // re-load file. ensure both accounts still exist
            using (var p = new AccountsPersister(TestFile))
            {
                p.Accounts.AccountsSettings.Count.Should().Be(2);

                var acct0 = p.Accounts.AccountsSettings[0];
                // new
                acct0.IdentityTokens.ExistingAccessToken.TokenValue.Should().Be("Atna|_NEW_");

                // still here
                acct0.AccountName.Should().Be("n0");
                acct0.Locale.CountryCode.Should().Be("us");
                var acct1 = p.Accounts.AccountsSettings[1];
                acct1.AccountName.Should().Be("n1");
                acct1.Locale.CountryCode.Should().Be("uk");
            }
        }
    }
}
