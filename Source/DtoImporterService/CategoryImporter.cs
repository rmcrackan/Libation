using System;
using System.Collections.Generic;
using System.Linq;
using AudibleApi.Common;
using AudibleUtilities;
using DataLayer;
using Dinah.Core.Collections.Generic;

namespace DtoImporterService
{
	public class CategoryImporter : ImporterBase<CategoryValidator>
	{
		public Dictionary<string, Category> Cache { get; private set; } = new();

		public CategoryImporter(LibationContext context) : base(context) { }

		protected override int DoImport(IEnumerable<ImportItem> importItems)
		{
			// get distinct
			var categoryIds = importItems
				.Select(i => i.DtoItem)
				.GetCategoriesDistinct()
				.Select(c => c.CategoryId)
				.Distinct()
				.ToList();

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

			// load existing => local
			Cache = DbContext.Categories
				.Where(c => categoryIds.Contains(c.AudibleCategoryId))
				.ToDictionarySafe(c => c.AudibleCategoryId);
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
						Cache.TryGetValue(pair[0].CategoryId, out parentCategory);

					if (!Cache.TryGetValue(id, out var category))
					{
						category = addCategory(id, name);
						qtyNew++;
					}

					category.UpdateParentCategory(parentCategory);
				}
			}

			return qtyNew;
		}

		private Category addCategory(string id, string name)
		{
			try
			{
				var category = new Category(new AudibleCategoryId(id), name);

				var entityEntry = DbContext.Categories.Add(category);
				var entity = entityEntry.Entity;

				Cache.Add(entity.AudibleCategoryId, entity);
				return entity;
			}
			catch (Exception ex)
			{
				Serilog.Log.Logger.Error(ex, "Error adding category. {@DebugInfo}", new { id, name });
				throw;
			}
		}
	}
}
