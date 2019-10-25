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

		public static IEnumerable<Person> GetNarratorsDistinct(this IEnumerable<Item> items)
			=> items.SelectMany(i => i.Narrators).DistinctBy(a => new { a.Name, a.Asin });
	}
}
