using DataLayer;
using System;
using System.Linq;

namespace LibationUiBase.GridView;

/// <summary>Sort key for the Series scope: first series name, numeric position, then title.</summary>
public sealed class SeriesBrowseKey : IComparable
{
	public const string MemberName = "SeriesBrowse";

	private string Name { get; }
	private float Position { get; }
	private string Title { get; }

	private SeriesBrowseKey(string name, float position, string title)
		=> (Name, Position, Title) = (name, position, title);

	private static SeriesBook? FirstLink(Book book)
	{
		var links = book.SeriesLink.ToList();
		return links.Where(s => !string.IsNullOrWhiteSpace(s.Series.Name)).OrderBy(s => s.Series.Name, StringComparer.CurrentCultureIgnoreCase).FirstOrDefault()
			?? links.FirstOrDefault();
	}

	/// <summary>Display name of the book's first series; equals the sort key's name.</summary>
	public static string GroupName(Book book)
	{
		var first = FirstLink(book);
		return first?.Series.Name ?? first?.Series.AudibleSeriesId ?? "";
	}

	public static SeriesBrowseKey For(Book book)
	{
		var first = FirstLink(book);
		var position = first is null || string.IsNullOrWhiteSpace(first.Order) || first.Order == "-1" ? float.MaxValue : first.Index;
		return new(GroupName(book), position, book.TitleSortable());
	}

	public int CompareTo(object? obj)
	{
		if (obj is not SeriesBrowseKey other) return 1;

		var compare = StringComparer.CurrentCultureIgnoreCase.Compare(Name, other.Name);
		if (compare == 0) compare = Position.CompareTo(other.Position);
		return compare != 0 ? compare : StringComparer.CurrentCultureIgnoreCase.Compare(Title, other.Title);
	}
}
