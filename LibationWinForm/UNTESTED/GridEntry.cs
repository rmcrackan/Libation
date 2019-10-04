using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using DataLayer;

namespace LibationWinForm
{
    internal class GridEntry
    {
        private LibraryBook libraryBook;
        private Book book => libraryBook.Book;

        public Book GetBook() => book;

        // this special case is obvious and ugly
        public void REPLACE_Library_Book(LibraryBook libraryBook) => this.libraryBook = libraryBook;

        public GridEntry(LibraryBook libraryBook) => this.libraryBook = libraryBook;

        // hide from public fields from Data Source GUI with [Browsable(false)]

        [Browsable(false)]
        public string Tags => book.UserDefinedItem.Tags;
        [Browsable(false)]
        public IEnumerable<string> TagsEnumerated => book.UserDefinedItem.TagsEnumerated;

        private Dictionary<string, string> formatReplacements { get; } = new Dictionary<string, string>();
        public bool TryGetFormatted(string key, out string value) => formatReplacements.TryGetValue(key, out value);

        public Image Cover =>
            Dinah.Core.Drawing.ImageConverter.GetPictureFromBytes(
                FileManager.PictureStorage.GetImage(book.PictureId, FileManager.PictureStorage.PictureSize._80x80)
                );

        public string Title
        {
            get
            {
                formatReplacements[nameof(Title)] = book.Title;

                var sortName = book.Title
                    .Replace("|", "")
                    .Replace(":", "")
                    .ToLowerInvariant();
                if (sortName.StartsWith("the ") || sortName.StartsWith("a ") || sortName.StartsWith("an "))
                    sortName = sortName.Substring(sortName.IndexOf(" ") + 1);

                return sortName;
            }
        }

        public string Authors => book.AuthorNames;
        public string Narrators => book.NarratorNames;

        public int Length
        {
            get
            {
                formatReplacements[nameof(Length)]
                    = book.LengthInMinutes == 0
                    ? "[pre-release]"
                    : $"{book.LengthInMinutes / 60} hr {book.LengthInMinutes % 60} min";

                return book.LengthInMinutes;
            }
        }

        public string Series => book.SeriesNames;

        public string Description
            => book.Description == null ? ""
            : book.Description.Length < 63 ? book.Description
            : book.Description.Substring(0, 60) + "...";

        public string Category => string.Join(" > ", book.CategoriesNames);

        // star ratings retain numeric value but display star text. this is needed because just using star text doesn't sort correctly:
        // - star
        // - star star
        // - star 1/2

        public string Product_Rating
        {
            get
            {
                Rating rating = book.Rating;

                formatReplacements[nameof(Product_Rating)] = starString(rating);

                return firstScore(rating);
            }
        }

        public DateTime? Purchase_Date => libraryBook.DateAdded;

        public string My_Rating
        {
            get
            {
                Rating rating = book.UserDefinedItem.Rating;

                formatReplacements[nameof(My_Rating)] = starString(rating);

                return firstScore(rating);
            }
        }

        private string starString(Rating rating)
            => (rating?.FirstScore != null && rating?.FirstScore > 0f)
            ? rating?.ToStarString()
            : "";
        private string firstScore(Rating rating) => rating?.FirstScore.ToString("0.0");

        // max 5 text rows
        public string Misc
        {
            get
            {
                var details = new List<string>();
                if (book.HasPdfs)
                    details.Add("Has PDFs");
                if (book.IsAbridged)
                    details.Add("Abridged");
                if (book.DatePublished.HasValue)
                    details.Add($"Date pub'd: {book.DatePublished.Value.ToString("MM/dd/yyyy")}");
                // this goes last since it's most likely to have a line-break
                if (!string.IsNullOrWhiteSpace(book.Publisher))
                    details.Add($"Pub: {book.Publisher}");

                if (!details.Any())
                    return "[details not imported]";

                return string.Join("\r\n", details);
            }
        }

        public string Download_Status
        {
            get
            {
                var print
                    = FileManager.AudibleFileStorage.Audio.ExistsAsync(book.AudibleProductId).GetAwaiter().GetResult() ? "Liberated"
                    : FileManager.AudibleFileStorage.AAX.ExistsAsync(book.AudibleProductId).GetAwaiter().GetResult() ? "DRM"
                    : "NOT d/l'ed";

                if (!book.Supplements.Any())
                    return print;

                print += "\r\n";
                
                var downloadStatuses = book.Supplements
                    .Select(d => FileManager.AudibleFileStorage.PDF.ExistsAsync(book.AudibleProductId).GetAwaiter().GetResult())
                    // break delayed execution right now!
                    .ToList();
                var count = downloadStatuses.Count;
                if (count == 1)
                {
                    print += downloadStatuses[0]
                        ? "PDF d/l'ed"
                        : "PDF NOT d/l'ed";
                }
                else
                {
                    var downloadedCount = downloadStatuses.Count(s => s);
                    print
                        += downloadedCount == count ? $"{count} PDFs d/l'ed"
                        : downloadedCount == 0 ? $"{count} PDFs NOT d/l'ed"
                        : $"{downloadedCount} of {count} PDFs d/l'ed";
                }

                return print;
            }
        }
    }
}
