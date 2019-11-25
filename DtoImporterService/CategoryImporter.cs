using System;
using System.Collections.Generic;
using System.Linq;
using AudibleApiDTOs;
using DataLayer;
using InternalUtilities;

namespace DtoImporterService
{
	public class CategoryImporter : ItemsImporterBase
	{
		public override IEnumerable<Exception> Validate(IEnumerable<Item> items) => new CategoryValidator().Validate(items);

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

			// load existing => local
			// remember to include default/empty/missing
			var emptyName = Contributor.GetEmpty().Name;
			if (remainingCategoryIds.Any())
				context.Categories.Where(c => remainingCategoryIds.Contains(c.AudibleCategoryId) || c.Name == emptyName).ToList();
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
