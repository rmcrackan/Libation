using DataLayer;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LibationWinForms.AvaloniaUI.ViewModels
{
#nullable enable
	internal static class QueryExtensions
	{
		public static IEnumerable<LibraryBookEntry2> BookEntries(this IEnumerable<GridEntry2> gridEntries)
			=> gridEntries.OfType<LibraryBookEntry2>();

		public static IEnumerable<SeriesEntrys2> SeriesEntries(this IEnumerable<GridEntry2> gridEntries)
			=> gridEntries.OfType<SeriesEntrys2>();

		public static T? FindByAsin<T>(this IEnumerable<T> gridEntries, string audibleProductID) where T : GridEntry2
			=> gridEntries.FirstOrDefault(i => i.AudibleProductId == audibleProductID);

		public static IEnumerable<SeriesEntrys2> EmptySeries(this IEnumerable<GridEntry2> gridEntries)
			=> gridEntries.SeriesEntries().Where(i => i.Children.Count == 0);

		public static SeriesEntrys2? FindSeriesParent(this IEnumerable<GridEntry2> gridEntries, LibraryBook seriesEpisode)
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
