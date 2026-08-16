using DataLayer;
using LibationSearchEngine;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ApplicationServices;

public static class SearchEngineCommands
{
	/// <summary>Serializes all search index access so only one reader/writer is active at a time, avoiding write.lock contention.</summary>
	private static readonly object IndexLock = new();

	#region Search
	public static SearchResultSet Search(string searchString) => performSafeQuery(e =>
		e.Search(searchString)
	);

	private static bool schemaChecked;

	/// <summary>
	/// A search field only exists in documents written after it was added, and nothing in Libation invalidates the
	/// index when the field list changes, so an index left over from an older version quietly finds nothing for a
	/// new field. Rebuilding once per session after an upgrade keeps the Filter Options list honest.
	/// </summary>
	private static void rebuildIfSchemaOutdated(SearchEngine engine)
	{
		if (schemaChecked)
			return;
		schemaChecked = true;

		try
		{
			if (!engine.IsIndexSchemaOutdated())
				return;

			Log.Information("The search index was built from an older set of search fields. Rebuilding it.");
			fullReIndex(engine);
		}
		catch (Exception ex)
		{
			// searching a stale index is better than not searching at all
			Log.Warning(ex, "Could not rebuild the search index for the current set of search fields.");
		}
	}

	private static T performSafeQuery<T>(Func<SearchEngine, T> func)
	{
		lock (IndexLock)
		{
			var engine = new SearchEngine();
			rebuildIfSchemaOutdated(engine);
			try
			{
				return func(engine);
			}
			catch (FileNotFoundException)
			{
				fullReIndex(engine);
				return func(engine);
			}
			catch (Exception ex) when (SearchEngine.IsRecoverableCorruptIndexException(ex))
			{
				Log.Warning(ex, "Search index unreadable or corrupt; rebuilding and retrying query.");
				fullReIndex(engine);
				return func(engine);
			}
		}
	}
	#endregion

	public static event EventHandler? SearchEngineUpdated;

	/// <summary>
	/// Occurs when the index could not be updated even after automatic repair, so it needs the user's help.
	/// </summary>
	public static event EventHandler<Exception>? UpdateFailed;

	#region Update
	private static bool isUpdating;

	/// <summary>Updates the index after books were added to or removed from the library.</summary>
	public static void OnLibrarySizeChanged(List<LibraryBook> libraryBooks)
		=> tryUpdate(() => FullReIndex(libraryBooks));

	/// <summary>Updates the index after book details, tags or statuses were committed.</summary>
	public static void OnBookUserDefinedItemCommitted(IEnumerable<LibraryBook> books)
		=> tryUpdate(() => UpdateBooks(books));

	/// <summary>
	/// The database change that triggers an update is committed before the update runs, and this index is derived
	/// from that database, so a failure here is reported instead of propagated. Letting it escape reported a
	/// successful scan as "Error importing library", and, since this is the first subscriber to those events,
	/// stopped the handlers that refresh the grid and the backup counts from running at all.
	/// </summary>
	private static void tryUpdate(Action update)
	{
		try
		{
			update();
		}
		catch (Exception ex)
		{
			Log.Error(ex, "Failed to update the search index. Library changes are saved; search and filter results may be stale until the next update succeeds.");
			UpdateFailed?.Invoke(null, ex);
		}
	}

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
		catch (Exception ex) when (SearchEngine.IsRecoverableCorruptIndexException(ex))
		{
			Log.Warning(ex, "Search index unreadable or corrupt; rebuilding and retrying.");
			fullReIndex(new SearchEngine());
			update(action);
		}
	}

	private static void update(Action<SearchEngine> action)
	{
		if (action is null)
			return;

		lock (IndexLock)
		{
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
