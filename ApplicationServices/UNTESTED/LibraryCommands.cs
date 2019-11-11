using System;
using System.Threading.Tasks;
using AudibleApi;
using DataLayer;
using DtoImporterService;
using InternalUtilities;

namespace ApplicationServices
{
	public static class LibraryCommands
	{
		public static async Task<(int totalCount, int newCount)> IndexLibraryAsync(ILoginCallback callback)
		{
			var audibleApiActions = new AudibleApiActions();
			var items = await audibleApiActions.GetAllLibraryItemsAsync(callback);
			var totalCount = items.Count;

			var libImporter = new LibraryImporter();
			var newCount = await Task.Run(() => libImporter.Import(items));

			await Task.Run(() => SearchEngineCommands.FullReIndex());

			return (totalCount, newCount);
		}

		public static int IndexChangedTags(Book book)
		{
			// update disconnected entity
			using var context = LibationContext.Create();
			context.Update(book);
			var qtyChanges = context.SaveChanges();

			// this part is tags-specific
			if (qtyChanges > 0)
				SearchEngineCommands.UpdateBookTags(book);

			return qtyChanges;
		}
	}
}
