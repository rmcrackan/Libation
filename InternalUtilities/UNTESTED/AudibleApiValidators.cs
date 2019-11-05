using System;
using System.Collections.Generic;
using System.Linq;
using AudibleApiDTOs;

namespace InternalUtilities
{
	public interface IValidator
	{
		IEnumerable<Exception> Validate(IEnumerable<Item> items);
	}
	public class LibraryValidator : IValidator
	{
		public IEnumerable<Exception> Validate(IEnumerable<Item> items)
		{
			var exceptions = new List<Exception>();

			if (items.Any(i => string.IsNullOrWhiteSpace(i.ProductId)))
				exceptions.Add(new ArgumentException($"Collection contains item(s) with null or blank {nameof(Item.ProductId)}", nameof(items)));
			if (items.Any(i => i.DateAdded < new DateTime(1980, 1, 1)))
				exceptions.Add(new ArgumentException($"Collection contains item(s) with invalid {nameof(Item.DateAdded)}", nameof(items)));

			return exceptions;
		}
	}
	public class BookValidator : IValidator
	{
		public IEnumerable<Exception> Validate(IEnumerable<Item> items)
		{
			var exceptions = new List<Exception>();

			if (items.Any(i => string.IsNullOrWhiteSpace(i.ProductId)))
				exceptions.Add(new ArgumentException($"Collection contains item(s) with blank {nameof(Item.ProductId)}", nameof(items)));
			if (items.Any(i => string.IsNullOrWhiteSpace(i.Title)))
				exceptions.Add(new ArgumentException($"Collection contains item(s) with blank {nameof(Item.Title)}", nameof(items)));
			if (items.Any(i => i.Authors is null))
				exceptions.Add(new ArgumentException($"Collection contains item(s) with null {nameof(Item.Authors)}", nameof(items)));

			return exceptions;
		}
	}
	public class CategoryValidator : IValidator
	{
		public IEnumerable<Exception> Validate(IEnumerable<Item> items)
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
	}
	public class ContributorValidator : IValidator
	{
		public IEnumerable<Exception> Validate(IEnumerable<Item> items)
		{
			var exceptions = new List<Exception>();

			if (items.GetAuthorsDistinct().Any(a => string.IsNullOrWhiteSpace(a.Name)))
				exceptions.Add(new ArgumentException($"Collection contains {nameof(Item.Authors)} with null {nameof(Person.Name)}", nameof(items)));
			if (items.GetNarratorsDistinct().Any(a => string.IsNullOrWhiteSpace(a.Name)))
				exceptions.Add(new ArgumentException($"Collection contains {nameof(Item.Narrators)} with null {nameof(Person.Name)}", nameof(items)));

			return exceptions;
		}
	}
	public class SeriesValidator : IValidator
	{
		public IEnumerable<Exception> Validate(IEnumerable<Item> items)
		{
			var exceptions = new List<Exception>();

			var distinct = items.GetSeriesDistinct();
			if (distinct.Any(s => s.SeriesId is null))
				exceptions.Add(new ArgumentException($"Collection contains {nameof(Item.Series)} with null {nameof(AudibleApiDTOs.Series.SeriesId)}", nameof(items)));
			if (distinct.Any(s => s.SeriesName is null))
				exceptions.Add(new ArgumentException($"Collection contains {nameof(Item.Series)} with null {nameof(AudibleApiDTOs.Series.SeriesName)}", nameof(items)));

			return exceptions;
		}
	}
}
