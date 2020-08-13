using Newtonsoft.Json;
using System.IO;

namespace FileManager
{
	public static class AudibleApiStorage
	{
		public static string AccountsSettingsFile => Path.Combine(Configuration.Instance.LibationFiles, "AccountsSettings.json");

		public static string GetJsonPath(
			//string username
			////, string locale
			)
		{
			return null;


			//var usernameSanitized = JsonConvert.ToString(username);

			////var localeSanitized = JsonConvert.ToString(locale);
			////return $"$.AccountsSettings[?(@.Username == '{usernameSanitized}' && @.IdentityTokens.Locale == '{localeSanitized}')].IdentityTokens";

			//return $"$.AccountsSettings[?(@.Username == '{usernameSanitized}')].IdentityTokens";
		}
	}
}
