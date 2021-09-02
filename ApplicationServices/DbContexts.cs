using System;
using System.Collections.Generic;
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

		public static List<LibraryBook> GetLibrary_Flat_NoTracking()
		{
			using var context = GetContext();
			return context.GetLibrary_Flat_NoTracking();
		}
	}
}
