using DataLayer;
using Serilog;
using System.Collections.Generic;
using System.Linq;

namespace ApplicationServices;

/// <summary>
/// Answers "is the book you searched for in the trash?".
/// A trashed book is filtered out of the library and out of the search index, so searching for one returns
/// nothing at all and looks exactly like a book that was never imported (issue #1925).
/// </summary>
public static class TrashBinSearch
{
	/// <summary>
	/// Books in the trash matching <paramref name="searchString"/>, using the same query syntax as the
	/// library search so a fielded query means the same thing in both places.
	/// </summary>
	/// <remarks>
	/// Indexes the trash on the spot rather than keeping an index in step with it. Call only once a library
	/// search has already come back empty: that happens on Enter, at human speed, so the work is rare.
	/// Returns nothing rather than throwing; this only ever powers a hint.
	/// </remarks>
	public static List<LibraryBook> Search(string? searchString)
	{
		if (string.IsNullOrWhiteSpace(searchString))
			return [];

		try
		{
			//GetDeletedLibraryBooks also returns parents so the trash grid can nest episodes; only the
			//genuinely deleted are candidates here.
			var trashed = DbContexts.GetDeletedLibraryBooks().Where(lb => lb.IsDeleted).ToList();
			if (trashed.Count == 0)
				return [];

			var searchEngine = new TempSearchEngine();
			if (!searchEngine.ReindexSearchEngine(trashed))
				return [];

			if (searchEngine.GetSearchResultSet(searchString) is not { } results)
				return [];

			var matchedIds = results.Docs.Select(d => d.ProductId).ToHashSet();
			return trashed.Where(lb => matchedIds.Contains(lb.Book.AudibleProductId)).ToList();
		}
		catch (System.Exception ex)
		{
			Log.Logger.Error(ex, "Failed to search the trash bin for {searchString}", searchString);
			return [];
		}
	}
}
