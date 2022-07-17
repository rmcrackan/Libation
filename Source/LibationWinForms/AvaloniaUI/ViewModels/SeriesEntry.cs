using Avalonia.Media;
using DataLayer;
using Dinah.Core;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace LibationWinForms.AvaloniaUI.ViewModels
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

			var removeCount = Children.Count(c => c.Remove == true);

			_remove = removeCount == 0 ? false : (removeCount == Children.Count ? true : null);
			this.RaisePropertyChanged(nameof(Remove));
		}

		#region Model properties exposed to the view
		public override bool? Remove
		{
			get => _remove;
			set
			{
				_remove = value ?? false;

				suspendCounting = true;

				foreach (var item in Children)
					item.Remove = value;

				suspendCounting = false;
				this.RaisePropertyChanged(nameof(Remove));
			}
		}

		public override LiberateButtonStatus Liberate { get; }
		public override BookTags BookTags { get; } = new();

		public override bool IsSeries => true;
		public override bool IsEpisode => false;
		public override bool IsBook => false;

		#endregion

		public SeriesEntry(LibraryBook parent, IEnumerable<LibraryBook> children)
		{
			Liberate = new LiberateButtonStatus(IsSeries) { Expanded = true };
			SeriesIndex = -1;
			LibraryBook = parent;

			LoadCover();

			Children = children
				.Select(c => new LibraryBookEntry(c) { Parent = this })
				.OrderBy(c => c.SeriesIndex)
				.ToList();

			Title = Book.Title;
			Series = Book.SeriesNames();
			MyRating = Book.UserDefinedItem.Rating?.ToStarString()?.DefaultIfNullOrWhiteSpace("");
			ProductRating = Book.Rating?.ToStarString()?.DefaultIfNullOrWhiteSpace("");
			Authors = Book.AuthorNames();
			Narrators = Book.NarratorNames();
			Category = string.Join(" > ", Book.CategoriesNames());
			Misc = GetMiscDisplay(LibraryBook);
			LongDescription = GetDescriptionDisplay(Book);
			Description = TrimTextToWord(LongDescription, 62);

			PurchaseDate = Children.Min(c => c.LibraryBook.DateAdded).ToString("d");
			int bookLenMins = Children.Sum(c => c.LibraryBook.Book.LengthInMinutes);
			Length = bookLenMins == 0 ? "" : $"{bookLenMins / 60} hr {bookLenMins % 60} min";
		}


		#region Data Sorting

		/// <summary>Create getters for all member object values by name</summary>
		protected override Dictionary<string, Func<object>> CreateMemberValueDictionary() => new()
		{
			{ nameof(Remove), () => Remove.HasValue ? Remove.Value ? RemoveStatus.Removed : RemoveStatus.NotRemoved : RemoveStatus.SomeRemoved },
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
			{ nameof(BookTags), () => BookTags?.Tags ?? string.Empty },
			{ nameof(Liberate), () => Liberate },
			{ nameof(DateAdded), () => DateAdded },
		};

		#endregion
	}
}
