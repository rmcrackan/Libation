using System;
using System.Collections.Generic;
using System.Linq;
using Dinah.Core;
using Microsoft.EntityFrameworkCore;

namespace DataLayer
{
    public class AudibleSeriesId
    {
        public string Id { get; }
        public AudibleSeriesId(string id)
        {
            ArgumentValidator.EnsureNotNullOrWhiteSpace(id, nameof(id));
            Id = id;
        }
    }
    public class Series
    {
        internal int SeriesId { get; private set; }
        public string AudibleSeriesId { get; private set; }

        /// <summary>optional</summary>
        public string Name { get; private set; }

        private HashSet<SeriesBook> _booksLink;
        public IEnumerable<SeriesBook> BooksLink
            => _booksLink?
                .OrderBy(sb => sb.Index)
                .ToList();

        private Series() { }
        /// <summary>special id class b/c it's too easy to get string order mixed up</summary>
        public Series(AudibleSeriesId audibleSeriesId, string name = null)
        {
            ArgumentValidator.EnsureNotNull(audibleSeriesId, nameof(audibleSeriesId));
            var id = audibleSeriesId.Id;
            ArgumentValidator.EnsureNotNullOrWhiteSpace(id, nameof(id));
            AudibleSeriesId = id;
            _booksLink = new HashSet<SeriesBook>();
            UpdateName(name);
        }

        public void UpdateName(string name)
        {
            // don't overwrite with null or whitespace but not an error
            if (!string.IsNullOrWhiteSpace(name))
                Name = name;
        }

        public void AddBook(Book book, float? index = null, DbContext context = null)
        {
            ArgumentValidator.EnsureNotNull(book, nameof(book));

            // our add() is conditional upon what's already included in the collection.
            // therefore if not loaded, a trip is required. might as well just load it
            if (_booksLink == null)
            {
                ArgumentValidator.EnsureNotNull(context, nameof(context));
                if (!context.Entry(this).IsKeySet)
                    throw new InvalidOperationException("Could not add series");

                context.Entry(this).Collection(s => s.BooksLink).Load();
            }

            if (_booksLink.SingleOrDefault(sb => sb.Book == book) == null)
                _booksLink.Add(new SeriesBook(this, book, index));
        }

		public override string ToString() => Name;
	}
}
