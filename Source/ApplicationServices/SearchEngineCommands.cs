using DataLayer;
using LibationSearchEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ApplicationServices;

public static class SearchEngineCommands
{
	#region Search
	public static SearchResultSet Search(string searchString) => performSafeQuery(e =>
		e.Search(searchString)
	);

	private static T performSafeQuery<T>(Func<SearchEngine, T> func)
	{
		var engine = new SearchEngine();
		try
		{
			return func(engine);
		}
		catch (FileNotFoundException)
		{
			fullReIndex(engine);
			return func(engine);
		}
	}
	#endregion

	public static event EventHandler? SearchEngineUpdated;

	#region Update
	private static bool isUpdating;

	public static void UpdateBooks(IEnumerable<LibraryBook> books)
	{
		// Semi-arbitrary. At some point it's more worth it to do a full re-index than to do one offs.
		// I did not benchmark before choosing the number here
		if (books.Count() > 15)
			FullReIndex();
		else
		{
			foreach (var book in books)
				UpdateUserDefinedItems(book);
		}
	}

	public static void FullReIndex() => performSafeCommand(fullReIndex);
	public static void FullReIndex(List<LibraryBook> libraryBooks)
		=> performSafeCommand(se => fullReIndex(se, libraryBooks.WithoutParents()));

	internal static void UpdateUserDefinedItems(LibraryBook book) => performSafeCommand(e =>
		{
			e.UpdateLiberatedStatus(book);
			e.UpdateTags(book.Book.AudibleProductId, book.Book.UserDefinedItem.Tags);
			e.UpdateUserRatings(book);
		}
	);

	private static void performSafeCommand(Action<SearchEngine> action)
	{
		try
		{
			update(action);
		}
		catch (FileNotFoundException)
		{
			fullReIndex(new SearchEngine());
			update(action);
		}
	}

	private static void update(Action<SearchEngine> action)
	{
		if (action is null)
			return;

		// support nesting incl recursion
		var prevIsUpdating = isUpdating;
		try
		{
			isUpdating = true;

			action(new SearchEngine());
			if (!prevIsUpdating)
				SearchEngineUpdated?.Invoke(null, EventArgs.Empty);
		}
		finally
		{
			isUpdating = prevIsUpdating;
		}
	}

	private static void fullReIndex(SearchEngine engine)
	{
		var library = DbContexts.GetLibrary_Flat_NoTracking();
		fullReIndex(engine, library);
	}

	private static void fullReIndex(SearchEngine engine, IEnumerable<LibraryBook> libraryBooks)
	=> engine.CreateNewIndex(libraryBooks);
	#endregion
}
