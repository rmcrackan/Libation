using Dinah.Core;

namespace DataLayer
{
    /// <summary>PDF/ZIP files only. Although book download info could be the same format, they're substantially different and subject to change</summary>
    public class Supplement
    {
        internal int SupplementId { get; private set; }
        internal int BookId { get; private set; }

        public Book Book { get; private set; }
        public string Url { get; private set; }

        private Supplement() { }
        public Supplement(Book book, string url)
        {
            ArgumentValidator.EnsureNotNull(book, nameof(book));
            ArgumentValidator.EnsureNotNullOrWhiteSpace(url, nameof(url));

            Book = book;
            Url = url;
        }
    }
}
