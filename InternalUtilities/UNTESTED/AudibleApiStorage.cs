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
		public static string AccountsSettingsFileLegacy30 => Path.Combine(Configuration.Instance.LibationFiles, "IdentityTokens.json");

		public static string AccountsSettingsFile => Path.Combine(Configuration.Instance.LibationFiles, "AccountsSettings.json");

		public static void EnsureAccountsSettingsFileExists()
		{
			if (File.Exists(AccountsSettingsFile))
				return;

			// saves. BEWARE: this will overwrite an existing file
			_ = new AccountsPersister(new Accounts(), AccountsSettingsFile);
		}

		// TEMP
		public static string GetJsonPath() => null;

		public static string GetJsonPath(string username, string locale)
		{
			var usernameSanitized = JsonConvert.ToString(username);
			var localeSanitized = JsonConvert.ToString(locale);

			return $"$.AccountsSettings[?(@.Username == '{usernameSanitized}' && @.IdentityTokens.Locale == '{localeSanitized}')].IdentityTokens";
		}
	}
}
