using Avalonia.Media;
using DataLayer;
using Dinah.Core;
using Dinah.Core.DataBinding;
using Dinah.Core.Drawing;
using LibationFileManager;
using LibationWinForms.GridView;
using ReactiveUI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;

namespace LibationWinForms.AvaloniaUI.ViewModels
{
	public enum RemoveStatus
	{
		NotRemoved,
		Removed,
		SomeRemoved
	}
	/// <summary>The View Model base for the DataGridView</summary>
	public abstract class GridEntry2 : ViewModelBase
	{
		[Browsable(false)] public string AudibleProductId => Book.AudibleProductId;
		[Browsable(false)] public LibraryBook LibraryBook { get; protected set; }
		[Browsable(false)] public float SeriesIndex { get; protected set; }
		[Browsable(false)] public string LongDescription { get; protected set; }
		[Browsable(false)] public abstract DateTime DateAdded { get; }
		[Browsable(false)] public int ListIndex { get; set; }
		[Browsable(false)] protected Book Book => LibraryBook.Book;

		#region Model properties exposed to the view

		private Avalonia.Media.Imaging.Bitmap _cover;
		public Avalonia.Media.Imaging.Bitmap Cover { get => _cover; protected set { this.RaiseAndSetIfChanged(ref _cover, value); } }
		public string PurchaseDate { get; protected set; }
		public string Series { get; protected set; }
		public string Title { get; protected set; }
		public string Length { get; protected set; }
		public string Authors { get; protected set; }
		public string Narrators { get; protected set; }
		public string Category { get; protected set; }
		public string Misc { get; protected set; }
		public string Description { get; protected set; }
		public string ProductRating { get; protected set; }
		public string MyRating { get; protected set; }

		protected bool? _remove = false;
		public abstract bool? Remove { get; set; }
		public abstract LiberateButtonStatus2 Liberate { get; }
		public abstract BookTags BookTags { get; }
		public abstract bool IsSeries { get; }
		public abstract bool IsEpisode { get; }
		public abstract bool IsBook { get; }
		public IBrush BackgroundBrush => IsEpisode ? App.SeriesEntryGridBackgroundBrush : null;

		#endregion

		#region Sorting

		public GridEntry2() => _memberValues = CreateMemberValueDictionary();

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
			{ typeof(LiberateButtonStatus2), new ObjectComparer<LiberateButtonStatus2>() },
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
			if (e.Definition.PictureId == Book.PictureId)
			{
				using var ms = new System.IO.MemoryStream(e.Picture);
				Cover = new Avalonia.Media.Imaging.Bitmap(ms);
				PictureStorage.PictureCached -= PictureStorage_PictureCached;
			}
		}

		#endregion

		#region Static library display functions		

		/// <summary>This information should not change during <see cref="GridEntry2"/> lifetime, so call only once.</summary>
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
		/// This information should not change during <see cref="GridEntry2"/> lifetime, so call only once.
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

		~GridEntry2()
		{
			PictureStorage.PictureCached -= PictureStorage_PictureCached;
		}
	}
}
