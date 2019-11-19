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

		// formatReplacements is what gets displayed
		// the value that gets returned from the property is the cell's value
		// this allows for the value to be sorted one way and displayed another
		// eg:
		//   orig title: The Computer
		//   formatReplacement: The Computer
		//   value for sorting: Computer
		private Dictionary<string, string> formatReplacements { get; } = new Dictionary<string, string>();
		public bool TryGetFormatted(string key, out string value) => formatReplacements.TryGetValue(key, out value);

		public Image Cover =>
			WindowsDesktopUtilities.WinAudibleImageServer.GetImage(book.PictureId, FileManager.PictureSize._80x80);

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
					? ""
					: $"{book.LengthInMinutes / 60} hr {book.LengthInMinutes % 60} min";

				return book.LengthInMinutes;
			}
		}

		public string Series => book.SeriesNames;

		private string descriptionCache = null;
		public string Description
		{
			get
			{
				// HtmlAgilityPack is expensive. cache results
				if (descriptionCache is null)
				{
					if (book.Description is null)
						descriptionCache = "";
					else
					{
						var doc = new HtmlAgilityPack.HtmlDocument();
						doc.LoadHtml(book.Description);
						var noHtml = doc.DocumentNode.InnerText;
						descriptionCache
							= noHtml.Length < 63
							? noHtml
							: noHtml.Substring(0, 60) + "...";
					}
				}

				return descriptionCache;
			}
		}

		public string Category => string.Join(" > ", book.CategoriesNames);

		// star ratings retain numeric value but display star text. this is needed because just using star text doesn't sort correctly:
		// - star
		// - star star
		// - star 1/2

		public string Product_Rating
		{
			get
			{
				formatReplacements[nameof(Product_Rating)] = starString(book.Rating);
				return firstScore(book.Rating);
			}
		}

		public string Purchase_Date
		{
			get
			{
				formatReplacements[nameof(Purchase_Date)] = libraryBook.DateAdded.ToString("d");
				return libraryBook.DateAdded.ToString("yyyy-MM-dd HH:mm:ss");
			}
		}

        public string My_Rating
        {
            get
            {
                formatReplacements[nameof(My_Rating)] = starString(book.UserDefinedItem.Rating);
                return firstScore(book.UserDefinedItem.Rating);
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
                if (book.HasPdf)
                    details.Add("Has PDF");
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
