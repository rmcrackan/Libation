using AudibleApi.Common;
using DataLayer;
using Dinah.Core.Threading;
using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace LibationUiBase.SeriesView
{
	/// <summary>
	/// base view model for the Series Viewer 'Availability' button column
	/// </summary>
	public abstract class SeriesButton : SynchronizeInvoker, IComparable, INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;
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
					OnPropertyChanged(nameof(InLibrary));
					OnPropertyChanged(nameof(DisplayText));
				}
			}
		}

		protected SeriesButton(Item item, bool inLibrary)
		{
			Item = item;
			this.inLibrary = inLibrary;
		}

		public abstract Task PerformClickAsync(LibraryBook accountBook);

		protected void OnPropertyChanged(string propertyName)
			=> Invoke(() => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)));

		public override string ToString() => DisplayText;

		public abstract int CompareTo(object ob);
	}
}
