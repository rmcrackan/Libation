using System;
using System.IO;
using FileManager;
using Newtonsoft.Json;

namespace InternalUtilities
{
	public static class AudibleApiStorage
	{
		public static string AccountsSettingsFile => Path.Combine(Configuration.Instance.LibationFiles, "AccountsSettings.json");

		public static void EnsureAccountsSettingsFileExists()
		{
			// saves. BEWARE: this will overwrite an existing file
			if (!File.Exists(AccountsSettingsFile))
				_ = new AccountsSettingsPersister(new AccountsSettings(), AccountsSettingsFile);
		}

		/// <summary>If you use this, be a good citizen and DISPOSE of it</summary>
		public static AccountsSettingsPersister GetAccountsSettingsPersister() => new AccountsSettingsPersister(AccountsSettingsFile);

		public static string GetIdentityTokensJsonPath(this Account account)
			=> GetIdentityTokensJsonPath(account.AccountId, account.Locale?.Name);
		public static string GetIdentityTokensJsonPath(string username, string localeName)
		{
			var usernameSanitized = trimSurroundingQuotes(JsonConvert.ToString(username));
			var localeNameSanitized = trimSurroundingQuotes(JsonConvert.ToString(localeName));

			return $"$.Accounts[?(@.AccountId == '{usernameSanitized}' && @.IdentityTokens.LocaleName == '{localeNameSanitized}')].IdentityTokens";
		}
		private static string trimSurroundingQuotes(string str)
		{
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

			return str.Substring(1, str.Length - 2);
		}
	}
}
