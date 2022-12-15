using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace DataLayer
{
    // only library importing should use tracking. All else should be NoTracking.
    // only library importing should directly query Book. All else should use LibraryBook
    public static class LibraryBookQueries
	{
        //// tracking is a bad idea for main grid. it prevents anything else from updating entities unless getting them from the grid
		//public static List<LibraryBook> GetLibrary_Flat_WithTracking(this LibationContext context)
		//	=> context
		//		.Library
		//		.GetLibrary()
		//		.ToList();

		public static List<LibraryBook> GetLibrary_Flat_NoTracking(this LibationContext context, bool includeParents = false)
            => context
                .LibraryBooks
                .AsNoTrackingWithIdentityResolution()
                .GetLibrary()
                .AsEnumerable()
                .Where(lb => !lb.Book.IsEpisodeParent() || includeParents)
                .ToList();

        public static LibraryBook GetLibraryBook_Flat_NoTracking(this LibationContext context, string productId)
            => context
                .LibraryBooks
                .AsNoTrackingWithIdentityResolution()
                .GetLibraryBook(productId);

        public static LibraryBook GetLibraryBook(this IQueryable<LibraryBook> library, string productId)
            => library
                .GetLibrary()
                .SingleOrDefault(lb => lb.Book.AudibleProductId == productId);

        /// <summary>This is still IQueryable. YOU MUST CALL ToList() YOURSELF</summary>
        public static IQueryable<LibraryBook> GetLibrary(this IQueryable<LibraryBook> library)
            => library
                .Where(lb => !lb.IsDeleted)
                .getLibrary();

        public static List<LibraryBook> GetDeletedLibraryBooks(this LibationContext context)
            => context
                .LibraryBooks
                .AsNoTrackingWithIdentityResolution()
                .Where(lb => lb.IsDeleted)
                .getLibrary()
                .ToList();

        /// <summary>This is still IQueryable. YOU MUST CALL ToList() YOURSELF</summary>
        private static IQueryable<LibraryBook> getLibrary(this IQueryable<LibraryBook> library)
            => library
                // owned items are always loaded. eg: book.UserDefinedItem, book.Supplements
                .Include(le => le.Book).ThenInclude(b => b.SeriesLink).ThenInclude(sb => sb.Series)
                .Include(le => le.Book).ThenInclude(b => b.ContributorsLink).ThenInclude(c => c.Contributor)
                .Include(le => le.Book).ThenInclude(b => b.Category).ThenInclude(c => c.ParentCategory);

        public static IEnumerable<LibraryBook> ParentedEpisodes(this IEnumerable<LibraryBook> libraryBooks)
            => libraryBooks.Where(lb => lb.Book.IsEpisodeParent()).SelectMany(libraryBooks.FindChildren);

        public static IEnumerable<LibraryBook> FindOrphanedEpisodes(this IEnumerable<LibraryBook> libraryBooks)
		    => libraryBooks
                .Where(lb => lb.Book.IsEpisodeChild())
                .ExceptBy(
                    libraryBooks
                    .ParentedEpisodes()
                    .Select(ge => ge.Book.AudibleProductId), ge => ge.Book.AudibleProductId);

#nullable enable
        public static LibraryBook? FindSeriesParent(this IEnumerable<LibraryBook> libraryBooks, LibraryBook seriesEpisode)
        {
            if (seriesEpisode.Book.SeriesLink is null) return null;

            try
            {
                //Parent books will always have exactly 1 SeriesBook due to how
                //they are imported in ApiExtended.getChildEpisodesAsync()
                return libraryBooks.FirstOrDefault(
                    lb =>
                    lb.Book.IsEpisodeParent() &&
                    seriesEpisode.Book.SeriesLink.Any(
                        s => s.Series.AudibleSeriesId == lb.Book.SeriesLink.Single().Series.AudibleSeriesId));
            }
            catch (System.Exception ex)
            {
                Serilog.Log.Error(ex, "Query error in {0}", nameof(FindSeriesParent));
                return null;
            }
        }
#nullable disable

        public static IEnumerable<LibraryBook> FindChildren(this IEnumerable<LibraryBook> bookList, LibraryBook parent)
            => bookList
            .Where(
                lb => 
                lb.Book.IsEpisodeChild() && 
                lb.Book.SeriesLink?
                    .Any(
                        s => 
                        s.Series.AudibleSeriesId == parent.Book.AudibleProductId
                        ) == true
                    ).ToList();

        public static IEnumerable<LibraryBook> UnLiberated(this IEnumerable<LibraryBook> bookList)
            => bookList
                .Where(
                    lb =>
                        lb.Book.UserDefinedItem.BookStatus is LiberatedStatus.NotLiberated or LiberatedStatus.PartialDownload
                        || lb.Book.UserDefinedItem.PdfStatus is LiberatedStatus.NotLiberated or LiberatedStatus.PartialDownload
                    );
    }
}
