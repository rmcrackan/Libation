using System.IO;

namespace FileManager
{
	public static class AudibleApiStorage
	{
		// not customizable. don't move to config
		public static string IdentityTokensFile => Path.Combine(Configuration.Instance.LibationFiles, "IdentityTokens.json");
	}
}
