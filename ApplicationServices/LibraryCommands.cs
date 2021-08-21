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
	public static class LibraryCommands
	{
		private static LibraryOptions.ResponseGroupOptions LibraryResponseGroups = LibraryOptions.ResponseGroupOptions.ALL_OPTIONS;

		public static async Task<List<LibraryBook>> FindInactiveBooks(Func<Account, ILoginCallback> loginCallbackFactoryFunc, List<LibraryBook> existingLibrary, params Account[] accounts)
        {
			//These are the minimum response groups required for the
			//library scanner to pass all validation and filtering.
			LibraryResponseGroups = 
				LibraryOptions.ResponseGroupOptions.ProductAttrs |
				LibraryOptions.ResponseGroupOptions.ProductDesc | 
				LibraryOptions.ResponseGroupOptions.Relationships;

			if (accounts is null || accounts.Length == 0)
				return new List<LibraryBook>();

			try
			{
				var libraryItems = await scanAccountsAsync(loginCallbackFactoryFunc, accounts);
				Log.Logger.Information($"GetAllLibraryItems: Total count {libraryItems.Count}");

				var missingBookList = existingLibrary.Where(b => !libraryItems.Any(i => i.DtoItem.Asin == b.Book.AudibleProductId)).ToList();

				return missingBookList;
			}
			catch (AudibleApi.Authentication.LoginFailedException lfEx)
			{
				lfEx.SaveFiles(FileManager.Configuration.Instance.LibationFiles);

				// nuget Serilog.Exceptions would automatically log custom properties
				//   However, it comes with a scary warning when used with EntityFrameworkCore which I'm not yet ready to implement:
				//   https://github.com/RehanSaeed/Serilog.Exceptions
				// work-around: use 3rd param. don't just put exception object in 3rd param -- info overload: stack trace, etc
				Log.Logger.Error(lfEx, "Error scanning library. Login failed. {@DebugInfo}", new
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
            finally
            {
				LibraryResponseGroups = LibraryOptions.ResponseGroupOptions.ALL_OPTIONS;
            }
		}
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

			var dtoItems = await AudibleApiActions.GetLibraryValidatedAsync(api, LibraryResponseGroups);
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
		public static int UpdateUserDefinedItem(Book book, string newTags, LiberatedStatus bookStatus, LiberatedStatus? pdfStatus)
		{
			try
			{
				using var context = DbContexts.GetContext();

				var udi = book.UserDefinedItem;

				var tagsChanged = udi.Tags != newTags;
				var bookStatusChanged = udi.BookStatus != bookStatus;
				var pdfStatusChanged = udi.PdfStatus != pdfStatus;

				if (!tagsChanged && !bookStatusChanged && !pdfStatusChanged)
					return 0;

				udi.Tags = newTags;
				udi.BookStatus = bookStatus;
				udi.PdfStatus = pdfStatus;

				// Attach() NoTracking entities before SaveChanges()
				context.Attach(udi).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
				var qtyChanges = context.SaveChanges();

				if (qtyChanges == 0)
					return 0;

				if (tagsChanged)
					SearchEngineCommands.UpdateBookTags(book);
				if (bookStatusChanged || pdfStatusChanged)
					SearchEngineCommands.UpdateLiberatedStatus(book);

				return qtyChanges;
			}
			catch (Exception ex)
			{
				Log.Logger.Error(ex, "Error updating tags");
				throw;
			}
		}

		public static int UpdateBook(LibraryBook libraryBook, LiberatedStatus liberatedStatus)
		{
			try
			{
				using var context = DbContexts.GetContext();

				var udi = libraryBook.Book.UserDefinedItem;

				if (udi.BookStatus == liberatedStatus)
					return 0;

				// Attach() NoTracking entities before SaveChanges()
				udi.BookStatus = liberatedStatus;
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

				if (udi.PdfStatus == liberatedStatus)
					return 0;

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

		public static LiberatedStatus Liberated_Status(Book book)
			=> book.Audio_Exists ? LiberatedStatus.Liberated
			: FileManager.AudibleFileStorage.AaxcExists(book.AudibleProductId) ? LiberatedStatus.PartialDownload
			: LiberatedStatus.NotLiberated;

		public static LiberatedStatus? Pdf_Status(Book book)
			=> !book.Supplements.Any() ? null
			: book.PDF_Exists ? LiberatedStatus.Liberated
			: LiberatedStatus.NotLiberated;

		public record LibraryStats(int booksFullyBackedUp, int booksDownloadedOnly, int booksNoProgress, int pdfsDownloaded, int pdfsNotDownloaded) { }
		public static LibraryStats GetCounts()
		{
			var libraryBooks = DbContexts.GetContext().GetLibrary_Flat_NoTracking();

			var results = libraryBooks
				.AsParallel()
				.Select(lb => Liberated_Status(lb.Book))
				.ToList();
			var booksFullyBackedUp = results.Count(r => r == LiberatedStatus.Liberated);
			var booksDownloadedOnly = results.Count(r => r == LiberatedStatus.PartialDownload);
			var booksNoProgress = results.Count(r => r == LiberatedStatus.NotLiberated);

			Log.Logger.Information("Book counts. {@DebugInfo}", new { total = results.Count, booksFullyBackedUp, booksDownloadedOnly, booksNoProgress });

			var boolResults = libraryBooks
				.AsParallel()
				.Where(lb => lb.Book.Supplements.Any())
				.Select(lb => Pdf_Status(lb.Book))
				.ToList();
			var pdfsDownloaded = boolResults.Count(r => r == LiberatedStatus.Liberated);
			var pdfsNotDownloaded = boolResults.Count(r => r == LiberatedStatus.NotLiberated);

			Log.Logger.Information("PDF counts. {@DebugInfo}", new { total = boolResults.Count, pdfsDownloaded, pdfsNotDownloaded });

			return new(booksFullyBackedUp, booksDownloadedOnly, booksNoProgress, pdfsDownloaded, pdfsNotDownloaded);
		}
	}
}
