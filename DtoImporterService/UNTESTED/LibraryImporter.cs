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
			var currentLibraryProductIds = DbContext.Library.Select(l => l.Book.AudibleProductId).ToList();
			var newItems = items.Where(dto => !currentLibraryProductIds.Contains(dto.ProductId)).ToList();

			foreach (var newItem in newItems)
			{
				var libraryBook = new LibraryBook(
					DbContext.Books.Local.Single(b => b.AudibleProductId == newItem.ProductId),
					newItem.DateAdded);
				DbContext.Library.Add(libraryBook);
			}

			var qtyNew = newItems.Count;
			return qtyNew;
		}
	}
}
