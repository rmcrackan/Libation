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
		public static Account TEST_GetFirstAccount()
			=> new AccountsPersister(AccountsSettingsFile).Accounts.GetAll().FirstOrDefault();
		// convenience for for tests and demos. don't use in production Libation
		public static string TEST_GetFirstIdentityTokensJsonPath()
			=> TEST_GetFirstAccount().GetIdentityTokensJsonPath();

		// TEMP
		public static string GetIdentityTokensJsonPath() => null;

		public static string GetIdentityTokensJsonPath(this Account account)
			=> GetIdentityTokensJsonPath(account.AccountId, account?.IdentityTokens?.Locale.Name);

		public static string GetIdentityTokensJsonPath(string username, string locale)
		{
			var usernameSanitized = JsonConvert.ToString(username);
			var localeSanitized = JsonConvert.ToString(locale);

			return $"$.AccountsSettings[?(@.Username == '{usernameSanitized}' && @.IdentityTokens.Locale == '{localeSanitized}')].IdentityTokens";
		}
	}
}
