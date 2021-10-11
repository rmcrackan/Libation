using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using FileManager;

namespace LibationFileManager
{
    public abstract class AudibleFileStorage
    {
        protected abstract string GetFilePathCustom(string productId);

        #region static
        public static string DownloadsInProgressDirectory => Directory.CreateDirectory(Path.Combine(Configuration.Instance.InProgress, "DownloadsInProgress")).FullName;
        public static string DecryptInProgressDirectory => Directory.CreateDirectory(Path.Combine(Configuration.Instance.InProgress, "DecryptInProgress")).FullName;
        public static string PdfDirectory => BooksDirectory;

        private static AaxcFileStorage AAXC { get; } = new AaxcFileStorage();
        public static bool AaxcExists(string productId) => AAXC.Exists(productId);

        public static AudioFileStorage Audio { get; } = new AudioFileStorage();

        public static string BooksDirectory
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Configuration.Instance.Books))
                    Configuration.Instance.Books = Path.Combine(Configuration.UserProfile, "Books");
                return Directory.CreateDirectory(Configuration.Instance.Books).FullName;
            }
        }
        #endregion

        #region instance
        private FileType FileType { get; }
        private string regexTemplate { get; }

        protected AudibleFileStorage(FileType fileType)
        {
            FileType = fileType;

            var extAggr = FileTypes.GetExtensions(FileType).Aggregate((a, b) => $"{a}|{b}");
            regexTemplate = $@"{{0}}.*?\.({extAggr})$";
        }

        protected string GetFilePath(string productId)
        {
            // primary lookup
            var cachedFile = FilePathCache.GetFirstPath(productId, FileType);
            if (cachedFile != null)
                return cachedFile;

            // secondary lookup attempt
            var firstOrNull = GetFilePathCustom(productId);
            if (firstOrNull is not null)
                FilePathCache.Insert(productId, firstOrNull);

            return firstOrNull;
        }

        protected Regex GetBookSearchRegex(string productId)
        {
            var pattern = string.Format(regexTemplate, productId);
            return new Regex(pattern, RegexOptions.IgnoreCase);
        }
        #endregion
    }

    public class AudioFileStorage : AudibleFileStorage
    {
        private static BackgroundFileSystem BookDirectoryFiles { get; set; }
        private static object bookDirectoryFilesLocker { get; } = new();
        protected override string GetFilePathCustom(string productId)
        {
            // If user changed the BooksDirectory: reinitialize
            lock (bookDirectoryFilesLocker)
                if (BooksDirectory != BookDirectoryFiles.RootDirectory)
                    BookDirectoryFiles = new BackgroundFileSystem(BooksDirectory, "*.*", SearchOption.AllDirectories);

            var regex = GetBookSearchRegex(productId);
            return BookDirectoryFiles.FindFile(regex);
        }

        internal AudioFileStorage() : base(FileType.Audio)
            => BookDirectoryFiles ??= new BackgroundFileSystem(BooksDirectory, "*.*", SearchOption.AllDirectories);

        public void Refresh() => BookDirectoryFiles.RefreshFiles();

        public string GetDestDir(string title, string asin)
        {
            // to prevent the paths from getting too long, we don't need after the 1st ":" for the folder
            var underscoreIndex = title.IndexOf(':');
            var titleDir
                = underscoreIndex < 4
                ? title
                : title.Substring(0, underscoreIndex);
            var finalDir = FileUtility.GetValidFilename(BooksDirectory, titleDir, null, asin);
            return finalDir;
        }

        public string GetPath(string productId) => GetFilePath(productId);
    }

    internal class AaxcFileStorage : AudibleFileStorage
    {
        protected override string GetFilePathCustom(string productId)
        {
            var regex = GetBookSearchRegex(productId);
            return Directory
                .EnumerateFiles(DownloadsInProgressDirectory, "*.*", SearchOption.AllDirectories)
                .FirstOrDefault(s => regex.IsMatch(s));
        }

        internal AaxcFileStorage() : base(FileType.AAXC) { }

        public bool Exists(string productId) => GetFilePath(productId) != null;
    }
}
