using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using ApplicationServices;
using DataLayer;
using Dinah.Core.DataBinding;
using Dinah.Core;
using Dinah.Core.Drawing;
using LibationFileManager;
using System.Threading.Tasks;

namespace LibationWinForms
{
	/// <summary>
	/// The View Model for a LibraryBook
	/// </summary>
	internal class LibraryBookEntry : GridEntry
	{
		#region implementation properties NOT exposed to the view
		// hide from public fields from Data Source GUI with [Browsable(false)]

		[Browsable(false)]
		public string AudibleProductId => Book.AudibleProductId;
		[Browsable(false)]
		public LibraryBook LibraryBook { get; private set; }
		[Browsable(false)]
		public string LongDescription { get; private set; }
		#endregion

		// alias
		protected override Book Book => LibraryBook.Book;
		#region Model properties exposed to the view

		private DateTime lastStatusUpdate = default;
		private LiberatedStatus _bookStatus;
		private LiberatedStatus? _pdfStatus;

		public override DateTime DateAdded => LibraryBook.DateAdded;
		public override string ProductRating { get; protected set; }
		public override string PurchaseDate { get; protected set; }
		public override string MyRating { get; protected set; }
		public override string Series { get; protected set; }
		public override string Title { get; protected set; }
		public override string Length { get; protected set; }
		public override string Authors { get; protected set; }
		public override string Narrators { get; protected set; }
		public override string Category { get; protected set; }
		public override string Misc { get; protected set; }
		public override string Description { get; protected set; }
		public override string DisplayTags => string.Join("\r\n", Book.UserDefinedItem.TagsEnumerated);

		// these 2 values being in 1 field is the trick behind getting the liberated+pdf 'stoplight' icon to draw. See: LiberateDataGridViewImageButtonCell.Paint
		public override LiberateStatus Liberate
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
				return new LiberateStatus { BookStatus = _bookStatus, PdfStatus = _pdfStatus, IsSeries = false };
			}
		}
		#endregion


		public LibraryBookEntry(LibraryBook libraryBook) => setLibraryBook(libraryBook);

		public void UpdateLibraryBook(LibraryBook libraryBook)
		{
			if (AudibleProductId != libraryBook.Book.AudibleProductId)
				throw new Exception("Invalid grid entry update. IDs must match");

			setLibraryBook(libraryBook);

			NotifyPropertyChanged();
		}

		private void setLibraryBook(LibraryBook libraryBook)
		{
			LibraryBook = libraryBook;
			_memberValues = CreateMemberValueDictionary();

			LoadCover();

			// Immutable properties
			{
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
			}

			UserDefinedItem.ItemChanged += UserDefinedItem_ItemChanged;
		}


		#region detect changes to the model, update the view, and save to database.

		/// <summary>
		/// This event handler receives notifications from the model that it has changed.
		/// Save to the database and notify the view that it's changed.
		/// </summary>
		private void UserDefinedItem_ItemChanged(object sender, string itemName)
		{
			var udi = sender as UserDefinedItem;

			if (udi.Book.AudibleProductId != Book.AudibleProductId)
				return;

			switch (itemName)
			{
				case nameof(udi.Tags):
					Book.UserDefinedItem.Tags = udi.Tags;
					NotifyPropertyChanged(nameof(DisplayTags));
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
		{
			// validate
			if (DisplayTags.EqualsInsensitive(newTags) &&
				Liberate.BookStatus == bookStatus &&
				Liberate.PdfStatus == pdfStatus)
				return;

			// update cache
			_bookStatus = bookStatus;
			_pdfStatus = pdfStatus;

			// set + save
			Book.UserDefinedItem.Tags = newTags;
			Book.UserDefinedItem.BookStatus = bookStatus;
			Book.UserDefinedItem.PdfStatus = pdfStatus;
			LibraryCommands.UpdateUserDefinedItem(Book);
		}

		#endregion

		#region Data Sorting
		// These methods are implementation of Dinah.Core.DataBinding.IMemberComparable
		// Used by Dinah.Core.DataBinding.SortableBindingList<T> for all sorting
		public override object GetMemberValue(string memberName) => _memberValues[memberName]();

		private Dictionary<string, Func<object>> _memberValues { get; set; }

		/// <summary>
		/// Create getters for all member object values by name
		/// </summary>
		private Dictionary<string, Func<object>> CreateMemberValueDictionary() => new()
		{
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
			{ nameof(DisplayTags), () => DisplayTags },
			{ nameof(Liberate), () => Liberate.BookStatus },
			{ nameof(DateAdded), () => DateAdded },
		};
		

		#endregion

		#region Static library display functions

		/// <summary>
		/// This information should not change during <see cref="LibraryBookEntry"/> lifetime, so call only once.
		/// </summary>
		private static string GetDescriptionDisplay(Book book)
		{
			var doc = new HtmlAgilityPack.HtmlDocument();
			doc.LoadHtml(book?.Description?.Replace("</p> ", "\r\n\r\n</p>") ?? "");
			return doc.DocumentNode.InnerText.Trim();
		}

		private static string TrimTextToWord(string text, int maxLength)
		{
			return
				text.Length <= maxLength ?
				text :
				text.Substring(0, maxLength - 3) + "...";
		}

		/// <summary>
		/// This information should not change during <see cref="LibraryBookEntry"/> lifetime, so call only once.
		/// Maximum of 5 text rows will fit in 80-pixel row height.
		/// </summary>
		private static string GetMiscDisplay(LibraryBook libraryBook)
		{
			var details = new List<string>();

			var locale = libraryBook.Book.Locale.DefaultIfNullOrWhiteSpace("[unknown]");
			var acct = libraryBook.Account.DefaultIfNullOrWhiteSpace("[unknown]");

			details.Add($"Account: {locale} - {acct}");

			if (libraryBook.Book.HasPdf())
				details.Add("Has PDF");
			if (libraryBook.Book.IsAbridged)
				details.Add("Abridged");
			if (libraryBook.Book.DatePublished.HasValue)
				details.Add($"Date pub'd: {libraryBook.Book.DatePublished.Value:MM/dd/yyyy}");
			// this goes last since it's most likely to have a line-break
			if (!string.IsNullOrWhiteSpace(libraryBook.Book.Publisher))
				details.Add($"Pub: {libraryBook.Book.Publisher.Trim()}");

			if (!details.Any())
				return "[details not imported]";

			return string.Join("\r\n", details);
		}

		#endregion

		~LibraryBookEntry()
		{
			UserDefinedItem.ItemChanged -= UserDefinedItem_ItemChanged;
		}
	}
}
