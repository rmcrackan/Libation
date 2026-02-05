using LibationSearchEngine;

namespace ApplicationServices;

/// <summary>
/// The main search engine used Libation.
/// Acts as an adapter to SearchEngineCommands.Search()
/// </summary>
public class MainSearchEngine : ISearchEngine
{
	public static MainSearchEngine Instance { get; } = new MainSearchEngine();
	private MainSearchEngine() { }
	public SearchResultSet? GetSearchResultSet(string? searchString)
		=> string.IsNullOrEmpty(searchString) ? null : SearchEngineCommands.Search(searchString);
}
