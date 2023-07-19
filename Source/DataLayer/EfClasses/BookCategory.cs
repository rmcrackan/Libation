using Dinah.Core;

namespace DataLayer
{
	public class BookCategory
	{
		internal int BookId { get; private set; }
		internal int CategoryLadderId { get; private set; }

		public Book Book { get; private set; }
		public CategoryLadder CategoryLadder { get; private set; }
		private BookCategory() { }

		internal BookCategory(Book book, CategoryLadder categoriesList)
		{
			Book = ArgumentValidator.EnsureNotNull(book, nameof(book));
			CategoryLadder = ArgumentValidator.EnsureNotNull(categoriesList, nameof(categoriesList));
		}
	}
}
