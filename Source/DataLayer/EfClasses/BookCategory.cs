using Dinah.Core;

namespace DataLayer
{
	public class BookCategory
	{
		internal int BookId { get; private set; }
		internal int CategoryLadderId { get; private set; }

		public Book Book { get; private set; }
		public CategoryLadder CategoryLadder { get; private set; }
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
		private BookCategory() { }
#pragma warning restore CS8618

		internal BookCategory(Book book, CategoryLadder categoriesList)
		{
			Book = ArgumentValidator.EnsureNotNull(book, nameof(book));
			CategoryLadder = ArgumentValidator.EnsureNotNull(categoriesList, nameof(categoriesList));
		}
	}
}
