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
        private static string TEMP_SINGLE_TEMPLATE { get; } = "<title> [<id>]";
        private static string TEMP_DIR_TEMPLATE { get; } = "<title short> [<id>]";
        private static string TEMP_MULTI_TEMPLATE { get; } = "<title> [<id>] - <ch# 0> - <ch title>";

        internal class MultipartRenamer
        {
            public LibraryBook libraryBook { get; }

            public MultipartRenamer(LibraryBook libraryBook) => this.libraryBook = libraryBook;

            internal string MultipartFilename(string outputFileName, int partsPosition, int partsTotal, AAXClean.NewSplitCallback newSplitCallback)
                => MultipartFilename(TEMP_MULTI_TEMPLATE, AudibleFileStorage.DecryptInProgressDirectory, Path.GetExtension(outputFileName), partsPosition, partsTotal, newSplitCallback?.Chapter?.Title ?? "");

            internal string MultipartFilename(string template, string fullDirPath, string extension, int partsPosition, int partsTotal, string chapterTitle)
            {
                var fileTemplate = GetFileTemplateSingle(template, libraryBook, fullDirPath, extension);

                fileTemplate.AddParameterReplacement("ch count", partsTotal.ToString());
                fileTemplate.AddParameterReplacement("ch#", partsPosition.ToString());
                fileTemplate.AddParameterReplacement("ch# 0", FileUtility.GetSequenceFormatted(partsPosition, partsTotal));
                fileTemplate.AddParameterReplacement("ch title", chapterTitle);

                return fileTemplate.GetFilePath();
            }
        }

        public static Func<string, int, int, AAXClean.NewSplitCallback, string> CreateMultipartRenamerFunc(this AudioFileStorage _, LibraryBook libraryBook)
            => new MultipartRenamer(libraryBook).MultipartFilename;

        public static string GetInProgressFilename(this AudioFileStorage _, LibraryBook libraryBook, string extension)
            => GetCustomDirFilename(_, libraryBook, AudibleFileStorage.DecryptInProgressDirectory, extension);

        public static string GetBooksDirectoryFilename(this AudioFileStorage _, LibraryBook libraryBook, string extension)
            => GetCustomDirFilename(_, libraryBook, AudibleFileStorage.BooksDirectory, extension);

        public static string GetDestinationDirectory(this AudioFileStorage _, LibraryBook libraryBook)
            => GetFileTemplateSingle(TEMP_DIR_TEMPLATE, libraryBook, AudibleFileStorage.BooksDirectory, null)
            .GetFilePath();

        public static string GetCustomDirFilename(this AudioFileStorage _, LibraryBook libraryBook, string dirFullPath, string extension)
            => GetFileTemplateSingle(TEMP_SINGLE_TEMPLATE, libraryBook, dirFullPath, extension)
            .GetFilePath();

        internal static FileTemplate GetFileTemplateSingle(string template, LibraryBook libraryBook, string dirFullPath, string extension)
        {
            ArgumentValidator.EnsureNotNullOrWhiteSpace(template, nameof(template));
            ArgumentValidator.EnsureNotNull(libraryBook, nameof(libraryBook));
            ArgumentValidator.EnsureNotNullOrWhiteSpace(dirFullPath, nameof(dirFullPath));

            var fullfilename = Path.Combine(dirFullPath, template + FileUtility.GetStandardizedExtension(extension));
            var fileTemplate = new FileTemplate(fullfilename) { IllegalCharacterReplacements = "_" };

            var title = libraryBook.Book.Title ?? "";

            fileTemplate.AddParameterReplacement("title", title);
            fileTemplate.AddParameterReplacement("title short", title.IndexOf(':') < 1 ? title : title.Substring(0, title.IndexOf(':')));
            fileTemplate.AddParameterReplacement("id", libraryBook.Book.AudibleProductId);

            return fileTemplate;
        }
    }
}
