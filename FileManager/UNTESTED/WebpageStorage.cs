using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FileManager
{
    public static class WebpageStorage
    {
        // not customizable. don't move to config
        private static string PagesDirectory { get; }
            = new DirectoryInfo(Configuration.Instance.LibationFiles).CreateSubdirectory("Pages").FullName;
        private static string BookDetailsDirectory { get; }
            = new DirectoryInfo(PagesDirectory).CreateSubdirectory("Book Details").FullName;

        public static string GetLibraryBatchName() => "Library_" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        public static string SavePageToBatch(string contents, string batchName, string extension)
        {
            var batch_dir = Path.Combine(PagesDirectory, batchName);

            Directory.CreateDirectory(batch_dir);

            var file = Path.Combine(batch_dir, batchName + '.' + extension.Trim('.'));
            var filename = FileUtility.GetValidFilename(file);
            File.WriteAllText(filename, contents);

            return filename;
        }

        public static List<FileInfo> GetJsonFiles(DirectoryInfo libDir)
            => libDir == null
            ? new List<FileInfo>()
            : Directory
                .EnumerateFiles(libDir.FullName, "*.json")
                .Select(f => new FileInfo(f))
                .ToList();

        public static DirectoryInfo GetMostRecentLibraryDir()
        {
            var dir = Directory
                .EnumerateDirectories(PagesDirectory, "Library_*")
                .OrderBy(a => a)
                .LastOrDefault();
            if (string.IsNullOrWhiteSpace(dir))
                return null;
            return new DirectoryInfo(dir);
        }

        public static FileInfo GetBookDetailHtmFileInfo(string productId)
        {
            var path = Path.Combine(BookDetailsDirectory, $"BookDetail-{productId}.htm");
            return new FileInfo(path);
        }

        public static FileInfo GetBookDetailJsonFileInfo(string productId)
        {
            var path = Path.Combine(BookDetailsDirectory, $"BookDetail-{productId}.json");
            return new FileInfo(path);
        }

        public static FileInfo SaveBookDetailsToHtm(string productId, string contents)
        {
            var fi = GetBookDetailHtmFileInfo(productId);
            File.WriteAllText(fi.FullName, contents);
            return fi;
        }
    }
}
