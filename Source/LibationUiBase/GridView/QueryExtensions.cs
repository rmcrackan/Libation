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
	}
#nullable disable
}
