using System;
using System.Collections.Generic;
using System.Linq;
using AudibleApi.Common;
using AudibleUtilities;
using DataLayer;

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
			// must include default/empty/missing
			categoryIds.Add(Category.GetEmpty().AudibleCategoryId);

			var localIds = DbContext.Categories.Local.Select(c => c.AudibleCategoryId).ToList();
			var remainingCategoryIds = categoryIds
				.Distinct()
				.Except(localIds)
				.ToList();

			// load existing => local
			if (remainingCategoryIds.Any())
				DbContext.Categories.Where(c => remainingCategoryIds.Contains(c.AudibleCategoryId)).ToList();
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
						// should be "Single()" but user is getting a strange error
						parentCategory = DbContext.Categories.Local.FirstOrDefault(c => c.AudibleCategoryId == pair[0].CategoryId);

					// should be "SingleOrDefault()" but user is getting a strange error
					var category = DbContext.Categories.Local.FirstOrDefault(c => c.AudibleCategoryId == id);
					if (category is null)
					{
						try
						{
							category = DbContext.Categories.Add(new Category(new AudibleCategoryId(id), name)).Entity;
						}
						catch (Exception ex)
						{
							Serilog.Log.Logger.Error(ex, "Error adding category. {@DebugInfo}", new { id, name });
							throw;
						}
						qtyNew++;
					}

					category.UpdateParentCategory(parentCategory);
				}
			}

			return qtyNew;
		}
	}
}
