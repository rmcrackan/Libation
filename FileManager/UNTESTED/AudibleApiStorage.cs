using System.IO;

namespace FileManager
{
	public static class AudibleApiStorage
	{
		public static string IdentityTokensFile => Path.Combine(Configuration.Instance.LibationFiles, "IdentityTokens.json");
	}
}
