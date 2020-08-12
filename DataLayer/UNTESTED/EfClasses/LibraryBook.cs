using System;
using Dinah.Core;

namespace DataLayer
{
    public class LibraryBook
    {
        internal int BookId { get; private set; }
        public Book Book { get; private set; }

        public DateTime DateAdded { get; private set; }

        // immutable-ish. should be immutable. mutability is necessary for v3 => v4 upgrades
        public string Account { get; private set; }

        private LibraryBook() { }
        public LibraryBook(Book book, DateTime dateAdded)
        {
            ArgumentValidator.EnsureNotNull(book, nameof(book));
            Book = book;
            DateAdded = dateAdded;
        }

		public override string ToString() => $"{DateAdded:d} {Book}";
	}
}
