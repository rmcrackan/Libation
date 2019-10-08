using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace DataLayer
{
    public static class LibraryQueries
    {
        public static List<LibraryBook> GetLibrary_Flat_NoTracking()
        {
			using var context = LibationContext.Create();
			return context
.Library
.AsNoTracking()
.GetLibrary()
.ToList();
		}

        public static LibraryBook GetLibraryBook_Flat_NoTracking(string productId)
        {
			using var context = LibationContext.Create();
			return context
.Library
.AsNoTracking()
.GetLibraryBook(productId);
		}

        /// <summary>This is still IQueryable. YOU MUST CALL ToList() YOURSELF</summary>
        public static IQueryable<LibraryBook> GetLibrary(this IQueryable<LibraryBook> library)
            => library
                // owned items are always loaded. eg: book.UserDefinedItem, book.Supplements
                .Include(le => le.Book).ThenInclude(b => b.SeriesLink).ThenInclude(sb => sb.Series)
                .Include(le => le.Book).ThenInclude(b => b.ContributorsLink).ThenInclude(c => c.Contributor)
                .Include(le => le.Book).ThenInclude(b => b.Category).ThenInclude(c => c.ParentCategory);

        public static LibraryBook GetLibraryBook(this IQueryable<LibraryBook> library, string productId)
            => library
                .GetLibrary()
                .SingleOrDefault(le => le.Book.AudibleProductId == productId);
    }
}
