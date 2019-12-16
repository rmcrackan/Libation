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
		public static async Task<(int totalCount, int newCount)> ImportLibraryAsync(ILoginCallback callback)
		{
			try
			{
				var audibleApiActions = new AudibleApiActions();
				var items = await audibleApiActions.GetAllLibraryItemsAsync(callback);
				var totalCount = items.Count;
				Serilog.Log.Logger.Debug($"GetAllLibraryItems: Total count {totalCount}");

				using var context = DbContexts.GetContext();
				var libImporter = new LibraryImporter(context);
				var newCount = await Task.Run(() => libImporter.Import(items));
				context.SaveChanges();
				Serilog.Log.Logger.Debug($"Import: New count {newCount}");

				await Task.Run(() => SearchEngineCommands.FullReIndex());
				Serilog.Log.Logger.Debug("FullReIndex: success");

				return (totalCount, newCount);
			}
			catch (Exception ex)
			{
				Serilog.Log.Logger.Error(ex, "Error importing library");
				throw;
			}
		}

		public static int UpdateTags(this LibationContext context, Book book, string newTags)
		{
			try
			{
				book.UserDefinedItem.Tags = newTags;

				var qtyChanges = context.SaveChanges();

				if (qtyChanges > 0)
					SearchEngineCommands.UpdateBookTags(book);

				return qtyChanges;
			}
			catch (Exception ex)
			{
				Serilog.Log.Logger.Error(ex, "Error updating tags");
				throw;
			}
		}
	}
}
