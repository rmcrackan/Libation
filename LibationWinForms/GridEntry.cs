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

namespace LibationWinForms
{
	/// <summary>
	/// The View Model for a LibraryBook
	/// </summary>
	internal class GridEntry : AsyncNotifyPropertyChanged, IMemberComparable
	{
		#region implementation properties
		// hide from public fields from Data Source GUI with [Browsable(false)]

		[Browsable(false)]
		public string AudibleProductId => Book.AudibleProductId;
		[Browsable(false)]
		public LibraryBook LibraryBook { get; }

		#endregion

		private Book Book => LibraryBook.Book;
		private Image _cover;
		private Action _refilter;

		public GridEntry(LibraryBook libraryBook, Action refilterOnChanged = null)
		{
			LibraryBook = libraryBook;
			_refilter = refilterOnChanged;
			_memberValues = CreateMemberValueDictionary();


			//Get cover art. If it's default, subscribe to PictureCached
			{
				(bool isDefault, byte[] picture) = FileManager.PictureStorage.GetPicture(new FileManager.PictureDefinition(Book.PictureId, FileManager.PictureSize._80x80));

				if (isDefault)
					FileManager.PictureStorage.PictureCached += PictureStorage_PictureCached;

				//Mutable property. Set the field so PropertyChanged isn't fired.
				_cover = ImageReader.ToImage(picture);
			}

			//Immutable properties
			{
				Title = Book.Title;
				Series = Book.SeriesNames;
				Length = Book.LengthInMinutes == 0 ? "" : $"{Book.LengthInMinutes / 60} hr {Book.LengthInMinutes % 60} min";
				MyRating = Book.UserDefinedItem.Rating?.ToStarString()?.DefaultIfNullOrWhiteSpace("");
				PurchaseDate = libraryBook.DateAdded.ToString("d");
				ProductRating = Book.Rating?.ToStarString()?.DefaultIfNullOrWhiteSpace("");
				Authors = Book.AuthorNames;
				Narrators = Book.NarratorNames;
				Category = string.Join(" > ", Book.CategoriesNames);
				Misc = GetMiscDisplay(libraryBook);
				Description = GetDescriptionDisplay(Book);
			}

			UserDefinedItem.ItemChanged += UserDefinedItem_ItemChanged;
		}

		private void PictureStorage_PictureCached(object sender, FileManager.PictureCachedEventArgs e)
		{
			if (e.Definition.PictureId == Book.PictureId)
			{
				Cover = ImageReader.ToImage(e.Picture);
				FileManager.PictureStorage.PictureCached -= PictureStorage_PictureCached;
			}
		}

		#region detect changes to the model and update the view

		/// <summary>
		/// This event handler receives notifications from the model that it has changed.
		/// Save to the database and notify the view that it's changed.
		/// </summary>
		private void UserDefinedItem_ItemChanged(object sender, string itemName)
		{
			var udi = sender as UserDefinedItem;

			if (udi.Book.AudibleProductId != LibraryBook.Book.AudibleProductId)
				return;

			switch (itemName)
			{
				case nameof(udi.Tags):
					{
						LibraryCommands.UpdateTags(LibraryBook.Book, udi.Tags);
						NotifyPropertyChanged(nameof(DisplayTags));
					}
					break;
				case nameof(udi.BookStatus):
					{
						var status = udi.BookStatus == LiberatedStatus.PartialDownload ? LiberatedStatus.NotLiberated : udi.BookStatus;
						LibraryCommands.UpdateBook(LibraryBook.Book, status);
						NotifyPropertyChanged(nameof(Liberate));
					}
					break;
				case nameof(udi.PdfStatus):
					{
						LibraryCommands.UpdatePdf(LibraryBook.Book, udi.PdfStatus);
						NotifyPropertyChanged(nameof(Liberate));
					}
					break;
			}

			_refilter?.Invoke();
		}

		#endregion	

		#region Model properties exposed to the view
		public Image Cover
		{
			get
			{
				return _cover;
			}
			private set
			{
				_cover = value;
				NotifyPropertyChanged();
			}
		}

		public string ProductRating { get; }
		public string PurchaseDate { get; }
		public string MyRating { get; }
		public string Series { get; }
		public string Title { get; }
		public string Length { get; }
		public string Authors { get; }
		public string Narrators { get; }
		public string Category { get; }
		public string Misc { get; }
		public string Description { get; }
		public string DisplayTags
		{
			get=> Book.UserDefinedItem.Tags;
			set => Book.UserDefinedItem.Tags = value;
		}
		public (LiberatedStatus BookStatus, LiberatedStatus? PdfStatus) Liberate
		{
			get => (LibraryCommands.Liberated_Status(LibraryBook.Book), LibraryCommands.Pdf_Status(LibraryBook.Book));
			
			set
			{
				LibraryBook.Book.UserDefinedItem.BookStatus = value.BookStatus;
				LibraryBook.Book.UserDefinedItem.PdfStatus = value.PdfStatus;
			}
		}

		#endregion

		#region Data Sorting

		private Dictionary<string, Func<object>> _memberValues { get; }

		/// <summary>
		/// Create getters for all member object values by name
		/// </summary>
		private Dictionary<string, Func<object>> CreateMemberValueDictionary() => new()
		{
			{ nameof(Title), () => GetSortName(Book.Title) },
			{ nameof(Series), () => GetSortName(Book.SeriesNames) },
			{ nameof(Length), () => Book.LengthInMinutes },
			{ nameof(MyRating), () => Book.UserDefinedItem.Rating.FirstScore },
			{ nameof(PurchaseDate), () => LibraryBook.DateAdded },
			{ nameof(ProductRating), () => Book.Rating.FirstScore },
			{ nameof(Authors), () => Authors },
			{ nameof(Narrators), () => Narrators },
			{ nameof(Description), () => Description },
			{ nameof(Category), () => Category },
			{ nameof(Misc), () => Misc },
			{ nameof(DisplayTags), () => DisplayTags },
			{ nameof(Liberate), () => Liberate.BookStatus }
		};

		// Instantiate comparers for every exposed member object type.
		private static readonly Dictionary<Type, IComparer> _memberTypeComparers = new()
		{
			{ typeof(string), new ObjectComparer<string>() },
			{ typeof(int), new ObjectComparer<int>() },
			{ typeof(float), new ObjectComparer<float>() },
			{ typeof(DateTime), new ObjectComparer<DateTime>() },
			{ typeof(LiberatedStatus), new ObjectComparer<LiberatedStatus>() },
		};

		public virtual object GetMemberValue(string memberName) => _memberValues[memberName]();
		public virtual IComparer GetMemberComparer(Type memberType) => _memberTypeComparers[memberType];

		private static readonly string[] _sortPrefixIgnores = { "the", "a", "an" };
		private static string GetSortName(string unformattedName)
		{
			var sortName = unformattedName
				.Replace("|", "")
				.Replace(":", "")
				.ToLowerInvariant()
				.Trim();

			if (_sortPrefixIgnores.Any(prefix => sortName.StartsWith(prefix + " ")))
				sortName = sortName.Substring(sortName.IndexOf(" ") + 1).TrimStart();

			return sortName;
		}

		#endregion

		#region Static library display functions
		
		/// <summary>
		/// This information should not change during <see cref="GridEntry"/> lifetime, so call only once.
		/// </summary>
		private static string GetDescriptionDisplay(Book book)
		{
			var doc = new HtmlAgilityPack.HtmlDocument();
			doc.LoadHtml(book.Description);
			var noHtml = doc.DocumentNode.InnerText;
			return
				noHtml.Length < 63 ?
				noHtml :
				noHtml.Substring(0, 60) + "...";
		}

		/// <summary>
		/// This information should not change during <see cref="GridEntry"/> lifetime, so call only once.
		/// Maximum of 5 text rows will fit in 80-pixel row height.
		/// </summary>
		private static string GetMiscDisplay(LibraryBook libraryBook)
		{
			var details = new List<string>();

			var locale = libraryBook.Book.Locale.DefaultIfNullOrWhiteSpace("[unknown]");
			var acct = libraryBook.Account.DefaultIfNullOrWhiteSpace("[unknown]");

			details.Add($"Account: {locale} - {acct}");

			if (libraryBook.Book.HasPdf)
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

		~GridEntry()
		{
			UserDefinedItem.ItemChanged -= UserDefinedItem_ItemChanged;
			FileManager.PictureStorage.PictureCached -= PictureStorage_PictureCached;
		}
	}

}
