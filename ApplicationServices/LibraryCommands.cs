﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AudibleApi;
using DataLayer;
using Dinah.Core;
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

				var newCount = await importIntoDbAsync(importItems);
				Log.Logger.Information($"Import: New count {newCount}");

				await Task.Run(() => SearchEngineCommands.FullReIndex());
				Log.Logger.Information("FullReIndex: success");

				return (totalCount, newCount);
			}
			catch (AudibleApi.Authentication.LoginFailedException lfEx)
			{
				lfEx.SaveFiles(FileManager.Configuration.Instance.LibationFiles);

				// nuget Serilog.Exceptions would automatically log custom properties
				//   However, it comes with a scary warning when used with EntityFrameworkCore which I'm not yet ready to implement:
				//   https://github.com/RehanSaeed/Serilog.Exceptions
				// work-around: use 3rd param. don't just put exception object in 3rd param -- info overload: stack trace, etc
				Log.Logger.Error(lfEx, "Error importing library. Login failed. {@DebugInfo}", new {
					lfEx.RequestUrl,
					ResponseStatusCodeNumber = (int)lfEx.ResponseStatusCode,
					ResponseStatusCodeDesc = lfEx.ResponseStatusCode,
					lfEx.ResponseInputFields,
					lfEx.ResponseBodyFilePaths
				});
				throw;
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
			ArgumentValidator.EnsureNotNull(account, nameof(account));

			Log.Logger.Information("ImportLibraryAsync. {@DebugInfo}", new
			{
				Account = account?.MaskedLogEntry ?? "[null]"
			});

			var dtoItems = await AudibleApiActions.GetLibraryValidatedAsync(api);
			return dtoItems.Select(d => new ImportItem { DtoItem = d, AccountId = account.AccountId, LocaleName = account.Locale?.Name }).ToList();
		}

		private static async Task<int> importIntoDbAsync(List<ImportItem> importItems)
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
