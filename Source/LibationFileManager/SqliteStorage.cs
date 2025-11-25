using System.IO;

namespace LibationFileManager
{
	public static class SqliteStorage
	{
		// not customizable. don't move to config
		public static string DatabasePath => Path.Combine(Configuration.Instance.LibationFiles.Location, "LibationContext.db");
		public static string ConnectionString => $"Data Source={DatabasePath};Foreign Keys=False;Pooling=False;";
	}
}
