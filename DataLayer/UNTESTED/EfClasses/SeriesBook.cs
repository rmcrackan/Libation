using Dinah.Core;

namespace DataLayer
{
    public class SeriesBook
    {
        internal int SeriesId { get; private set; }
        internal int BookId { get; private set; }

        /// <summary>
        /// <para>"index" not "order". This is both for sequence and display</para>
        /// <para>Float allows for in-between books. eg: 2.5</para>
        /// <para>To show 2 editions as the same book in a series, give them the same index</para>
        /// <para>null IS NOT the same as 0. Some series call a book "book 0"</para>
        /// </summary>
        public float? Index { get; private set; }

        public Series Series { get; private set; }
        public Book Book { get; private set; }

        private SeriesBook() { }
        internal SeriesBook(Series series, Book book, float? index = null)
        {
            ArgumentValidator.EnsureNotNull(series, nameof(series));
            ArgumentValidator.EnsureNotNull(book, nameof(book));

            Series = series;
            Book = book;
            Index = index;
        }

        public void UpdateIndex(float? index)
        {
            if (index.HasValue)
                Index = index.Value;
        }
    }
}
