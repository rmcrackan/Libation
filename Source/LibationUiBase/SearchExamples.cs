using DataLayer;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LibationUiBase;

public record SearchExample(string Query, string Description);

/// <summary>Builds search-syntax examples from the user's own library.</summary>
public static class SearchExamples
{
	private const int MaxExamples = 10;
	private const int MinExamples = 3;

	/// <summary>Empty when fewer than three examples match anything.</summary>
	public static IReadOnlyList<SearchExample> Build(IReadOnlyCollection<LibraryBook> library, Func<string, int> countMatches)
	{
		var books = library
			.Where(lb => !lb.IsDeleted && lb.Book.ContentType != ContentType.Parent)
			.ToList();
		if (books.Count == 0)
			return [];

		var results = new List<SearchExample>();
		foreach (var candidate in Candidates(books))
		{
			if (candidate is null)
				continue;
			try
			{
				if (countMatches(candidate.Query) > 0)
					results.Add(candidate);
			}
			catch
			{
			}
			if (results.Count == MaxExamples)
				break;
		}
		return results.Count >= MinExamples ? results : [];
	}

	private static IEnumerable<SearchExample?> Candidates(List<LibraryBook> books)
	{
		var authorOf = books
			.Select(lb => (lb, name: Clean(lb.Book.Authors.FirstOrDefault()?.Name)))
			.Where(t => t.name.Length > 0)
			.ToList();
		var authorNames = authorOf.Select(t => t.name).ToHashSet(StringComparer.OrdinalIgnoreCase);

		var topAuthor = MostCommon(authorOf.Select(t => t.name));
		yield return topAuthor is null ? null : new($"author:\"{topAuthor}\"", $"Books by {topAuthor}");

		var notDownloaded = MostCommon(authorOf
			.Where(t => t.lb.Book.UserDefinedItem.BookStatus != LiberatedStatus.Liberated)
			.Select(t => t.name));
		yield return notDownloaded is null ? null
			: new($"author:\"{notDownloaded}\" AND -IsLiberated", $"{notDownloaded} books you haven't downloaded");

		var narrator = MostCommon(books
			.SelectMany(lb => lb.Book.Narrators.Select(n => Clean(n.Name)))
			.Where(n => n.Length > 0 && !authorNames.Contains(n)));
		yield return narrator is null ? null : new($"narrator:\"{narrator}\"", $"Narrated by {narrator}");

		var series = books
			.SelectMany(lb => lb.Book.SeriesLink
				.Select(s => Clean(s.Series.Name))
				.Where(n => n.Length > 0)
				.Distinct(StringComparer.OrdinalIgnoreCase))
			.GroupBy(n => n, StringComparer.OrdinalIgnoreCase)
			.OrderByDescending(g => g.Count())
			.ThenBy(g => g.Key, StringComparer.OrdinalIgnoreCase)
			.Take(2)
			.Select(g => g.Key)
			.ToList();
		yield return series.Count switch
		{
			0 => null,
			1 => new($"series:\"{series[0]}\"", $"Books in {series[0]}"),
			_ => new($"series:\"{series[0]}\" OR series:\"{series[1]}\"", $"Books in {series[0]} or {series[1]}"),
		};

		var phrase = books
			.Select(lb => TitlePhrase(lb.Book.Title))
			.FirstOrDefault(p => p is not null);
		yield return phrase is null ? null : new($"title:\"{phrase}\"", $"Titles containing “{phrase}”");

		var tag = MostCommon(books.SelectMany(lb => lb.Book.UserDefinedItem.TagsEnumerated));
		yield return tag is null ? null : new($"[{tag}]", $"Books tagged {tag}");

		var lengths = books.Select(lb => lb.Book.LengthInMinutes).Where(m => m > 0).Order().ToList();
		if (lengths.Count == 0)
			yield return null;
		else if (lengths[^1] >= 600)
			yield return new("hours:[10 TO 999]", "10 hours or longer");
		else
		{
			var m = lengths[lengths.Count / 2];
			yield return new($"length:[0 TO {m}]", $"Shorter than {Math.Max(1, (int)Math.Round(m / 60.0))} hours");
		}

		var year = MostCommon(books
			.Where(lb => lb.Book.DatePublished.HasValue)
			.Select(lb => lb.Book.DatePublished!.Value.Year.ToString("0000")));
		yield return year is null ? null : new($"datepublished:[{year}0101 TO {year}1231]", $"Published in {year}");

		var rated = books.Count(lb => lb.Book.UserDefinedItem.Rating.OverallRating > 0f);
		yield return rated > 0 && rated < books.Count ? new("-IsRated", "Books you haven't rated") : null;

		var byId = books.FirstOrDefault(lb => !string.IsNullOrWhiteSpace(lb.Book.AudibleProductId));
		yield return byId is null ? null : new($"id:{byId.Book.AudibleProductId}", $"“{Clean(byId.Book.Title)}” by its Audible ID");
	}

	private static string Clean(string? s) => (s ?? "").Replace("\"", "").Replace("\\", "").Trim();

	private static string? MostCommon(IEnumerable<string> values) => values
		.GroupBy(v => v, StringComparer.OrdinalIgnoreCase)
		.OrderByDescending(g => g.Count())
		.ThenBy(g => g.Key, StringComparer.OrdinalIgnoreCase)
		.Select(g => g.Key)
		.FirstOrDefault();

	private static string? TitlePhrase(string? title)
	{
		var words = (title ?? "")
			.Split(' ', StringSplitOptions.RemoveEmptyEntries)
			.Select(w => new string(w.Where(char.IsLetterOrDigit).ToArray()).ToLowerInvariant())
			.Where(w => w.Length > 0)
			.Take(2)
			.ToList();
		return words.Count == 2 ? $"{words[0]} {words[1]}" : null;
	}
}
