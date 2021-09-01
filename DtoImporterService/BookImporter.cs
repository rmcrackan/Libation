using System;
using System.Collections.Generic;
using System.Linq;
using AudibleApi.Common;
using DataLayer;
using InternalUtilities;

namespace DtoImporterService
{
	public class BookImporter : ItemsImporterBase
	{
		public BookImporter(LibationContext context) : base(context) { }

		public override IEnumerable<Exception> Validate(IEnumerable<ImportItem> importItems) => new BookValidator().Validate(importItems.Select(i => i.DtoItem));

		protected override int DoImport(IEnumerable<ImportItem> importItems)
		{
			// pre-req.s
			new ContributorImporter(DbContext).Import(importItems);
			new SeriesImporter(DbContext).Import(importItems);
			new CategoryImporter(DbContext).Import(importItems);

			// get distinct
			var productIds = importItems.Select(i => i.DtoItem.ProductId).ToList();

			// load db existing => .Local
			loadLocal_books(productIds);

			// upsert
			var qtyNew = upsertBooks(importItems);
			return qtyNew;
		}

		private void loadLocal_books(List<string> productIds)
		{
			var localProductIds = DbContext.Books.Local.Select(b => b.AudibleProductId);
			var remainingProductIds = productIds
				.Distinct()
				.Except(localProductIds)
				.ToList();

			// GetBooks() eager loads Series, category, et al
			if (remainingProductIds.Any())
				DbContext.Books.GetBooks(b => remainingProductIds.Contains(b.AudibleProductId)).ToList();
		}

		private int upsertBooks(IEnumerable<ImportItem> importItems)
		{
			var qtyNew = 0;

			foreach (var item in importItems)
			{
				var book = DbContext.Books.Local.SingleOrDefault(p => p.AudibleProductId == item.DtoItem.ProductId);
				if (book is null)
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

			var contentType = item.IsEpisodes ? DataLayer.ContentType.Episode : DataLayer.ContentType.Product;

			// absence of authors is very rare, but possible
			if (!item.Authors?.Any() ?? true)
				item.Authors = new[] { new Person { Name = "", Asin = null } };

			// nested logic is required so order of names is retained. else, contributors may appear in the order they were inserted into the db
			var authors = item
				.Authors
				.Select(a => DbContext.Contributors.Local.Single(c => a.Name == c.Name))
				.ToList();

			var narrators
				= item.Narrators is null || !item.Narrators.Any()
				// if no narrators listed, author is the narrator
				? authors
				// nested logic is required so order of names is retained. else, contributors may appear in the order they were inserted into the db
				: item
					.Narrators
					.Select(n => DbContext.Contributors.Local.Single(c => n.Name == c.Name))
					.ToList();

			// categories are laid out for a breadcrumb. category is 1st, subcategory is 2nd
			// absence of categories is also possible

			// CATEGORY HACK: only use the 1st 2 categories
			// (real impl: var lastCategory = item.Categories.LastOrDefault()?.CategoryId ?? "";)
			var lastCategory
				= item.Categories.Length == 0 ? ""
				: item.Categories.Length == 1 ? item.Categories[0].CategoryId
				// 2+
				: item.Categories[1].CategoryId;

			var category = DbContext.Categories.Local.SingleOrDefault(c => c.AudibleCategoryId == lastCategory);

			var book = DbContext.Books.Add(new Book(
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

			var publisherName = item.Publisher;
			if (!string.IsNullOrWhiteSpace(publisherName))
			{
				var publisher = DbContext.Contributors.Local.Single(c => publisherName == c.Name);
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

			// set/update book-specific info which may have changed
			book.PictureId = item.PictureId;
			book.UpdateProductRating(item.Product_OverallStars, item.Product_PerformanceStars, item.Product_StoryStars);

			// needed during v3 => v4 migration
			book.UpdateLocale(importItem.LocaleName);

			// important to update user-specific info. this will have changed if user has rated/reviewed the book since last library import
			book.UserDefinedItem.UpdateRating(item.MyUserRating_Overall, item.MyUserRating_Performance, item.MyUserRating_Story);

			// update series even for existing books. these are occasionally updated
			// these will upsert over library-scraped series, but will not leave orphans
			if (item.Series != null)
			{
				foreach (var seriesEntry in item.Series)
				{
					var series = DbContext.Series.Local.Single(s => seriesEntry.SeriesId == s.AudibleSeriesId);

					var index = 0f;
					try
					{
						index = seriesEntry.Index;
					}
					catch (Exception ex)
					{
						Serilog.Log.Logger.Error(ex, $"Error parsing series index. Title: {item.Title}. ASIN: {item.Asin}. Series index: {seriesEntry.Sequence}");
					}

					book.UpsertSeries(series, index);
				}
			}
		}
	}
}
