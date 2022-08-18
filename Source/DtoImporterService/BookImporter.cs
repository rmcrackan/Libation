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
				.ToList();

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

			// categories are laid out for a breadcrumb. category is 1st, subcategory is 2nd
			// absence of categories is also possible

			// CATEGORY HACK: only use the 1st 2 categories
			// after we support full arbitrary-depth category trees and multiple categories per book, the real impl will be something like this
			//   var lastCategory = item.Categories.LastOrDefault()?.CategoryId ?? "";
			var lastCategory
				= item.Categories.Length == 0 ? ""
				: item.Categories.Length == 1 ? item.Categories[0].CategoryId
				// 2+
				: item.Categories[1].CategoryId;

			var category = categoryImporter.Cache[lastCategory];

			Book book;
			try
			{
				book = DbContext.Books.Add(new Book(
					new AudibleProductId(item.ProductId),
					item.TitleWithSubtitle,
					item.Description,
					item.LengthInMinutes,
					contentType,
					authors,
					narrators,
					category,
					importItem.LocaleName)
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
					Category = category?.Name,
					importItem.LocaleName
				});
				throw;
			}

			var publisherName = item.Publisher;
			if (!string.IsNullOrWhiteSpace(publisherName))
			{
				var publisher = contributorImporter.Cache[publisherName];
				book.ReplacePublisher(publisher);
			}

			book.UpdateBookDetails(item.IsAbridged, item.DatePublished);

			if (item.PdfUrl is not null)
				book.AddSupplementDownloadUrl(item.PdfUrl.ToString());

			return book;
		}

		private void updateBook(ImportItem importItem, Book book)
		{
			var item = importItem.DtoItem;

			var codec = item.AvailableCodecs?.Max(f => AudioFormat.FromString(f.EnhancedCodec)) ?? new AudioFormat();
			book.AudioFormat = codec;

			// set/update book-specific info which may have changed
			if (item.PictureId is not null)
				book.PictureId = item.PictureId;
			
			if (item.PictureLarge is not null)
				book.PictureLarge = item.PictureLarge;

			book.UpdateProductRating(item.Product_OverallStars, item.Product_PerformanceStars, item.Product_StoryStars);

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
