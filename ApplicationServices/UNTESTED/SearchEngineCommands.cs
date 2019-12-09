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

		public static SearchResultSet Search(string searchString)
		{
			var engine = new SearchEngine(DbContexts.GetContext());
			try
			{
				return engine.Search(searchString);
			}
			catch (FileNotFoundException)
			{
				FullReIndex();
				return engine.Search(searchString);
			}
		}

		public static void UpdateBookTags(Book book)
		{
			var engine = new SearchEngine(DbContexts.GetContext());
			try
			{
				engine.UpdateTags(book.AudibleProductId, book.UserDefinedItem.Tags);
			}
			catch (FileNotFoundException)
			{
				FullReIndex();
				engine.UpdateTags(book.AudibleProductId, book.UserDefinedItem.Tags);
			}
		}
	}
}
