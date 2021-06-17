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
		public CategoryImporter(LibationContext context) : base(context) { }

		public override IEnumerable<Exception> Validate(IEnumerable<ImportItem> importItems) => new CategoryValidator().Validate(importItems.Select(i => i.DtoItem));

		protected override int DoImport(IEnumerable<ImportItem> importItems)
		{
			// get distinct
			var categoryIds = importItems
				.Select(i => i.DtoItem)
				.GetCategoriesDistinct()
				.Select(c => c.CategoryId).ToList();

			// load db existing => .Local
			loadLocal_categories(categoryIds);

			// upsert
			var categoryPairs = importItems
				.Select(i => i.DtoItem)
				.GetCategoryPairsDistinct()
				.ToList();
			var qtyNew = upsertCategories(categoryPairs);
			return qtyNew;
		}

		private void loadLocal_categories(List<string> categoryIds)
		{
			var localIds = DbContext.Categories.Local.Select(c => c.AudibleCategoryId);
			var remainingCategoryIds = categoryIds
				.Distinct()
				.Except(localIds)
				.ToList();

			// load existing => local
			// remember to include default/empty/missing
			var emptyName = Contributor.GetEmpty().Name;
			if (remainingCategoryIds.Any())
				DbContext.Categories.Where(c => remainingCategoryIds.Contains(c.AudibleCategoryId) || c.Name == emptyName).ToList();
		}

		// only use after loading contributors => local
		private int upsertCategories(List<Ladder[]> categoryPairs)
		{
			var qtyNew = 0;

			foreach (var pair in categoryPairs)
			{
				for (var i = 0; i < pair.Length; i++)
				{
					// CATEGORY HACK: not yet supported: depth beyond 0 and 1
					if (i > 1)
						break;

					var id = pair[i].CategoryId;
					var name = pair[i].CategoryName;

					Category parentCategory = null;
					if (i == 1)
						parentCategory = DbContext.Categories.Local.Single(c => c.AudibleCategoryId == pair[0].CategoryId);

					var category = DbContext.Categories.Local.SingleOrDefault(c => c.AudibleCategoryId == id);
					if (category is null)
					{
						category = DbContext.Categories.Add(new Category(new AudibleCategoryId(id), name)).Entity;
						qtyNew++;
					}

					category.UpdateParentCategory(parentCategory);
				}
			}

			return qtyNew;
		}
	}
}
