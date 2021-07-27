using System;
using System.IO;
using DataLayer;
using LibationSearchEngine;

namespace ApplicationServices
{
	public static class SearchEngineCommands
	{
		public static void FullReIndex()
		{
			var engine = new SearchEngine(DbContexts.GetContext());
			engine.CreateNewIndex();
		}

		public static SearchResultSet Search(string searchString) => performSearchEngineFunc_safe(e =>
			e.Search(searchString)
		);

		public static void UpdateBookTags(Book book) => performSearchEngineAction_safe(e =>
			e.UpdateTags(book.AudibleProductId, book.UserDefinedItem.Tags)
		);

		public static void UpdateLiberatedStatus(Book book) => performSearchEngineAction_safe(e =>
			e.UpdateLiberatedStatus(book)
		);

		private static void performSearchEngineAction_safe(Action<SearchEngine> action)
		{
			var engine = new SearchEngine(DbContexts.GetContext());
			try
			{
				action(engine);
			}
			catch (FileNotFoundException)
			{
				FullReIndex();
				action(engine);
			}
		}

		private static T performSearchEngineFunc_safe<T>(Func<SearchEngine, T> action)
		{
			var engine = new SearchEngine(DbContexts.GetContext());
			try
			{
				return action(engine);
			}
			catch (FileNotFoundException)
			{
				FullReIndex();
				return action(engine);
			}
		}
	}
}
