using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace DataLayer
{
    // only library importing should use tracking. All else should be NoTracking.
    // only library importing should directly query Book. All else should use LibraryBook
    public static class BookQueries
    {
        public static Book GetBook_Flat_NoTracking(this LibationContext context, string productId)
            => context
                .Books
                .AsNoTrackingWithIdentityResolution()
                .GetBook(productId);

        public static Book GetBook(this IQueryable<Book> books, string productId)
            => books
                .GetBooks()
                // 'Single' is more accurate but 'First' is faster and less error prone
                .FirstOrDefault(b => b.AudibleProductId == productId);

        /// <summary>This is still IQueryable. YOU MUST CALL ToList() YOURSELF</summary>
        public static IQueryable<Book> GetBooks(this IQueryable<Book> books, Expression<Func<Book, bool>> predicate)
            => books
                .GetBooks()
                .Where(predicate);

        /// <summary>This is still IQueryable. YOU MUST CALL ToList() YOURSELF</summary>
        public static IQueryable<Book> GetBooks(this IQueryable<Book> books)
            => books
                // owned items are always loaded. eg: book.UserDefinedItem, book.Supplements
                .Include(b => b.SeriesLink).ThenInclude(sb => sb.Series)
                .Include(b => b.ContributorsLink).ThenInclude(c => c.Contributor)
                .Include(b => b.Category).ThenInclude(c => c.ParentCategory);

        public static bool IsProduct(this Book book)
            => book.ContentType is not ContentType.Episode and not ContentType.Parent;

        public static bool IsEpisodeChild(this Book book)
            => book.ContentType is ContentType.Episode;

        public static bool IsEpisodeParent(this Book book)
            => book.ContentType is ContentType.Parent;

    }
}
