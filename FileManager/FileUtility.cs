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
        /// <summary>
        /// "txt" => ".txt"
        /// <br />".txt" => ".txt"
        /// <br />null or whitespace => ""
        /// </summary>
        public static string GetStandardizedExtension(string extension)
            => string.IsNullOrWhiteSpace(extension)
            ? (extension ?? "")?.Trim()
            : '.' + extension.Trim('.');

        public static string GetValidFilename(string dirFullPath, string filename, string extension, string metadataSuffix)
        {
            ArgumentValidator.EnsureNotNullOrWhiteSpace(dirFullPath, nameof(dirFullPath));
            ArgumentValidator.EnsureNotNullOrWhiteSpace(filename, nameof(filename));

            var template = $"<title> [<id>]";

            var fullfilename = Path.Combine(dirFullPath, template + GetStandardizedExtension(extension));

            var fileTemplate = new FileTemplate(fullfilename) { IllegalCharacterReplacements = "_" };
            fileTemplate.AddParameterReplacement("title", filename);
            fileTemplate.AddParameterReplacement("id", metadataSuffix);
            return fileTemplate.GetFilename();
        }

        public static string GetMultipartFileName(string originalPath, int partsPosition, int partsTotal, string suffix)
        {
            ArgumentValidator.EnsureNotNull(originalPath, nameof(originalPath));
            ArgumentValidator.EnsureGreaterThan(partsPosition, nameof(partsPosition), 0);
            ArgumentValidator.EnsureGreaterThan(partsTotal, nameof(partsTotal), 0);
            if (partsPosition > partsTotal)
                throw new ArgumentException($"{partsPosition} may not be greater than {partsTotal}");

            // 1-9     => 1-9
            // 10-99   => 01-99
            // 100-999 => 001-999
            var chapterCountLeadingZeros = partsPosition.ToString().PadLeft(partsTotal.ToString().Length, '0');

            var template = Path.ChangeExtension(originalPath, null) + " - <chapter> - <title>" + Path.GetExtension(originalPath);

            var fileTemplate = new FileTemplate(template) { IllegalCharacterReplacements = " " };
            fileTemplate.AddParameterReplacement("chapter", chapterCountLeadingZeros);
            fileTemplate.AddParameterReplacement("title", suffix);

            return fileTemplate.GetFilename();
        }

        private const int MAX_FILENAME_LENGTH = 255;
        private const int MAX_DIRECTORY_LENGTH = 247;

        /// <summary>
        /// Ensure valid file name path: 
        /// <br/>- remove invalid chars
        /// <br/>- ensure uniqueness
        /// <br/>- enforce max file length
        /// </summary>
        public static string GetValidFilename(string path, string illegalCharacterReplacements = "")
        {
            ArgumentValidator.EnsureNotNull(path, nameof(path));

            // remove invalid chars
            path = GetSafePath(path, illegalCharacterReplacements);

            // ensure uniqueness and check lengths
            var dir = Path.GetDirectoryName(path);
            dir = dir.Truncate(MAX_DIRECTORY_LENGTH);

            var filename = Path.GetFileNameWithoutExtension(path);
            var fileStem = Path.Combine(dir, filename);

            var extension = Path.GetExtension(path);

            var fullfilename = fileStem.Truncate(MAX_FILENAME_LENGTH - extension.Length) + extension;

            var i = 0;
            while (File.Exists(fullfilename))
            {
                var increm = $" ({++i})";
                fullfilename = fileStem.Truncate(MAX_FILENAME_LENGTH - increm.Length - extension.Length) + increm + extension;
            }

            return fullfilename;
        }

        // GetInvalidFileNameChars contains everything in GetInvalidPathChars plus ':', '*', '?', '\\', '/'

        /// <summary>Use with file name, not full path. Valid path charaters which are invalid file name characters will be replaced: ':', '\\', '/'</summary>
        public static string GetSafeFileName(string str, string illegalCharacterReplacements = "")
            => string.Join(illegalCharacterReplacements ?? "", str.Split(Path.GetInvalidFileNameChars()));

        /// <summary>Use with full path, not file name. Valid path charaters which are invalid file name characters will be retained: '\\', '/'</summary>
        public static string GetSafePath(string path, string illegalCharacterReplacements = "")
        {
            ArgumentValidator.EnsureNotNull(path, nameof(path));

            var fixedPath = string
                .Join(illegalCharacterReplacements ?? "", path.Split(Path.GetInvalidPathChars()))
                .Replace("*", illegalCharacterReplacements)
                .Replace("?", illegalCharacterReplacements)
                .Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);

            // replace all colons except within the first 2 chars
            var builder = new System.Text.StringBuilder();
            for (var i = 0; i < fixedPath.Length; i++)
            {
                var c = fixedPath[i];
                if (i >= 2 && c == ':')
                    builder.Append(illegalCharacterReplacements);
                else
                    builder.Append(c);
            }
            fixedPath = builder.ToString();
            return fixedPath;
        }

        /// <summary>
        /// Move file.
        /// <br/>- Ensure valid file name path: remove invalid chars, ensure uniqueness, enforce max file length
        /// <br/>- Perform <see cref="SaferMove"/>
        /// <br/>- Return valid path
        /// </summary>
        public static string SaferMoveToValidPath(string source, string destination)
        {
            destination = GetValidFilename(destination);
            SaferMove(source, destination);
            return destination;
        }

        private static int maxRetryAttempts { get; } = 3;
        private static TimeSpan pauseBetweenFailures { get; } = TimeSpan.FromMilliseconds(100);
        private static RetryPolicy retryPolicy { get; } =
            Policy
            .Handle<Exception>()
            .WaitAndRetry(maxRetryAttempts, i => pauseBetweenFailures);

        /// <summary>Delete file. No error when source does not exist. Retry up to 3 times before throwing exception.</summary>
        public static void SaferDelete(string source)
            => retryPolicy.Execute(() =>
            {
                try
                {
                    if (File.Exists(source))
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

        /// <summary>Move file. No error when source does not exist. Retry up to 3 times before throwing exception.</summary>
		public static void SaferMove(string source, string destination)
            => retryPolicy.Execute(() =>
            {
                try
                {
                    if (File.Exists(source))
                    {
                        SaferDelete(destination);
                        Directory.CreateDirectory(Path.GetDirectoryName(destination));
                        File.Move(source, destination);
                        Serilog.Log.Logger.Information("File successfully moved", new { source, destination });
                    }
                }
                catch (Exception e)
                {
                    Serilog.Log.Logger.Error(e, "Failed to move file", new { source, destination });
                    throw;
                }
            });
    }
}
