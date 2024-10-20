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

			int parallelism = int.Max(1, Environment.ProcessorCount - 1);

			(int numPer, int rem) = int.DivRem(products.Length, parallelism);
			if (rem != 0) numPer++;

			var tasks = new Task<IGridEntry[]>[parallelism];
			var syncContext = SynchronizationContext.Current;

			for (int i = 0; i < parallelism; i++)
			{
				int start = i * numPer;
				tasks[i] = Task.Run(() =>
				{
					SynchronizationContext.SetSynchronizationContext(syncContext);

					int length = int.Min(numPer, products.Length - start);
					if (length < 1) return Array.Empty<IGridEntry>();

					var result = new IGridEntry[length];

					for (int j = 0; j < length; j++)
						result[j] = new LibraryBookEntry<TStatus>(products[start + j]);

					return result;
				});
			}

			return (await Task.WhenAll(tasks)).SelectMany(a => a).ToList();
		}

		public override string AddToPlaylistText => "Add to Playlist";

        protected override string GetBookTags() => string.Join("\r\n", Book.UserDefinedItem.TagsEnumerated);
	}
}
