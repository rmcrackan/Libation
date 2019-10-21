using System.IO;
using FileManager;

namespace AudibleApiDomainService
{
	public class Settings
	{
		public string IdentityFilePath { get; }
		public string LocaleCountryCode { get; }

		public Settings(Configuration config)
		{
			IdentityFilePath = Path.Combine(config.LibationFiles, "identityTokens.json");
			LocaleCountryCode = config.LocaleCountryCode;
		}
	}
}