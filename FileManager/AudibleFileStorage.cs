using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Dinah.Core;
using Dinah.Core.Collections.Generic;

namespace FileManager
{
    public enum FileType { Unknown, Audio, AAXC, PDF, Zip }

	public abstract class AudibleFileStorage : Enumeration<AudibleFileStorage>
	{
		protected abstract string[] Extensions { get; }
		public abstract string StorageDirectory { get; }

		public static string DownloadsInProgress => Directory.CreateDirectory(Path.Combine(Configuration.Instance.InProgress, "DownloadsInProgress")).FullName;
        public static string DecryptInProgress => Directory.CreateDirectory(Path.Combine(Configuration.Instance.InProgress, "DecryptInProgress")).FullName;

        public static string PdfStorageDirectory => BooksDirectory;

		private static AaxcFileStorage AAXC { get; } = new AaxcFileStorage();
        public static bool AaxcExists(string productId) => AAXC.Exists(productId);

		#region static
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

        private static object bookDirectoryFilesLocker { get; } = new();
        internal static BackgroundFileSystem BookDirectoryFiles { get; set; }
        #endregion

        #region instance
        public FileType FileType => (FileType)Value;

		protected IEnumerable<string> extensions_noDots { get; }
        private string extAggr { get; }

        protected AudibleFileStorage(FileType fileType) : base((int)fileType, fileType.ToString())
		{
			extensions_noDots = Extensions.Select(ext => ext.ToLower().Trim('.')).ToList();
			extAggr = extensions_noDots.Aggregate((a, b) => $"{a}|{b}");
            BookDirectoryFiles ??= new BackgroundFileSystem(BooksDirectory, "*.*", SearchOption.AllDirectories);
        }

        protected string GetFilePath(string productId)
        {
            var cachedFile = FilePathCache.GetPath(productId, FileType);
            if (cachedFile != null)
                return cachedFile;

            var regex = new Regex($@"{productId}.*?\.({extAggr})$", RegexOptions.IgnoreCase);

            string firstOrNull;

            if (StorageDirectory == BooksDirectory)
            {
                //If user changed the BooksDirectory, reinitialize.
                lock (bookDirectoryFilesLocker)
                    if (StorageDirectory != BookDirectoryFiles.RootDirectory)
                        BookDirectoryFiles = new BackgroundFileSystem(StorageDirectory, "*.*", SearchOption.AllDirectories);

                firstOrNull = BookDirectoryFiles.FindFile(regex);
            }
            else
            {
                firstOrNull =
                    Directory
                    .EnumerateFiles(StorageDirectory, "*.*", SearchOption.AllDirectories)
                    .FirstOrDefault(s => regex.IsMatch(s));
            }

			if (firstOrNull is not null)
                FilePathCache.Upsert(productId, FileType, firstOrNull);

			return firstOrNull;
        }
        #endregion
    }

    public class AudioFileStorage : AudibleFileStorage
    {
        protected override string[] Extensions { get; } = new[] { "m4b", "mp3", "aac", "mp4", "m4a", "ogg", "flac" };

        // we always want to use the latest config value, therefore
        // - DO use 'get' arrow "=>"
        // - do NOT use assign "="
        public override string StorageDirectory => BooksDirectory;

        public AudioFileStorage() : base(FileType.Audio) { }

        public void Refresh() => BookDirectoryFiles.RefreshFiles();

        public string GetDestDir(string title, string asin)
        {
            // to prevent the paths from getting too long, we don't need after the 1st ":" for the folder
            var underscoreIndex = title.IndexOf(':');
            var titleDir
                = underscoreIndex < 4
                ? title
                : title.Substring(0, underscoreIndex);
            var finalDir = FileUtility.GetValidFilename(StorageDirectory, titleDir, null, asin);
            return finalDir;
        }

        public bool IsFileTypeMatch(FileInfo fileInfo)
            => extensions_noDots.ContainsInsensative(fileInfo.Extension.Trim('.'));

        public string GetPath(string productId) => GetFilePath(productId);
    }

    public class AaxcFileStorage : AudibleFileStorage
    {
        protected override string[] Extensions { get; } = new[] { "aaxc" };

        // we always want to use the latest config value, therefore
        // - DO use 'get' arrow "=>"
        // - do NOT use assign "="
        public override string StorageDirectory => DownloadsInProgress;

        public AaxcFileStorage() : base(FileType.AAXC) { }

        /// <summary>
        /// Example for full books:
        /// Search recursively in _books directory. Full book exists if either are true
        /// - a directory name has the product id and an audio file is immediately inside
        /// - any audio filename contains the product id
        /// </summary>
        public bool Exists(string productId) => GetFilePath(productId) != null;
    }
}
