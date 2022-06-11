using DataLayer;
using Dinah.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace LibationWinForms.GridView
{
	/// <summary>The View Model for a LibraryBook that is ContentType.Parent</summary>
	public class SeriesEntry : GridEntry
	{
		[Browsable(false)] public List<LibraryBookEntry> Children { get; }
		[Browsable(false)] public override DateTime DateAdded => Children.Max(c => c.DateAdded);

		private bool suspendCounting = false;
		public void ChildRemoveUpdate()
		{
			if (suspendCounting) return;

			var removeCount = Children.Count(c => c.Remove is RemoveStatus.Removed);

			if (removeCount == 0)
				_remove = RemoveStatus.NotRemoved;
			else if (removeCount == Children.Count)
				_remove = RemoveStatus.Removed;
			else
				_remove = RemoveStatus.SomeRemoved;
			NotifyPropertyChanged(nameof(Remove));
		}

		#region Model properties exposed to the view
		public override RemoveStatus Remove
		{
			get
			{
				return _remove;
			}
			set
			{
				_remove = value is RemoveStatus.SomeRemoved ? RemoveStatus.NotRemoved : value;

				suspendCounting = true;

				foreach (var item in Children)
					item.Remove = value;

				suspendCounting = false;

				NotifyPropertyChanged();
			}
		}

		public override LiberateButtonStatus Liberate { get; }
		public override string DisplayTags { get; } = string.Empty;

		#endregion

		private SeriesEntry(LibraryBook parent)
		{
			Liberate = new LiberateButtonStatus { IsSeries = true };
			SeriesIndex = -1;
			LibraryBook = parent;
			LoadCover();
		}

		public SeriesEntry(LibraryBook parent, IEnumerable<LibraryBook> children) : this(parent)
		{
			Children = children
				.Select(c => new LibraryBookEntry(c) { Parent = this })
				.OrderBy(c => c.SeriesIndex)
				.ToList();
			UpdateSeries(parent);
		}

		public SeriesEntry(LibraryBook parent, LibraryBook child) : this(parent)
		{
			Children = new() { new LibraryBookEntry(child) { Parent = this } };
			UpdateSeries(parent);
		}

		public void UpdateSeries(LibraryBook parent)
		{
			LibraryBook = parent;

			Title = Book.Title;
			Series = Book.SeriesNames();
			MyRating = Book.UserDefinedItem.Rating?.ToStarString()?.DefaultIfNullOrWhiteSpace("");
			PurchaseDate = Children.Min(c => c.LibraryBook.DateAdded).ToString("d");
			ProductRating = Book.Rating?.ToStarString()?.DefaultIfNullOrWhiteSpace("");
			Authors = Book.AuthorNames();
			Narrators = Book.NarratorNames();
			Category = string.Join(" > ", Book.CategoriesNames());
			Misc = GetMiscDisplay(LibraryBook);
			LongDescription = GetDescriptionDisplay(Book);
			Description = TrimTextToWord(LongDescription, 62);

			int bookLenMins = Children.Sum(c => c.LibraryBook.Book.LengthInMinutes);
			Length = bookLenMins == 0 ? "" : $"{bookLenMins / 60} hr {bookLenMins % 60} min";

			NotifyPropertyChanged();
		}

		#region Data Sorting

		/// <summary>Create getters for all member object values by name</summary>
		protected override Dictionary<string, Func<object>> CreateMemberValueDictionary() => new()
		{
			{ nameof(Title), () => Book.TitleSortable() },
			{ nameof(Series), () => Book.SeriesSortable() },
			{ nameof(Length), () => Children.Sum(c => c.LibraryBook.Book.LengthInMinutes) },
			{ nameof(MyRating), () => Book.UserDefinedItem.Rating.FirstScore() },
			{ nameof(PurchaseDate), () => Children.Min(c => c.LibraryBook.DateAdded) },
			{ nameof(ProductRating), () => Book.Rating.FirstScore() },
			{ nameof(Authors), () => Authors },
			{ nameof(Narrators), () => Narrators },
			{ nameof(Description), () => Description },
			{ nameof(Category), () => Category },
			{ nameof(Misc), () => Misc },
			{ nameof(DisplayTags), () => string.Empty },
			{ nameof(Liberate), () => Liberate },
			{ nameof(DateAdded), () => DateAdded },
		};

		#endregion
	}
}
