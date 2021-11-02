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
        public class MultipartRenamer
        {
            private LibraryBookDto libraryBookDto { get; }

            public MultipartRenamer(LibraryBook libraryBook) : this(libraryBook.ToDto()) { }
            public MultipartRenamer(LibraryBookDto libraryBookDto) => this.libraryBookDto = libraryBookDto;

            internal string MultipartFilename(AaxDecrypter.MultiConvertFileProperties props)
                => MultipartFilename(props, Configuration.Instance.ChapterFileTemplate, AudibleFileStorage.DecryptInProgressDirectory);

            public string MultipartFilename(AaxDecrypter.MultiConvertFileProperties props, string template, string fullDirPath)
            {
                var fileNamingTemplate = GetFileNamingTemplate(template, libraryBookDto, fullDirPath, Path.GetExtension(props.OutputFileName));

                fileNamingTemplate.AddParameterReplacement(TemplateTags.ChCount, props.PartsTotal);
                fileNamingTemplate.AddParameterReplacement(TemplateTags.ChNumber, props.PartsPosition);
                fileNamingTemplate.AddParameterReplacement(TemplateTags.ChNumber0, FileUtility.GetSequenceFormatted(props.PartsPosition, props.PartsTotal));
                fileNamingTemplate.AddParameterReplacement(TemplateTags.ChTitle, props.Title ?? "");

                return fileNamingTemplate.GetFilePath();
            }
        }

        public static Func<AaxDecrypter.MultiConvertFileProperties, string> CreateMultipartRenamerFunc(this AudioFileStorage _, LibraryBook libraryBook)
            => new MultipartRenamer(libraryBook).MultipartFilename;
        public static Func<AaxDecrypter.MultiConvertFileProperties, string> CreateMultipartRenamerFunc(this AudioFileStorage _, LibraryBookDto libraryBookDto)
            => new MultipartRenamer(libraryBookDto).MultipartFilename;

        /// <summary>
        /// DownloadDecryptBook:
        /// Path: in progress directory.
        /// File name: final file name.
        /// </summary>
        public static string GetInProgressFilename(this AudioFileStorage _, LibraryBook libraryBook, string extension)
            => GetFileNamingTemplate(Configuration.Instance.FileTemplate, libraryBook.ToDto(), AudibleFileStorage.DecryptInProgressDirectory, extension)
            .GetFilePath();

        /// <summary>
        /// DownloadDecryptBook:
        /// File path for where to move files into.
        /// Path: directory nested inside of Books directory
        /// File name: n/a
        /// </summary>
        public static string GetDestinationDirectory(this AudioFileStorage _, LibraryBook libraryBook)
            => GetFileNamingTemplate(Configuration.Instance.FolderTemplate, libraryBook.ToDto(), AudibleFileStorage.BooksDirectory, null)
            .GetFilePath();

        /// <summary>
        /// PDF: audio file does not exist
        /// </summary>
        public static string GetBooksDirectoryFilename(this AudioFileStorage _, LibraryBook libraryBook, string extension)
            => GetFileNamingTemplate(Configuration.Instance.FileTemplate, libraryBook.ToDto(), AudibleFileStorage.BooksDirectory, extension)
            .GetFilePath();

        /// <summary>
        /// PDF: audio file already exists
        /// </summary>
        public static string GetCustomDirFilename(this AudioFileStorage _, LibraryBook libraryBook, string dirFullPath, string extension)
            => GetFileNamingTemplate(Configuration.Instance.FileTemplate, libraryBook.ToDto(), dirFullPath, extension)
            .GetFilePath();

        public static FileNamingTemplate GetFileNamingTemplate(string template, LibraryBookDto libraryBookDto, string dirFullPath, string extension)
        {
            ArgumentValidator.EnsureNotNullOrWhiteSpace(template, nameof(template));
            ArgumentValidator.EnsureNotNull(libraryBookDto, nameof(libraryBookDto));

            dirFullPath = dirFullPath?.Trim() ?? "";
            var t = template + FileUtility.GetStandardizedExtension(extension);
            var fullfilename = dirFullPath == "" ? t : Path.Combine(dirFullPath, t);

            var fileNamingTemplate = new FileNamingTemplate(fullfilename) { IllegalCharacterReplacements = "_" };

            var title = libraryBookDto.Title ?? "";
            var titleShort = title.IndexOf(':') < 1 ? title : title.Substring(0, title.IndexOf(':'));

            fileNamingTemplate.AddParameterReplacement(TemplateTags.Id, libraryBookDto.AudibleProductId);
            fileNamingTemplate.AddParameterReplacement(TemplateTags.Title, title);
            fileNamingTemplate.AddParameterReplacement(TemplateTags.TitleShort, titleShort);
            fileNamingTemplate.AddParameterReplacement(TemplateTags.Author, libraryBookDto.AuthorNames);
            fileNamingTemplate.AddParameterReplacement(TemplateTags.FirstAuthor, libraryBookDto.FirstAuthor);
            fileNamingTemplate.AddParameterReplacement(TemplateTags.Narrator, libraryBookDto.NarratorNames);
            fileNamingTemplate.AddParameterReplacement(TemplateTags.FirstNarrator, libraryBookDto.FirstNarrator);
            fileNamingTemplate.AddParameterReplacement(TemplateTags.Series, libraryBookDto.SeriesName);
            fileNamingTemplate.AddParameterReplacement(TemplateTags.SeriesNumber, libraryBookDto.SeriesNumber);
            fileNamingTemplate.AddParameterReplacement(TemplateTags.Account, libraryBookDto.Account);
            fileNamingTemplate.AddParameterReplacement(TemplateTags.Locale, libraryBookDto.Locale);

            return fileNamingTemplate;
        }
    }
}
