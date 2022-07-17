using ApplicationServices;
using DataLayer;
using Dinah.Core;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace LibationWinForms.AvaloniaUI.ViewModels
{
	/// <summary>The View Model for a LibraryBook that is ContentType.Product or ContentType.Episode</summary>
	public class LibraryBookEntry : GridEntry
	{
		[Browsable(false)] public override DateTime DateAdded => LibraryBook.DateAdded;
		[Browsable(false)] public SeriesEntry Parent { get; init; }

		#region Model properties exposed to the view

		private DateTime lastStatusUpdate = default;
		private LiberatedStatus _bookStatus;
		private LiberatedStatus? _pdfStatus;

		public override bool? Remove
		{
			get => _remove;
			set
			{
				_remove = value ?? false;

				Parent?.ChildRemoveUpdate();
				this.RaisePropertyChanged(nameof(Remove));
			}
		}

		public override LiberateButtonStatus Liberate
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
				return new LiberateButtonStatus(IsSeries) { BookStatus = _bookStatus, PdfStatus = _pdfStatus };
			}
		}

		public override BookTags BookTags => new() { Tags = string.Join("\r\n", Book.UserDefinedItem.TagsEnumerated) };

		public override bool IsSeries => false;
		public override bool IsEpisode => Parent is not null;
		public override bool IsBook => Parent is null;

		#endregion

		public LibraryBookEntry(LibraryBook libraryBook)
		{
			LibraryBook = libraryBook;
			LoadCover();

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

			UserDefinedItem.ItemChanged += UserDefinedItem_ItemChanged;
		}

		#region detect changes to the model, update the view.

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
					this.RaisePropertyChanged(nameof(BookTags));
					break;
				case nameof(udi.BookStatus):
					Book.UserDefinedItem.BookStatus = udi.BookStatus;
					_bookStatus = udi.BookStatus;
					this.RaisePropertyChanged(nameof(Liberate));
					break;
				case nameof(udi.PdfStatus):
					Book.UserDefinedItem.PdfStatus = udi.PdfStatus;
					_pdfStatus = udi.PdfStatus;
					this.RaisePropertyChanged(nameof(Liberate));
					break;
			}
		}

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

		~LibraryBookEntry()
		{
			UserDefinedItem.ItemChanged -= UserDefinedItem_ItemChanged;
		}
	}
}
