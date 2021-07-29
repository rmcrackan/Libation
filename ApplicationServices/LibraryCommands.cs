using System;
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
	// subtly different from DataLayer.LiberatedStatus
	// - DataLayer.LiberatedStatus: has no concept of partially downloaded
	// - ApplicationServices.LiberatedState: has no concept of Error/skipped
	public enum LiberatedState { NotDownloaded, PartialDownload, Liberated }

	public enum PdfState { NoPdf, Downloaded, NotDownloaded }

	public static class LibraryCommands
	{
		#region FULL LIBRARY scan and import
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
				Log.Logger.Error(lfEx, "Error importing library. Login failed. {@DebugInfo}", new
				{
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
		#endregion

		#region Update book details
		public static int UpdateTags(Book book, string newTags)
		{
			try
			{
				using var context = DbContexts.GetContext();

				var udi = book.UserDefinedItem;

				// Attach() NoTracking entities before SaveChanges()
				udi.Tags = newTags;
				context.Attach(udi).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
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

		public static int UpdateBook(LibraryBook libraryBook, LiberatedStatus liberatedStatus, string finalAudioPath)
		{
			try
			{
				using var context = DbContexts.GetContext();

				var udi = libraryBook.Book.UserDefinedItem;

				// Attach() NoTracking entities before SaveChanges()
				udi.BookStatus = liberatedStatus;
				udi.BookLocation = finalAudioPath;
				context.Attach(udi).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
				var qtyChanges = context.SaveChanges();
				if (qtyChanges > 0)
					SearchEngineCommands.UpdateLiberatedStatus(libraryBook.Book);

				return qtyChanges;
			}
			catch (Exception ex)
			{
				Log.Logger.Error(ex, "Error updating tags");
				throw;
			}
		}

		public static int UpdatePdf(LibraryBook libraryBook, LiberatedStatus liberatedStatus)
		{
			try
			{
				using var context = DbContexts.GetContext();

				var udi = libraryBook.Book.UserDefinedItem;

				// Attach() NoTracking entities before SaveChanges()
				udi.PdfStatus = liberatedStatus;
				context.Attach(udi).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
				var qtyChanges = context.SaveChanges();

				return qtyChanges;
			}
			catch (Exception ex)
			{
				Log.Logger.Error(ex, "Error updating tags");
				throw;
			}
		}
		#endregion

		// below are queries, not commands. maybe I should make a LibraryQueries. except there's already one of those...

		public static LiberatedState Liberated_Status(Book book)
			=> TransitionalFileLocator.Audio_Exists(book) ? LiberatedState.Liberated
			: TransitionalFileLocator.AAXC_Exists(book) ? LiberatedState.PartialDownload
			: LiberatedState.NotDownloaded;

		public static PdfState Pdf_Status(Book book)
			=> !book.Supplements.Any() ? PdfState.NoPdf
			: TransitionalFileLocator.PDF_Exists(book) ? PdfState.Downloaded
			: PdfState.NotDownloaded;

		public record LibraryStats(int booksFullyBackedUp, int booksDownloadedOnly, int booksNoProgress, int pdfsDownloaded, int pdfsNotDownloaded) { }
		public static LibraryStats GetCounts()
		{
			var libraryBooks = DbContexts.GetContext().GetLibrary_Flat_NoTracking();

			var results = libraryBooks
				.AsParallel()
				.Select(lb => Liberated_Status(lb.Book))
				.ToList();
			var booksFullyBackedUp = results.Count(r => r == LiberatedState.Liberated);
			var booksDownloadedOnly = results.Count(r => r == LiberatedState.PartialDownload);
			var booksNoProgress = results.Count(r => r == LiberatedState.NotDownloaded);

			Log.Logger.Information("Book counts. {@DebugInfo}", new { total = results.Count, booksFullyBackedUp, booksDownloadedOnly, booksNoProgress });

			var boolResults = libraryBooks
				.AsParallel()
				.Where(lb => lb.Book.Supplements.Any())
				.Select(lb => Pdf_Status(lb.Book))
				.ToList();
			var pdfsDownloaded = boolResults.Count(r => r == PdfState.Downloaded);
			var pdfsNotDownloaded = boolResults.Count(r => r == PdfState.NotDownloaded);

			Log.Logger.Information("PDF counts. {@DebugInfo}", new { total = boolResults.Count, pdfsDownloaded, pdfsNotDownloaded });

			return new(booksFullyBackedUp, booksDownloadedOnly, booksNoProgress, pdfsDownloaded, pdfsNotDownloaded);
		}
	}
}
