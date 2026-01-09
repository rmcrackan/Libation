using LibationSearchEngine;

#nullable enable
namespace ApplicationServices;

public interface ISearchEngine
{
	SearchResultSet? GetSearchResultSet(string? searchString);
}
