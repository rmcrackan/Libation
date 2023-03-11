using DataLayer;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LibationUiBase.GridView
{
	/// <summary>The View Model for a LibraryBook that is ContentType.Parent</summary>
	public class SeriesEntry<TStatus> : GridEntry<TStatus>, ISeriesEntry where TStatus : IEntryStatus
	{
		public List<ILibraryBookEntry> Children { get; }
		public override DateTime DateAdded => Children.Max(c => c.DateAdded);

		private bool suspendCounting = false;
		public void ChildRemoveUpdate()
		{
			if (suspendCounting) return;

			var removeCount = Children.Count(c => c.Remove == true);

			remove = removeCount == 0 ? false : removeCount == Children.Count ? true : null;
			RaisePropertyChanged(nameof(Remove));
		}

		public override bool? Remove
		{
			get => remove;
			set
			{
				remove = value ?? false;

				suspendCounting = true;

				foreach (var item in Children)
					item.Remove = value;

				suspendCounting = false;
				RaisePropertyChanged(nameof(Remove));
			}
		}

		public SeriesEntry(LibraryBook parent, LibraryBook child) : this(parent, new[] { child }) { }
		public SeriesEntry(LibraryBook parent, IEnumerable<LibraryBook> children)
		{
			LastDownload = new();
			SeriesIndex = -1;

			Children = children
				.Select(c => new LibraryBookEntry<TStatus>(c, this))
				.OrderBy(c => c.SeriesIndex)
				.ToList<ILibraryBookEntry>();

			UpdateLibraryBook(parent);
			LoadCover();
		}

		public void RemoveChild(ILibraryBookEntry lbe)
		{
			Children.Remove(lbe);
			PurchaseDate = GetPurchaseDateString();
			Length = GetBookLengthString();
		}

		protected override string GetBookTags() => null;
		protected override int GetLengthInMinutes() => Children.Sum(c => c.LibraryBook.Book.LengthInMinutes);
		protected override DateTime GetPurchaseDate() => Children.Min(c => c.LibraryBook.DateAdded);
	}
}
