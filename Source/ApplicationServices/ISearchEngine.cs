using LibationSearchEngine;

namespace ApplicationServices;

public interface ISearchEngine
{
	SearchResultSet? GetSearchResultSet(string? searchString);
}
