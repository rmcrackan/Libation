using ApplicationServices;
using Avalonia.Media;
using DataLayer;
using Dinah.Core;
using FileLiberator;
using LibationFileManager;
using LibationUiBase;
using ReactiveUI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace LibationAvalonia.ViewModels
{
	public enum RemoveStatus
	{
		NotRemoved,
		Removed,
		SomeRemoved
	}
	/// <summary>The View Model base for the DataGridView</summary>
	public abstract class GridEntry : ViewModelBase
	{
		[Browsable(false)] public string AudibleProductId => Book.AudibleProductId;
		[Browsable(false)] public LibraryBook LibraryBook { get; protected set; }
		[Browsable(false)] public float SeriesIndex { get; protected set; }
		[Browsable(false)] public string LongDescription { get; protected set; }
		[Browsable(false)] public abstract DateTime DateAdded { get; }
		[Browsable(false)] public int ListIndex { get; set; }
		[Browsable(false)] public Book Book => LibraryBook.Book;

		#region Model properties exposed to the view

		private Avalonia.Media.Imaging.Bitmap _cover;
		private string _purchasedate;
		private string _series;
		private string _title;
		private string _length;
		private string _authors;
		private string _narrators;
		private string _category;
		private string _misc;
		private LastDownloadStatus _lastDownload;
		private string _description;
		private Rating _productrating;
		protected Rating _myRating;

		public Avalonia.Media.Imaging.Bitmap Cover { get => _cover; protected set => this.RaiseAndSetIfChanged(ref _cover, value); }
		public string PurchaseDate { get => _purchasedate; protected set => this.RaiseAndSetIfChanged(ref _purchasedate, value); }
		public string Series { get => _series; protected set => this.RaiseAndSetIfChanged(ref _series, value); }
		public string Title { get => _title; protected set => this.RaiseAndSetIfChanged(ref _title, value); }
		public string Length { get => _length; protected set => this.RaiseAndSetIfChanged(ref _length, value); }
		public string Authors { get => _authors; protected set => this.RaiseAndSetIfChanged(ref _authors, value); }
		public string Narrators { get => _narrators; protected set => this.RaiseAndSetIfChanged(ref _narrators, value); }
		public string Category { get => _category; protected set => this.RaiseAndSetIfChanged(ref _category, value); }
		public LastDownloadStatus LastDownload { get => _lastDownload; protected set => this.RaiseAndSetIfChanged(ref _lastDownload, value); }
		public string Misc { get => _misc; protected set => this.RaiseAndSetIfChanged(ref _misc, value); }
		public string Description { get => _description; protected set => this.RaiseAndSetIfChanged(ref _description, value); }
		public Rating ProductRating { get => _productrating; protected set => this.RaiseAndSetIfChanged(ref _productrating, value); }
		public Rating MyRating
		{
			get => _myRating;
			set
			{
				if (_myRating != value
					&& value.OverallRating != 0
					&& updateReviewTask?.IsCompleted is not false)
				{
					updateReviewTask = UpdateRating(value);
				}
			}
		}

		protected bool? _remove = false;
		public abstract bool? Remove { get; set; }
		public abstract LiberateButtonStatus Liberate { get; }
		public abstract BookTags BookTags { get; }
		public abstract bool IsEpisode { get; }
		public abstract bool IsBook { get; }
		public IBrush BackgroundBrush => IsEpisode ? App.SeriesEntryGridBackgroundBrush : Brushes.Transparent;

		#endregion

		#region User rating

		private Task updateReviewTask;
		private async Task UpdateRating(Rating rating)
		{
			var api = await LibraryBook.GetApiAsync();

			if (await api.ReviewAsync(Book.AudibleProductId, (int)rating.OverallRating, (int)rating.PerformanceRating, (int)rating.StoryRating))
			{
				_myRating = rating;
				LibraryBook.Book.UpdateUserDefinedItem(Book.UserDefinedItem.Tags, Book.UserDefinedItem.BookStatus, Book.UserDefinedItem.PdfStatus, rating);
			}

			this.RaisePropertyChanged(nameof(MyRating));
		}

		#endregion

		#region Sorting

		public GridEntry() => _memberValues = CreateMemberValueDictionary();

		// These methods are implementation of Dinah.Core.DataBinding.IMemberComparable
		// Used by GridEntryBindingList for all sorting
		public virtual object GetMemberValue(string memberName) => _memberValues[memberName]();
		public IComparer GetMemberComparer(Type memberType) => _memberTypeComparers[memberType];
		protected abstract Dictionary<string, Func<object>> CreateMemberValueDictionary();
		private Dictionary<string, Func<object>> _memberValues { get; set; }

		// Instantiate comparers for every exposed member object type.
		private static readonly Dictionary<Type, IComparer> _memberTypeComparers = new()
		{
			{ typeof(RemoveStatus), new ObjectComparer<RemoveStatus>() },
			{ typeof(string), new ObjectComparer<string>() },
			{ typeof(int), new ObjectComparer<int>() },
			{ typeof(float), new ObjectComparer<float>() },
			{ typeof(bool), new ObjectComparer<bool>() },
			{ typeof(DateTime), new ObjectComparer<DateTime>() },
			{ typeof(LiberateButtonStatus), new ObjectComparer<LiberateButtonStatus>() },
			{ typeof(LastDownloadStatus), new ObjectComparer<LastDownloadStatus>() },
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
			using var ms = new System.IO.MemoryStream(picture);
			_cover = new Avalonia.Media.Imaging.Bitmap(ms);
		}

		private void PictureStorage_PictureCached(object sender, PictureCachedEventArgs e)
        {
            // state validation
            if (e is null ||
                e.Definition.PictureId is null ||
                Book?.PictureId is null ||
                e.Picture is null ||
                e.Picture.Length == 0)
                return;

            // logic validation
            if (e.Definition.PictureId == Book.PictureId)
			{
				using var ms = new System.IO.MemoryStream(e.Picture);
				Cover = new Avalonia.Media.Imaging.Bitmap(ms);
				PictureStorage.PictureCached -= PictureStorage_PictureCached;
			}
		}

		#endregion

		#region Static library display functions		

		/// <summary>This information should not change during <see cref="GridEntry"/> lifetime, so call only once.</summary>
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
		/// This information should not change during <see cref="GridEntry"/> lifetime, so call only once.
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
