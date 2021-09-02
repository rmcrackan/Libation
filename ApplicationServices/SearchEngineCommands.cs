using System;
using System.IO;
using DataLayer;
using LibationSearchEngine;

namespace ApplicationServices
{
	public static class SearchEngineCommands
	{
		public static void FullReIndex(SearchEngine engine = null)
		{
			engine ??= new SearchEngine();
			var library = DbContexts.GetLibrary_Flat_NoTracking();
			engine.CreateNewIndex(library);
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
			var engine = new SearchEngine();
			try
			{
				action(engine);
			}
			catch (FileNotFoundException)
			{
				FullReIndex(engine);
				action(engine);
			}
		}

		private static T performSearchEngineFunc_safe<T>(Func<SearchEngine, T> func)
		{
			var engine = new SearchEngine();
			try
			{
				return func(engine);
			}
			catch (FileNotFoundException)
			{
				FullReIndex(engine);
				return func(engine);
			}
		}
	}
}
