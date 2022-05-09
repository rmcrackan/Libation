using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DataLayer;
using Dinah.Core;
using FileManager;
using LibationFileManager;

namespace FileLiberator
{
    public static class AudioFileStorageExt
    {
        private class MultipartRenamer
        {
            private LibraryBook libraryBook { get; }

            internal MultipartRenamer(LibraryBook libraryBook) => this.libraryBook = libraryBook;

            internal string MultipartFilename(AaxDecrypter.MultiConvertFileProperties props)
                => Templates.ChapterFile.GetFilename(libraryBook.ToDto(), props);
        }

        public static Func<AaxDecrypter.MultiConvertFileProperties, string> CreateMultipartRenamerFunc(this AudioFileStorage _, LibraryBook libraryBook)
            => new MultipartRenamer(libraryBook).MultipartFilename;

        /// <summary>
        /// DownloadDecryptBook:
        /// File path for where to move files into.
        /// Path: directory nested inside of Books directory
        /// File name: n/a
        /// </summary>
        public static string GetDestinationDirectory(this AudioFileStorage _, LibraryBook libraryBook)
            => Templates.Folder.GetFilename(libraryBook.ToDto());

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
