using System;
using System.Collections.Generic;
using System.Linq;
using Dinah.Core.Collections.Generic;

namespace DTOs
{
	public static class LibraryApiV10Extensions
	{
		public static IEnumerable<Person> GetAuthorsDistinct(this IEnumerable<Item> items)
			=> items.SelectMany(i => i.Authors).DistinctBy(a => new { a.Name, a.Asin });

		public static IEnumerable<string> GetNarratorsDistinct(this IEnumerable<Item> items)
			=> items.SelectMany(i => i.Narrators, (i, n) => n.Name).Distinct();

		public static IEnumerable<string> GetPublishersDistinct(this IEnumerable<Item> items)
			=> items.Select(i => i.Publisher).Distinct();

		public static IEnumerable<Series> GetSeriesDistinct(this IEnumerable<Item> items)
			=> items.SelectMany(i => i.Series).DistinctBy(s => new { s.SeriesName, s.SeriesId });

		public static IEnumerable<Ladder> GetParentCategoriesDistinct(this IEnumerable<Item> items)
			=> items.Select(l => l.ParentCategory).DistinctBy(l => new { l.CategoryName, l.CategoryId });

		public static IEnumerable<Ladder> GetChildCategoriesDistinct(this IEnumerable<Item> items)
			=> items
				.Select(l => l.ChildCategory)
				.Where(l => l != null)
				.DistinctBy(l => new { l.CategoryName, l.CategoryId });
	}
}
