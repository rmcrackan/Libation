using DataLayer;
using LibationFileManager;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

#nullable enable
namespace ApplicationServices
{
    public static class DbContexts
    {
        /// <summary>Use for fully functional context, incl. SaveChanges(). For query-only, use the other method</summary>
        public static LibationContext GetContext()
        {
            var context = !string.IsNullOrEmpty(Configuration.Instance.PostgresqlConnectionString)
                ? LibationContextFactory.CreatePostgres(Configuration.Instance.PostgresqlConnectionString)
                : LibationContextFactory.CreateSqlite(SqliteStorage.ConnectionString);
            context.Database.Migrate();
            return context;
        }

        /// <summary>Use for full library querying. No lazy loading</summary>
        public static List<LibraryBook> GetLibrary_Flat_NoTracking(bool includeParents = false)
        {
            using var context = GetContext();
            return context.GetLibrary_Flat_NoTracking(includeParents);
        }

        public static List<LibraryBook> GetUnliberated_Flat_NoTracking(bool includeParents = false)
        {
            using var context = GetContext();
            return context.GetUnLiberated_Flat_NoTracking();
        }

        public static List<LibraryBook> GetDeletedLibraryBooks()
        {
            using var context = GetContext();
            return context.GetDeletedLibraryBooks();
        }

		public static LibraryBook? GetLibraryBook_Flat_NoTracking(string productId, bool caseSensative = true)
        {
			using var context = GetContext();
			return context.GetLibraryBook_Flat_NoTracking(productId, caseSensative);
		}
	}
}
