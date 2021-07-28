using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using DataLayer;

namespace LibationWinForms
{
	internal class GridEntry
	{
		private LibraryBook libraryBook { get; }
		private Book book => libraryBook.Book;

		public Book GetBook() => book;

		public GridEntry(LibraryBook libraryBook) => this.libraryBook = libraryBook;

		// hide from public fields from Data Source GUI with [Browsable(false)]

		[Browsable(false)]
		public string Tags => book.UserDefinedItem.Tags;
		[Browsable(false)]
		public IEnumerable<string> TagsEnumerated => book.UserDefinedItem.TagsEnumerated;

		public enum LiberatedState { NotDownloaded, PartialDownload, Liberated }
		[Browsable(false)]
		public LiberatedState Liberated_Status
			=> FileManager.AudibleFileStorage.Audio.Exists(book.AudibleProductId) ? LiberatedState.Liberated
			: FileManager.AudibleFileStorage.AAXC.Exists(book.AudibleProductId) ? LiberatedState.PartialDownload
			: LiberatedState.NotDownloaded;

		public enum PdfState { NoPdf, Downloaded, NotDownloaded }
		[Browsable(false)]
		public PdfState Pdf_Status
			=> !book.Supplements.Any() ? PdfState.NoPdf
			: FileManager.AudibleFileStorage.PDF.Exists(book.AudibleProductId) ? PdfState.Downloaded
			: PdfState.NotDownloaded;

		// displayValues is what gets displayed
		// the value that gets returned from the property is the cell's value
		// this allows for the value to be sorted one way and displayed another
		// eg:
		//   orig title: The Computer
		//   formatReplacement: The Computer
		//   value for sorting: Computer
		private Dictionary<string, string> displayValues { get; } = new Dictionary<string, string>();
		public bool TryDisplayValue(string key, out string value) => displayValues.TryGetValue(key, out value);

		public Image Cover =>
			WindowsDesktopUtilities.WinAudibleImageServer.GetImage(book.PictureId, FileManager.PictureSize._80x80);

		public string Title
		{
			get
			{
				displayValues[nameof(Title)] = book.Title;
				return getSortName(book.Title);
			}
		}

		public string Authors => book.AuthorNames;
		public string Narrators => book.NarratorNames;

		public int Length
		{
			get
			{
				displayValues[nameof(Length)]
					= book.LengthInMinutes == 0
					? ""
					: $"{book.LengthInMinutes / 60} hr {book.LengthInMinutes % 60} min";

				return book.LengthInMinutes;
			}
		}

		public string Series
		{
			get
			{
				displayValues[nameof(Series)] = book.SeriesNames;
				return getSortName(book.SeriesNames);
			}
		}

		private static string[] sortPrefixIgnores { get; } = new[] { "the", "a", "an" };
		private static string getSortName(string unformattedName)
		{
			var sortName = unformattedName
				.Replace("|", "")
				.Replace(":", "")
				.ToLowerInvariant()
				.Trim();

			if (sortPrefixIgnores.Any(prefix => sortName.StartsWith(prefix + " ")))
				sortName = sortName.Substring(sortName.IndexOf(" ") + 1).TrimStart();

			return sortName;
		}

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
				displayValues[nameof(Product_Rating)] = starString(book.Rating);
				return firstScore(book.Rating);
			}
		}

		public string Purchase_Date
		{
			get
			{
				displayValues[nameof(Purchase_Date)] = libraryBook.DateAdded.ToString("d");
				return libraryBook.DateAdded.ToString("yyyy-MM-dd HH:mm:ss");
			}
		}

        public string My_Rating
        {
            get
            {
                displayValues[nameof(My_Rating)] = starString(book.UserDefinedItem.Rating);
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

				var locale
					= string.IsNullOrWhiteSpace(book.Locale)
					? "[unknown]"
					: book.Locale;
				var acct
					= string.IsNullOrWhiteSpace(libraryBook.Account)
					? "[unknown]"
					: libraryBook.Account;
				details.Add($"Account: {locale} - {acct}");

                if (book.HasPdf)
                    details.Add("Has PDF");
                if (book.IsAbridged)
                    details.Add("Abridged");
                if (book.DatePublished.HasValue)
                    details.Add($"Date pub'd: {book.DatePublished.Value:MM/dd/yyyy}");
                // this goes last since it's most likely to have a line-break
                if (!string.IsNullOrWhiteSpace(book.Publisher))
                    details.Add($"Pub: {book.Publisher.Trim()}");

				if (!details.Any())
                    return "[details not imported]";

                return string.Join("\r\n", details);
            }
        }
    }
}
