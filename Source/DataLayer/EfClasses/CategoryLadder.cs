using Dinah.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

#nullable enable
namespace DataLayer
{
	public class CategoryLadder : IEquatable<CategoryLadder>
	{
		internal int CategoryLadderId { get; private set; }

		internal List<Category> _categories;
		private ReadOnlyCollection<Category>? _categoriesReadOnly;
		public ReadOnlyCollection<Category> Categories
		{
			get
			{
				if (_categoriesReadOnly?.SequenceEqual(_categories) is not true)
					_categoriesReadOnly = _categories.AsReadOnly();
				return _categoriesReadOnly;
			}
		}

		private HashSet<BookCategory>? _booksLink;
		public IEnumerable<BookCategory>? BooksLink => _booksLink?.ToList();
		private CategoryLadder() { _categories = new(); }
		public CategoryLadder(List<Category> categories)
		{
			ArgumentValidator.EnsureNotNull(categories, nameof(categories));
			ArgumentValidator.EnsureGreaterThan(categories.Count, nameof(categories), 0);
			_booksLink = new HashSet<BookCategory>();
			_categories = categories;
		}

		public override int GetHashCode()
		{
			HashCode hashCode = default;
			foreach (var category in _categories)
				hashCode.Add(category.AudibleCategoryId);
			return hashCode.ToHashCode();
		}

		public bool Equals(CategoryLadder? other)
			=> other?._categories is not null
			&& Equals(other._categories.Select(c => c.AudibleCategoryId));

		public bool Equals(IEnumerable<string?>? categoryIds)
			=> categoryIds is not null
			&& _categories.Select(c => c.AudibleCategoryId).SequenceEqual(categoryIds);

		public override bool Equals(object? obj)
			=> obj is CategoryLadder other && Equals(other);

		public override string ToString() => string.Join(" > ", _categories.Select(c => c.Name));
	}
}
