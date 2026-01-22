using System.Collections.Generic;
using System.Linq;
using Dinah.Core;
using Microsoft.EntityFrameworkCore;

#nullable enable
namespace DataLayer;

// only library importing should use tracking. All else should be NoTracking.
// only library importing should directly query Book. All else should use LibraryBook
public static class LibraryBookQueries
{
	private static System.Linq.Expressions.Expression<System.Func<LibraryBook, bool>> IsUnLiberatedExpression { get; }
		= lb =>
		!lb.AbsentFromLastScan &&
		(lb.Book.ContentType == ContentType.Product || lb.Book.ContentType == ContentType.Episode) &&
		(lb.Book.UserDefinedItem.PdfStatus == LiberatedStatus.NotLiberated || lb.Book.UserDefinedItem.BookStatus == LiberatedStatus.NotLiberated || lb.Book.UserDefinedItem.BookStatus == LiberatedStatus.PartialDownload);

	extension(LibationContext context)
	{
		//// tracking is a bad idea for main grid. it prevents anything else from updating entities unless getting them from the grid
		//public static List<LibraryBook> GetLibrary_Flat_WithTracking(this LibationContext context)
		//	=> context
		//		.Library
		//		.GetLibrary()
		//		.ToList();

		public List<LibraryBook> GetLibrary_Flat_NoTracking(bool includeParents = false)
			=> context
			.LibraryBooks
			.AsNoTrackingWithIdentityResolution()
			.GetLibrary()
			.Where(lb => lb.Book.ContentType != ContentType.Parent || includeParents)
			.AsEnumerable()
			.ToList();

		public LibraryBook? GetLibraryBook_Flat_NoTracking(string productId, bool caseSensative = true)
		{
			var libraryQuery
				= context
				.LibraryBooks
				.AsNoTrackingWithIdentityResolution()
				.GetLibrary();

			return caseSensative ? libraryQuery.SingleOrDefault(lb => lb.Book.AudibleProductId == productId)
				: libraryQuery.SingleOrDefault(lb => EF.Functions.Collate(lb.Book.AudibleProductId, "NOCASE") == productId);
		}

		public List<LibraryBook> GetUnLiberated_Flat_NoTracking()
			=> context
			.LibraryBooks
			.AsNoTrackingWithIdentityResolution()
			.GetLibrary()
			.Where(IsUnLiberatedExpression)
			.AsEnumerable()
			.ToList();

		public List<LibraryBook> GetDeletedLibraryBooks()
			=> context
			.LibraryBooks
			.AsNoTrackingWithIdentityResolution()
			//Return all parents so the trash bin grid can show podcasts beneath their parents
			.Where(lb => lb.IsDeleted || lb.Book.ContentType == ContentType.Parent)
			.getLibrary()
			.ToList();
	}

	extension(IQueryable<LibraryBook> library)
	{
		/// <summary>This is still IQueryable. YOU MUST CALL ToList() YOURSELF</summary>
		public IQueryable<LibraryBook> GetLibrary()
			=> library.Where(lb => !lb.IsDeleted).getLibrary();

		/// <summary>This is still IQueryable. YOU MUST CALL ToList() YOURSELF</summary>
		private IQueryable<LibraryBook> getLibrary()
			=> library
				// owned items are always loaded. eg: book.UserDefinedItem, book.Supplements
				.Include(le => le.Book).ThenInclude(b => b.SeriesLink).ThenInclude(sb => sb.Series)
				.Include(le => le.Book).ThenInclude(b => b.ContributorsLink).ThenInclude(c => c.Contributor)
				.Include(le => le.Book).ThenInclude(b => b.CategoriesLink).ThenInclude(c => c.CategoryLadder).ThenInclude(c => c._categories);
	}

	extension(IEnumerable<LibraryBook> libraryBooks)
	{
		public IEnumerable<LibraryBook> UnLiberated()
			=> libraryBooks.Where(lb => lb.NeedsPdfDownload || lb.NeedsBookDownload);

		public IEnumerable<LibraryBook> ParentedEpisodes()
				=> libraryBooks.Where(lb => lb.Book.IsEpisodeParent()).SelectMany(libraryBooks.FindChildren);

		public IEnumerable<LibraryBook> FindOrphanedEpisodes()
			=> libraryBooks
				.Where(lb => lb.Book.IsEpisodeChild())
				.ExceptBy(
					libraryBooks
					.ParentedEpisodes()
					.Select(ge => ge.Book.AudibleProductId), ge => ge.Book.AudibleProductId);

		public IEnumerable<LibraryBook> FindChildren(LibraryBook parent)
			=> libraryBooks.Where(lb => lb.Book.IsEpisodeChild() && lb.HasSeriesId(parent.Book.AudibleProductId));

		public LibraryBook? FindSeriesParent(LibraryBook seriesEpisode)
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
	}

	extension(LibraryBook libraryBook)
	{
		public bool HasSeriesId(string audibleSeriesId) => libraryBook.Book.SeriesLink?.Any(s => s.Series.AudibleSeriesId.EqualsInsensitive(audibleSeriesId)) is true;
		public bool Downloadable => !libraryBook.AbsentFromLastScan && libraryBook.Book.ContentType is ContentType.Product or ContentType.Episode;
		public bool NeedsPdfDownload => libraryBook.Downloadable && libraryBook.Book.UserDefinedItem.PdfStatus is LiberatedStatus.NotLiberated;
		public bool NeedsBookDownload => libraryBook.Downloadable && libraryBook.Book.UserDefinedItem.BookStatus is LiberatedStatus.NotLiberated or LiberatedStatus.PartialDownload;
	}
}
