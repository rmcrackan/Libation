using Dinah.Core;

namespace DataLayer
{
    public class BookContributor
    {
        internal int BookId { get; private set; }
        internal int ContributorId { get; private set; }
        public Role Role { get; private set; }
        public byte Order { get; private set; }

        public Book Book { get; private set; }
        public Contributor Contributor { get; private set; }

        private BookContributor() { }
        internal BookContributor(Book book, Contributor contributor, Role role, byte order)
        {
            ArgumentValidator.EnsureNotNull(book, nameof(book));
            ArgumentValidator.EnsureNotNull(contributor, nameof(contributor));

            Book = book;
            Contributor = contributor;
            Role = role;
            Order = order;
        }
    }
}
