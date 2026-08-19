using AudibleApi;
using AudibleUtilities;
using DataLayer;
using Dinah.Core;
using Dinah.Core.Logging;
using DtoImporterService;
using FileManager;
using LibationFileManager;
using Newtonsoft.Json.Linq;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static DtoImporterService.PerfLogger;

namespace ApplicationServices;

public static class LibraryCommands
{
	public static event EventHandler<int>? ScanBegin;
	public static event EventHandler<int>? ScanEnd;

	public static bool Scanning { get; private set; }

	/// <summary>
	/// Serializes library scan and import operations so only one path reads/writes
	/// <see cref="Book"/> / <see cref="LibraryBook"/> rows at a time (prevents duplicate ASIN inserts).
	/// </summary>
	private static readonly SemaphoreSlim ImportGate = new(1, 1);

	static LibraryCommands()
	{
		ScanBegin += (_, __) => Scanning = true;
		ScanEnd += (_, __) => Scanning = false;
	}

	public static async Task<List<LibraryBook>> FindInactiveBooks(IEnumerable<LibraryBook> existingLibrary, params Account[] accounts)
	{
		logRestart();

		if (accounts is null || accounts.Length == 0)
			return new List<LibraryBook>();

		await ImportGate.WaitAsync();
		try
		{
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

			try
			{
				logTime($"pre {nameof(scanAccountsAsync)} all");
				var scan = await scanAccountsAsync(accounts, libraryOptions, allowInteractiveLogin: true);
				logTime($"post {nameof(scanAccountsAsync)} all");

				var libraryItems = scan.Items;
				var totalCount = libraryItems.Count;
				Log.Logger.Information($"GetAllLibraryItems: Total count {totalCount}");

				var existing = existingLibrary?.ToList() ?? [];

				EnsureScanCanIdentifyInactiveBooks(totalCount, existing.Count, scan.FailedAccounts);

				var inactive = existing.Where(b => !libraryItems.Any(i => i.DtoItem.Asin == b.Book.AudibleProductId)).ToList();

				Log.Logger.Information(
					"Books absent from the library scan. {@DebugInfo}",
					new { InactiveCount = inactive.Count, CandidateCount = existing.Count, ScannedCount = totalCount });

				return inactive;
			}
			catch (AudibleApi.Authentication.LoginFailedException lfEx)
			{
				lfEx.SaveFiles(Configuration.Instance.LibationFiles.Location);

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
				ScanEnd?.Invoke(null, 0);
				GC.Collect(GC.MaxGeneration, GCCollectionMode.Aggressive, true, true);
			}
		}
		finally
		{
			ImportGate.Release();
		}
	}

	/// <summary>
	/// Refuse to call anything "no longer in your library" from a scan that could not see the whole library.
	/// Importing tolerates a partial scan because it only adds and updates; removal cannot, because a book
	/// missing from one bad scan is indistinguishable from a book the user no longer owns.
	/// </summary>
	/// <exception cref="LibraryScanIncompleteException">The scan cannot support that decision.</exception>
	public static void EnsureScanCanIdentifyInactiveBooks(int scannedItemCount, int existingBookCount, IReadOnlyCollection<string>? failedAccounts)
	{
		if (failedAccounts?.Count > 0)
			throw LibraryScanIncompleteException.ForFailedAccounts(failedAccounts);

		// An account whose every book vanished at once is a broken scan, not an emptied library.
		if (scannedItemCount == 0 && existingBookCount > 0)
			throw LibraryScanIncompleteException.ForEmptyScan(existingBookCount);
	}

	#region FULL LIBRARY scan and import
	public static Task<(int totalCount, int newCount)> ImportAccountAsync(params Account[]? accounts)
		=> ImportAccountAsync(accounts, allowInteractiveLogin: true);

	public static async Task<(int totalCount, int newCount)> ImportAccountAsync(Account[]? accounts, bool allowInteractiveLogin)
	{
		logRestart();

		if (accounts is null || accounts.Length == 0)
			return (0, 0);

		await ImportGate.WaitAsync();
		int newCount = 0;
		try
		{
			ScanBegin?.Invoke(null, accounts.Length);

			try
			{
				logTime($"pre {nameof(scanAccountsAsync)} all");
				var libraryOptions = new LibraryOptions
				{
					ResponseGroups
					= LibraryOptions.ResponseGroupOptions.Rating | LibraryOptions.ResponseGroupOptions.Media
					| LibraryOptions.ResponseGroupOptions.Relationships | LibraryOptions.ResponseGroupOptions.ProductDesc
					| LibraryOptions.ResponseGroupOptions.Contributors | LibraryOptions.ResponseGroupOptions.ProvidedReview
					| LibraryOptions.ResponseGroupOptions.ProductPlans | LibraryOptions.ResponseGroupOptions.Series
					| LibraryOptions.ResponseGroupOptions.CategoryLadders | LibraryOptions.ResponseGroupOptions.ProductExtendedAttrs
					| LibraryOptions.ResponseGroupOptions.PdfUrl | LibraryOptions.ResponseGroupOptions.OriginAsin
						| LibraryOptions.ResponseGroupOptions.IsFinished,
					ImageSizes = LibraryOptions.ImageSizeOptions._500 | LibraryOptions.ImageSizeOptions._1215
				};
				//Importing only adds and updates, so a partially scanned library is still worth importing.
				var importItems = (await scanAccountsAsync(accounts, libraryOptions, allowInteractiveLogin)).Items;
				logTime($"post {nameof(scanAccountsAsync)} all");

				var totalCount = importItems.Count;
				Log.Logger.Information($"GetAllLibraryItems: Total count {totalCount}");

				if (totalCount == 0)
					return default;

				Log.Logger.Information("Begin long-running import");
				logTime($"pre {nameof(ImportIntoDbAsync)}");
				newCount = await Task.Run(() => ImportIntoDbAsync(importItems));
				logTime($"post {nameof(ImportIntoDbAsync)}");
				Log.Logger.Information($"Import complete. New count {newCount}");

				return (totalCount, newCount);
			}
			catch (AudibleApi.Authentication.LoginFailedException lfEx)
			{
				lfEx.SaveFiles(Configuration.Instance.LibationFiles.Location);

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
				ScanEnd?.Invoke(null, newCount);
				GC.Collect(GC.MaxGeneration, GCCollectionMode.Aggressive, true, true);
			}
		}
		finally
		{
			ImportGate.Release();
		}
	}

	public static async Task<int> ImportSingleToDbAsync(AudibleApi.Common.Item item, string accountId, string localeName)
	{
		await ImportGate.WaitAsync();
		try
		{
			return importSingleToDb(item, accountId, localeName);
		}
		finally
		{
			ImportGate.Release();
		}
	}
	private static int importSingleToDb(AudibleApi.Common.Item item, string accountId, string localeName)
	{
		ArgumentValidator.EnsureNotNull(item, nameof(item));
		ArgumentValidator.EnsureNotNull(accountId, nameof(accountId));
		ArgumentValidator.EnsureNotNull(localeName, nameof(localeName));

		var importItem = new ImportItem(item, accountId, localeName);

		var importItems = new List<ImportItem> { importItem };
		var validator = new LibraryValidator();
		var exceptions = validator.Validate(importItems.Select(i => i.DtoItem));

		if (exceptions?.Any() ?? false)
		{
			Log.Logger.Error(new AggregateException(exceptions), "Error validating library book. {@DebugInfo}", new { item, accountId, localeName });
			return 0;
		}

		return DoDbSizeChangeOperation(ctx =>
		{
			if (importItem.DtoItem.ProductId is null)
				return;

			var bookImporter = new BookImporter(ctx);
			bookImporter.Import(importItems);
			var book = ctx.LibraryBooks.FirstOrDefault(lb => lb.Book.AudibleProductId == importItem.DtoItem.ProductId);

			if (book is null)
			{
				book = new LibraryBook(bookImporter.Cache[importItem.DtoItem.ProductId], importItem.DtoItem.DateAdded, importItem.AccountId);
				ctx.LibraryBooks.Add(book);
			}
			else
			{
				book.AbsentFromLastScan = false;
			}
			book.SetIncludedUntil(importItem.DtoItem.GetExpirationDate());
			book.SetIsAudiblePlus(importItem.DtoItem.IsAyce is true);
		});
	}

	private static LogArchiver? openLogArchive(string? archivePath)
	{
		if (string.IsNullOrWhiteSpace(archivePath))
			return null;

		try
		{
			return new LogArchiver(archivePath);
		}
		catch (System.IO.InvalidDataException)
		{
			try
			{
				Log.Logger.Warning($"Deleting corrupted {nameof(LogArchiver)} at {archivePath}");
				FileUtility.SaferDelete(archivePath);
				return new LogArchiver(archivePath);
			}
			catch (Exception ex)
			{
				Log.Logger.Error(ex, $"Failed to open {nameof(LogArchiver)} at {archivePath}");
			}
		}
		catch (Exception ex)
		{
			Log.Logger.Error(ex, $"Failed to open {nameof(LogArchiver)} at {archivePath}");
		}
		return null;
	}

	/// <summary>Outcome of scanning one or more accounts.</summary>
	/// <param name="Items">Everything the accounts that did scan returned.</param>
	/// <param name="FailedAccounts">
	/// Accounts that could not be scanned. Their books are absent from <paramref name="Items"/> for that reason
	/// alone, so a caller deciding what to remove must not treat the result as the whole library.
	/// </param>
	private sealed record ScanResult(List<ImportItem> Items, IReadOnlyCollection<string> FailedAccounts);

	private static async Task<ScanResult> scanAccountsAsync(Account[] accounts, LibraryOptions libraryOptions, bool allowInteractiveLogin)
	{
		var tasks = new List<Task<List<ImportItem>>>();
		var failedAccounts = new List<string>();

		await using LogArchiver? archiver
			 = Log.Logger.IsDebugEnabled()
			 ? openLogArchive(System.IO.Path.Combine(Configuration.Instance.LibationFiles.Location, "LibraryScans.zip"))
			 : default;

		archiver?.DeleteAllButNewestN(20);

		foreach (var account in accounts)
		{
			try
			{
				// get APIs in serial b/c of logins. do NOT move inside of parallel (Task.WhenAll)
				var apiExtended = await ApiExtended.CreateAsync(account, allowInteractiveLogin);

				// add scanAccountAsync as a TASK: do not await
				tasks.Add(scanAccountAsync(apiExtended, account, libraryOptions, archiver));
			}
			catch (Exception ex) when (!allowInteractiveLogin && AuthenticationExceptionHelper.IsAuthenticationFailure(ex))
			{
				throw;
			}
			catch (Exception ex)
			{
				//Catch to allow other accounts to continue scanning.
				Log.Logger.Error(ex, "Failed to scan account");
				failedAccounts.Add(account.MaskedLogEntry ?? account.AccountId ?? "[unknown account]");
			}
		}

		// import library in parallel
		var arrayOfLists = await Task.WhenAll(tasks);
		var importItems = arrayOfLists.SelectMany(a => a).ToList();
		return new ScanResult(importItems, failedAccounts);
	}

	private static async Task<List<ImportItem>> scanAccountAsync(ApiExtended apiExtended, Account account, LibraryOptions libraryOptions, LogArchiver? archiver)
	{
		ArgumentValidator.EnsureNotNull(account, nameof(account));
		var locale = ArgumentValidator.EnsureNotNull(account.Locale, nameof(account.Locale));

		Log.Logger.Information("ImportLibraryAsync. {@DebugInfo}", new
		{
			Account = account.MaskedLogEntry ?? "[null]"
		});

		logTime($"pre scanAccountAsync {account.AccountName}");

		try
		{
			var dtoItems = await apiExtended.GetLibraryValidatedAsync(libraryOptions);

			logTime($"post scanAccountAsync {account.AccountName} qty: {dtoItems.Count}");

			await logDtoItemsAsync(dtoItems);

			return dtoItems.Select(d => new ImportItem(d, account.AccountId, locale.Name)).ToList();
		}
		catch (ImportValidationException ex)
		{
			await logDtoItemsAsync(ex.Items, ex.InnerExceptions.ToArray());
			//If ImportValidationException is thrown, all Dto items get logged as part of the exception
			throw new AggregateException(ex.InnerExceptions);
		}

		async Task logDtoItemsAsync(IEnumerable<AudibleApi.Common.Item> dtoItems, IEnumerable<Exception>? exceptions = null)
		{
			if (archiver is not null)
			{
				var fileName = $"{DateTime.Now:u} {account.MaskedLogEntry}.json";
				var items = await Task.Run(() => JArray.FromObject(dtoItems.Select(i => i.SourceJson)));

				var scanFile = new JObject
				{
					{ "Account", account.MaskedLogEntry },
					{ "ScannedDateTime", DateTime.Now.ToString("u") },
				};

				if (exceptions?.Any() is true)
					scanFile.Add("Exceptions", JArray.FromObject(exceptions));

				scanFile.Add("Items", items);

				await archiver.AddFileAsync(fileName, scanFile);
			}
		}
	}

	private static async Task<int> ImportIntoDbAsync(List<ImportItem> importItems) => await Task.Run(() => importIntoDb(importItems));
	private static int importIntoDb(List<ImportItem> importItems)
	{
		logTime("importIntoDbAsync -- pre db");

		int newCount = 0;

		DoDbSizeChangeOperation(ctx =>
	{
		var libraryBookImporter = new LibraryBookImporter(ctx);
		newCount = libraryBookImporter.Import(importItems);
		logTime("importIntoDbAsync -- post Import()");
	});
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

	/// <summary>
	/// Record every change to the trash. Removal is a soft delete, so a book can leave the library and sit
	/// out of sight indefinitely; without this the log cannot say when that happened or how much is in there.
	/// </summary>
	private static void logTrashChange(string action, int qtyChanges)
	{
		if (qtyChanges < 1)
			return;

		try
		{
			Log.Logger.Information("Trash bin changed. {@DebugInfo}", new
			{
				Action = action,
				Books = qtyChanges,
				BooksInTrash = DbContexts.GetTrashedBookCount()
			});
		}
		catch (Exception ex)
		{
			//The change itself already succeeded. Never fail it over a log line.
			Log.Logger.Warning(ex, "Trash bin changed ({action}, {qtyChanges}) but the trash count could not be read", action, qtyChanges);
		}
	}

	public static Task<int> RemoveBooksAsync(this IEnumerable<LibraryBook?>? idsToRemove) => Task.Run(() => removeBooks(idsToRemove));
	private static int removeBooks(IEnumerable<LibraryBook?>? removeLibraryBooks)
	{
		if (removeLibraryBooks is null || !removeLibraryBooks.Any())
			return 0;

		var qtyChanges = DoDbSizeChangeOperation(ctx =>
		{
			// Entry() NoTracking entities before SaveChanges()
			foreach (var lb in removeLibraryBooks.OfType<LibraryBook>())
			{
				lb.IsDeleted = true;
				ctx.Entry(lb).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
			}
		});

		logTrashChange("Moved to trash", qtyChanges);
		return qtyChanges;
	}

	public static Task<int> RestoreBooksAsync(this IEnumerable<LibraryBook> idsToRemove) => Task.Run(() => restoreBooks(idsToRemove));
	private static int restoreBooks(this IEnumerable<LibraryBook> libraryBooks)
	{
		if (libraryBooks is null || !libraryBooks.Any())
			return 0;
		try
		{
			var qtyChanges = DoDbSizeChangeOperation(ctx =>
			{
				// Entry() NoTracking entities before SaveChanges()
				foreach (var lb in libraryBooks)
				{
					lb.IsDeleted = false;
					ctx.Entry(lb).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
				}
			});

			logTrashChange("Restored from trash", qtyChanges);
			return qtyChanges;
		}
		catch (Exception ex)
		{
			Log.Logger.Error(ex, "Error restoring books");
			throw;
		}
	}

	public static Task<int> PermanentlyDeleteBooksAsync(this IEnumerable<LibraryBook?>? idsToRemove) => Task.Run(() => permanentlyDeleteBooks(idsToRemove));
	private static int permanentlyDeleteBooks(this IEnumerable<LibraryBook?>? libraryBooks)
	{
		if (libraryBooks is null || !libraryBooks.Any())
			return 0;
		try
		{
			var qtyChanges = DoDbSizeChangeOperation(ctx =>
				{
					ctx.LibraryBooks.RemoveRange(libraryBooks.OfType<LibraryBook>());
					ctx.Books.RemoveRange(libraryBooks.OfType<LibraryBook>().Select(lb => lb.Book));
				});

			logTrashChange("Permanently deleted", qtyChanges);
			return qtyChanges;
		}
		catch (Exception ex)
		{
			Log.Logger.Error(ex, "Error restoring books");
			throw;
		}
	}

	static int DoDbSizeChangeOperation(Action<LibationContext> action)
	{
		try
		{
			int qtyChanges;
			List<LibraryBook>? library;

			using (var context = DbContexts.GetContext())
			{
				action?.Invoke(context);

				qtyChanges = SaveContext(context);
				logTime("importIntoDbAsync -- post SaveChanges");
				library = qtyChanges == 0 ? null : context.GetLibrary_Flat_NoTracking(includeParents: true);
			}

			if (library is not null)
				finalizeLibrarySizeChange(library);
			logTime("importIntoDbAsync -- post finalizeLibrarySizeChange");

			return qtyChanges;
		}
		catch (Exception ex)
		{
			Log.Logger.Error(ex, "Error performing DB Size change operation");
			throw;
		}
	}

	#endregion

	// call this whenever books are added or removed from library
	private static void finalizeLibrarySizeChange(List<LibraryBook> library)
	{
		LibrarySizeChanged?.Invoke(null, library);
	}

	/// <summary>Occurs when the size of the library changes. ie: books are added or removed</summary>
	public static event EventHandler<List<LibraryBook>>? LibrarySizeChanged;

	/// <summary>
	/// Occurs when the size of the library does not change but book(s) details do. Especially when <see cref="UserDefinedItem.Tags"/>, <see cref="UserDefinedItem.BookStatus"/>, or <see cref="UserDefinedItem.PdfStatus"/> changed values are successfully persisted.
	/// </summary>
	public static event EventHandler<IEnumerable<LibraryBook>>? BookUserDefinedItemCommitted;

	#region Update book details
	public static async Task<int> UpdateUserDefinedItemAsync(
		this LibraryBook lb,
		string? tags = null,
		LiberatedStatus? bookStatus = null,
		LiberatedStatus? pdfStatus = null,
		Rating? rating = null)
		=> await UpdateUserDefinedItemAsync([lb], tags, bookStatus, pdfStatus, rating);

	public static async Task<int> UpdateUserDefinedItemAsync(
		this IEnumerable<LibraryBook> lb,
		string? tags = null,
		LiberatedStatus? bookStatus = null,
		LiberatedStatus? pdfStatus = null,
		Rating? rating = null)
		=> await UpdateUserDefinedItemAsync(
			lb,
			udi =>
			{
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

	public static async Task<int> UpdateBookStatusAsync(this LibraryBook lb, LiberatedStatus bookStatus, Version? libationVersion, AudioFormat? audioFormat, string audioVersion)
		=> await lb.UpdateUserDefinedItemAsync(udi => { udi.BookStatus = bookStatus; udi.SetLastDownloaded(libationVersion, audioFormat, audioVersion); });

	public static async Task<int> UpdateBookStatusAsync(this LibraryBook libraryBook, LiberatedStatus bookStatus)
		=> await libraryBook.UpdateUserDefinedItemAsync(udi => udi.BookStatus = bookStatus);
	public static async Task<int> UpdateBookStatusAsync(this IEnumerable<LibraryBook> libraryBooks, LiberatedStatus bookStatus)
		=> await libraryBooks.UpdateUserDefinedItemAsync(udi => udi.BookStatus = bookStatus);

	public static async Task<int> UpdatePdfStatusAsync(this LibraryBook libraryBook, LiberatedStatus pdfStatus)
		=> await libraryBook.UpdateUserDefinedItemAsync(udi => udi.SetPdfStatus(pdfStatus));
	public static async Task<int> UpdatePdfStatusAsync(this IEnumerable<LibraryBook> libraryBooks, LiberatedStatus pdfStatus)
		=> await libraryBooks.UpdateUserDefinedItemAsync(udi => udi.SetPdfStatus(pdfStatus));

	public static async Task<int> UpdateTagsAsync(this LibraryBook libraryBook, string tags)
		=> await libraryBook.UpdateUserDefinedItemAsync(udi => udi.Tags = tags);
	public static async Task<int> UpdateTagsAsync(this IEnumerable<LibraryBook> libraryBooks, string? tags)
		=> await libraryBooks.UpdateUserDefinedItemAsync(udi => udi.Tags = tags ?? string.Empty);

	public static async Task<int> UpdateUserDefinedItemAsync(this LibraryBook libraryBook, Action<UserDefinedItem> action)
			=> await UpdateUserDefinedItemAsync([libraryBook], action);

	public static Task<int> UpdateUserDefinedItemAsync(this IEnumerable<LibraryBook?>? libraryBooks, Action<UserDefinedItem> action)
			=> Task.Run(() => libraryBooks.updateUserDefinedItem(action));

	private static int updateUserDefinedItem(this IEnumerable<LibraryBook?>? libraryBooks, Action<UserDefinedItem> action)
	{
		try
		{
			if (libraryBooks is null || !libraryBooks.Any())
				return 0;

			var nonNullBooks = libraryBooks.OfType<LibraryBook>();
			if (!nonNullBooks.Any())
				return 0;

			int qtyChanges;
			var statusChanged = new List<LibraryBook>();
			using (var context = DbContexts.GetContext())
			{
				// Entry() instead of Attach() due to possible stack overflow with large tables
				foreach (var book in nonNullBooks)
				{
					var statusBefore = book.Book.UserDefinedItem.BookStatus;

					action?.Invoke(book.Book.UserDefinedItem);

					if (book.Book.UserDefinedItem.BookStatus != statusBefore)
						statusChanged.Add(book);

					var udiEntity = context.Entry(book.Book.UserDefinedItem);

					udiEntity.State = Microsoft.EntityFrameworkCore.EntityState.Modified;
					if (udiEntity.Reference(udi => udi.Rating).TargetEntry is Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<Rating> ratingEntry)
						ratingEntry.State = Microsoft.EntityFrameworkCore.EntityState.Modified;
				}

				qtyChanges = context.SaveChanges();
			}
			if (qtyChanges > 0)
			{
				// Changing a title's download status is the user saying they want a different outcome for it,
				// so drop any wait Libation was observing before attempting it again. Compared against the
				// previous value rather than acting on every call: editing tags or a rating must not quietly
				// put a title Audible just refused back into the next scheduled run.
				foreach (var book in statusChanged)
					DownloadAttemptFailureStore.Clear(book);

				BookUserDefinedItemCommitted?.Invoke(null, nonNullBooks);
			}

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
		=> book.AudioExists ? book.UserDefinedItem.BookStatus
		: AudibleFileStorage.AaxcExists(book.AudibleProductId) ? LiberatedStatus.PartialDownload
		: LiberatedStatus.NotLiberated;

	// exists here for feature predictability. It makes sense for this to be where Liberated_Status is
	public static LiberatedStatus? Pdf_Status(Book book) => book.UserDefinedItem.PdfStatus;

	// below are queries, not commands. maybe I should make a LibraryQueries. except there's already one of those...

	public record LibraryStats(int booksFullyBackedUp, int booksDownloadedOnly, int booksNoProgress, int booksError, int booksUnavailable, int pdfsDownloaded, int pdfsNotDownloaded, int pdfsUnavailable, IEnumerable<LibraryBook> LibraryBooks)
	{
		public int PendingBooks => booksNoProgress + booksDownloadedOnly;
		public bool HasPendingBooks => PendingBooks > 0;

		public bool HasBookResults => 0 < (booksFullyBackedUp + booksDownloadedOnly + booksNoProgress + booksError + booksUnavailable);
		public bool HasPdfResults => 0 < (pdfsNotDownloaded + pdfsDownloaded + pdfsUnavailable);

		public string StatusString => HasPdfResults ? $"{toBookStatusString()}  |  {toPdfStatusString()}" : toBookStatusString();

		private string toBookStatusString()
		{
			if (!HasBookResults) return "No books. Begin by importing your library";

			if (!HasPendingBooks && booksError + booksUnavailable == 0) return $"All {"book".PluralizeWithCount(booksFullyBackedUp)} backed up";

			var sb = new StringBuilder($"BACKUPS: No progress: {booksNoProgress}  In process: {booksDownloadedOnly}  Fully backed up: {booksFullyBackedUp}");

			if (booksError > 0)
				sb.Append($"  Errors: {booksError}");
			if (booksUnavailable > 0)
				sb.Append($"  Unavailable: {booksUnavailable}");

			return sb.ToString();
		}

		private string toPdfStatusString()
		{
			if (pdfsNotDownloaded + pdfsUnavailable == 0) return $"All {pdfsDownloaded} PDFs downloaded";

			var sb = new StringBuilder($"PDFs: NOT d/l'ed: {pdfsNotDownloaded}  Downloaded: {pdfsDownloaded}");

			if (pdfsUnavailable > 0)
				sb.Append($"  Unavailable: {pdfsUnavailable}");

			return sb.ToString();
		}
	}

	/// <param name="requestedBy">
	/// Names the caller in the log. Both UIs count the whole library and the visible subset as the
	/// library loads, and the two produce identical lines whenever no filter is applied, so without
	/// this there is no way to tell which set a given count describes.
	/// </param>
	public static LibraryStats GetCounts(IEnumerable<LibraryBook>? libraryBooks = null, [CallerMemberName] string requestedBy = "")
	{
		libraryBooks ??= DbContexts.GetLibrary_Flat_NoTracking();

		var results = libraryBooks
			.AsParallel()
		.WithoutParents()
		.Select(lb => new { absent = lb.AbsentFromLastScan, status = Liberated_Status(lb.Book) })
			.ToList();

		var booksFullyBackedUp = results.Count(r => r.status == LiberatedStatus.Liberated);
		var booksDownloadedOnly = results.Count(r => !r.absent && r.status == LiberatedStatus.PartialDownload);
		var booksNoProgress = results.Count(r => !r.absent && r.status == LiberatedStatus.NotLiberated);
		var booksError = results.Count(r => r.status == LiberatedStatus.Error);
		var booksUnavailable = results.Count(r => r.absent && r.status is LiberatedStatus.NotLiberated or LiberatedStatus.PartialDownload);

		Log.Logger.Information("Book counts for {RequestedBy}. {@DebugInfo}", requestedBy, new { total = results.Count, booksFullyBackedUp, booksDownloadedOnly, booksNoProgress, booksError, booksUnavailable });

		var pdfResults = libraryBooks
			.AsParallel()
			.Where(lb => lb.Book.HasPdf)
			.Select(lb => new { absent = lb.AbsentFromLastScan, status = Pdf_Status(lb.Book) })
			.ToList();

		var pdfsDownloaded = pdfResults.Count(r => r.status == LiberatedStatus.Liberated);
		var pdfsNotDownloaded = pdfResults.Count(r => !r.absent && r.status == LiberatedStatus.NotLiberated);
		// Error means Audible has a supplement listed for the title but will not deliver one, so the count it
		// belongs in is the one for PDFs Libation cannot get. Without this such a title is in no count at all.
		var pdfsUnavailable = pdfResults.Count(r => r.status == LiberatedStatus.Error || (r.absent && r.status == LiberatedStatus.NotLiberated));

		Log.Logger.Information("PDF counts for {RequestedBy}. {@DebugInfo}", requestedBy, new { total = pdfResults.Count, pdfsDownloaded, pdfsNotDownloaded, pdfsUnavailable });

		return new(booksFullyBackedUp, booksDownloadedOnly, booksNoProgress, booksError, booksUnavailable, pdfsDownloaded, pdfsNotDownloaded, pdfsUnavailable, libraryBooks);
	}
}
