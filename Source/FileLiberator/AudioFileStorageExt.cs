using AaxDecrypter;
using DataLayer;
using LibationFileManager;
using LibationFileManager.Templates;
using System;
using System.Linq;

namespace FileLiberator;

public static class AudioFileStorageExt
{
	/// <summary>
	/// DownloadDecryptBook:
	/// File path for where to move files into.
	/// Path: directory nested inside of Books directory
	/// File name: n/a
	/// </summary>
	public static string GetDestinationDirectory(this AudioFileStorage _, LibraryBook libraryBook, Configuration? config = null)
	{
		if (AudibleFileStorage.BooksDirectory is not { } books)
			throw new InvalidOperationException("Books directory is not set.");

		config ??= Configuration.Instance;
		if (libraryBook.Book.IsEpisodeChild() && config.SavePodcastsToParentFolder)
		{
			var series = libraryBook.Book.SeriesLink.SingleOrDefault();
			if (series is not null)
			{
				LibraryBook? seriesParent = ApplicationServices.DbContexts.GetLibraryBook_Flat_NoTracking(
					series.Series.AudibleSeriesId,
					account: libraryBook.Account);
				if (seriesParent is not null)
				{
					return Templates.Folder.GetFilename(seriesParent.ToDto(), books, "");
				}
			}
		}
		return Templates.Folder.GetFilename(libraryBook.ToDto(), books, "");
	}

	/// <summary>
	/// A file name from the file template, directly under the Books directory rather than in the book's own
	/// folder. Only the "save a copy of the cover art" dialogs use this, and only for the name they suggest.
	/// </summary>
	public static string GetBooksDirectoryFilename(this AudioFileStorage _, LibraryBook libraryBook, string extension, bool returnFirstExisting = false)
		=> AudibleFileStorage.BooksDirectory is { } books ? Templates.File.GetFilename(libraryBook.ToDto(), books, extension, null, returnFirstExisting)
		: throw new InvalidOperationException("Books directory is not set.");

	/// <summary>
	/// A file name from the file template, in a directory the caller has already chosen.
	/// </summary>
	public static string GetCustomDirFilename(this AudioFileStorage _, LibraryBook libraryBook, string dirFullPath, string extension, MultiConvertFileProperties? partProperties = null, bool returnFirstExisting = false)
		=> partProperties is null ? Templates.File.GetFilename(libraryBook.ToDto(), dirFullPath, extension, returnFirstExisting: returnFirstExisting)
		: Templates.ChapterFile.GetFilename(libraryBook.ToDto(), partProperties, dirFullPath, extension, returnFirstExisting: returnFirstExisting);
}
