using System;
using System.Collections.Generic;
using System.Linq;
using AudibleUtilities;
using DataLayer;
using Dinah.Core;
using Dinah.Core.Collections.Generic;

namespace DtoImporterService
{
	public class LibraryBookImporter : ItemsImporterBase
	{
		protected override IValidator Validator => new LibraryValidator();

		private BookImporter bookImporter { get; }

		public LibraryBookImporter(LibationContext context) : base(context)
		{
			bookImporter = new BookImporter(DbContext);
		}

		protected override int DoImport(IEnumerable<ImportItem> importItems)
		{
			bookImporter.Import(importItems);

			var qtyNew = upsertLibraryBooks(importItems);
			return qtyNew;
		}

		private int upsertLibraryBooks(IEnumerable<ImportItem> importItems)
		{
			// technically, we should be able to have duplicate books from separate accounts.
			// this would violate the current pk and would be difficult to deal with elsewhere:
			// - what to show in the grid
			// - which to consider liberated
			//
			// sqlite cannot alter pk. the work around is an extensive headache
			// - update: now possible in .net5/efcore5
			//
			// currently, inserting LibraryBook will throw error if the same book is in multiple accounts for the same region.
			//
			// CURRENT SOLUTION: don't re-insert

			var newItems = importItems
				.ExceptBy(DbContext.LibraryBooks.Select(lb => lb.Book.AudibleProductId), imp => imp.DtoItem.ProductId)
				.ToList();

			// if 2 accounts try to import the same book in the same transaction: error since we're only tracking and pulling by asin.
			// just use the first
			var hash = newItems.ToDictionarySafe(dto => dto.DtoItem.ProductId);
			foreach (var kvp in hash)
			{
				var newItem = kvp.Value;

				var libraryBook = new LibraryBook(
					bookImporter.Cache[newItem.DtoItem.ProductId],
					newItem.DtoItem.DateAdded,
					newItem.AccountId)
				{
					AbsentFromLastScan = isPlusTitleUnavailable(newItem)
				};

				try
				{
					DbContext.LibraryBooks.Add(libraryBook);
				}
				catch (Exception ex)
				{
					Serilog.Log.Logger.Error(ex, "Error adding library book. {@DebugInfo}", new { libraryBook.Book, libraryBook.Account });
				}
			}

			//If an existing Book wasn't found in the import, the owning LibraryBook's Book will be null. 
			foreach (var nullBook in DbContext.LibraryBooks.AsEnumerable().Where(lb => lb.Book is null))
				nullBook.AbsentFromLastScan = true;

			//Join importItems on LibraryBooks before iterating over LibraryBooks to avoid
			//quadratic complexity caused by searching all of importItems for each LibraryBook.
			//Join uses hashing, so complexity should approach O(N) instead of O(N^2).
			var items_lbs
			   = importItems
			   .Join(DbContext.LibraryBooks, o => (o.AccountId, o.DtoItem.ProductId), i => (i.Account, i.Book?.AudibleProductId), (o, i) => (o, i));

			foreach ((ImportItem item, LibraryBook lb) in items_lbs)
				lb.AbsentFromLastScan = isPlusTitleUnavailable(item);

			var qtyNew = hash.Count;
			return qtyNew;
		}


		//This SEEMS to work to detect plus titles which are no longer available.
		//I have my doubts it won't yield false negatives, but I have more
		//confidence that it won't yield many/any false positives.
		private static bool isPlusTitleUnavailable(ImportItem item)
			=> item.DtoItem.IsAyce is true
			&& !item.DtoItem.Plans.Any(p => p.PlanName.ContainsInsensitive("Minerva") || p.PlanName.ContainsInsensitive("Free"));
	}
}
