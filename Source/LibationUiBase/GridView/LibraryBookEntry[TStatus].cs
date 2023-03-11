using DataLayer;
using System;
using System.ComponentModel;

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

		protected override string GetBookTags() => string.Join("\r\n", Book.UserDefinedItem.TagsEnumerated);
	}
}
