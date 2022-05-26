using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DataLayer;
using LibationSearchEngine;

namespace ApplicationServices
{
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

		public static EventHandler SearchEngineUpdated;

		#region Update
		private static bool isUpdating;

		public static void UpdateBooks(IEnumerable<Book> books)
		{
			// Semi-arbitrary. At some point it's more worth it to do a full re-index than to do one offs.
			// I did not benchmark before choosing the number here
			if (books.Count() > 15)
				FullReIndex();
			else
			{
				foreach (var book in books)
				{
					UpdateLiberatedStatus(book);
					UpdateBookTags(book);
				}
			}
		}

		public static void FullReIndex() => performSafeCommand(e =>
			fullReIndex(e)
		);

		internal static void UpdateLiberatedStatus(Book book) => performSafeCommand(e =>
			e.UpdateLiberatedStatus(book)
		);

		internal static void UpdateBookTags(Book book) => performSafeCommand(e =>
			e.UpdateTags(book.AudibleProductId, book.UserDefinedItem.Tags)
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
					SearchEngineUpdated?.Invoke(null, null);
			}
			finally
			{
				isUpdating = prevIsUpdating;
			}
		}

		private static void fullReIndex(SearchEngine engine)
		{
			var library = DbContexts.GetLibrary_Flat_NoTracking();
			engine.CreateNewIndex(library);
		}
		#endregion
	}
}
