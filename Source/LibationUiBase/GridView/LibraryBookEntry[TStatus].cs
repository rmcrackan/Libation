using DataLayer;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

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

		/// <summary>
		/// Creates <see cref="LibraryBookEntry{TStatus}"/> for all non-episode books in an enumeration of <see cref="LibraryBook"/>.
		/// </summary>
		/// <remarks>Can be called from any thread, but requires the calling thread's <see cref="System.Threading.SynchronizationContext.Current"/> to be valid.</remarks>
		public static async Task<List<IGridEntry>> GetAllProductsAsync(IEnumerable<LibraryBook> libraryBooks)
			=> await GetAllProductsAsync(libraryBooks, lb => lb.Book.IsProduct(), lb => new LibraryBookEntry<TStatus>(lb) as IGridEntry);

		protected override string GetBookTags() => string.Join("\r\n", Book.UserDefinedItem.TagsEnumerated);
	}
}
