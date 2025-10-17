using System;
using System.Collections.Generic;
using System.Linq;
using AudibleApi.Common;
using AudibleUtilities;
using DataLayer;
using Dinah.Core.Collections.Generic;

namespace DtoImporterService
{
	public class BookImporter : ItemsImporterBase
	{
		protected override IValidator Validator => new BookValidator();

		public Dictionary<string, Book> Cache { get; private set; } = new();

		private ContributorImporter contributorImporter { get; }
		private SeriesImporter seriesImporter { get; }
		private CategoryImporter categoryImporter { get; }

		public BookImporter(LibationContext context) : base(context)
		{
			contributorImporter = new ContributorImporter(DbContext);
			seriesImporter = new SeriesImporter(DbContext);
			categoryImporter = new CategoryImporter(DbContext);
		}

		protected override int DoImport(IEnumerable<ImportItem> importItems)
		{
			// pre-req.s
			contributorImporter.Import(importItems);
			seriesImporter.Import(importItems);
			categoryImporter.Import(importItems);

			// load db existing => hash table
			loadLocal_books(importItems);

			// upsert
			var qtyNew = upsertBooks(importItems);
			return qtyNew;
		}

		private void loadLocal_books(IEnumerable<ImportItem> importItems)
		{
			// get distinct
			var productIds = importItems
				.Select(i => i.DtoItem.ProductId)
				.Distinct()
				.ToHashSet();

			Cache = DbContext.Books
				.GetBooks(b => productIds.Contains(b.AudibleProductId))
				.ToDictionarySafe(b => b.AudibleProductId);
		}

		private int upsertBooks(IEnumerable<ImportItem> importItems)
		{
			var qtyNew = 0;

			foreach (var item in importItems)
			{
				if (!Cache.TryGetValue(item.DtoItem.ProductId, out var book))
				{
					book = createNewBook(item);
					qtyNew++;
				}

				updateBook(item, book);
			}

			return qtyNew;
		}

		private Book createNewBook(ImportItem importItem)
		{
			var item = importItem.DtoItem;

			var contentType = GetContentType(item);

			// absence of authors is very rare, but possible
			if (!item.Authors?.Any() ?? true)
				item.Authors = new[] { new Person { Name = "", Asin = null } };

			// nested logic is required so order of names is retained. else, contributors may appear in the order they were inserted into the db
			var authors = item
				.Authors
                .DistinctBy(a => a.Name)
                .Select(a => contributorImporter.Cache[a.Name])
				.ToList();

			var narrators
				= item.Narrators is null || !item.Narrators.Any()
				// if no narrators listed, author is the narrator
				? authors
				// nested logic is required so order of names is retained. else, contributors may appear in the order they were inserted into the db
				: item
					.Narrators
					.DistinctBy(a => a.Name)
                    .Select(n => contributorImporter.Cache[n.Name])
					.ToList();
		     
			//Used to determine when your audible plus or free book will expire from your library  
			//plan.IsAyce from underlying AudibleApi project determines the plans to look at, first plan found is used. 
			DateTime? includedUntil = null;
			if (item.Plans is not null)
			{
				foreach (var plan in item.Plans)
				{
					if (plan.IsAyce && plan.EndDate.Value.Year != 2099 && plan.EndDate.HasValue)
					{
						includedUntil = plan.EndDate.Value.LocalDateTime;
					}
				}
			}

			Book book;
			try
			{
				book = DbContext.Books.Add(new Book(
					new AudibleProductId(item.ProductId),
					item.Title,
					item.Subtitle,
					item.Description,
					item.LengthInMinutes,
					contentType,
					authors,
					narrators,
					importItem.LocaleName,
					includedUntil
					)
					).Entity;
				Cache.Add(book.AudibleProductId, book);
			}
			catch (Exception ex)
			{
				Serilog.Log.Logger.Error(ex, "Error adding book. {@DebugInfo}", new {
					item.ProductId,
					item.TitleWithSubtitle,
					item.Description,
					item.LengthInMinutes,
					contentType,
					QtyAuthors = authors?.Count,
					QtyNarrators = narrators?.Count,
					importItem.LocaleName,
					includedUntil
				});
				throw;
			}

			var publisherName = item.Publisher;
			if (!string.IsNullOrWhiteSpace(publisherName))
			{
				var publisher = contributorImporter.Cache[publisherName];
				book.ReplacePublisher(publisher);
			}

            if (item.PdfUrl is not null)
				book.AddSupplementDownloadUrl(item.PdfUrl.ToString());

			return book;
		}

		private void updateBook(ImportItem importItem, Book book)
		{
			var item = importItem.DtoItem;

			book.UpdateLengthInMinutes(item.LengthInMinutes);

			// Update the book titles, since formatting can change
			book.UpdateTitle(item.Title, item.Subtitle);

			// set/update book-specific info which may have changed
			if (item.PictureId is not null)
				book.PictureId = item.PictureId;
			
			if (item.PictureLarge is not null)
				book.PictureLarge = item.PictureLarge;

			if (item.IsFinished is not null)
                book.UserDefinedItem.IsFinished = item.IsFinished.Value;

            // 2023-02-01
            // updateBook must update language on books which were imported before the migration which added language.
            // 2025-07-30
            // updateBook must update isSpatial on books which were imported before the migration which added isSpatial.
            book.UpdateBookDetails(item.IsAbridged, item.AssetDetails?.Any(a => a.IsSpatial), item.DatePublished, item.Language);

            book.UpdateProductRating(
				(float)(item.Rating?.OverallDistribution?.AverageRating ?? 0),
				(float)(item.Rating?.PerformanceDistribution?.AverageRating ?? 0),
				(float)(item.Rating?.StoryDistribution?.AverageRating ?? 0));

			// important to update user-specific info. this will have changed if user has rated/reviewed the book since last library import
			book.UserDefinedItem.UpdateRating(item.MyUserRating_Overall, item.MyUserRating_Performance, item.MyUserRating_Story);

			// update series even for existing books. these are occasionally updated
			// these will upsert over library-scraped series, but will not leave orphans
			if (item.Series is not null)
			{
				foreach (var seriesEntry in item.Series)
				{
					var series = seriesImporter.Cache[seriesEntry.SeriesId];
					book.UpsertSeries(series, seriesEntry.Sequence);
				}
			}

			if (item.CategoryLadders is not null)
			{
				var ladders = new List<DataLayer.CategoryLadder>();
				foreach (var ladder in item.CategoryLadders.Select(cl => cl.Ladder).Where(l => l?.Length > 0))
				{
					var categoryIds = ladder.Select(l => l.CategoryId).ToList();
					ladders.Add(categoryImporter.LadderCache.Single(c => c.Equals(categoryIds)));
				}
				//Set all ladders at once so ladders that have been
				//removed by audible can be removed from the DB
				book.SetCategoryLadders(ladders);
			}
		}

		private static DataLayer.ContentType GetContentType(Item item)
		{
			if (item.IsEpisodes)
				return DataLayer.ContentType.Episode;
			else if (item.IsSeriesParent)
				return DataLayer.ContentType.Parent;
			else 
				return DataLayer.ContentType.Product;			
		}
	}
}
