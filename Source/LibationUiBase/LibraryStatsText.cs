using System.Text;
using ApplicationServices;
using LibationUiBase.Resources;

namespace LibationUiBase;

/// <summary>The backup counts line in the main window's status bar, in both UIs.</summary>
public static class LibraryStatsText
{
	public static string Of(LibraryCommands.LibraryStats stats)
		=> stats.HasPdfResults ? $"{Books(stats)}  |  {Pdfs(stats)}" : Books(stats);

	private static string Books(LibraryCommands.LibraryStats s)
	{
		if (!s.HasBookResults)
			return Strings.LibraryStatsText_NoBooks;

		if (!s.HasPendingBooks && s.booksError + s.booksUnavailable == 0)
			return string.Format(Strings.LibraryStatsText_AllBooksBackedUp, Counted.Books(s.booksFullyBackedUp));

		var sb = new StringBuilder(string.Format(Strings.LibraryStatsText_Backups, s.booksNoProgress, s.booksDownloadedOnly, s.booksFullyBackedUp));
		if (s.booksError > 0)
			sb.Append(string.Format(Strings.LibraryStatsText_Errors, s.booksError));
		if (s.booksUnavailable > 0)
			sb.Append(string.Format(Strings.LibraryStatsText_Unavailable, s.booksUnavailable));
		return sb.ToString();
	}

	private static string Pdfs(LibraryCommands.LibraryStats s)
	{
		if (s.pdfsNotDownloaded + s.pdfsUnavailable == 0)
			return string.Format(Strings.LibraryStatsText_AllPdfsDownloaded, s.pdfsDownloaded);

		var sb = new StringBuilder(string.Format(Strings.LibraryStatsText_Pdfs, s.pdfsNotDownloaded, s.pdfsDownloaded));
		if (s.pdfsUnavailable > 0)
			sb.Append(string.Format(Strings.LibraryStatsText_Unavailable, s.pdfsUnavailable));
		return sb.ToString();
	}
}
