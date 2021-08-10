using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using ApplicationServices;
using DataLayer;
using Dinah.Core.Drawing;
using Dinah.Core.Windows.Forms;

namespace LibationWinForms
{
	internal class GridEntry : INotifyPropertyChanged, IObjectMemberComparable
	{
		public const string LIBERATE_COLUMN_NAME = "Liberate";
		public const string EDIT_TAGS_COLUMN_NAME = "DisplayTags";

        #region implementation properties
        // hide from public fields from Data Source GUI with [Browsable(false)]

        [Browsable(false)]
		public string AudibleProductId => Book.AudibleProductId;
		[Browsable(false)]
		public string Tags => Book.UserDefinedItem.Tags;
		[Browsable(false)]
		public IEnumerable<string> TagsEnumerated => Book.UserDefinedItem.TagsEnumerated;
		[Browsable(false)]
		public LibraryBook LibraryBook { get; }

		#endregion

		public event PropertyChangedEventHandler PropertyChanged;
		private Book Book => LibraryBook.Book;
		private SynchronizationContext SyncContext { get; } = SynchronizationContext.Current;
		private Image _cover;

		public GridEntry(LibraryBook libraryBook)
		{
			LibraryBook = libraryBook;

			_compareValues = CreatePropertyValueDictionary();

			//Get cover art. If it's default, subscribe to PictureCached
			var picDef = new FileManager.PictureDefinition(Book.PictureId, FileManager.PictureSize._80x80);
			(bool isDefault, byte[] picture) = FileManager.PictureStorage.GetPicture(picDef);

			if (isDefault)
				FileManager.PictureStorage.PictureCached += PictureStorage_PictureCached;

			//Mutable property. Set the field so PropertyChanged doesn't fire.
			_cover = ImageReader.ToImage(picture);

			//Immutable properties
			{
				Title = Book.Title;
				Series = Book.SeriesNames;
				Length = Book.LengthInMinutes == 0 ? "" : $"{Book.LengthInMinutes / 60} hr {Book.LengthInMinutes % 60} min";
				MyRating = GetStarString(Book.UserDefinedItem.Rating);
				PurchaseDate = libraryBook.DateAdded.ToString("d");
				ProductRating = GetStarString(Book.Rating);
				Authors = Book.AuthorNames;
				Narrators = Book.NarratorNames;
				Category = string.Join(" > ", Book.CategoriesNames);
				Misc = GetMiscDisplay(libraryBook);
				Description = GetDescriptionDisplay(Book);
			}

			//DisplayTags and Liberate are live.
		}

        private void PictureStorage_PictureCached(object sender, string pictureId)
		{
			if (pictureId == Book.PictureId)
			{
				//GridEntry SHOULD be UI-ignorant, but PropertyChanged
				Cover = WindowsDesktopUtilities.WinAudibleImageServer.GetImage(pictureId, FileManager.PictureSize._80x80);
				FileManager.PictureStorage.PictureCached -= PictureStorage_PictureCached;
			}
		}

		protected void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
			=> SyncContext.Post(
				args => OnPropertyChangedAsync(args as AsyncCompletedEventArgs),
				new AsyncCompletedEventArgs(null, false, new PropertyChangedEventArgs(propertyName))
				);

		private void OnPropertyChangedAsync(AsyncCompletedEventArgs e) =>
			PropertyChanged?.Invoke(this, e.UserState as PropertyChangedEventArgs);

		#region Data Source properties
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

		public string DisplayTags => string.Join("\r\n", TagsEnumerated);
		public (LiberatedState, PdfState) Liberate => (LibraryCommands.Liberated_Status(Book), LibraryCommands.Pdf_Status(Book));

		#endregion

		#region Data Sorting

		private Dictionary<string, Func<object>> _compareValues { get; }
		private static Dictionary<Type, IComparer> _objectComparers;

		public virtual object GetMemberValue(string propertyName) => _compareValues[propertyName]();
		public virtual IComparer GetComparer(Type propertyType) => _objectComparers[propertyType];

		/// <summary>
		/// Instantiate comparers for every type needed to sort columns.
		/// </summary>
		static GridEntry()
		{
			_objectComparers = new Dictionary<Type, IComparer>()
			{
				{ typeof(string), new ObjectComparer<string>() },
				{ typeof(int), new ObjectComparer<int>() },
				{ typeof(float), new ObjectComparer<float>() },
				{ typeof(DateTime), new ObjectComparer<DateTime>() },
				{ typeof(LiberatedState), new ObjectComparer<LiberatedState>() },
			};
		}
		/// <summary>
		/// Create getters for all member values by name
		/// </summary>
		Dictionary<string, Func<object>> CreatePropertyValueDictionary() => new()
			{
				{ nameof(Title), () => getSortName(Book.Title)},
				{ nameof(Series),() => getSortName(Book.SeriesNames)},
				{ nameof(Length), () => Book.LengthInMinutes},
				{ nameof(MyRating), () => Book.UserDefinedItem.Rating.FirstScore},
				{ nameof(PurchaseDate), () => LibraryBook.DateAdded},
				{ nameof(ProductRating), () => Book.Rating.FirstScore},
				{ nameof(Authors), () => Authors},
				{ nameof(Narrators), () => Narrators},
				{ nameof(Description), () => Description},
				{ nameof(Category), () => Category},
				{ nameof(Misc), () => Misc},
				{ nameof(DisplayTags), () => DisplayTags},
				{ nameof(Liberate), () => Liberate.Item1}
			};

		private static readonly string[] sortPrefixIgnores = { "the", "a", "an" };
		private static string getSortName(string unformattedName)
		{
			var sortName = unformattedName
				.Replace("|", "")
				.Replace(":", "")
				.ToLowerInvariant()
				.Trim();

			if (sortPrefixIgnores.Any(prefix => sortName.StartsWith(prefix + " ")))
				sortName = sortName.Substring(sortName.IndexOf(" ") + 1).TrimStart();

			return sortName;
		}

        #endregion

		#region Static library display functions
	
		public static (string mouseoverText, Bitmap buttonImage) GetLiberateDisplay(LiberatedState liberatedStatus, PdfState pdfStatus)
		{
			string text;
			Bitmap image;

			// get mouseover text
			{
				var libState = liberatedStatus switch
				{
					LiberatedState.Liberated => "Liberated",
					LiberatedState.PartialDownload => "File has been at least\r\npartially downloaded",
					LiberatedState.NotDownloaded => "Book NOT downloaded",
					_ => throw new Exception("Unexpected liberation state")
				};

				var pdfState = pdfStatus switch
				{
					PdfState.Downloaded => "\r\nPDF downloaded",
					PdfState.NotDownloaded => "\r\nPDF NOT downloaded",
					PdfState.NoPdf => "",
					_ => throw new Exception("Unexpected PDF state")
				};

				text = libState + pdfState;

				if (liberatedStatus == LiberatedState.NotDownloaded ||
					liberatedStatus == LiberatedState.PartialDownload ||
					pdfStatus == PdfState.NotDownloaded)
					text += "\r\nClick to complete";

			}

			// get image
			{
				var image_lib
					= liberatedStatus == LiberatedState.NotDownloaded ? "red"
					: liberatedStatus == LiberatedState.PartialDownload ? "yellow"
					: liberatedStatus == LiberatedState.Liberated ? "green"
					: throw new Exception("Unexpected liberation state");
				var image_pdf
					= pdfStatus == PdfState.NoPdf ? ""
					: pdfStatus == PdfState.NotDownloaded ? "_pdf_no"
					: pdfStatus == PdfState.Downloaded ? "_pdf_yes"
					: throw new Exception("Unexpected PDF state");

				image = (Bitmap)Properties.Resources.ResourceManager.GetObject($"liberate_{image_lib}{image_pdf}");
			}

			return (text, image);
		}

		/// <summary>
		/// This information should not change during <see cref="GridEntry"/> lifetime, so call only once.
		/// </summary>
		private static string GetDescriptionDisplay(Book book)
        {
			var doc = new HtmlAgilityPack.HtmlDocument();
			doc.LoadHtml(book.Description);
			var noHtml = doc.DocumentNode.InnerText;
			return 
				noHtml.Length < 63?
				noHtml : 
				noHtml.Substring(0, 60) + "...";
		}

		/// <summary>
		/// This information should not change during <see cref="GridEntry"/> lifetime, so call only once.
		/// </summary>
		private static string GetMiscDisplay(LibraryBook libraryBook)
		{
			// max 5 text rows

			var details = new List<string>();

			var locale
				= string.IsNullOrWhiteSpace(libraryBook.Book.Locale)
				? "[unknown]"
				: libraryBook.Book.Locale;
			var acct
				= string.IsNullOrWhiteSpace(libraryBook.Account)
				? "[unknown]"
				: libraryBook.Account;
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

		private static string GetStarString(Rating rating)
			=> (rating?.FirstScore != null && rating?.FirstScore > 0f)
			? rating?.ToStarString()
			: "";

		#endregion
	}	
}
