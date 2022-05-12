using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataLayer
{
    public static class EntityExtensions
    {
        public static string TitleSortable(this Book book) => Formatters.GetSortName(book.Title);

        public static string AuthorNames(this Book book) => string.Join(", ", book.Authors.Select(a => a.Name));
        public static string NarratorNames(this Book book) => string.Join(", ", book.Narrators.Select(n => n.Name));

        /// <summary>True if IsLiberated or Error. False if NotLiberated</summary>
        public static bool Audio_Exists(this Book book) => book.UserDefinedItem.BookStatus != LiberatedStatus.NotLiberated;
        /// <summary>True if exists and IsLiberated. Else false</summary>
        public static bool PDF_Exists(this Book book) => book.UserDefinedItem.PdfStatus == LiberatedStatus.Liberated;

        public static string SeriesSortable(this Book book) => Formatters.GetSortName(book.SeriesNames());
		public static bool HasPdf(this Book book) => book.Supplements.Any();
        public static string SeriesNames(this Book book)
        {
            if (book.SeriesLink is null)
                return "";

            // first: alphabetical by name
            var withNames = book.SeriesLink
                .Where(s => !string.IsNullOrWhiteSpace(s.Series.Name))
                .Select(s => s.Series.Name)
                .OrderBy(a => a)
                .ToList();
            // then un-named are alpha by series id
            var nullNames = book.SeriesLink
                .Where(s => string.IsNullOrWhiteSpace(s.Series.Name))
                .Select(s => s.Series.AudibleSeriesId)
                .OrderBy(a => a)
                .ToList();

            var all = withNames.Union(nullNames).ToList();
            return string.Join(", ", all);
        }
        public static string[] CategoriesNames(this Book book)
            => book.Category is null ? new string[0]
            : book.Category.ParentCategory is null ? new[] { book.Category.Name }
            : new[] { book.Category.ParentCategory.Name, book.Category.Name };
        public static string[] CategoriesIds(this Book book)
            => book.Category is null ? null
            : book.Category.ParentCategory is null ? new[] { book.Category.AudibleCategoryId }
            : new[] { book.Category.ParentCategory.AudibleCategoryId, book.Category.AudibleCategoryId };

        public static string AggregateTitles(this IEnumerable<LibraryBook> libraryBooks, int max = 5)
		{
			if (libraryBooks is null || !libraryBooks.Any())
				return "";

			max = Math.Max(max, 1);

			var titles = libraryBooks.Select(lb => "- " + lb.Book.Title).ToList();
			var titlesAgg = titles.Take(max).Aggregate((a, b) => $"{a}\r\n{b}");
			if (titles.Count == max + 1)
				titlesAgg += $"\r\n\r\nand 1 other";
			else if (titles.Count > max + 1)
				titlesAgg += $"\r\n\r\nand {titles.Count - max } others";
			return titlesAgg;
		}

        public static float FirstScore(this Rating rating)
            => rating.OverallRating > 0 ? rating.OverallRating
            : rating.PerformanceRating > 0 ? rating.PerformanceRating
            : rating.StoryRating;
        public static string ToStarString(this Rating rating)
        {
            var items = new List<string>();

            if (rating.OverallRating > 0)
                items.Add($"Overall:   {getStars(rating.OverallRating)}");
            if (rating.PerformanceRating > 0)
                items.Add($"Perform: {getStars(rating.PerformanceRating)}");
            if (rating.StoryRating > 0)
                items.Add($"Story:      {getStars(rating.StoryRating)}");

            return string.Join("\r\n", items);
        }
        /// <summary>character: ★</summary>
        const char STAR = '\u2605';
        /// <summary>character: ½</summary>
        const char HALF = '\u00BD';
        private static string getStars(float score)
        {
            var fullStars = (int)Math.Floor(score);

            var starString = new string(STAR, fullStars);

            if (score - fullStars >= 0.25f)
                starString += HALF;

            return starString;
        }
    }
}
