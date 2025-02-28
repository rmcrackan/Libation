using DataLayer;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Threading;
using System.Linq;

namespace LibationUiBase.GridView
{
	/// <summary>The View Model for a LibraryBook that is ContentType.Product or ContentType.Episode</summary>
	public class LibraryBookEntry<TStatus> : GridEntry<TStatus>, ILibraryBookEntry where TStatus : IEntryStatus
	{
		[Browsable(false)] public override DateTime DateAdded => LibraryBook.DateAdded;
		[Browsable(false)] public ISeriesEntry Parent { get; }

		public override bool? Remove
		{
			get => remove;
			set
			{
				remove = value ?? false;

				Parent?.ChildRemoveUpdate();
				RaisePropertyChanged(nameof(Remove));
			}
		}

		public LibraryBookEntry(LibraryBook libraryBook, ISeriesEntry parent = null)
		{
			Parent = parent;
			UpdateLibraryBook(libraryBook);
			LoadCover();
		}

		public static async Task<List<IGridEntry>> GetAllProductsAsync(IEnumerable<LibraryBook> libraryBooks)
		{
			var products = libraryBooks.Where(lb => lb.Book.IsProduct()).ToArray();
			if (products.Length == 0)
				return [];

			int parallelism = int.Max(1, Environment.ProcessorCount - 1);

			(int batchSize, int rem) = int.DivRem(products.Length, parallelism);
			if (rem != 0) batchSize++;

			var syncContext = SynchronizationContext.Current;

			//Asynchronously create an ILibraryBookEntry for every book in the library
			var tasks = products.Chunk(batchSize).Select(batch => Task.Run(() =>
			{
				SynchronizationContext.SetSynchronizationContext(syncContext);
				return batch.Select(lb => new LibraryBookEntry<TStatus>(lb) as IGridEntry);
			}));

			return (await Task.WhenAll(tasks)).SelectMany(a => a).ToList();
		}

		protected override string GetBookTags() => string.Join("\r\n", Book.UserDefinedItem.TagsEnumerated);
	}
}
