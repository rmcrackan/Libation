using System;
using System.Collections.Generic;
using System.Linq;
using AudibleApiDTOs;
using DataLayer;
using InternalUtilities;

namespace DtoImporterService
{
	public class BookImporter : ItemsImporterBase
	{
		public override IEnumerable<Exception> Validate(IEnumerable<Item> items) => new BookValidator().Validate(items);

		protected override int DoImport(IEnumerable<Item> items, LibationContext context)
		{
			// pre-req.s
			new ContributorImporter().Import(items, context);
			new SeriesImporter().Import(items, context);
			new CategoryImporter().Import(items, context);

			// get distinct
			var productIds = items.Select(i => i.ProductId).ToList();

			// load db existing => .Local
			loadLocal_books(productIds, context);

			// upsert
			var qtyNew = upsertBooks(items, context);
			return qtyNew;
		}

		private void loadLocal_books(List<string> productIds, LibationContext context)
		{
			var localProductIds = context.Books.Local.Select(b => b.AudibleProductId);
			var remainingProductIds = productIds
				.Distinct()
				.Except(localProductIds)
				.ToList();

			// GetBooks() eager loads Series, category, et al
			if (remainingProductIds.Any())
				context.Books.GetBooks(b => remainingProductIds.Contains(b.AudibleProductId)).ToList();
		}

		private int upsertBooks(IEnumerable<Item> items, LibationContext context)
		{
			var qtyNew = 0;

			foreach (var item in items)
			{
				var book = context.Books.Local.SingleOrDefault(p => p.AudibleProductId == item.ProductId);
				if (book is null)
				{
					// nested logic is required so order of names is retained. else, contributors may appear in the order they were inserted into the db
					var authors = item
						.Authors
						.Select(a => context.Contributors.Local.Single(c => a.Name == c.Name))
						.ToList();

					book = context.Books.Add(new Book(
						new AudibleProductId(item.ProductId), item.Title, item.Description, item.LengthInMinutes, authors))
						.Entity;

					qtyNew++;
				}

				// if no narrators listed, author is the narrator
				if (item.Narrators is null || !item.Narrators.Any())
					item.Narrators = item.Authors;
				// nested logic is required so order of names is retained. else, contributors may appear in the order they were inserted into the db
				var narrators = item
					.Narrators
					.Select(n => context.Contributors.Local.Single(c => n.Name == c.Name))
					.ToList();
				// not all books have narrators. these will already be using author as narrator. don't undo this
				if (narrators.Any())
					book.ReplaceNarrators(narrators);

				// set/update book-specific info which may have changed
				book.PictureId = item.PictureId;
				book.UpdateProductRating(item.Product_OverallStars, item.Product_PerformanceStars, item.Product_StoryStars);
				if (!string.IsNullOrWhiteSpace(item.SupplementUrl))
					book.AddSupplementDownloadUrl(item.SupplementUrl);

				var publisherName = item.Publisher;
				if (!string.IsNullOrWhiteSpace(publisherName))
				{
					var publisher = context.Contributors.Local.Single(c => publisherName == c.Name);
					book.ReplacePublisher(publisher);
				}

				// important to update user-specific info. this will have changed if user has rated/reviewed the book since last library import
				book.UserDefinedItem.UpdateRating(item.MyUserRating_Overall, item.MyUserRating_Performance, item.MyUserRating_Story);

				//
				// this was round 1 when it was a 2 step process
				//
				//// update series even for existing books. these are occasionally updated
				//var seriesIds = item.Series.Select(kvp => kvp.SeriesId).ToList();
				//var allSeries = context.Series.Local.Where(c => seriesIds.Contains(c.AudibleSeriesId)).ToList();
				//foreach (var series in allSeries)
				//	book.UpsertSeries(series);

				// these will upsert over library-scraped series, but will not leave orphans
				if (item.Series != null)
				{
					foreach (var seriesEntry in item.Series)
					{
						var series = context.Series.Local.Single(s => seriesEntry.SeriesId == s.AudibleSeriesId);
						book.UpsertSeries(series, seriesEntry.Index);
					}
				}

				// categories are laid out for a breadcrumb. category is 1st, subcategory is 2nd
				var category = context.Categories.Local.SingleOrDefault(c => c.AudibleCategoryId == item.Categories.LastOrDefault().CategoryId);
				if (category != null)
					book.UpdateCategory(category, context);

				book.UpdateBookDetails(item.IsAbridged, item.DatePublished);
			}

			return qtyNew;
		}
	}
}
