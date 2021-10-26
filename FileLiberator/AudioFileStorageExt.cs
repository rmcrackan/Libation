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
        private static void AddParameterReplacement(this FileTemplate fileTemplate, TemplateTags templateTags, object value)
            => fileTemplate.AddParameterReplacement(templateTags.TagName, value);

        internal class MultipartRenamer
        {
            public LibraryBook libraryBook { get; }

            public MultipartRenamer(LibraryBook libraryBook) => this.libraryBook = libraryBook;

            internal string MultipartFilename(string outputFileName, int partsPosition, int partsTotal, AAXClean.NewSplitCallback newSplitCallback)
                => MultipartFilename(Configuration.Instance.ChapterFileTemplate, AudibleFileStorage.DecryptInProgressDirectory, Path.GetExtension(outputFileName), partsPosition, partsTotal, newSplitCallback?.Chapter?.Title ?? "");

            internal string MultipartFilename(string template, string fullDirPath, string extension, int partsPosition, int partsTotal, string chapterTitle)
            {
                var fileTemplate = GetFileTemplateSingle(template, libraryBook, fullDirPath, extension);

                fileTemplate.AddParameterReplacement(TemplateTags.ChCount, partsTotal);
                fileTemplate.AddParameterReplacement(TemplateTags.ChNumber, partsPosition);
                fileTemplate.AddParameterReplacement(TemplateTags.ChNumber0, FileUtility.GetSequenceFormatted(partsPosition, partsTotal));
                fileTemplate.AddParameterReplacement(TemplateTags.ChTitle, chapterTitle);

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
            => GetFileTemplateSingle(Configuration.Instance.FolderTemplate, libraryBook, AudibleFileStorage.BooksDirectory, null)
            .GetFilePath();

        public static string GetCustomDirFilename(this AudioFileStorage _, LibraryBook libraryBook, string dirFullPath, string extension)
            => GetFileTemplateSingle(Configuration.Instance.FileTemplate, libraryBook, dirFullPath, extension)
            .GetFilePath();

        internal static FileTemplate GetFileTemplateSingle(string template, LibraryBook libraryBook, string dirFullPath, string extension)
        {
            ArgumentValidator.EnsureNotNullOrWhiteSpace(template, nameof(template));
            ArgumentValidator.EnsureNotNull(libraryBook, nameof(libraryBook));
            ArgumentValidator.EnsureNotNullOrWhiteSpace(dirFullPath, nameof(dirFullPath));

            var fullfilename = Path.Combine(dirFullPath, template + FileUtility.GetStandardizedExtension(extension));
            var fileTemplate = new FileTemplate(fullfilename) { IllegalCharacterReplacements = "_" };

            var title = libraryBook.Book.Title ?? "";

            fileTemplate.AddParameterReplacement(TemplateTags.Id, libraryBook.Book.AudibleProductId);
            fileTemplate.AddParameterReplacement(TemplateTags.Title, title);
            fileTemplate.AddParameterReplacement(TemplateTags.TitleShort, title.IndexOf(':') < 1 ? title : title.Substring(0, title.IndexOf(':')));
            fileTemplate.AddParameterReplacement(TemplateTags.Author, libraryBook.Book.AuthorNames);
            fileTemplate.AddParameterReplacement(TemplateTags.FirstAuthor, libraryBook.Book.Authors.FirstOrDefault()?.Name);
            fileTemplate.AddParameterReplacement(TemplateTags.Narrator, libraryBook.Book.NarratorNames);
            fileTemplate.AddParameterReplacement(TemplateTags.FirstNarrator, libraryBook.Book.Narrators.FirstOrDefault()?.Name);

            var seriesLink = libraryBook.Book.SeriesLink.FirstOrDefault();
            fileTemplate.AddParameterReplacement(TemplateTags.Series, seriesLink?.Series.Name);
            fileTemplate.AddParameterReplacement(TemplateTags.SeriesNumber, seriesLink?.Order);

            return fileTemplate;
        }
    }
}
