using ApplicationServices;
using DataLayer;
using Dinah.Core;
using LibationWinForms.GridView;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace LibationWinForms.AvaloniaUI.ViewModels
{
	/// <summary>The View Model for a LibraryBook that is ContentType.Product or ContentType.Episode</summary>
	public class LibraryBookEntry2 : GridEntry2
	{
		[Browsable(false)] public override DateTime DateAdded => LibraryBook.DateAdded;
		[Browsable(false)] public SeriesEntrys2 Parent { get; init; }

		#region Model properties exposed to the view

		private DateTime lastStatusUpdate = default;
		private LiberatedStatus _bookStatus;
		private LiberatedStatus? _pdfStatus;

		public override bool? Remove
		{
			get => _remove;
			set
			{
				_remove = value.HasValue ? value.Value : false;
				Parent?.ChildRemoveUpdate();
				NotifyPropertyChanged();
			}
		}

		public override LiberateButtonStatus2 Liberate
		{
			get
			{
				//Cache these statuses for faster sorting.
				if ((DateTime.Now - lastStatusUpdate).TotalSeconds > 2)
				{
					_bookStatus = LibraryCommands.Liberated_Status(LibraryBook.Book);
					_pdfStatus = LibraryCommands.Pdf_Status(LibraryBook.Book);
					lastStatusUpdate = DateTime.Now;
				}
				return new LiberateButtonStatus2 { BookStatus = _bookStatus, PdfStatus = _pdfStatus, IsSeries = false };
			}
		}

		public override BookTags BookTags => new() { Tags = string.Join("\r\n", Book.UserDefinedItem.TagsEnumerated) };

		#endregion

		public LibraryBookEntry2(LibraryBook libraryBook)
		{
			setLibraryBook(libraryBook);
			LoadCover();
		}

		public void UpdateLibraryBook(LibraryBook libraryBook)
		{
			if (AudibleProductId != libraryBook.Book.AudibleProductId)
				throw new Exception("Invalid grid entry update. IDs must match");

			UserDefinedItem.ItemChanged -= UserDefinedItem_ItemChanged;
			setLibraryBook(libraryBook);
		}

		private void setLibraryBook(LibraryBook libraryBook)
		{
			LibraryBook = libraryBook;

			Title = Book.Title;
			Series = Book.SeriesNames();
			Length = Book.LengthInMinutes == 0 ? "" : $"{Book.LengthInMinutes / 60} hr {Book.LengthInMinutes % 60} min";
			MyRating = Book.UserDefinedItem.Rating?.ToStarString()?.DefaultIfNullOrWhiteSpace("");
			PurchaseDate = libraryBook.DateAdded.ToString("d");
			ProductRating = Book.Rating?.ToStarString()?.DefaultIfNullOrWhiteSpace("");
			Authors = Book.AuthorNames();
			Narrators = Book.NarratorNames();
			Category = string.Join(" > ", Book.CategoriesNames());
			Misc = GetMiscDisplay(libraryBook);
			LongDescription = GetDescriptionDisplay(Book);
			Description = TrimTextToWord(LongDescription, 62);
			SeriesIndex = Book.SeriesLink.FirstOrDefault()?.Index ?? 0;

			NotifyPropertyChanged(nameof(Title));
			NotifyPropertyChanged(nameof(Series));
			NotifyPropertyChanged(nameof(Length));
			NotifyPropertyChanged(nameof(MyRating));
			NotifyPropertyChanged(nameof(PurchaseDate));
			NotifyPropertyChanged(nameof(ProductRating));
			NotifyPropertyChanged(nameof(Authors));
			NotifyPropertyChanged(nameof(Narrators));
			NotifyPropertyChanged(nameof(Category));
			NotifyPropertyChanged(nameof(Misc));
			NotifyPropertyChanged(nameof(LongDescription));
			NotifyPropertyChanged(nameof(Description));
			NotifyPropertyChanged(nameof(SeriesIndex));

			UserDefinedItem.ItemChanged += UserDefinedItem_ItemChanged;
		}

		#region detect changes to the model, update the view, and save to database.

		/// <summary>
		/// This event handler receives notifications from the model that it has changed.
		/// Notify the view that it's changed.
		/// </summary>
		private void UserDefinedItem_ItemChanged(object sender, string itemName)
		{
			var udi = sender as UserDefinedItem;

			if (udi.Book.AudibleProductId != Book.AudibleProductId)
				return;

			// UDI changed, possibly in a different context/view. Update this viewmodel. Call NotifyPropertyChanged to notify view.
			// - This method responds to tons of incidental changes. Do not persist to db from here. Committing to db must be a volitional action by the caller, not incidental. Otherwise batch changes would be impossible; we would only have slow one-offs
			// - Don't restrict notifying view to 'only if property changed'. This same book instance can get passed to a different view, then changed there. When the chain of events makes its way back here, the property is unchanged (because it's the same instance), but this view is out of sync. NotifyPropertyChanged will then update this view.
			switch (itemName)
			{
				case nameof(udi.Tags):
					Book.UserDefinedItem.Tags = udi.Tags;
					NotifyPropertyChanged(nameof(BookTags));
					break;
				case nameof(udi.BookStatus):
					Book.UserDefinedItem.BookStatus = udi.BookStatus;
					_bookStatus = udi.BookStatus;
					NotifyPropertyChanged(nameof(Liberate));
					break;
				case nameof(udi.PdfStatus):
					Book.UserDefinedItem.PdfStatus = udi.PdfStatus;
					_pdfStatus = udi.PdfStatus;
					NotifyPropertyChanged(nameof(Liberate));
					break;
			}
		}

		/// <summary>Save edits to the database</summary>
		public void Commit(string newTags, LiberatedStatus bookStatus, LiberatedStatus? pdfStatus)
			// MVVM pass-through
			=> Book.UpdateBook(newTags, bookStatus: bookStatus, pdfStatus: pdfStatus);

		#endregion

		#region Data Sorting

		/// <summary>Create getters for all member object values by name </summary>
		protected override Dictionary<string, Func<object>> CreateMemberValueDictionary() => new()
		{
			{ nameof(Remove), () => Remove.HasValue ? Remove.Value ? RemoveStatus.Removed : RemoveStatus.NotRemoved : RemoveStatus.SomeRemoved },
			{ nameof(Title), () => Book.TitleSortable() },
			{ nameof(Series), () => Book.SeriesSortable() },
			{ nameof(Length), () => Book.LengthInMinutes },
			{ nameof(MyRating), () => Book.UserDefinedItem.Rating.FirstScore() },
			{ nameof(PurchaseDate), () => LibraryBook.DateAdded },
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

		~LibraryBookEntry2()
		{
			UserDefinedItem.ItemChanged -= UserDefinedItem_ItemChanged;
		}
	}
}
