using System;
using System.Collections.Generic;

namespace Scraping.BookDetail
{
    public class SeriesEntry
    {
        public string SeriesId;
        public string SeriesName;
        public float? Index;
    }
    public class BookDetailDTO
    {
        public string ProductId { get; set; }

        /// <summary>DEBUG only</summary>
        public string Title { get; set; }

        /// <summary>UNUSED: currently unused: desc from book-details is better desc in lib, but book-details also contains html tags</summary>
        public string Description { get; set; }

        public bool IsAbridged { get; set; }

        // order matters: don't use hashtable/dictionary
        public List<string> Narrators { get; } = new List<string>();

        public string Publisher { get; set; }

        public DateTime DatePublished { get; set; }

        // order matters: don't use hashtable/dictionary
        public List<(string categoryId, string categoryName)> Categories { get; } = new List<(string categoryId, string categoryName)>();

        public List<SeriesEntry> Series { get; } = new List<SeriesEntry>();
    }
}
