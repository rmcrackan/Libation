using System;
using System.Collections.Generic;
using System.Linq;
using AudibleApi.Common;
using AudibleUtilities;
using DataLayer;
using Dinah.Core.Collections.Generic;

namespace DtoImporterService
{
	public class CategoryImporter : ItemsImporterBase
	{
		protected override IValidator Validator => new CategoryValidator();

		private Dictionary<string, Category> CategoryCache { get; set; } = new();
		public HashSet<DataLayer.CategoryLadder> LadderCache { get; private set; } = new();

		public CategoryImporter(LibationContext context) : base(context) { }

		protected override int DoImport(IEnumerable<ImportItem> importItems)
		{
			// load db existing => .Local
			loadLocal_categories();

			// upsert
			//Import item may not have no (null) categories
			var categoryLadders = importItems
				.Where(i => i.DtoItem.CategoryLadders is not null)
				.SelectMany(i => i.DtoItem.CategoryLadders)
				.Select(cl => cl?.Ladder)
				.Where(l => l?.Length > 0)
				.ToList();

			var qtyNew = upsertCategories(categoryLadders);
			return qtyNew;
		}

		private void loadLocal_categories()
		{
			// load existing => local
			LadderCache = DbContext.GetCategoryLadders().ToHashSet();
			CategoryCache = LadderCache.SelectMany(cl => cl.Categories).ToDictionarySafe(c => c.AudibleCategoryId);
		}

		// only use after loading contributors => local
		private int upsertCategories(List<Ladder[]> ladders)
		{
			var qtyNew = 0;

			foreach (var ladder in ladders)
			{
				var categories = new List<Category>(ladder.Length);

				for (var i = 0; i < ladder.Length; i++)
				{
					var id = ladder[i].CategoryId;
					var name = ladder[i].CategoryName;

					if (!CategoryCache.TryGetValue(id, out var category))
					{
						category = addCategory(id, name);
					}

					categories.Add(category);
				}

				var categoryLadder = new DataLayer.CategoryLadder(categories);
				if (!LadderCache.Contains(categoryLadder))
				{
					addCategoryLadder(categoryLadder);
					qtyNew++;
				}
			}

			return qtyNew;
		}

		private DataLayer.CategoryLadder addCategoryLadder(DataLayer.CategoryLadder categoryList)
		{
			try
			{
				var entityEntry = DbContext.CategoryLadders.Add(categoryList);
				var entity = entityEntry.Entity;

				LadderCache.Add(entity);
				return entity;
			}
			catch (Exception ex)
			{
				Serilog.Log.Logger.Error(ex, "Error adding category ladder. {@DebugInfo}", categoryList);
				throw;
			}
		}

		private Category addCategory(string id, string name)
		{
			try
			{
				var category = new Category(new AudibleCategoryId(id), name);

				var entityEntry = DbContext.Categories.Add(category);
				var entity = entityEntry.Entity;

				CategoryCache.Add(entity.AudibleCategoryId, entity);
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
