using DataLayer;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LibationWinForms.GridView
{
#nullable enable
	internal static class QueryExtensions
	{
		public static IEnumerable<LibraryBook> FindChildren(this IEnumerable<LibraryBook> bookList, LibraryBook parent)
			=> bookList.Where(lb => lb.Book.SeriesLink?.Any(s => s.Series.AudibleSeriesId == parent.Book.AudibleProductId) == true);

		public static IEnumerable<LibraryBookEntry> BookEntries(this IEnumerable<GridEntry> gridEntries)
			=> gridEntries.OfType<LibraryBookEntry>();

		public static IEnumerable<SeriesEntry> SeriesEntries(this IEnumerable<GridEntry> gridEntries)
			=> gridEntries.OfType<SeriesEntry>();

		public static T? FindByAsin<T>(this IEnumerable<T> gridEntries, string audibleProductID) where T : GridEntry
			=> gridEntries.FirstOrDefault(i => i.AudibleProductId == audibleProductID);

		public static IEnumerable<SeriesEntry> EmptySeries(this IEnumerable<GridEntry> gridEntries)
			=> gridEntries.SeriesEntries().Where(i => i.Children.Count == 0);

		public static bool IsProduct(this LibraryBook lb)
			=> lb.Book.ContentType is not ContentType.Episode and not ContentType.Parent;

		public static bool IsEpisodeChild(this LibraryBook lb)
			=> lb.Book.ContentType is ContentType.Episode;

		public static bool IsEpisodeParent(this LibraryBook lb)
			=> lb.Book.ContentType is ContentType.Parent;

		public static SeriesEntry? FindSeriesParent(this IEnumerable<GridEntry> gridEntries, LibraryBook seriesEpisode)
		{
			if (seriesEpisode.Book.SeriesLink is null) return null;

			//Parent books will always have exactly 1 SeriesBook due to how
			//they are imported in ApiExtended.getChildEpisodesAsync()
			return gridEntries.SeriesEntries().FirstOrDefault(
				lb =>
				seriesEpisode.Book.SeriesLink.Any(
					s => s.Series.AudibleSeriesId == lb.LibraryBook.Book.SeriesLink.Single().Series.AudibleSeriesId));
		}

		public static LibraryBook? FindSeriesParent(this IEnumerable<LibraryBook> libraryBooks, LibraryBook seriesEpisode)
		{
			if (seriesEpisode.Book.SeriesLink is null) return null;

			//Parent books will always have exactly 1 SeriesBook due to how
			//they are imported in ApiExtended.getChildEpisodesAsync()
			return libraryBooks.FirstOrDefault(
				lb =>
				lb.IsEpisodeParent() &&
				seriesEpisode.Book.SeriesLink.Any(
					s => s.Series.AudibleSeriesId == lb.Book.SeriesLink.Single().Series.AudibleSeriesId));
		}
	}
#nullable disable
}
