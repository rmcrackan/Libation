using ApplicationServices;
using DataLayer;
using LibationUiBase.GridView;
using System.Linq;

namespace LibationAvalonia.ViewModels;

public enum LibraryScope
{
	All,
	NotDownloaded,
	InProgress,
	Downloaded,
	NeedsAttention,
	Unavailable,
	Series,
	Podcasts,
	AudiblePlus
}

public static class LibraryScopes
{
	/// <summary>Mirrors the buckets in <see cref="LibraryCommands.GetCounts"/> so a sidebar count equals its list.</summary>
	public static bool Matches(this LibraryScope scope, LibraryBookEntry entry)
	{
		if (scope is LibraryScope.All)
			return true;

		var lb = entry.LibraryBook;
		var book = lb.Book;

		switch (scope)
		{
			case LibraryScope.Series: return book.IsProduct() && book.SeriesLink.Any();
			case LibraryScope.Podcasts: return !book.IsProduct();
			case LibraryScope.AudiblePlus: return lb.IsAudiblePlus;
		}

		var status = LibraryCommands.Liberated_Status(book);
		return scope switch
		{
			LibraryScope.NotDownloaded => !lb.AbsentFromLastScan && status is LiberatedStatus.NotLiberated,
			LibraryScope.InProgress => !lb.AbsentFromLastScan && status is LiberatedStatus.PartialDownload,
			LibraryScope.Downloaded => status is LiberatedStatus.Liberated,
			LibraryScope.NeedsAttention => status is LiberatedStatus.Error,
			LibraryScope.Unavailable => lb.AbsentFromLastScan && status is LiberatedStatus.NotLiberated or LiberatedStatus.PartialDownload,
			_ => true
		};
	}
}
