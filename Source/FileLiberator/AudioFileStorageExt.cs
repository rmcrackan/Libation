using System;
using System.Collections.Generic;
using System.Linq;
using DataLayer;
using LibationFileManager;

namespace FileLiberator
{
	public static class AudioFileStorageExt
	{
		/// <summary>
		/// DownloadDecryptBook:
		/// File path for where to move files into.
		/// Path: directory nested inside of Books directory
		/// File name: n/a
		/// </summary>
		public static string GetDestinationDirectory(this AudioFileStorage _, LibraryBook libraryBook)
		{
			if (libraryBook.Book.IsEpisodeChild() && Configuration.Instance.SavePodcastsToParentFolder)
			{
				var series = libraryBook.Book.SeriesLink.SingleOrDefault();
				if (series is not null)
				{
					var seriesParent = ApplicationServices.DbContexts.GetContext().GetLibraryBook_Flat_NoTracking(series.Series.AudibleSeriesId);

					if (seriesParent is not null)
					{
						var baseDir = Templates.Folder.GetFilename(seriesParent.ToDto(), AudibleFileStorage.BooksDirectory, "");
						return Templates.Folder.GetFilename(libraryBook.ToDto(), baseDir, "");
					}
				}
			}
			return Templates.Folder.GetFilename(libraryBook.ToDto(), AudibleFileStorage.BooksDirectory, "");
		}

		/// <summary>
		/// DownloadDecryptBook:
		/// Path: in progress directory.
		/// File name: final file name.
		/// </summary>
		public static string GetInProgressFilename(this AudioFileStorage _, LibraryBook libraryBook, string extension)
			=> Templates.File.GetFilename(libraryBook.ToDto(), AudibleFileStorage.DecryptInProgressDirectory, extension, returnFirstExisting: true);

		/// <summary>
		/// PDF: audio file does not exist
		/// </summary>
		public static string GetBooksDirectoryFilename(this AudioFileStorage _, LibraryBook libraryBook, string extension)
			=> Templates.File.GetFilename(libraryBook.ToDto(), AudibleFileStorage.BooksDirectory, extension);

		/// <summary>
		/// PDF: audio file already exists
		/// </summary>
		public static string GetCustomDirFilename(this AudioFileStorage _, LibraryBook libraryBook, string dirFullPath, string extension)
			=> Templates.File.GetFilename(libraryBook.ToDto(), dirFullPath, extension);
	}
}
