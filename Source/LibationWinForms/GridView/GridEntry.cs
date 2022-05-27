using DataLayer;
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
	public abstract class GridEntry : AsyncNotifyPropertyChanged, IMemberComparable
	{
		protected abstract Book Book { get; }

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
		public new bool InvokeRequired => base.InvokeRequired;
		public abstract DateTime DateAdded { get; }
		public abstract float SeriesIndex { get; }
		public abstract string ProductRating { get; protected set; }
		public abstract string PurchaseDate { get; protected set; }
		public abstract string MyRating { get; protected set; }
		public abstract string Series { get; protected set; }
		public abstract string Title { get; protected set; }
		public abstract string Length { get; protected set; }
		public abstract string Authors { get; protected set; }
		public abstract string Narrators { get; protected set; }
		public abstract string Category { get; protected set; }
		public abstract string Misc { get; protected set; }
		public abstract string Description { get; protected set; }
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

		~GridEntry()
		{
			PictureStorage.PictureCached -= PictureStorage_PictureCached;
		}
	}

	internal static class GridEntryExtensions
	{
#nullable enable
		public static IEnumerable<SeriesEntry> Series(this IEnumerable<GridEntry> gridEntries)
			=> gridEntries.OfType<SeriesEntry>();
		public static IEnumerable<LibraryBookEntry> LibraryBooks(this IEnumerable<GridEntry> gridEntries)
			=> gridEntries.OfType<LibraryBookEntry>();
		public static LibraryBookEntry? FindBookByAsin(this IEnumerable<LibraryBookEntry> gridEntries, string audibleProductID)
			=> gridEntries.FirstOrDefault(i => i.AudibleProductId == audibleProductID);
		public static SeriesEntry? FindBookSeriesEntry(this IEnumerable<GridEntry> gridEntries, IEnumerable<SeriesBook> matchSeries)
			=> gridEntries.Series().FirstOrDefault(i => matchSeries.Any(s => s.Series.Name == i.Series));
		public static IEnumerable<SeriesEntry> EmptySeries(this IEnumerable<GridEntry> gridEntries)
			=> gridEntries.Series().Where(i => i.Children.Count == 0);
		public static bool IsEpisodeChild(this LibraryBook lb) => lb.Book.ContentType == ContentType.Episode;
	}
}
