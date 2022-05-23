using DataLayer;
using Dinah.Core.DataBinding;
using Dinah.Core.Drawing;
using LibationFileManager;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;

namespace LibationWinForms
{
	public interface IHierarchical<T> where T : class
	{
		T Parent { get; }
		List<T> Children { get; }
	}
	internal class LiberateStatus
	{
		public LiberatedStatus BookStatus;
		public LiberatedStatus? PdfStatus;
		public bool IsSeries;
		public bool Expanded;
	}

	internal abstract class GridEntry : AsyncNotifyPropertyChanged, IMemberComparable, IHierarchical<GridEntry>
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
		public GridEntry Parent { get; set; }
		public List<GridEntry> Children { get; set; }
		public abstract string ProductRating { get; protected set; }
		public abstract string PurchaseDate { get; protected set; }
		public abstract DateTime DateAdded { get; }
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
		public abstract LiberateStatus Liberate { get; }
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
			{ typeof(LiberatedStatus), new ObjectComparer<LiberatedStatus>() },
		};

		~GridEntry()
		{
			PictureStorage.PictureCached -= PictureStorage_PictureCached;
		}
	}
}
