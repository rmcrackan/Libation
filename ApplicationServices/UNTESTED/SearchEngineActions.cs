using System.Threading.Tasks;
using DataLayer;

namespace ApplicationServices
{
	public static class SearchEngineActions
	{
		public static async Task FullReIndexAsync()
		{
			var engine = new LibationSearchEngine.SearchEngine();
			await engine.CreateNewIndexAsync().ConfigureAwait(false);
		}

		public static void UpdateBookTags(Book book)
		{
			var engine = new LibationSearchEngine.SearchEngine();
			engine.UpdateTags(book.AudibleProductId, book.UserDefinedItem.Tags);
		}
	}
}
