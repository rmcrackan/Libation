using DataLayer;
using Dinah.Core;
using Dinah.Core.DataBinding;
using Dinah.Core.Drawing;
using LibationFileManager;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace LibationWinForms.GridView
{
	/// <summary>The View Model base for the DataGridView</summary>
	public abstract class GridEntry : AsyncNotifyPropertyChanged, IMemberComparable
	{
		public string AudibleProductId => Book.AudibleProductId;
		public LibraryBook LibraryBook { get; protected set; }
		protected Book Book => LibraryBook.Book;
		private Image _cover;

		#region Model properties exposed to the view
		public Image Cover
		{
			get => _cover;
			protected set
			{
				_cover = value;
				NotifyPropertyChanged();
			}
		}
		public float SeriesIndex { get; protected set; }
		public string ProductRating { get; protected set; }
		public string PurchaseDate { get; protected set; }
		public string MyRating { get; protected set; }
		public string Series { get; protected set; }
		public string Title { get; protected set; }
		public string Length { get; protected set; }
		public string Authors { get; protected set; }
		public string Narrators { get; protected set; }
		public string Category { get; protected set; }
		public string Misc { get; protected set; }
		public string Description { get; protected set; }
		public string LongDescription { get; protected set; }
		public abstract DateTime DateAdded { get; }
		public abstract string DisplayTags { get; }
		public abstract LiberateButtonStatus Liberate { get; }
		#endregion

		#region Sorting

		public GridEntry() => _memberValues = CreateMemberValueDictionary();
		private Dictionary<string, Func<object>> _memberValues { get; set; }
		protected abstract Dictionary<string, Func<object>> CreateMemberValueDictionary();

		// These methods are implementation of Dinah.Core.DataBinding.IMemberComparable
		// Used by GridEntryBindingList for all sorting
		public virtual object GetMemberValue(string memberName) => _memberValues[memberName]();
		public IComparer GetMemberComparer(Type memberType) => _memberTypeComparers[memberType];

		// Instantiate comparers for every exposed member object type.
		private static readonly Dictionary<Type, IComparer> _memberTypeComparers = new()
		{
			{ typeof(string), new ObjectComparer<string>() },
			{ typeof(int), new ObjectComparer<int>() },
			{ typeof(float), new ObjectComparer<float>() },
			{ typeof(bool), new ObjectComparer<bool>() },
			{ typeof(DateTime), new ObjectComparer<DateTime>() },
			{ typeof(LiberateButtonStatus), new ObjectComparer<LiberateButtonStatus>() },
		};

		#endregion

		protected void LoadCover()
		{
			// Get cover art. If it's default, subscribe to PictureCached
			{
				(bool isDefault, byte[] picture) = PictureStorage.GetPicture(new PictureDefinition(Book.PictureId, PictureSize._80x80));

				if (isDefault)
					PictureStorage.PictureCached += PictureStorage_PictureCached;

				// Mutable property. Set the field so PropertyChanged isn't fired.
				_cover = ImageReader.ToImage(picture);
			}
		}

		private void PictureStorage_PictureCached(object sender, PictureCachedEventArgs e)
		{
			if (e.Definition.PictureId == Book.PictureId)
			{
				Cover = ImageReader.ToImage(e.Picture);
				PictureStorage.PictureCached -= PictureStorage_PictureCached;
			}
		}

		#region Static library display functions		

		/// <summary>
		/// This information should not change during <see cref="LibraryBookEntry"/> lifetime, so call only once.
		/// </summary>
		protected static string GetDescriptionDisplay(Book book)
		{
			var doc = new HtmlAgilityPack.HtmlDocument();
			doc.LoadHtml(book?.Description?.Replace("</p> ", "\r\n\r\n</p>") ?? "");
			return doc.DocumentNode.InnerText.Trim();
		}

		protected static string TrimTextToWord(string text, int maxLength)
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
		protected static string GetMiscDisplay(LibraryBook libraryBook)
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

		~GridEntry()
		{
			PictureStorage.PictureCached -= PictureStorage_PictureCached;
		}
	}
}
