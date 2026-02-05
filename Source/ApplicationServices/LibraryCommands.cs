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
using System.Text;
using System.Threading.Tasks;
using static DtoImporterService.PerfLogger;

namespace ApplicationServices;

public static class LibraryCommands
{
	public static event EventHandler<int>? ScanBegin;
	public static event EventHandler<int>? ScanEnd;

	public static bool Scanning { get; private set; }
	private static object _lock { get; } = new();

	static LibraryCommands()
	{
		ScanBegin += (_, __) => Scanning = true;
		ScanEnd += (_, __) => Scanning = false;
	}

	public static async Task<List<LibraryBook>> FindInactiveBooks(IEnumerable<LibraryBook> existingLibrary, params Account[] accounts)
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
			var libraryItems = await scanAccountsAsync(accounts, libraryOptions);
			logTime($"post {nameof(scanAccountsAsync)} all");

			var totalCount = libraryItems.Count;
			Log.Logger.Information($"GetAllLibraryItems: Total count {totalCount}");

			var missingBookList = existingLibrary.Where(b => !libraryItems.Any(i => i.DtoItem.Asin == b.Book.AudibleProductId)).ToList();

			return missingBookList;
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

	#region FULL LIBRARY scan and import
	public static async Task<(int totalCount, int newCount)> ImportAccountAsync(params Account[]? accounts)
	{
		logRestart();

		if (accounts is null || accounts.Length == 0)
			return (0, 0);

		int newCount = 0;
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
			var importItems = await scanAccountsAsync(accounts, libraryOptions);
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

	public static Task<int> ImportSingleToDbAsync(AudibleApi.Common.Item item, string accountId, string localeName) => Task.Run(() => importSingleToDb(item, accountId, localeName));
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

	private static async Task<List<ImportItem>> scanAccountsAsync(Account[] accounts, LibraryOptions libraryOptions)
	{
		var tasks = new List<Task<List<ImportItem>>>();

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
				var apiExtended = await ApiExtended.CreateAsync(account);

				// add scanAccountAsync as a TASK: do not await
				tasks.Add(scanAccountAsync(apiExtended, account, libraryOptions, archiver));
			}
			catch (Exception ex)
			{
				//Catch to allow other accounts to continue scanning.
				Log.Logger.Error(ex, "Failed to scan account");
			}
		}

		// import library in parallel
		var arrayOfLists = await Task.WhenAll(tasks);
		var importItems = arrayOfLists.SelectMany(a => a).ToList();
		return importItems;
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
	public static Task<int> RemoveBooksAsync(this IEnumerable<LibraryBook?>? idsToRemove) => Task.Run(() => removeBooks(idsToRemove));
	private static int removeBooks(IEnumerable<LibraryBook?>? removeLibraryBooks)
	{
		if (removeLibraryBooks is null || !removeLibraryBooks.Any())
			return 0;

		return DoDbSizeChangeOperation(ctx =>
		{
			// Entry() NoTracking entities before SaveChanges()
			foreach (var lb in removeLibraryBooks.OfType<LibraryBook>())
			{
				lb.IsDeleted = true;
				ctx.Entry(lb).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
			}
		});
	}

	public static Task<int> RestoreBooksAsync(this IEnumerable<LibraryBook> idsToRemove) => Task.Run(() => restoreBooks(idsToRemove));
	private static int restoreBooks(this IEnumerable<LibraryBook> libraryBooks)
	{
		if (libraryBooks is null || !libraryBooks.Any())
			return 0;
		try
		{
			return DoDbSizeChangeOperation(ctx =>
			{
				// Entry() NoTracking entities before SaveChanges()
				foreach (var lb in libraryBooks)
				{
					lb.IsDeleted = false;
					ctx.Entry(lb).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
				}
			});
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
			return DoDbSizeChangeOperation(ctx =>
				{
					ctx.LibraryBooks.RemoveRange(libraryBooks.OfType<LibraryBook>());
					ctx.Books.RemoveRange(libraryBooks.OfType<LibraryBook>().Select(lb => lb.Book));
				});
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
			using (var context = DbContexts.GetContext())
			{
				// Entry() instead of Attach() due to possible stack overflow with large tables
				foreach (var book in nonNullBooks)
				{
					action?.Invoke(book.Book.UserDefinedItem);

					var udiEntity = context.Entry(book.Book.UserDefinedItem);

					udiEntity.State = Microsoft.EntityFrameworkCore.EntityState.Modified;
					if (udiEntity.Reference(udi => udi.Rating).TargetEntry is Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<Rating> ratingEntry)
						ratingEntry.State = Microsoft.EntityFrameworkCore.EntityState.Modified;
				}

				qtyChanges = context.SaveChanges();
			}
			if (qtyChanges > 0)
				BookUserDefinedItemCommitted?.Invoke(null, nonNullBooks);

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

	public static LibraryStats GetCounts(IEnumerable<LibraryBook>? libraryBooks = null)
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

		Log.Logger.Information("Book counts. {@DebugInfo}", new { total = results.Count, booksFullyBackedUp, booksDownloadedOnly, booksNoProgress, booksError, booksUnavailable });

		var pdfResults = libraryBooks
			.AsParallel()
			.Where(lb => lb.Book.HasPdf)
			.Select(lb => new { absent = lb.AbsentFromLastScan, status = Pdf_Status(lb.Book) })
			.ToList();

		var pdfsDownloaded = pdfResults.Count(r => r.status == LiberatedStatus.Liberated);
		var pdfsNotDownloaded = pdfResults.Count(r => !r.absent && r.status == LiberatedStatus.NotLiberated);
		var pdfsUnavailable = pdfResults.Count(r => r.absent && r.status == LiberatedStatus.NotLiberated);

		Log.Logger.Information("PDF counts. {@DebugInfo}", new { total = pdfResults.Count, pdfsDownloaded, pdfsNotDownloaded, pdfsUnavailable });

		return new(booksFullyBackedUp, booksDownloadedOnly, booksNoProgress, booksError, booksUnavailable, pdfsDownloaded, pdfsNotDownloaded, pdfsUnavailable, libraryBooks);
	}
}
