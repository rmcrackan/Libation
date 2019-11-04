using System;
using System.Collections.Generic;
using System.Linq;
using AudibleApiDTOs;
using DataLayer;

namespace DtoImporterService
{
	public class CategoryImporter : ItemsImporterBase
	{
		public override IEnumerable<Exception> Validate(IEnumerable<Item> items)
		{
			var exceptions = new List<Exception>();

			var distinct = items.GetCategoriesDistinct();
			if (distinct.Any(s => s.CategoryId is null))
				exceptions.Add(new ArgumentException($"Collection contains {nameof(Item.Categories)} with null {nameof(Ladder.CategoryId)}", nameof(items)));
			if (distinct.Any(s => s.CategoryName is null))
				exceptions.Add(new ArgumentException($"Collection contains {nameof(Item.Categories)} with null {nameof(Ladder.CategoryName)}", nameof(items)));

			if (items.GetCategoryPairsDistinct().Any(p => p.Length > 2))
				exceptions.Add(new ArgumentException($"Collection contains {nameof(Item.Categories)} with wrong number of categories. Expecting 0, 1, or 2 categories per title", nameof(items)));

			return exceptions;
		}

		protected override int DoImport(IEnumerable<Item> items, LibationContext context)
		{
			// get distinct
			var categoryIds = items.GetCategoriesDistinct().Select(c => c.CategoryId).ToList();

			// load db existing => .Local
			loadLocal_categories(categoryIds, context);

			// upsert
			var categoryPairs = items.GetCategoryPairsDistinct().ToList();
			var qtyNew = upsertCategories(categoryPairs, context);
			return qtyNew;
		}

		private void loadLocal_categories(List<string> categoryIds, LibationContext context)
		{
			var localIds = context.Categories.Local.Select(c => c.AudibleCategoryId);
			var remainingCategoryIds = categoryIds
				.Distinct()
				.Except(localIds)
				.ToList();

			if (remainingCategoryIds.Any())
				context.Categories.Where(c => remainingCategoryIds.Contains(c.AudibleCategoryId)).ToList();
		}

		// only use after loading contributors => local
		private int upsertCategories(List<Ladder[]> categoryPairs, LibationContext context)
		{
			var qtyNew = 0;

			foreach (var pair in categoryPairs)
			{
				for (var i = 0; i < pair.Length; i++)
				{
					var id = pair[i].CategoryId;
					var name = pair[i].CategoryName;

					Category parentCategory = null;
					if (i == 1)
						parentCategory = context.Categories.Local.Single(c => c.AudibleCategoryId == pair[0].CategoryId);

					var category = context.Categories.Local.SingleOrDefault(c => c.AudibleCategoryId == id);
					if (category is null)
					{
						category = context.Categories.Add(new Category(new AudibleCategoryId(id), name)).Entity;
						qtyNew++;
					}

					category.UpdateParentCategory(parentCategory);
				}
			}

			return qtyNew;
		}
	}
}
