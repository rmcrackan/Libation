using System;
using System.Collections.Generic;
using System.Linq;
using AudibleApi.Common;
using AudibleUtilities;
using DataLayer;
using Dinah.Core.Collections.Generic;

namespace DtoImporterService
{
	public class SeriesImporter : ItemsImporterBase
	{
		protected override IValidator Validator => new SeriesValidator();

		public Dictionary<string, DataLayer.Series> Cache { get; private set; } = new();

		public SeriesImporter(LibationContext context) : base(context) { }

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
			var seriesIds = series.Select(s => s.SeriesId).Distinct().ToList();

			if (seriesIds.Count != 0)
				Cache = DbContext.Series
					.Where(s => seriesIds.Contains(s.AudibleSeriesId))
					.ToDictionarySafe(s => s.AudibleSeriesId);
		}

		private int upsertSeries(List<AudibleApi.Common.Series> requestedSeries)
		{
			var qtyNew = 0;

			foreach (var s in requestedSeries)
			{
				if (string.IsNullOrEmpty(s.SeriesId))
					continue;
				// AudibleApi.Common.Series.SeriesId == DataLayer.AudibleSeriesId
				if (!Cache.TryGetValue(s.SeriesId, out var series))
				{
					series = addSeries(s.SeriesId);
					qtyNew++;
				}
				series.UpdateName(s.SeriesName);
			}

			return qtyNew;
		}

		private DataLayer.Series addSeries(string seriesId)
		{
			try
			{
				var series = new DataLayer.Series(new AudibleSeriesId(seriesId));

				var entityEntry = DbContext.Series.Add(series);
				var entity = entityEntry.Entity;

				Cache.Add(entity.AudibleSeriesId, entity);
				return entity;
			}
			catch (Exception ex)
			{
				Serilog.Log.Logger.Error(ex, "Error adding series. {@DebugInfo}", new { seriesId });
				throw;
			}
		}
	}
}
