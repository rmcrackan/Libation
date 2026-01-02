using AudibleApi.Common;
using DataLayer;
using System;
using System.Threading.Tasks;

namespace LibationUiBase.SeriesView
{
	/// <summary>
	/// base view model for the Series Viewer 'Availability' button column
	/// </summary>
	public abstract class SeriesButton : ReactiveObject, IComparable
	{
		private bool inLibrary;
		protected Item Item { get; }
		public abstract string DisplayText { get; }
		public abstract bool HasButtonAction { get; }
		public abstract bool Enabled { get; protected set; }
		public bool InLibrary
		{
			get => inLibrary;
			protected set
			{
				if (inLibrary != value)
				{
					inLibrary = value;
					RaisePropertyChanged(nameof(InLibrary));
					RaisePropertyChanged(nameof(DisplayText));
				}
			}
		}

		protected SeriesButton(Item item, bool inLibrary)
		{
			Item = item;
			this.inLibrary = inLibrary;
		}

		public abstract Task PerformClickAsync(LibraryBook accountBook);

		public override string ToString() => DisplayText;

		public abstract int CompareTo(object? ob);
	}
}
