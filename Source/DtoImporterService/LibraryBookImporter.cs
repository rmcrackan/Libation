using System;
using System.Collections.Generic;
using System.Linq;
using AudibleUtilities;
using DataLayer;
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

			var currentLibraryProductIds = DbContext.LibraryBooks.Select(l => l.Book.AudibleProductId).ToList();
			var newItems = importItems
				.Where(dto => !currentLibraryProductIds.Contains(dto.DtoItem.ProductId))
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
					newItem.AccountId);
				try
				{
					DbContext.LibraryBooks.Add(libraryBook);
				}
				catch (Exception ex)
				{
					Serilog.Log.Logger.Error(ex, "Error adding library book. {@DebugInfo}", new { libraryBook.Book, libraryBook.Account });
				}
			}

			var qtyNew = hash.Count;
			return qtyNew;
		}
	}
}
