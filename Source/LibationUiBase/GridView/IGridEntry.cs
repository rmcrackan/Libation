using DataLayer;
using Dinah.Core.DataBinding;
using System;
using System.ComponentModel;

namespace LibationUiBase.GridView
{
	public interface IGridEntry : IMemberComparable, INotifyPropertyChanged
	{
		EntryStatus Liberate { get; }
		float SeriesIndex { get; }
		string AudibleProductId { get; }
		string LongDescription { get; }
		LibraryBook LibraryBook { get; }
		Book Book { get; }
		DateTime DateAdded { get; }
		bool? Remove { get; set; }
		string PurchaseDate { get; }
		object Cover { get; }
		string Length { get; }
		LastDownloadStatus LastDownload { get; }
		string Series { get; }
		SeriesOrder SeriesOrder { get; }
		string Title { get; }
		string Authors { get; }
		string Narrators { get; }
		string Category { get; }
		string Misc { get; }
		string Description { get; }
		Rating ProductRating { get; }
		Rating MyRating { get; set; }
		string BookTags { get; }
		void UpdateLibraryBook(LibraryBook libraryBook);
	}
}
