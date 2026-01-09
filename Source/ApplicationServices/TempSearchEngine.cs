using DataLayer;
using LibationFileManager;
using LibationSearchEngine;
using System.Collections.Generic;

#nullable enable
namespace ApplicationServices;

/// <summary>
/// A temporary search engine created in InProgress/TempSearchEngine
/// Used for Trash Bin searches to avoid interfering with the main search engine
/// </summary>
public class TempSearchEngine : ISearchEngine
{
	public static string SearchEnginePath { get; }
		= System.IO.Path.Combine(Configuration.Instance.InProgress, nameof(TempSearchEngine));
	private SearchEngine SearchEngine { get; } = new SearchEngine(SearchEnginePath);

	public bool ReindexSearchEngine(IEnumerable<LibraryBook> books)
	{
		try
		{
			SearchEngine.CreateNewIndex(books, overwrite: true);
			return true;
		}
		catch
		{
			return false;
		}
	}
	public SearchResultSet? GetSearchResultSet(string? searchString)
	{
		if (string.IsNullOrEmpty(searchString))
			return null;

		try
		{
			return SearchEngine.Search(searchString);
		}
		catch
		{
			return null;
		}
	}
}
