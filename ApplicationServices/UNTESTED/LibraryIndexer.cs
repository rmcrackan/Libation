using System;
using System.Threading.Tasks;
using AudibleApi;
using DtoImporterService;
using InternalUtilities;

namespace ApplicationServices
{
	public class LibraryIndexer
	{
		public async Task<(int totalCount, int newCount)> IndexAsync(ILoginCallback callback)
		{
			var audibleApiActions = new AudibleApiActions();
			var items = await audibleApiActions.GetAllLibraryItemsAsync(callback);
			var totalCount = items.Count;

			var libImporter = new LibraryImporter();
			var newCount = await Task.Run(() => libImporter.Import(items));

			await SearchEngineActions.FullReIndexAsync();

			return (totalCount, newCount);
		}
	}
}
