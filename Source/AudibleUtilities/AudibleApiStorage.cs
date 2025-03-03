using System;
using System.IO;
using LibationFileManager;
using Newtonsoft.Json;

namespace AudibleUtilities
{
	public class AccountSettingsLoadErrorEventArgs : ErrorEventArgs
	{
		/// <summary>
		/// Create a new, empty <see cref="AccountsSettings"/> file if true, otherwise throw
		/// </summary>
		public bool Handled { get; set; }
		/// <summary>
		/// The file path of the AccountsSettings.json file
		/// </summary>
		public string SettingsFilePath { get; }

		public AccountSettingsLoadErrorEventArgs(string path, Exception exception)
			: base(exception)
		{
			SettingsFilePath = path;
		}
	}

	public static class AudibleApiStorage
	{
		public static string AccountsSettingsFile => Path.Combine(Configuration.Instance.LibationFiles, "AccountsSettings.json");

		public static event EventHandler<AccountSettingsLoadErrorEventArgs> LoadError;

		public static void EnsureAccountsSettingsFileExists()
		{
			// saves. BEWARE: this will overwrite an existing file
			if (!File.Exists(AccountsSettingsFile))
			{
				//Save the JSON file manually so that AccountsSettingsPersister.Saving and AccountsSettingsPersister.Saved
				//are not fired. There's no need to fire those events on an empty AccountsSettings file.
				var accountSerializerSettings = AudibleApi.Authorization.Identity.GetJsonSerializerSettings();
				File.WriteAllText(AccountsSettingsFile, JsonConvert.SerializeObject(new AccountsSettings(), Formatting.Indented, accountSerializerSettings));
			}
		}

		/// <summary>If you use this, be a good citizen and DISPOSE of it</summary>
		public static AccountsSettingsPersister GetAccountsSettingsPersister()
		{
			try
			{
				return new AccountsSettingsPersister(AccountsSettingsFile);
			}
			catch (Exception ex)
			{
				var args = new AccountSettingsLoadErrorEventArgs(AccountsSettingsFile, ex);
				LoadError?.Invoke(null, args);
				if (args.Handled)
					return GetAccountsSettingsPersister();
				throw;
			}
		}

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
