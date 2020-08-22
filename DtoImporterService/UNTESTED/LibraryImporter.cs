using System;
using System.Collections.Generic;
using System.Linq;
using AudibleApiDTOs;
using DataLayer;
using InternalUtilities;

namespace DtoImporterService
{
	public class LibraryImporter : ItemsImporterBase
	{
		public LibraryImporter(LibationContext context, Account account) : base(context, account) { }

		public override IEnumerable<Exception> Validate(IEnumerable<Item> items) => new LibraryValidator().Validate(items);

		protected override int DoImport(IEnumerable<Item> items)
		{
			new BookImporter(DbContext, Account).Import(items);

			var qtyNew = upsertLibraryBooks(items);
			return qtyNew;
		}

		private int upsertLibraryBooks(IEnumerable<Item> items)
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
			var newItems = items.Where(dto => !currentLibraryProductIds.Contains(dto.ProductId)).ToList();

			foreach (var newItem in newItems)
			{
				var libraryBook = new LibraryBook(
					DbContext.Books.Local.Single(b => b.AudibleProductId == newItem.ProductId),
					newItem.DateAdded,
					Account.AccountId);
				DbContext.Library.Add(libraryBook);
			}

			// needed for v3 => v4 upgrade
			var toUpdate = DbContext.Library.Where(l => l.Account == null);
			foreach (var u in toUpdate)
				u.UpdateAccount(Account.AccountId);

			var qtyNew = newItems.Count;
			return qtyNew;
		}
	}
}
