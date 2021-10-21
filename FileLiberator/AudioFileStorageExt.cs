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
    public class MultipartRenamer
    {
        LibraryBook libraryBook;

        public MultipartRenamer(LibraryBook libraryBook) => this.libraryBook = libraryBook;

        public string MultipartFilename(string outputFileName, int partsPosition, int partsTotal, AAXClean.NewSplitCallback newSplitCallback)
        {
            var template = Path.ChangeExtension(outputFileName, null) + " - <chapter> - <title>" + Path.GetExtension(outputFileName);

            var fileTemplate = new FileTemplate(template) { IllegalCharacterReplacements = " " };
            fileTemplate.AddParameterReplacement("chapter", FileUtility.GetSequenceFormatted(partsPosition, partsTotal));
            fileTemplate.AddParameterReplacement("title", newSplitCallback?.Chapter?.Title ?? "");

            return fileTemplate.GetFilePath();
        }
    }

	public static class AudioFileStorageExt
    {
        public static MultipartRenamer CreateMultipartRenamer(this AudioFileStorage _, LibraryBook libraryBook) => CreateMultipartRenamer(libraryBook);
        public static MultipartRenamer CreateMultipartRenamer(LibraryBook libraryBook) => new(libraryBook);

        public static string GetInProgressFilename(this AudioFileStorage _, LibraryBook libraryBook, string extension)
            => GetInProgressFilename(libraryBook, extension);
        public static string GetInProgressFilename(LibraryBook libraryBook, string extension)
            => GetValidFilename(AudibleFileStorage.DecryptInProgressDirectory, libraryBook.Book.Title, extension, libraryBook);

        public static string GetBooksDirectoryFilename(this AudioFileStorage _, LibraryBook libraryBook, string extension)
            => GetBooksDirectoryFilename(libraryBook, extension);
        public static string GetBooksDirectoryFilename(LibraryBook libraryBook, string extension)
            => GetValidFilename(AudibleFileStorage.BooksDirectory, libraryBook.Book.Title, extension, libraryBook);

        public static string CreateDestinationDirectory(this AudioFileStorage _, LibraryBook libraryBook)
            => CreateDestinationDirectory(libraryBook);
        public static string CreateDestinationDirectory(LibraryBook libraryBook)
        {
            var title = libraryBook.Book.Title;

            // to prevent the paths from getting too long, we don't need after the 1st ":" for the folder
            var underscoreIndex = title.IndexOf(':');
            var titleDir
                = underscoreIndex < 4
                ? title
                : title.Substring(0, underscoreIndex);
            var destinationDir = GetValidFilename(AudibleFileStorage.BooksDirectory, titleDir, null, libraryBook);
            Directory.CreateDirectory(destinationDir);
            return destinationDir;
        }

        public static string GetValidFilename(string dirFullPath, string filename, string extension, LibraryBook libraryBook)
        {
            ArgumentValidator.EnsureNotNullOrWhiteSpace(dirFullPath, nameof(dirFullPath));
            ArgumentValidator.EnsureNotNullOrWhiteSpace(filename, nameof(filename));

            var template = $"<title> [<id>]";

            var fullfilename = Path.Combine(dirFullPath, template + FileUtility.GetStandardizedExtension(extension));

            var fileTemplate = new FileTemplate(fullfilename) { IllegalCharacterReplacements = "_" };
            fileTemplate.AddParameterReplacement("title", filename);
            fileTemplate.AddParameterReplacement("id", libraryBook.Book.AudibleProductId);
            return fileTemplate.GetFilePath();
        }
    }
}
