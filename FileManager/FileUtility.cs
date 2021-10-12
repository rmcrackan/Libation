using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dinah.Core;
using Polly;
using Polly.Retry;

namespace FileManager
{
    public static class FileUtility
    {
        private const int MAX_FILENAME_LENGTH = 255;
        private const int MAX_DIRECTORY_LENGTH = 247;

        public static string GetValidFilename(string dirFullPath, string filename, string extension, string metadataSuffix)
        {
            if (string.IsNullOrWhiteSpace(dirFullPath))
                throw new ArgumentException($"{nameof(dirFullPath)} may not be null or whitespace", nameof(dirFullPath));

            filename ??= "";

            // sanitize. omit invalid characters. exception: colon => underscore
            filename = filename.Replace(':', '_');
            filename = PathLib.ToPathSafeString(filename);

            if (filename.Length > 50)
                filename = filename.Substring(0, 50) + "[...]";

            if (!string.IsNullOrWhiteSpace(metadataSuffix))
                filename += $" [{metadataSuffix}]";

            // extension is null when this method is used for directory names
            if (!string.IsNullOrWhiteSpace(extension))
                extension = '.' + extension.Trim('.');

            // ensure uniqueness
            var fullfilename = Path.Combine(dirFullPath, filename + extension);
            var i = 0;
            while (File.Exists(fullfilename))
                fullfilename = Path.Combine(dirFullPath, filename + $" ({++i})" + extension);

            return fullfilename;
        }

        public static string GetMultipartFileName(string originalPath, int partsPosition, int partsTotal, string suffix)
        {
            // 1-9     => 1-9
            // 10-99   => 01-99
            // 100-999 => 001-999
            var chapterCountLeadingZeros = partsPosition.ToString().PadLeft(partsTotal.ToString().Length, '0');

            string extension = Path.GetExtension(originalPath);

            var filenameBase = $"{Path.GetFileNameWithoutExtension(originalPath)} - {chapterCountLeadingZeros} - {suffix}";

            // Replace illegal path characters with spaces
            var filenameBaseSafe = PathLib.ToPathSafeString(filenameBase, " ");
            var fileName = filenameBaseSafe.Truncate(MAX_FILENAME_LENGTH - extension.Length);
            var path = Path.Combine(Path.GetDirectoryName(originalPath), fileName + extension);
            return path;
        }

        public static string Move(string source, string destination)
        {
            // TODO: destination must be valid path. Use: " (#)" when needed
            SaferMove(source, destination);
            return destination;
        }

        public static string GetValidFilename(string path)
        {
            // TODO: destination must be valid path. Use: " (#)" when needed
            return path;
		}

        private static int maxRetryAttempts { get; } = 3;
        private static TimeSpan pauseBetweenFailures { get; } = TimeSpan.FromMilliseconds(100);
        private static RetryPolicy retryPolicy { get; } =
            Policy
            .Handle<Exception>()
            .WaitAndRetry(maxRetryAttempts, i => pauseBetweenFailures);

        /// <summary>Delete file. No error when source does not exist. Retry up to 3 times.</summary>
        public static void SaferDelete(string source)
            => retryPolicy.Execute(() =>
            {
                try
                {
                    if (!File.Exists(source))
                    {
                        File.Delete(source);
                        Serilog.Log.Logger.Information("File successfully deleted", new { source });
                    }
                }
                catch (Exception e)
                {
                    Serilog.Log.Logger.Error(e, "Failed to delete file", new { source });
                    throw;
                }
            });

        /// <summary>Move file. No error when source does not exist. Retry up to 3 times.</summary>
		public static void SaferMove(string source, string target)
            => retryPolicy.Execute(() =>
            {
                try
                {
                    if (!File.Exists(source))
                    {
                        SaferDelete(target);
                        File.Move(source, target);
                        Serilog.Log.Logger.Information("File successfully moved", new { source, target });
                    }
                }
                catch (Exception e)
                {
                    Serilog.Log.Logger.Error(e, "Failed to move file", new { source, target });
                    throw;
                }
            });
    }
}
