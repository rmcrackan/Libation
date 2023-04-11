using ApplicationServices;
using DataLayer;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LibationUiBase.GridView
{
#nullable enable
	public static class QueryExtensions
	{
		public static IEnumerable<ILibraryBookEntry> BookEntries(this IEnumerable<IGridEntry> gridEntries)
			=> gridEntries.OfType<ILibraryBookEntry>();

		public static IEnumerable<ISeriesEntry> SeriesEntries(this IEnumerable<IGridEntry> gridEntries)
			=> gridEntries.OfType<ISeriesEntry>();

		public static T? FindByAsin<T>(this IEnumerable<T> gridEntries, string audibleProductID) where T : IGridEntry
			=> gridEntries.FirstOrDefault(i => i.AudibleProductId == audibleProductID);

		public static IEnumerable<ISeriesEntry> EmptySeries(this IEnumerable<IGridEntry> gridEntries)
			=> gridEntries.SeriesEntries().Where(i => i.Children.Count == 0);

		public static ISeriesEntry? FindSeriesParent(this IEnumerable<IGridEntry> gridEntries, LibraryBook seriesEpisode)
		{
			if (seriesEpisode.Book.SeriesLink is null) return null;

			try
			{
				//Parent books will always have exactly 1 SeriesBook due to how
				//they are imported in ApiExtended.getChildEpisodesAsync()
				return gridEntries.SeriesEntries().FirstOrDefault(
					lb =>
					seriesEpisode.Book.SeriesLink.Any(
						s => s.Series.AudibleSeriesId == lb.LibraryBook.Book.SeriesLink.Single().Series.AudibleSeriesId));
			}
			catch (Exception ex)
			{
				Serilog.Log.Error(ex, "Query error in {0}", nameof(FindSeriesParent));
				return null;
			}
		}

		public static bool SearchSetsDiffer(this HashSet<IGridEntry>? searchSet, HashSet<IGridEntry>? otherSet)
			=> searchSet is null != otherSet is null ||
					(searchSet is not null &&
					otherSet is not null &&
					searchSet.Intersect(otherSet).Count() != searchSet.Count);

		public static HashSet<IGridEntry>? FilterEntries(this IEnumerable<IGridEntry> entries, string searchString)
		{
			if (string.IsNullOrEmpty(searchString)) return null;

			var searchResultSet = SearchEngineCommands.Search(searchString);

			var booksFilteredIn = entries.BookEntries().Join(searchResultSet.Docs, lbe => lbe.AudibleProductId, d => d.ProductId, (lbe, d) => (IGridEntry)lbe);

			//Find all series containing children that match the search criteria
			var seriesFilteredIn = entries.SeriesEntries().Where(s => s.Children.Join(searchResultSet.Docs, lbe => lbe.AudibleProductId, d => d.ProductId, (lbe, d) => lbe).Any());

			return booksFilteredIn.Concat(seriesFilteredIn).ToHashSet();
		}
	}
#nullable disable
}
