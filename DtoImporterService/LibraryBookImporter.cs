using System;
using System.Collections.Generic;
using System.Linq;
using DataLayer;
using InternalUtilities;

namespace DtoImporterService
{
	public class LibraryBookImporter : ItemsImporterBase
	{
		public LibraryBookImporter(LibationContext context) : base(context) { }

		public override IEnumerable<Exception> Validate(IEnumerable<ImportItem> importItems) => new LibraryValidator().Validate(importItems.Select(i => i.DtoItem));

		protected override int DoImport(IEnumerable<ImportItem> importItems)
		{
			new BookImporter(DbContext).Import(importItems);

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
			var newItems = importItems.Where(dto => !currentLibraryProductIds.Contains(dto.DtoItem.ProductId)).ToList();

			foreach (var newItem in newItems)
			{
				var libraryBook = new LibraryBook(
					DbContext.Books.Local.Single(b => b.AudibleProductId == newItem.DtoItem.ProductId),
					newItem.DtoItem.DateAdded,
					newItem.AccountId);
				DbContext.LibraryBooks.Add(libraryBook);
			}

			var qtyNew = newItems.Count;
			return qtyNew;
		}
	}
}
