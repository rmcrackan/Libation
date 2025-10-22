using System;
using System.Collections.Generic;
using DataLayer;
using LibationFileManager;

namespace ApplicationServices
{
	public static class DbContexts
	{
		/// <summary>Use for fully functional context, incl. SaveChanges(). For query-only, use the other method</summary>
		public static LibationContext GetContext()
			=> InstanceQueue<LibationContext>.WaitToCreateInstance(() => LibationContext.Create(SqliteStorage.ConnectionString));

		/// <summary>Use for full library querying. No lazy loading</summary>
		public static List<LibraryBook> GetLibrary_Flat_NoTracking(bool includeParents = false)
		{
			using var context = GetContext();
			return context.GetLibrary_Flat_NoTracking(includeParents);
		}
	}
}
