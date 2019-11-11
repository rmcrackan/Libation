using System.Threading.Tasks;
using DataLayer;
using LibationSearchEngine;

namespace ApplicationServices
{
	public static class SearchEngineCommands
	{
		public static void FullReIndex()
		{
			var engine = new SearchEngine();
			engine.CreateNewIndex();
		}

		public static SearchResultSet Search(string searchString)
		{
			var engine = new SearchEngine();
			try
			{
				return engine.Search(searchString);
			}
			catch (System.IO.FileNotFoundException)
			{
				FullReIndex();
				return engine.Search(searchString);
			}
		}

		public static void UpdateBookTags(Book book)
		{
			var engine = new SearchEngine();
			try
			{
				engine.UpdateTags(book.AudibleProductId, book.UserDefinedItem.Tags);
			}
			catch (System.IO.FileNotFoundException)
			{
				FullReIndex();
				engine.UpdateTags(book.AudibleProductId, book.UserDefinedItem.Tags);
			}
		}
	}
}
