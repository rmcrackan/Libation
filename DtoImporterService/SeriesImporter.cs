using System;
using System.Collections.Generic;
using System.Linq;
using AudibleApi.Common;
using AudibleUtilities;
using DataLayer;

namespace DtoImporterService
{
	public class SeriesImporter : ItemsImporterBase
	{
		public SeriesImporter(LibationContext context) : base(context) { }

		public override IEnumerable<Exception> Validate(IEnumerable<ImportItem> importItems) => new SeriesValidator().Validate(importItems.Select(i => i.DtoItem));

		protected override int DoImport(IEnumerable<ImportItem> importItems)
		{
			// get distinct
			var series = importItems
				.Select(i => i.DtoItem)
				.GetSeriesDistinct()
				.ToList();

			// load db existing => .Local
			loadLocal_series(series);

			// upsert
			var qtyNew = upsertSeries(series);
			return qtyNew;
		}

		private void loadLocal_series(List<AudibleApi.Common.Series> series)
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

		private int upsertSeries(List<AudibleApi.Common.Series> requestedSeries)
		{
			var qtyNew = 0;

			foreach (var s in requestedSeries)
			{
				var series = DbContext.Series.Local.FirstOrDefault(c => c.AudibleSeriesId == s.SeriesId);
				if (series is null)
				{
					try
					{
						series = DbContext.Series.Add(new DataLayer.Series(new AudibleSeriesId(s.SeriesId))).Entity;
					}
					catch (Exception ex)
					{
						Serilog.Log.Logger.Error(ex, "Error adding series. {@DebugInfo}", new { s?.SeriesId });
						throw;
					}
					qtyNew++;
				}
				series.UpdateName(s.SeriesName);
			}

			return qtyNew;
		}
	}
}
