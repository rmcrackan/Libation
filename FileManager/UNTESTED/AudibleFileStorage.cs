using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Dinah.Core;
using Dinah.Core.Collections.Generic;

namespace FileManager
{
    // could add images here, but for now images are stored in a well-known location
    public enum FileType { Unknown, Audio, AAX, PDF }

    /// <summary>
    /// Files are large. File contents are never read by app.
    /// Paths are varied.
    /// Files are written during download/decrypt/backup/liberate.
    /// Paths are read at app launch and during download/decrypt/backup/liberate.
    /// Many files are often looked up at once
    /// </summary>
    public sealed class AudibleFileStorage : Enumeration<AudibleFileStorage>
    {
        #region static
        public static AudibleFileStorage Audio { get; }
        public static AudibleFileStorage AAX { get; }
        public static AudibleFileStorage PDF { get; }

        public static string DownloadsInProgress { get; }
        public static string DecryptInProgress { get; }
        public static string BooksDirectory => Configuration.Instance.Books;
        // not customizable. don't move to config
        public static string DownloadsFinal { get; }
            = new DirectoryInfo(Configuration.Instance.LibationFiles).CreateSubdirectory("DownloadsFinal").FullName;

        static AudibleFileStorage()
        {
            #region init DecryptInProgress
            if (!Configuration.Instance.DecryptInProgressEnum.In("WinTemp", "LibationFiles"))
                Configuration.Instance.DecryptInProgressEnum = "WinTemp";
            var M4bRootDir
                = Configuration.Instance.DecryptInProgressEnum == "WinTemp" // else "LibationFiles"
                ? Configuration.WinTemp
                : Configuration.Instance.LibationFiles;
            DecryptInProgress = Path.Combine(M4bRootDir, "DecryptInProgress");
            Directory.CreateDirectory(DecryptInProgress);
            #endregion

            #region init DownloadsInProgress
            if (!Configuration.Instance.DownloadsInProgressEnum.In("WinTemp", "LibationFiles"))
                Configuration.Instance.DownloadsInProgressEnum = "WinTemp";
            var AaxRootDir
                = Configuration.Instance.DownloadsInProgressEnum == "WinTemp" // else "LibationFiles"
                ? Configuration.WinTemp
                : Configuration.Instance.LibationFiles;
            DownloadsInProgress = Path.Combine(AaxRootDir, "DownloadsInProgress");
            Directory.CreateDirectory(DownloadsInProgress);
            #endregion

            #region init BooksDirectory
            if (string.IsNullOrWhiteSpace(Configuration.Instance.Books))
                Configuration.Instance.Books = Path.Combine(Configuration.Instance.LibationFiles, "Books");
            Directory.CreateDirectory(Configuration.Instance.Books);
            #endregion

            // must do this in static ctor, not w/inline properties
            // static properties init before static ctor so these dir.s would still be null
            Audio = new AudibleFileStorage(FileType.Audio, BooksDirectory, "m4b", "mp3", "aac", "mp4", "m4a", "ogg", "flac");
            AAX = new AudibleFileStorage(FileType.AAX, DownloadsFinal, "aax");
            PDF = new AudibleFileStorage(FileType.PDF, BooksDirectory, "pdf", "zip");
        }
        #endregion

        #region instance
        public FileType FileType => (FileType)Value;

        public string StorageDirectory => DisplayName;

		private IEnumerable<string> extensions_noDots { get; }
		private string extAggr { get; }

        private AudibleFileStorage(FileType fileType, string storageDirectory, params string[] extensions) : base((int)fileType, storageDirectory)
		{
			extensions_noDots = extensions.Select(ext => ext.Trim('.')).ToList();
			extAggr = extensions_noDots.Aggregate((a, b) => $"{a}|{b}");
		}

        /// <summary>
        /// Example for full books:
        /// Search recursively in _books directory. Full book exists if either are true
        /// - a directory name has the product id and an audio file is immediately inside
        /// - any audio filename contains the product id
        /// </summary>
        public bool Exists(string productId)
            => GetPath(productId) != null;

		public string GetPath(string productId)
        {
			{
                var cachedFile = FilePathCache.GetPath(productId, FileType);
                if (cachedFile != null)
                    return cachedFile;
            }

			var firstOrNull =
				Directory
				.EnumerateFiles(StorageDirectory, "*.*", SearchOption.AllDirectories)
				.FirstOrDefault(s => Regex.IsMatch(s, $@"{productId}.*?\.({extAggr})$", RegexOptions.IgnoreCase));

			if (firstOrNull is null)
				return null;
			FilePathCache.Upsert(productId, FileType, firstOrNull);
			return firstOrNull;
        }

        public bool IsFileTypeMatch(FileInfo fileInfo)
            => extensions_noDots.ContainsInsensative(fileInfo.Extension.Trim('.'));
        #endregion
    }
}
