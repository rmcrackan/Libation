using System;
using DataLayer;
using FileManager;

namespace ApplicationServices
{
	public static class DbContexts
	{
		//// idea for future command/query separation
		// public static LibationContext GetCommandContext() { }
		// public static LibationContext GetQueryContext() { }

		public static LibationContext GetContext()
			=> LibationContext.Create(SqliteStorage.ConnectionString);
	}
}
