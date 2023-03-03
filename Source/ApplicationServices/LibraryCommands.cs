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
        public static event EventHandler<int> ScanBegin;
        public static event EventHandler ScanEnd;

        public static bool Scanning { get; private set; }
        private static object _lock { get; } = new();

        static LibraryCommands()
        {
            ScanBegin += (_, __) => Scanning = true;
            ScanEnd += (_, __) => Scanning = false;
        }

        public static async Task<List<LibraryBook>> FindInactiveBooks(Func<Account, Task<ApiExtended>> apiExtendedfunc, IEnumerable<LibraryBook> existingLibrary, params Account[] accounts)
        {
            logRestart();

            lock (_lock)
            {
                if (Scanning)
                    return new();
            }
            ScanBegin?.Invoke(null, accounts.Length);

            //These are the minimum response groups required for the
            //library scanner to pass all validation and filtering.
            var libraryOptions = new LibraryOptions
            {
                ResponseGroups
                 = LibraryOptions.ResponseGroupOptions.ProductAttrs
                 | LibraryOptions.ResponseGroupOptions.ProductDesc
                 | LibraryOptions.ResponseGroupOptions.Relationships
            };
            if (accounts is null || accounts.Length == 0)
                return new List<LibraryBook>();

            try
            {
                logTime($"pre {nameof(scanAccountsAsync)} all");
                var libraryItems = await scanAccountsAsync(apiExtendedfunc, accounts, libraryOptions);
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
                ScanEnd?.Invoke(null, null);
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
                lock (_lock)
                {
                    if (Scanning)
                        return (0, 0);
                }
                ScanBegin?.Invoke(null, accounts.Length);

                logTime($"pre {nameof(scanAccountsAsync)} all");
                var libraryOptions = new LibraryOptions
                {
                    ResponseGroups = LibraryOptions.ResponseGroupOptions.ALL_OPTIONS,
                    ImageSizes = LibraryOptions.ImageSizeOptions._500 | LibraryOptions.ImageSizeOptions._1215
                };
                var importItems = await scanAccountsAsync(apiExtendedfunc, accounts, libraryOptions);
                logTime($"post {nameof(scanAccountsAsync)} all");

                var totalCount = importItems.Count;
                Log.Logger.Information($"GetAllLibraryItems: Total count {totalCount}");

                if (totalCount == 0)
                    return default;

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
                ScanEnd?.Invoke(null, null);
            }
        }

        private static async Task<List<ImportItem>> scanAccountsAsync(Func<Account, Task<ApiExtended>> apiExtendedfunc, Account[] accounts, LibraryOptions libraryOptions)
        {
            var tasks = new List<Task<List<ImportItem>>>();
            foreach (var account in accounts)
            {
                // get APIs in serial b/c of logins. do NOT move inside of parallel (Task.WhenAll)
                var apiExtended = await apiExtendedfunc(account);

                // add scanAccountAsync as a TASK: do not await
                tasks.Add(scanAccountAsync(apiExtended, account, libraryOptions));
            }

            // import library in parallel
            var arrayOfLists = await Task.WhenAll(tasks);
            var importItems = arrayOfLists.SelectMany(a => a).ToList();
            return importItems;
        }

        private static async Task<List<ImportItem>> scanAccountAsync(ApiExtended apiExtended, Account account, LibraryOptions libraryOptions)
        {
            ArgumentValidator.EnsureNotNull(account, nameof(account));

            Log.Logger.Information("ImportLibraryAsync. {@DebugInfo}", new
            {
                Account = account?.MaskedLogEntry ?? "[null]"
            });

            logTime($"pre scanAccountAsync {account.AccountName}");

            var dtoItems = await apiExtended.GetLibraryValidatedAsync(libraryOptions, Configuration.Instance.ImportEpisodes);

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
            int qtyChanges = SaveContext(context);
            logTime("importIntoDbAsync -- post SaveChanges");

            // this is any changes at all to the database, not just new books
            if (qtyChanges > 0)
                await Task.Run(() => finalizeLibrarySizeChange());
            logTime("importIntoDbAsync -- post finalizeLibrarySizeChange");

            return newCount;
        }

        public static int SaveContext(LibationContext context)
        {
            try
            {
                return context.SaveChanges();
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException ex)
            {
                // DbUpdateException exceptions can wreck serilog. Condense it until we can find a better solution. I suspect the culprit is the "WithExceptionDetails" serilog extension

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

        #region remove/restore books
        public static Task<int> RemoveBooksAsync(this IEnumerable<LibraryBook> idsToRemove) => Task.Run(() => removeBooks(idsToRemove));
        public static int RemoveBook(this LibraryBook idToRemove) => removeBooks(new[] { idToRemove });
        private static int removeBooks(IEnumerable<LibraryBook> removeLibraryBooks)
        {
            try
            {
                if (removeLibraryBooks is null || !removeLibraryBooks.Any())
                    return 0;

                using var context = DbContexts.GetContext();

                // Attach() NoTracking entities before SaveChanges()
                foreach (var lb in removeLibraryBooks)
                {
                    lb.IsDeleted = true;
                    context.Attach(lb).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                }

                var qtyChanges = context.SaveChanges();
                if (qtyChanges > 0)
                    finalizeLibrarySizeChange();

                return qtyChanges;
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "Error removing books");
                throw;
            }
        }

        public static int RestoreBooks(this IEnumerable<LibraryBook> libraryBooks)
        {
            try
            {
                if (libraryBooks is null || !libraryBooks.Any())
                    return 0;

                using var context = DbContexts.GetContext();

                // Attach() NoTracking entities before SaveChanges()
                foreach (var lb in libraryBooks)
                {
                    lb.IsDeleted = false;
                    context.Attach(lb).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                }

                var qtyChanges = context.SaveChanges();
                if (qtyChanges > 0)
                    finalizeLibrarySizeChange();

                return qtyChanges;
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "Error restoring books");
                throw;
            }
        }

        public static int PermanentlyDeleteBooks(this IEnumerable<LibraryBook> libraryBooks)
        {
            try
            {
                if (libraryBooks is null || !libraryBooks.Any())
                    return 0;

                using var context = DbContexts.GetContext();

				context.LibraryBooks.RemoveRange(libraryBooks);
				context.Books.RemoveRange(libraryBooks.Select(lb => lb.Book));				

                var qtyChanges = context.SaveChanges();
				if (qtyChanges > 0)
					finalizeLibrarySizeChange();

				return qtyChanges;
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "Error restoring books");
                throw;
            }
        }
        #endregion

        // call this whenever books are added or removed from library
        private static void finalizeLibrarySizeChange() => LibrarySizeChanged?.Invoke(null, null);

        /// <summary>Occurs when the size of the library changes. ie: books are added or removed</summary>
        public static event EventHandler LibrarySizeChanged;

        /// <summary>
        /// Occurs when the size of the library does not change but book(s) details do. Especially when <see cref="UserDefinedItem.Tags"/>, <see cref="UserDefinedItem.BookStatus"/>, or <see cref="UserDefinedItem.PdfStatus"/> changed values are successfully persisted.
        /// </summary>
        public static event EventHandler<IEnumerable<Book>> BookUserDefinedItemCommitted;

        #region Update book details
        public static int UpdateUserDefinedItem(
            this Book book,
            string tags = null,
            LiberatedStatus? bookStatus = null,
            LiberatedStatus? pdfStatus = null,
            Rating rating = null)
            => new[] { book }.UpdateUserDefinedItem(tags, bookStatus, pdfStatus, rating);

        public static int UpdateUserDefinedItem(
            this IEnumerable<Book> books,
            string tags = null,
            LiberatedStatus? bookStatus = null,
            LiberatedStatus? pdfStatus = null,
            Rating rating = null)
            => updateUserDefinedItem(
                books,
                udi => {
                    // blank tags are expected. null tags are not
                    if (tags is not null)
                        udi.Tags = tags;

                    if (bookStatus.HasValue)
                        udi.BookStatus = bookStatus.Value;

                    // method handles null logic
                    udi.SetPdfStatus(pdfStatus);

                    if (rating is not null)
                        udi.UpdateRating(rating.OverallRating, rating.PerformanceRating, rating.StoryRating);
				});

        public static int UpdateBookStatus(this Book book, LiberatedStatus bookStatus, Version libationVersion)
            => book.UpdateUserDefinedItem(udi => { udi.BookStatus = bookStatus; udi.SetLastDownloaded(libationVersion); });
        public static int UpdateBookStatus(this Book book, LiberatedStatus bookStatus)
            => book.UpdateUserDefinedItem(udi => udi.BookStatus = bookStatus);
        public static int UpdateBookStatus(this IEnumerable<Book> books, LiberatedStatus bookStatus)
            => books.UpdateUserDefinedItem(udi => udi.BookStatus = bookStatus);
        public static int UpdateBookStatus(this LibraryBook libraryBook, LiberatedStatus bookStatus)
            => libraryBook.UpdateUserDefinedItem(udi => udi.BookStatus = bookStatus);
        public static int UpdateBookStatus(this IEnumerable<LibraryBook> libraryBooks, LiberatedStatus bookStatus)
            => libraryBooks.UpdateUserDefinedItem(udi => udi.BookStatus = bookStatus);

        public static int UpdatePdfStatus(this Book book, LiberatedStatus pdfStatus)
            => book.UpdateUserDefinedItem(udi => udi.SetPdfStatus(pdfStatus));
        public static int UpdatePdfStatus(this IEnumerable<Book> books, LiberatedStatus pdfStatus)
            => books.UpdateUserDefinedItem(udi => udi.SetPdfStatus(pdfStatus));
        public static int UpdatePdfStatus(this LibraryBook libraryBook, LiberatedStatus pdfStatus)
            => libraryBook.UpdateUserDefinedItem(udi => udi.SetPdfStatus(pdfStatus));
        public static int UpdatePdfStatus(this IEnumerable<LibraryBook> libraryBooks, LiberatedStatus pdfStatus)
            => libraryBooks.UpdateUserDefinedItem(udi => udi.SetPdfStatus(pdfStatus));

        public static int UpdateTags(this Book book, string tags)
            => book.UpdateUserDefinedItem(udi => udi.Tags = tags);
        public static int UpdateTags(this IEnumerable<Book> books, string tags)
            => books.UpdateUserDefinedItem(udi => udi.Tags = tags);
        public static int UpdateTags(this LibraryBook libraryBook, string tags)
            => libraryBook.UpdateUserDefinedItem(udi => udi.Tags = tags);
        public static int UpdateTags(this IEnumerable<LibraryBook> libraryBooks, string tags)
            => libraryBooks.UpdateUserDefinedItem(udi => udi.Tags = tags);

        public static int UpdateUserDefinedItem(this LibraryBook libraryBook, Action<UserDefinedItem> action)
            => libraryBook.Book.updateUserDefinedItem(action);
        public static int UpdateUserDefinedItem(this IEnumerable<LibraryBook> libraryBooks, Action<UserDefinedItem> action)
            => libraryBooks.Select(lb => lb.Book).updateUserDefinedItem(action);

        public static int UpdateUserDefinedItem(this Book book, Action<UserDefinedItem> action) => book.updateUserDefinedItem(action);
        public static int UpdateUserDefinedItem(this IEnumerable<Book> books, Action<UserDefinedItem> action) => books.updateUserDefinedItem(action);

        private static int updateUserDefinedItem(this Book book, Action<UserDefinedItem> action) => new[] { book }.updateUserDefinedItem(action);
        private static int updateUserDefinedItem(this IEnumerable<Book> books, Action<UserDefinedItem> action)
        {
            try
            {
                if (books is null || !books.Any())
                    return 0;

                foreach (var book in books)
                    action?.Invoke(book.UserDefinedItem);

                using var context = DbContexts.GetContext();

                // Attach() NoTracking entities before SaveChanges()
                foreach (var book in books)
                {
                    context.Attach(book.UserDefinedItem).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                    context.Attach(book.UserDefinedItem.Rating).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                }

                var qtyChanges = context.SaveChanges();
                if (qtyChanges > 0)
                    BookUserDefinedItemCommitted?.Invoke(null, books);

                return qtyChanges;
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, $"Error updating {nameof(Book.UserDefinedItem)}");
                throw;
            }
        }
        #endregion

        // must be here instead of in db layer due to AaxcExists
        public static LiberatedStatus Liberated_Status(Book book)
            => book.Audio_Exists() ? book.UserDefinedItem.BookStatus
            : AudibleFileStorage.AaxcExists(book.AudibleProductId) ? LiberatedStatus.PartialDownload
            : LiberatedStatus.NotLiberated;

        // exists here for feature predictability. It makes sense for this to be where Liberated_Status is
        public static LiberatedStatus? Pdf_Status(Book book) => book.UserDefinedItem.PdfStatus;

        // below are queries, not commands. maybe I should make a LibraryQueries. except there's already one of those...

        public record LibraryStats(int booksFullyBackedUp, int booksDownloadedOnly, int booksNoProgress, int booksError, int pdfsDownloaded, int pdfsNotDownloaded)
        {
            public int PendingBooks => booksNoProgress + booksDownloadedOnly;
            public bool HasPendingBooks => PendingBooks > 0;

            public bool HasBookResults => 0 < (booksFullyBackedUp + booksDownloadedOnly + booksNoProgress + booksError);
            public bool HasPdfResults => 0 < (pdfsNotDownloaded + pdfsDownloaded);
        }
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
                .Where(lb => lb.Book.HasPdf())
                .Select(lb => Pdf_Status(lb.Book))
                .ToList();
            var pdfsDownloaded = boolResults.Count(r => r == LiberatedStatus.Liberated);
            var pdfsNotDownloaded = boolResults.Count(r => r == LiberatedStatus.NotLiberated);

            Log.Logger.Information("PDF counts. {@DebugInfo}", new { total = boolResults.Count, pdfsDownloaded, pdfsNotDownloaded });

            return new(booksFullyBackedUp, booksDownloadedOnly, booksNoProgress, booksError, pdfsDownloaded, pdfsNotDownloaded);
        }
    }
}
