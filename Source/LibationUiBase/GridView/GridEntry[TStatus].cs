using ApplicationServices;
using DataLayer;
using Dinah.Core;
using FileLiberator;
using LibationFileManager;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LibationUiBase.GridView
{
	public enum RemoveStatus
	{
		NotRemoved,
		Removed,
		SomeRemoved
	}

	/// <summary>The View Model base for the DataGridView</summary>
	public abstract class GridEntry : ReactiveObject
	{
		[Browsable(false)] public string AudibleProductId => Book.AudibleProductId;
		[Browsable(false)] public LibraryBook LibraryBook { get; protected set; }
		[Browsable(false)] public float SeriesIndex { get; protected set; }
		[Browsable(false)] public abstract DateTime DateAdded { get; }
		[Browsable(false)] public Book Book => LibraryBook.Book;

		#region Model properties exposed to the view

		protected bool? remove = false;
		private EntryStatus _liberate;
		private string _purchasedate;
		private string _length;
		private LastDownloadStatus _lastDownload;
		private Lazy<object> _lazyCover;
		private string _series;
		private SeriesOrder _seriesOrder;
		private string _title;
		private string _authors;
		private string _narrators;
		private string _category;
		private string _misc;
		private string _description;
		private Rating _productrating;
		private string _bookTags;
		private Rating _myRating;

		public abstract bool? Remove { get; set; }
		public EntryStatus Liberate { get => _liberate; private set => RaiseAndSetIfChanged(ref _liberate, value); }
		public string PurchaseDate { get => _purchasedate; protected set => RaiseAndSetIfChanged(ref _purchasedate, value); }
		public string Length { get => _length; protected set => RaiseAndSetIfChanged(ref _length, value); }
		public LastDownloadStatus LastDownload { get => _lastDownload; protected set => RaiseAndSetIfChanged(ref _lastDownload, value); }
		public object Cover { get => _lazyCover.Value; }
		public string Series { get => _series; private set => RaiseAndSetIfChanged(ref _series, value); }
		public SeriesOrder SeriesOrder { get => _seriesOrder; private set => RaiseAndSetIfChanged(ref _seriesOrder, value); }
		public string Title { get => _title; private set => RaiseAndSetIfChanged(ref _title, value); }
		public string Authors { get => _authors; private set => RaiseAndSetIfChanged(ref _authors, value); }
		public string Narrators { get => _narrators; private set => RaiseAndSetIfChanged(ref _narrators, value); }
		public string Category { get => _category; private set => RaiseAndSetIfChanged(ref _category, value); }
		public string Misc { get => _misc; private set => RaiseAndSetIfChanged(ref _misc, value); }
		public string Description { get => _description; private set => RaiseAndSetIfChanged(ref _description, value); }
		public Rating ProductRating { get => _productrating; private set => RaiseAndSetIfChanged(ref _productrating, value); }
		public string BookTags { get => _bookTags; private set => RaiseAndSetIfChanged(ref _bookTags, value); }

		public Rating MyRating
		{
			get => _myRating;
			set
			{
				if (_myRating != value && value.OverallRating != 0 && updateReviewTask?.IsCompleted is not false)
					updateReviewTask = UpdateRating(value);
			}
		}

		#endregion

		#region User rating

		private Task updateReviewTask;
		private async Task UpdateRating(Rating rating)
		{
			var api = await LibraryBook.GetApiAsync();

			if (await api.ReviewAsync(Book.AudibleProductId, (int)rating.OverallRating, (int)rating.PerformanceRating, (int)rating.StoryRating))
				LibraryBook.UpdateUserDefinedItem(Book.UserDefinedItem.Tags, Book.UserDefinedItem.BookStatus, Book.UserDefinedItem.PdfStatus, rating);
		}

		#endregion

		#region View property updating

		public void UpdateLibraryBook(LibraryBook libraryBook)
		{
			UserDefinedItem.ItemChanged -= UserDefinedItem_ItemChanged;

			LibraryBook = libraryBook;

			var expanded = Liberate?.Expanded ?? false;
			Liberate = new EntryStatus(libraryBook);
			Liberate.Expanded = expanded;

			Title = Book.TitleWithSubtitle;
			Series = Book.SeriesNames(includeIndex: true);
			SeriesOrder = new SeriesOrder(Book.SeriesLink);
			Length = GetBookLengthString();
			RaiseAndSetIfChanged(ref _myRating, Book.UserDefinedItem.Rating, nameof(MyRating));
			PurchaseDate = GetPurchaseDateString();
			ProductRating = Book.Rating ?? new Rating(0, 0, 0);
			Authors = Book.AuthorNames();
			Narrators = Book.NarratorNames();
			Category = string.Join(", ", Book.LowestCategoryNames());
			Misc = GetMiscDisplay(libraryBook);
			LastDownload = new(Book.UserDefinedItem);
			Description = GetDescriptionDisplay(Book);
			SeriesIndex = Book.SeriesLink.FirstOrDefault()?.Index ?? 0;
			BookTags = GetBookTags();

			UserDefinedItem.ItemChanged += UserDefinedItem_ItemChanged;
		}

		protected abstract string GetBookTags();
		protected virtual DateTime GetPurchaseDate() => LibraryBook.DateAdded;
		protected virtual int GetLengthInMinutes() => Book.LengthInMinutes;
		protected string GetPurchaseDateString() => GetPurchaseDate().ToString("d");
		protected string GetBookLengthString()
		{
			int bookLenMins = GetLengthInMinutes();
			return bookLenMins == 0 ? "" : $"{bookLenMins / 60} hr {bookLenMins % 60} min";
		}
		
		#endregion

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

			if (udi.Book != LibraryBook.Book)
			{
				//If UserDefinedItem was changed on a different Book instance (such as when batch liberating via menus),
				//Liberate.Book and LibraryBook.Book instances will not have the current DB state.
				Invoke(() => UpdateLibraryBook(new LibraryBook(udi.Book, LibraryBook.DateAdded, LibraryBook.Account)));
				return;
			}

			// UDI changed, possibly in a different context/view. Update this viewmodel. Call NotifyPropertyChanged to notify view.
			// - This method responds to tons of incidental changes. Do not persist to db from here. Committing to db must be a volitional action by the caller, not incidental. Otherwise batch changes would be impossible; we would only have slow one-offs
			// - Don't restrict notifying view to 'only if property changed'. This same book instance can get passed to a different view, then changed there. When the chain of events makes its way back here, the property is unchanged (because it's the same instance), but this view is out of sync. NotifyPropertyChanged will then update this view.
			switch (itemName)
			{
				case nameof(udi.BookStatus):
				case nameof(udi.PdfStatus):
					Liberate.Invalidate(nameof(Liberate.BookStatus), nameof(Liberate.PdfStatus), nameof(Liberate.IsUnavailable), nameof(Liberate.ButtonImage), nameof(Liberate.ToolTip));
					RaisePropertyChanged(nameof(Liberate));
					break;
				case nameof(udi.Tags):
					BookTags = GetBookTags();
					Liberate.Invalidate(nameof(Liberate.Opacity));
					RaisePropertyChanged(nameof(Liberate));
					break;
				case nameof(udi.LastDownloaded):
					LastDownload = new (udi);
					break;
				case nameof(udi.Rating):
					_myRating = udi.Rating;
					//Ratings are changed using Update(), which is a problem for Avalonia data bindings because
					//the reference doesn't change. Must call RaisePropertyChanged instead of RaiseAndSetIfChanged
					RaisePropertyChanged(nameof(MyRating));
					break;
			}
		}

		#endregion

		#region Sorting

		public object GetMemberValue(string memberName) => memberName switch
		{
			nameof(Remove) => Remove.HasValue ? Remove.Value ? RemoveStatus.Removed : RemoveStatus.NotRemoved : RemoveStatus.SomeRemoved,
			nameof(Title) => Book.TitleSortable(),
			nameof(Series) => Book.SeriesSortable(),
			nameof(SeriesOrder) => SeriesOrder,
			nameof(Length) => GetLengthInMinutes(),
			nameof(MyRating) => Book.UserDefinedItem.Rating,
			nameof(PurchaseDate) => GetPurchaseDate(),
			nameof(ProductRating) => Book.Rating,
			nameof(Authors) => Authors,
			nameof(Narrators) => Narrators,
			nameof(Description) => Description,
			nameof(Category) => Category,
			nameof(Misc) => Misc,
			nameof(LastDownload) => LastDownload,
			nameof(BookTags) => BookTags ?? string.Empty,
			nameof(Liberate) => Liberate,
			nameof(DateAdded) => DateAdded,
			_ => null
		};

		public IComparer GetMemberComparer(Type memberType)
			=> memberTypeComparers.TryGetValue(memberType, out IComparer value) ? value : memberTypeComparers[memberType.BaseType];

		// Instantiate comparers for every exposed member object type.
		private static readonly Dictionary<Type, IComparer> memberTypeComparers = new()
		{
			{ typeof(RemoveStatus), Comparer<RemoveStatus>.Default },
			{ typeof(string), Comparer<string>.Default },
			{ typeof(int), Comparer <int>.Default },
			{ typeof(float), Comparer<float >.Default },
			{ typeof(bool), Comparer<bool>.Default },
			{ typeof(Rating), Comparer<Rating>.Default },
			{ typeof(DateTime), Comparer<DateTime>.Default },
			{ typeof(EntryStatus), Comparer<EntryStatus>.Default },
			{ typeof(SeriesOrder), Comparer<SeriesOrder>.Default },
			{ typeof(LastDownloadStatus), Comparer<LastDownloadStatus>.Default },
		};

		#endregion

		#region Cover Art

		protected void LoadCover()
		{
			// Get cover art. If it's default, subscribe to PictureCached
			(bool isDefault, byte[] picture) = PictureStorage.GetPicture(new PictureDefinition(Book.PictureId, PictureSize._80x80));

			if (isDefault)
				PictureStorage.PictureCached += PictureStorage_PictureCached;

			// Mutable property. Set the field so PropertyChanged isn't fired.
			_lazyCover = new Lazy<object>(() => BaseUtil.LoadImage(picture, PictureSize._80x80));
		}

		private void PictureStorage_PictureCached(object sender, PictureCachedEventArgs e)
		{
			// state validation
			if (e?.Definition.PictureId is null ||
				Book?.PictureId is null ||
				e.Picture?.Length == 0)
				return;

			// logic validation
			if (e.Definition.PictureId == Book.PictureId)
			{
				_lazyCover = new Lazy<object>(() => BaseUtil.LoadImage(e.Picture, PictureSize._80x80));
				RaisePropertyChanged(nameof(Cover));
				PictureStorage.PictureCached -= PictureStorage_PictureCached;
			}
		}

		#endregion

		#region Static library display functions		

		/// <summary>This information should not change during <see cref="GridEntry"/> lifetime, so call only once.</summary>
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
		/// This information should not change during <see cref="GridEntry"/> lifetime, so call only once.
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

		/// <summary>
		/// Creates <see cref="GridEntry"/> for all non-episode books in an enumeration of <see cref="DataLayer.LibraryBook"/>.
		/// </summary>
		/// <remarks>Can be called from any thread, but requires the calling thread's <see cref="SynchronizationContext.Current"/> to be valid.</remarks>
		public static  async Task<List<TEntry>> GetAllProductsAsync<TEntry>(IEnumerable<LibraryBook> libraryBooks, Func<LibraryBook, bool> includeIf, Func<LibraryBook, TEntry> factory)
			where TEntry : GridEntry
		{
			var products = libraryBooks.Where(includeIf).ToArray();
			if (products.Length == 0)
				return [];

			int parallelism = int.Max(1, Environment.ProcessorCount - 1);

			(int batchSize, int rem) = int.DivRem(products.Length, parallelism);
			if (rem != 0) batchSize++;

			var syncContext = SynchronizationContext.Current;

			//Asynchronously create a GridEntry for every book in the library
			var tasks = products.Chunk(batchSize).Select(batch => Task.Run(() =>
			{
				SynchronizationContext.SetSynchronizationContext(syncContext);
				return batch.Select(factory).OfType<TEntry>().ToArray();
			}));

			return (await Task.WhenAll(tasks)).SelectMany(a => a).ToList();
		}


		~GridEntry()
		{
			PictureStorage.PictureCached -= PictureStorage_PictureCached;
			UserDefinedItem.ItemChanged -= UserDefinedItem_ItemChanged;
		}
	}
}
