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
            if (cachedFile is not null && File.Exists(cachedFile))
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

    internal class AaxcFileStorage : AudibleFileStorage
    {
        internal AaxcFileStorage() : base(FileType.AAXC) { }

        protected override string GetFilePathCustom(string productId)
        {
            var regex = GetBookSearchRegex(productId);
            return FileUtility
                .SaferEnumerateFiles(DownloadsInProgressDirectory, "*.*", SearchOption.AllDirectories)
                .FirstOrDefault(s => regex.IsMatch(s));
        }

        public bool Exists(string productId) => GetFilePath(productId) != null;
    }

    public class AudioFileStorage : AudibleFileStorage
    {
        internal AudioFileStorage() : base(FileType.Audio)
            => BookDirectoryFiles ??= new BackgroundFileSystem(BooksDirectory, "*.*", SearchOption.AllDirectories);

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

        public void Refresh() => BookDirectoryFiles.RefreshFiles();

        public string GetPath(string productId) => GetFilePath(productId);
    }
}
