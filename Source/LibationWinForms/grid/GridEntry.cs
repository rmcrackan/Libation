using DataLayer;
using Dinah.Core.DataBinding;
using Dinah.Core.Drawing;
using LibationFileManager;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;using System.Drawing;
using System.Linq;

namespace LibationWinForms
{
	public interface IHierarchical<T> where T : class
	{
		T Parent { get; }
	}

	public class LiberateButtonStatus : IComparable
	{
		public LiberatedStatus BookStatus;
		public LiberatedStatus? PdfStatus;
		public bool IsSeries;
		public bool Expanded;

		public int CompareTo(object obj)
		{
			if (obj is not LiberateButtonStatus second) return -1;

			if (IsSeries && !second.IsSeries) return -1;
			else if (!IsSeries && second.IsSeries) return 1;
			else if (IsSeries && second.IsSeries) return 0;
			else if (BookStatus == LiberatedStatus.Liberated && second.BookStatus != LiberatedStatus.Liberated) return -1;
			else if (BookStatus != LiberatedStatus.Liberated && second.BookStatus == LiberatedStatus.Liberated) return 1;
			else return BookStatus.CompareTo(second.BookStatus);
		}
	}

	public abstract class GridEntry : AsyncNotifyPropertyChanged, IMemberComparable, IHierarchical<GridEntry>
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

		[Browsable(false)]
		public new bool InvokeRequired => base.InvokeRequired;
		[Browsable(false)]
		public GridEntry Parent { get; set; }
		[Browsable(false)]
		public abstract DateTime DateAdded { get; }
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
		public abstract object GetMemberValue(string memberName);
		#endregion
		public IComparer GetMemberComparer(Type memberType) => _memberTypeComparers[memberType];

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
		public static IEnumerable<SeriesEntry> Series(this IEnumerable<GridEntry> gridEntries) 
			=> gridEntries.Where(i => i is SeriesEntry).Cast<SeriesEntry>();
		public static IEnumerable<LibraryBookEntry> LibraryBooks(this IEnumerable<GridEntry> gridEntries) 
			=> gridEntries.Where(i => i is LibraryBookEntry).Cast<LibraryBookEntry>();
	}
}
