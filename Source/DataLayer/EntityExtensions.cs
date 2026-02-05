using System;
using System.Collections.Generic;
using System.Linq;

namespace DataLayer;

public static class EntityExtensions
{
	public static IEnumerable<BookContributor> ByRole(this IEnumerable<BookContributor> contributors, Role role)
		=> contributors
			.Where(a => a.Role == role)
			.OrderBy(a => a.Order);


	extension(Book book)
	{
		public string SeriesSortable() => Formatters.GetSortName(book.SeriesNames(true));
		public string TitleSortable() => Formatters.GetSortName(book.Title + book.Subtitle);

		public string AuthorNames => string.Join(", ", book.Authors.Select(a => a.Name));
		public string NarratorNames => string.Join(", ", book.Narrators.Select(n => n.Name));
		/// <summary>True if IsLiberated or Error. False if NotLiberated</summary>
		public bool AudioExists => book.UserDefinedItem.BookStatus is LiberatedStatus.Liberated or LiberatedStatus.Error;
		/// <summary>True if exists and IsLiberated. Else false</summary>
		public bool PdfExists => book.UserDefinedItem.PdfStatus is LiberatedStatus.Liberated;
		/// <summary> Whether the book has any supplements </summary>
		public bool HasPdf => book.Supplements.Any();

		public string SeriesNames(bool includeIndex = false)
		{
			if (book.SeriesLink is null)
				return "";

			// first: alphabetical by name
			var withNames = book.SeriesLink
				.Where(s => !string.IsNullOrWhiteSpace(s.Series.Name))
				.Select(getSeriesNameString)
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

			string getSeriesNameString(SeriesBook sb)
				=> includeIndex && !string.IsNullOrWhiteSpace(sb.Order) && sb.Order != "-1"
				? $"{sb.Series.Name} (#{sb.Order})"
				: sb.Series.Name ?? "";
		}

		public string[] LowestCategoryNames()
			=> book.CategoriesLink?.Count is 0 or null ? []
			: book
				.CategoriesLink
				.Select(cl => cl.CategoryLadder.Categories.LastOrDefault()?.Name)
				.OfType<string>()
				.Distinct()
				.ToArray();

		public string[] AllCategoryNames()
			=> book.CategoriesLink?.Any() is not true ? Array.Empty<string>()
			: book
				.CategoriesLink
				.SelectMany(cl => cl.CategoryLadder.Categories)
				.Select(c => c.Name)
				.ToArray();

		public string[]? AllCategoryIds()
			=> book.CategoriesLink?.Count is null or 0 ? null
			: book
				.CategoriesLink
				.SelectMany(cl => cl.CategoryLadder.Categories)
				.Select(c => c.AudibleCategoryId)
				.ToArray();
	}


	public static string AggregateTitles(this IEnumerable<LibraryBook> libraryBooks, int max = 5)
	{
		if (libraryBooks is null || !libraryBooks.Any())
			return "";

		max = Math.Max(max, 1);

		var titles = libraryBooks.Select(lb => "- " + lb.Book.TitleWithSubtitle).ToList();
		var titlesAgg = titles.Take(max).Aggregate((a, b) => $"{a}\r\n{b}");
		if (titles.Count == max + 1)
			titlesAgg += $"\r\n\r\nand 1 other";
		else if (titles.Count > max + 1)
			titlesAgg += $"\r\n\r\nand {titles.Count - max} others";
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

		if (score - fullStars >= 0.75f)
			starString += STAR;
		else if (score - fullStars >= 0.25f)
			starString += HALF;

		return starString;
	}
}
