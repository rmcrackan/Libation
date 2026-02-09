using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace DataLayer;

public static class CategoryQueries
{
	public static IQueryable<CategoryLadder> GetCategoryLadders(this LibationContext context)
		=> context.CategoryLadders.Include(c => c._categories);
}
