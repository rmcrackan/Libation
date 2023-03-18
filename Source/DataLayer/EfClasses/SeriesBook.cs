using Dinah.Core;

namespace DataLayer
{
    public class SeriesBook
    {
        internal int SeriesId { get; private set; }
        internal int BookId { get; private set; }

        public string Order { get; private set; }
        public float Index { get; }

        public Series Series { get; private set; }
        public Book Book { get; private set; }

        private SeriesBook() { }
        internal SeriesBook(Series series, Book book, string order)
        {
            ArgumentValidator.EnsureNotNull(series, nameof(series));
            ArgumentValidator.EnsureNotNull(book, nameof(book));

            Series = series;
            Book = book;
            Order = order;
            Index = StringLib.ExtractFirstNumber(Order);
		}

        public void UpdateOrder(string order)
        {
            if (!string.IsNullOrWhiteSpace(order))
                Order = order;
        }

		public override string ToString() => $"Series={Series} Book={Book}";
	}
}
