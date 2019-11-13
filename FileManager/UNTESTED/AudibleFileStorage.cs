using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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
        // centralize filetype mappings to ensure uniqueness
        private static Dictionary<string, FileType> extensionMap => new Dictionary<string, FileType>
        {
            [".m4b"] = FileType.Audio,
            [".mp3"] = FileType.Audio,
            [".aac"] = FileType.Audio,
            [".mp4"] = FileType.Audio,
            [".m4a"] = FileType.Audio,
			[".ogg"] = FileType.Audio,
			[".flac"] = FileType.Audio,

			[".aax"] = FileType.AAX,

            [".pdf"] = FileType.PDF,
            [".zip"] = FileType.PDF,
        };

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
                ? Configuration.Instance.WinTemp
                : Configuration.Instance.LibationFiles;
            DecryptInProgress = Path.Combine(M4bRootDir, "DecryptInProgress");
            Directory.CreateDirectory(DecryptInProgress);
            #endregion

            #region init DownloadsInProgress
            if (!Configuration.Instance.DownloadsInProgressEnum.In("WinTemp", "LibationFiles"))
                Configuration.Instance.DownloadsInProgressEnum = "WinTemp";
            var AaxRootDir
                = Configuration.Instance.DownloadsInProgressEnum == "WinTemp" // else "LibationFiles"
                ? Configuration.Instance.WinTemp
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
            Audio = new AudibleFileStorage(FileType.Audio, BooksDirectory);
            AAX = new AudibleFileStorage(FileType.AAX, DownloadsFinal);
            PDF = new AudibleFileStorage(FileType.PDF, BooksDirectory);
        }
        #endregion

        #region instance
        public FileType FileType => (FileType)Value;

        public string StorageDirectory => DisplayName;

        public IEnumerable<string> Extensions => extensionMap.Where(kvp => kvp.Value == FileType).Select(kvp => kvp.Key);

        private AudibleFileStorage(FileType fileType, string storageDirectory) : base((int)fileType, storageDirectory) { }

        /// <summary>
        /// Example for full books:
        /// Search recursively in _books directory. Full book exists if either are true
        /// - a directory name has the product id and an audio file is immediately inside
        /// - any audio filename contains the product id
        /// </summary>
        public async Task<bool> ExistsAsync(string productId)
            => (await GetAsync(productId).ConfigureAwait(false)) != null;

        public async Task<string> GetAsync(string productId)
            => await getAsync(productId).ConfigureAwait(false);

        private async Task<string> getAsync(string productId)
        {
            {
                var cachedFile = FilePathCache.GetPath(productId, FileType);
                if (cachedFile != null)
                    return cachedFile;
            }

            // this is how files are saved by default. check this method first
            {
                var diskFile_byDirName = (await Task.Run(() => getFile_checkDirName(productId)).ConfigureAwait(false));
                if (diskFile_byDirName != null)
                {
                    FilePathCache.Upsert(productId, FileType, diskFile_byDirName);
                    return diskFile_byDirName;
                }
            }

            {
                var diskFile_byFileName = (await Task.Run(() => getFile_checkFileName(productId, StorageDirectory, SearchOption.AllDirectories)).ConfigureAwait(false));
                if (diskFile_byFileName != null)
                {
                    FilePathCache.Upsert(productId, FileType, diskFile_byFileName);
                    return diskFile_byFileName;
                }
            }

            return null;
        }

        // returns audio file if there is a directory where both are true
        // - the directory name contains the productId
        // - the directory contains an audio file in it's top dir (not recursively)
        private string getFile_checkDirName(string productId)
        {
            foreach (var d in Directory.EnumerateDirectories(StorageDirectory, "*.*", SearchOption.AllDirectories))
            {
                if (!fileHasId(d, productId))
                    continue;

                var firstAudio = Directory
                    .EnumerateFiles(d, "*.*", SearchOption.TopDirectoryOnly)
                    .FirstOrDefault(f => IsFileTypeMatch(f));
                if (firstAudio != null)
                    return firstAudio;
            }
            return null;
        }

        // returns audio file if there is an file where both are true
        // - the file name contains the productId
        // - the file is an audio type
        private string getFile_checkFileName(string productId, string dir, SearchOption searchOption)
            => Directory
            .EnumerateFiles(dir, "*.*", searchOption)
            .FirstOrDefault(f => fileHasId(f, productId) && IsFileTypeMatch(f));

        public bool IsFileTypeMatch(string filename)
            => Extensions.ContainsInsensative(Path.GetExtension(filename));

        public bool IsFileTypeMatch(FileInfo fileInfo)
            => Extensions.ContainsInsensative(fileInfo.Extension);

        // use GetFileName, NOT GetFileNameWithoutExtension. This tests files AND directories. if the dir has a dot in the final part of the path, it will be treated like the file extension
        private static bool fileHasId(string file, string productId)
            => Path.GetFileName(file).ContainsInsensitive(productId);
        #endregion
    }
}
