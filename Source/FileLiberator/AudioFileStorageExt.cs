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
				LibraryBook? seriesParent = ApplicationServices.DbContexts.GetLibraryBook_Flat_NoTracking(series.Series.AudibleSeriesId);
				if (seriesParent is not null)
				{
					return Templates.Folder.GetFilename(seriesParent.ToDto(), books, "");
				}
			}
		}
		return Templates.Folder.GetFilename(libraryBook.ToDto(), books, "");
	}

	/// <summary>
	/// PDF: audio file does not exist
	/// </summary>
	public static string GetBooksDirectoryFilename(this AudioFileStorage _, LibraryBook libraryBook, string extension, bool returnFirstExisting = false)
		=> AudibleFileStorage.BooksDirectory is { } books ? Templates.File.GetFilename(libraryBook.ToDto(), books, extension, null, returnFirstExisting)
		: throw new InvalidOperationException("Books directory is not set.");

	/// <summary>
	/// PDF: audio file already exists
	/// </summary>
	public static string GetCustomDirFilename(this AudioFileStorage _, LibraryBook libraryBook, string dirFullPath, string extension, MultiConvertFileProperties? partProperties = null, bool returnFirstExisting = false)
		=> partProperties is null ? Templates.File.GetFilename(libraryBook.ToDto(), dirFullPath, extension, returnFirstExisting: returnFirstExisting)
		: Templates.ChapterFile.GetFilename(libraryBook.ToDto(), partProperties, dirFullPath, extension, returnFirstExisting: returnFirstExisting);
}
