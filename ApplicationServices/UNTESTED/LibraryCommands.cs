using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AudibleApi;
using DataLayer;
using DtoImporterService;
using InternalUtilities;
using Serilog;

namespace ApplicationServices
{
	public static class LibraryCommands
	{
//		public static async Task<(int totalCount, int newCount)> ImportAccountsAsync(IEnumerable<Account> accounts, ILoginCallback callback)
//		{
////throw new NotImplementedException();
////			foreach (var account in accounts)
////			{
////			}
//		}
		public static async Task<(int totalCount, int newCount)> ImportAccountAsync(Account account, ILoginCallback callback)
		{
			try
			{
				Log.Logger.Information("ImportLibraryAsync. {@DebugInfo}", new
				{
					account.AccountName,
					account.AccountId,
					LocaleName = account.Locale.Name,
				});

				var dtoItems = await AudibleApiActions.GetAllLibraryItemsAsync(account, callback);
				var items = dtoItems.Select(d => new ImportItem { DtoItem = d, Account = account }).ToList();

				var totalCount = items.Count;
				Log.Logger.Information($"GetAllLibraryItems: Total count {totalCount}");

				using var context = DbContexts.GetContext();
				var libraryImporter = new LibraryImporter(context);
				var newCount = await Task.Run(() => libraryImporter.Import(items));
				context.SaveChanges();
				Log.Logger.Information($"Import: New count {newCount}");

				await Task.Run(() => SearchEngineCommands.FullReIndex());
				Log.Logger.Information("FullReIndex: success");

				return (totalCount, newCount);
			}
			catch (Exception ex)
			{
				Log.Logger.Error(ex, "Error importing library");
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
				Log.Logger.Error(ex, "Error updating tags");
				throw;
			}
		}
	}
}
