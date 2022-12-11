using DataLayer;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LibationAvalonia.ViewModels
{
#nullable enable
	internal static class QueryExtensions
	{
		public static IEnumerable<LibraryBookEntry> BookEntries(this IEnumerable<GridEntry> gridEntries)
			=> gridEntries.OfType<LibraryBookEntry>();

		public static IEnumerable<SeriesEntry> SeriesEntries(this IEnumerable<GridEntry> gridEntries)
			=> gridEntries.OfType<SeriesEntry>();

		public static SeriesEntry? FindSeriesParent(this IEnumerable<GridEntry> gridEntries, LibraryBook seriesEpisode)
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
