using DataLayer;
using Dinah.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LibationWinForms.GridView
{
	/// <summary>The View Model for a LibraryBook that is ContentType.Parent</summary>
	public class SeriesEntry : GridEntry
	{
		public List<LibraryBookEntry> Children { get; } = new();
		public override DateTime DateAdded => Children.Max(c => c.DateAdded);
		public override string DisplayTags { get; } = string.Empty;
		public override LiberateButtonStatus Liberate { get; }

		private SeriesEntry(LibraryBook parent)
		{
			LibraryBook = parent;
			Liberate = new LiberateButtonStatus { IsSeries = true };
			SeriesIndex = -1;
		}

		public SeriesEntry(LibraryBook parent, IEnumerable<LibraryBook> children) : this(parent)
		{
			Children = children
				.Select(c => new LibraryBookEntry(c) { Parent = this })
				.OrderBy(c => c.SeriesIndex)
				.ToList();

			UpdateSeries(parent);
			LoadCover();
		}

		public SeriesEntry(LibraryBook parent, LibraryBook child) : this(parent)
		{
			Children = new() { new LibraryBookEntry(child) { Parent = this } };

			UpdateSeries(parent);
			LoadCover();
		}

		public void UpdateSeries(LibraryBook libraryBook)
		{
			LibraryBook = libraryBook;

			// Immutable properties
			{
				Title = Book.Title;
				Series = Book.SeriesNames();
				MyRating = Book.UserDefinedItem.Rating?.ToStarString()?.DefaultIfNullOrWhiteSpace("");
				PurchaseDate = Children.Min(c => c.LibraryBook.DateAdded).ToString("d");
				ProductRating = Book.Rating?.ToStarString()?.DefaultIfNullOrWhiteSpace("");
				Authors = Book.AuthorNames();
				Narrators = Book.NarratorNames();
				Category = string.Join(" > ", Book.CategoriesNames());
				Misc = GetMiscDisplay(libraryBook);
				LongDescription = GetDescriptionDisplay(Book);
				Description = TrimTextToWord(LongDescription, 62);

				int bookLenMins = Children.Sum(c => c.LibraryBook.Book.LengthInMinutes);
				Length = bookLenMins == 0 ? "" : $"{bookLenMins / 60} hr {bookLenMins % 60} min";
			}

			NotifyPropertyChanged();
		}

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
	}
}
