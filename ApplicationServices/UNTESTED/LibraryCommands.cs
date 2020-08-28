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
		public static async Task<(int totalCount, int newCount)> ImportAccountAsync(Func<Account, ILoginCallback> loginCallbackFactoryFunc, params Account[] accounts)
		{
			if (accounts is null || accounts.Length == 0)
				return (0, 0);

			try
			{
				var importItems = await scanAccountsAsync(loginCallbackFactoryFunc, accounts);

				var totalCount = importItems.Count;
				Log.Logger.Information($"GetAllLibraryItems: Total count {totalCount}");

				var newCount = await getNewCountAsync(importItems);
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

		private static async Task<List<ImportItem>> scanAccountsAsync(Func<Account, ILoginCallback> loginCallbackFactoryFunc, Account[] accounts)
		{
			var tasks = new List<Task<List<ImportItem>>>();
			foreach (var account in accounts)
			{
				var callback = loginCallbackFactoryFunc(account);

				// get APIs in serial, esp b/c of logins
				var api = await AudibleApiActions.GetApiAsync(callback, account);

				// add scanAccountAsync as a TASK: do not await
				tasks.Add(scanAccountAsync(api, account));
			}

			// import library in parallel
			var arrayOfLists = await Task.WhenAll(tasks);
			var importItems = arrayOfLists.SelectMany(a => a).ToList();
			return importItems;
		}

		private static async Task<List<ImportItem>> scanAccountAsync(Api api, Account account)
		{
			Dinah.Core.ArgumentValidator.EnsureNotNull(account, nameof(account));

			var localeName = account.Locale?.Name;
			Log.Logger.Information("ImportLibraryAsync. {@DebugInfo}", new
			{
				account.AccountName,
				account.AccountId,
				LocaleName = localeName,
			});

			var dtoItems = await AudibleApiActions.GetLibraryValidatedAsync(api);
			return dtoItems.Select(d => new ImportItem { DtoItem = d, AccountId = account.AccountId, LocaleName = localeName }).ToList();
		}

		private static async Task<int> getNewCountAsync(List<ImportItem> importItems)
		{
			using var context = DbContexts.GetContext();
			var libraryImporter = new LibraryImporter(context);
			var newCount = await Task.Run(() => libraryImporter.Import(importItems));
			context.SaveChanges();

			return newCount;
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
