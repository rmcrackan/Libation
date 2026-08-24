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

	private static T performSafeQuery<T>(Func<SearchEngine, T> func)
	{
		lock (IndexLock)
		{
			var engine = new SearchEngine();
			repairShortIndex(engine);
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

	/// <summary>Set once the index has been measured against the library, so the check costs one query per run.</summary>
	private static bool indexCounted;

	/// <summary>
	/// Rebuilds an index that holds fewer books than the library does.
	/// <para>
	/// A book the index never received is not merely unsearchable. Filtering intersects the grid with the
	/// query's hits, so a positive term such as <c>Absent</c> cannot return it, while the negated <c>-Absent</c>
	/// resolves to "every document in the index" and therefore drops it from the grid - which looks exactly
	/// like the negated filter working correctly. Reported as issue #1989.
	/// </para>
	/// <para>
	/// Nothing else notices: the index is only ever written as a whole, and <see cref="tryUpdate"/>
	/// deliberately swallows a rebuild that fails so a bad index cannot fail a good scan. A short index
	/// therefore stays short until something happens to change the library again.
	/// </para>
	/// </summary>
	private static void repairShortIndex(SearchEngine engine)
	{
		if (indexCounted)
			return;

		// before the work, not after: a check that throws must not run on every query for the rest of the session
		indexCounted = true;

		try
		{
			var indexed = engine.GetIndexedBookCount();

			// no index yet, or one too damaged to read. Both already have their own recovery.
			if (indexed < 0)
				return;

			var expected = DbContexts.GetIndexableBookCount();

			// more documents than books is stale rather than harmful: an extra document matches no grid row.
			if (indexed >= expected)
				return;

			Log.Warning("The search index holds {Indexed} of {Expected} books. Rebuilding it: search cannot find the rest, and a negated filter hides them.", indexed, expected);

			var failures = fullReIndex(engine);

			if (failures > 0)
				Log.Error("{Failures} book(s) could not be added to the search index. Search cannot find them, and a negated filter hides them.", failures);
		}
		catch (Exception ex)
		{
			// Searching with a short index still beats not searching at all.
			Log.Error(ex, "Could not check the search index against the library.");
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

	public static void FullReIndex() => performSafeCommand(e => fullReIndex(e));
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

	/// <returns>How many books could not be indexed.</returns>
	private static int fullReIndex(SearchEngine engine)
	{
		var library = DbContexts.GetLibrary_Flat_NoTracking();
		return fullReIndex(engine, library);
	}

	/// <returns>How many books could not be indexed.</returns>
	private static int fullReIndex(SearchEngine engine, IEnumerable<LibraryBook> libraryBooks)
	=> engine.CreateNewIndex(libraryBooks);
	#endregion
}
