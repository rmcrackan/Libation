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
		public override IEnumerable<Exception> Validate(IEnumerable<Item> items) => new LibraryValidator().Validate(items);

		protected override int DoImport(IEnumerable<Item> items, LibationContext context)
		{
			new BookImporter().Import(items, context);

			var qtyNew = upsertLibraryBooks(items, context);
			return qtyNew;
		}

		private int upsertLibraryBooks(IEnumerable<Item> items, LibationContext context)
		{
			var currentLibraryProductIds = context.Library.Select(l => l.Book.AudibleProductId).ToList();
			var newItems = items.Where(dto => !currentLibraryProductIds.Contains(dto.ProductId)).ToList();

			foreach (var newItem in newItems)
			{
				var libraryBook = new LibraryBook(
					context.Books.Local.Single(b => b.AudibleProductId == newItem.ProductId),
					newItem.DateAdded
// needed for scraping
//,FileManager.FileUtility.RestoreDeclawed(newLibraryDTO.DownloadBookLink)
					);
				context.Library.Add(libraryBook);
			}

			var qtyNew = newItems.Count;
			return qtyNew;
		}
	}
}
