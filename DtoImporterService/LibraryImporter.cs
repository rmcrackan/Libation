using System;
using System.Collections.Generic;
using System.Linq;
using AudibleApi.Common;
using DataLayer;
using InternalUtilities;

namespace DtoImporterService
{
	public class LibraryImporter : ItemsImporterBase
	{
		public LibraryImporter(LibationContext context) : base(context) { }

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
			// sqlite cannot alter pk. the work around is an extensive headache. it'll be fixed in pre .net5/efcore5
			//
			// currently, inserting LibraryBook will throw error if the same book is in multiple accounts for the same region.
			//
			// CURRENT SOLUTION: don't re-insert

			var currentLibraryProductIds = DbContext.Library.Select(l => l.Book.AudibleProductId).ToList();
			var newItems = importItems.Where(dto => !currentLibraryProductIds.Contains(dto.DtoItem.ProductId)).ToList();

			foreach (var newItem in newItems)
			{
				var libraryBook = new LibraryBook(
					DbContext.Books.Local.Single(b => b.AudibleProductId == newItem.DtoItem.ProductId),
					newItem.DtoItem.DateAdded,
					newItem.AccountId);
				DbContext.Library.Add(libraryBook);
			}

			// needed for v3 => v4 upgrade
			var toUpdate = DbContext.Library.Where(l => l.Account == null);
			foreach (var u in toUpdate)
			{
				var item = importItems.FirstOrDefault(ii => ii.DtoItem.ProductId == u.Book.AudibleProductId);
				if (item != null)
					u.UpdateAccount(item.AccountId);
			}

			var qtyNew = newItems.Count;
			return qtyNew;
		}
	}
}
