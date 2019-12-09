using System.IO;

namespace FileManager
{
	public static class SqliteStorage
	{
		// not customizable. don't move to config
		private static string databasePath => Path.Combine(Configuration.Instance.LibationFiles, "LibationContext.db");
		public static string ConnectionString => $"Data Source={databasePath};Foreign Keys=False;";
	}
}
