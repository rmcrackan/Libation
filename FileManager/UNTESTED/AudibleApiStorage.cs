using System.IO;

namespace FileManager
{
	public static class AudibleApiStorage
	{
		// not customizable. don't move to config
		public static string AccountsSettingsFile => Path.Combine(Configuration.Instance.LibationFiles, "AccountsSettings.json");
	}
}
