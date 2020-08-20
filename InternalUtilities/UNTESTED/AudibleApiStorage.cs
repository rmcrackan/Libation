using System;
using System.IO;
using System.Linq;
using AudibleApi;
using FileManager;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace InternalUtilities
{
	public static class AudibleApiStorage
	{
		public static string AccountsSettingsFile => Path.Combine(Configuration.Instance.LibationFiles, "AccountsSettings.json");

		public static void EnsureAccountsSettingsFileExists()
		{
			// saves. BEWARE: this will overwrite an existing file
			if (!File.Exists(AccountsSettingsFile))
				_ = new AccountsPersister(new Accounts(), AccountsSettingsFile);
		}

		// convenience for for tests and demos. don't use in production Libation
		public static string TEST_GetFirstIdentityTokensJsonPath()
			=> TEST_GetFirstAccount().GetIdentityTokensJsonPath();
		// convenience for for tests and demos. don't use in production Libation
		public static Account TEST_GetFirstAccount()
			=> new AccountsPersister(AccountsSettingsFile).Accounts.GetAll().FirstOrDefault();

		public static string GetIdentityTokensJsonPath(this Account account)
			=> GetIdentityTokensJsonPath(account.AccountId, account.Locale?.Name);
		public static string GetIdentityTokensJsonPath(string username, string locale)
		{
			var usernameSanitized = trimSurroundingQuotes(JsonConvert.ToString(username));
			var localeSanitized = trimSurroundingQuotes(JsonConvert.ToString(locale));

			return $"$.AccountsSettings[?(@.AccountId == '{usernameSanitized}' && @.IdentityTokens.LocaleName == '{localeSanitized}')].IdentityTokens";
		}
		// SubString algo is better than .Trim("\"")
		//   orig string  "
		//   json string  "\""
		// Eg:
		//   =>           str.Trim("\"")
		//   output       \
		// vs
		//   =>           str.Substring(1, str.Length - 2)
		//   output       \"
		// also works with surrounding single quotes
		private static string trimSurroundingQuotes(string str)
			=> str.Substring(1, str.Length - 2);
	}
}
