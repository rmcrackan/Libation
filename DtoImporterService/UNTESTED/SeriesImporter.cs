using System;
using System.Collections.Generic;
using System.Linq;
using AudibleApiDTOs;
using DataLayer;
using InternalUtilities;

namespace DtoImporterService
{
	public class SeriesImporter : ItemsImporterBase
	{
		public SeriesImporter(LibationContext context, Account account) : base(context, account) { }

		public override IEnumerable<Exception> Validate(IEnumerable<Item> items) => new SeriesValidator().Validate(items);

		protected override int DoImport(IEnumerable<Item> items)
		{
			// get distinct
			var series = items.GetSeriesDistinct().ToList();

			// load db existing => .Local
			loadLocal_series(series);

			// upsert
			var qtyNew = upsertSeries(series);
			return qtyNew;
		}

		private void loadLocal_series(List<AudibleApiDTOs.Series> series)
		{
			var seriesIds = series.Select(s => s.SeriesId).ToList();
			var localIds = DbContext.Series.Local.Select(s => s.AudibleSeriesId).ToList();
			var remainingSeriesIds = seriesIds
				.Distinct()
				.Except(localIds)
				.ToList();

			if (remainingSeriesIds.Any())
				DbContext.Series.Where(s => remainingSeriesIds.Contains(s.AudibleSeriesId)).ToList();
		}

		private int upsertSeries(List<AudibleApiDTOs.Series> requestedSeries)
		{
			var qtyNew = 0;

			foreach (var s in requestedSeries)
			{
				var series = DbContext.Series.Local.SingleOrDefault(c => c.AudibleSeriesId == s.SeriesId);
				if (series is null)
				{
					series = DbContext.Series.Add(new DataLayer.Series(new AudibleSeriesId(s.SeriesId))).Entity;
					qtyNew++;
				}
				series.UpdateName(s.SeriesName);
			}

			return qtyNew;
		}
	}
}
