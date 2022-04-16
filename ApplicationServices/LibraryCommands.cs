using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AudibleApi;
using AudibleUtilities;
using DataLayer;
using Dinah.Core;
using DtoImporterService;
using LibationFileManager;
using Serilog;
using static DtoImporterService.PerfLogger;

namespace ApplicationServices
{
	public static class LibraryCommands
	{
		public static async Task<List<LibraryBook>> FindInactiveBooks(Func<Account, Task<ApiExtended>> apiExtendedfunc, List<LibraryBook> existingLibrary, params Account[] accounts)
		{
			logRestart();

			//These are the minimum response groups required for the
			//library scanner to pass all validation and filtering.
			var libraryResponseGroups =
				LibraryOptions.ResponseGroupOptions.ProductAttrs |
				LibraryOptions.ResponseGroupOptions.ProductDesc |
				LibraryOptions.ResponseGroupOptions.Relationships;

			if (accounts is null || accounts.Length == 0)
				return new List<LibraryBook>();

			try
			{
				logTime($"pre {nameof(scanAccountsAsync)} all");
				var libraryItems = await scanAccountsAsync(apiExtendedfunc, accounts, libraryResponseGroups);
				logTime($"post {nameof(scanAccountsAsync)} all");

				var totalCount = libraryItems.Count;
				Log.Logger.Information($"GetAllLibraryItems: Total count {totalCount}");

				var missingBookList = existingLibrary.Where(b => !libraryItems.Any(i => i.DtoItem.Asin == b.Book.AudibleProductId)).ToList();

				return missingBookList;
			}
			catch (AudibleApi.Authentication.LoginFailedException lfEx)
			{
				lfEx.SaveFiles(Configuration.Instance.LibationFiles);

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
				Log.Logger.Error(ex, "Error scanning library");
				throw;
			}
			finally
			{
				stop();
				var putBreakPointHere = logOutput;
			}
		}

		#region FULL LIBRARY scan and import
		public static async Task<(int totalCount, int newCount)> ImportAccountAsync(Func<Account, Task<ApiExtended>> apiExtendedfunc, params Account[] accounts)
		{
			logRestart();

			if (accounts is null || accounts.Length == 0)
				return (0, 0);

			try
			{
				logTime($"pre {nameof(scanAccountsAsync)} all");
				var importItems = await scanAccountsAsync(apiExtendedfunc, accounts, LibraryOptions.ResponseGroupOptions.ALL_OPTIONS);
				logTime($"post {nameof(scanAccountsAsync)} all");

				var totalCount = importItems.Count;
				Log.Logger.Information($"GetAllLibraryItems: Total count {totalCount}");

				Log.Logger.Information("Begin long-running import");
				logTime($"pre {nameof(importIntoDbAsync)}");
				var newCount = await importIntoDbAsync(importItems);
				logTime($"post {nameof(importIntoDbAsync)}");
				Log.Logger.Information($"Import complete. New count {newCount}");

				return (totalCount, newCount);
			}
			catch (AudibleApi.Authentication.LoginFailedException lfEx)
			{
				lfEx.SaveFiles(Configuration.Instance.LibationFiles);

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
			finally
			{
				stop();
				var putBreakPointHere = logOutput;
			}
		}

		private static async Task<List<ImportItem>> scanAccountsAsync(Func<Account, Task<ApiExtended>> apiExtendedfunc, Account[] accounts, LibraryOptions.ResponseGroupOptions libraryResponseGroups)
		{
			var tasks = new List<Task<List<ImportItem>>>();
			foreach (var account in accounts)
			{
				// get APIs in serial b/c of logins. do NOT move inside of parallel (Task.WhenAll)
				var apiExtended = await apiExtendedfunc(account);

				// add scanAccountAsync as a TASK: do not await
				tasks.Add(scanAccountAsync(apiExtended, account, libraryResponseGroups));
			}

			// import library in parallel
			var arrayOfLists = await Task.WhenAll(tasks);
			var importItems = arrayOfLists.SelectMany(a => a).ToList();
			return importItems;
		}

		private static async Task<List<ImportItem>> scanAccountAsync(ApiExtended apiExtended, Account account, LibraryOptions.ResponseGroupOptions libraryResponseGroups)
		{
			ArgumentValidator.EnsureNotNull(account, nameof(account));

			Log.Logger.Information("ImportLibraryAsync. {@DebugInfo}", new
			{
				Account = account?.MaskedLogEntry ?? "[null]"
			});

			logTime($"pre scanAccountAsync {account.AccountName}");

			var dtoItems = await apiExtended.GetLibraryValidatedAsync(libraryResponseGroups, Configuration.Instance.ImportEpisodes);

			logTime($"post scanAccountAsync {account.AccountName} qty: {dtoItems.Count}");

			return dtoItems.Select(d => new ImportItem { DtoItem = d, AccountId = account.AccountId, LocaleName = account.Locale?.Name }).ToList();
		}

		private static async Task<int> importIntoDbAsync(List<ImportItem> importItems)
        {
            logTime("importIntoDbAsync -- pre db");
            using var context = DbContexts.GetContext();
            var libraryBookImporter = new LibraryBookImporter(context);
            var newCount = await Task.Run(() => libraryBookImporter.Import(importItems));
            logTime("importIntoDbAsync -- post Import()");
            int qtyChanges = saveChanges(context);
            logTime("importIntoDbAsync -- post SaveChanges");

            if (qtyChanges > 0)
                await Task.Run(() => finalizeLibrarySizeChange());
            logTime("importIntoDbAsync -- post finalizeLibrarySizeChange");

            return newCount;
        }

        private static int saveChanges(LibationContext context)
		{
			try
			{
				return context.SaveChanges();
			}
			catch (Microsoft.EntityFrameworkCore.DbUpdateException ex)
			{
				// DbUpdateException exceptions can wreck serilog. Condense it until we can find a better solution. I suspect the culpret is the "WithExceptionDetails" serilog extension

				static string format(Exception ex) => $"\r\nMessage: {ex.Message}\r\nStack Trace:\r\n{ex.StackTrace}";

				var msg = "Microsoft.EntityFrameworkCore.DbUpdateException";
				if (ex.InnerException is null)
					throw new Exception($"{msg}{format(ex)}");
				throw new Exception(
					$"{msg}{format(ex)}",
					new Exception($"Inner Exception{format(ex.InnerException)}"));
			}
        }
        #endregion

        #region remove books
        public static Task<List<LibraryBook>> RemoveBooksAsync(List<string> idsToRemove) => Task.Run(() => removeBooks(idsToRemove));
		private static List<LibraryBook> removeBooks(List<string> idsToRemove)
		{
			using var context = DbContexts.GetContext();
			var libBooks = context.GetLibrary_Flat_NoTracking();

			var removeLibraryBooks = libBooks.Where(lb => idsToRemove.Contains(lb.Book.AudibleProductId)).ToList();
			context.LibraryBooks.RemoveRange(removeLibraryBooks);
			context.Books.RemoveRange(removeLibraryBooks.Select(lb => lb.Book));

			var qtyChanges = context.SaveChanges();
			if (qtyChanges > 0)
				finalizeLibrarySizeChange();

			return removeLibraryBooks;
		}
		#endregion

		// call this whenever books are added or removed from library
		private static void finalizeLibrarySizeChange()
		{
			SearchEngineCommands.FullReIndex();
			LibrarySizeChanged?.Invoke(null, null);
		}

		/// <summary>Occurs when books are added or removed from library</summary>
		public static event EventHandler LibrarySizeChanged;

		/// <summary>
		/// Occurs when <see cref="UserDefinedItem.Tags"/>, <see cref="UserDefinedItem.BookStatus"/>, or <see cref="UserDefinedItem.PdfStatus"/>
		/// changed values are successfully persisted.
		/// </summary>
		public static event EventHandler<string> BookUserDefinedItemCommitted;

		#region Update book details
		public static int UpdateUserDefinedItem(Book book)
		{
			try
			{
				using var context = DbContexts.GetContext();

				// Attach() NoTracking entities before SaveChanges()
				context.Attach(book.UserDefinedItem).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
				var qtyChanges = context.SaveChanges();
				if (qtyChanges > 0)
				{
					SearchEngineCommands.UpdateLiberatedStatus(book);
					SearchEngineCommands.UpdateBookTags(book);
					BookUserDefinedItemCommitted?.Invoke(null, book.AudibleProductId);
				}

				return qtyChanges;
			}
			catch (Exception ex)
			{
				Log.Logger.Error(ex, $"Error updating {nameof(book.UserDefinedItem)}");
				throw;
			}
		}
		#endregion

		// must be here instead of in db layer due to AaxcExists
		public static LiberatedStatus Liberated_Status(Book book)
			=> book.Audio_Exists ? book.UserDefinedItem.BookStatus
			: AudibleFileStorage.AaxcExists(book.AudibleProductId) ? LiberatedStatus.PartialDownload
			: LiberatedStatus.NotLiberated;

		// exists here for feature predictability. It makes sense for this to be where Liberated_Status is
		public static LiberatedStatus? Pdf_Status(Book book) => book.UserDefinedItem.PdfStatus;

		// below are queries, not commands. maybe I should make a LibraryQueries. except there's already one of those...

		public record LibraryStats(int booksFullyBackedUp, int booksDownloadedOnly, int booksNoProgress, int booksError, int pdfsDownloaded, int pdfsNotDownloaded) { }
		public static LibraryStats GetCounts()
		{
			var libraryBooks = DbContexts.GetLibrary_Flat_NoTracking();

			var results = libraryBooks
				.AsParallel()
				.Select(lb => Liberated_Status(lb.Book))
				.ToList();
			var booksFullyBackedUp = results.Count(r => r == LiberatedStatus.Liberated);
			var booksDownloadedOnly = results.Count(r => r == LiberatedStatus.PartialDownload);
			var booksNoProgress = results.Count(r => r == LiberatedStatus.NotLiberated);
			var booksError = results.Count(r => r == LiberatedStatus.Error);

			Log.Logger.Information("Book counts. {@DebugInfo}", new { total = results.Count, booksFullyBackedUp, booksDownloadedOnly, booksNoProgress, booksError });

			var boolResults = libraryBooks
				.AsParallel()
				.Where(lb => lb.Book.HasPdf)
				.Select(lb => Pdf_Status(lb.Book))
				.ToList();
			var pdfsDownloaded = boolResults.Count(r => r == LiberatedStatus.Liberated);
			var pdfsNotDownloaded = boolResults.Count(r => r == LiberatedStatus.NotLiberated);

			Log.Logger.Information("PDF counts. {@DebugInfo}", new { total = boolResults.Count, pdfsDownloaded, pdfsNotDownloaded });

			return new(booksFullyBackedUp, booksDownloadedOnly, booksNoProgress, booksError, pdfsDownloaded, pdfsNotDownloaded);
		}
	}
}
