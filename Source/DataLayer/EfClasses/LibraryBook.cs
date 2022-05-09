using System;
using Dinah.Core;

namespace DataLayer
{
    public class LibraryBook
    {
        internal int BookId { get; private set; }
        public Book Book { get; private set; }

        public DateTime DateAdded { get; private set; }
        public string Account { get; private set; }

        private LibraryBook() { }
        public LibraryBook(Book book, DateTime dateAdded, string account)
        {
            ArgumentValidator.EnsureNotNull(book, nameof(book));
            ArgumentValidator.EnsureNotNull(account, nameof(account));

            Book = book;
            DateAdded = dateAdded;
            Account = account;
        }

        public override string ToString() => $"{DateAdded:d} {Book}";
	}
}
