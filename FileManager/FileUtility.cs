using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
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
            : '.' + extension.Trim().Trim('.');

        /// <summary>
        /// Return position with correct number of leading zeros.
        /// <br />- 2 of 9 => "2"
        /// <br />- 2 of 90 => "02"
        /// <br />- 2 of 900 => "002"
        /// </summary>
        /// <param name="position">position in sequence. The 'x' in 'x of y'</param>
        /// <param name="total">total qty in sequence. The 'y' in 'x of y'</param>
        public static string GetSequenceFormatted(int position, int total)
        {
            ArgumentValidator.EnsureGreaterThan(position, nameof(position), 0);
            ArgumentValidator.EnsureGreaterThan(total, nameof(total), 0);
            if (position > total)
                throw new ArgumentException($"{position} may not be greater than {total}");

            return position.ToString().PadLeft(total.ToString().Length, '0');
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

            fullfilename = removeInvalidWhitespace(fullfilename);

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

            path = replaceInvalidChars(path, illegalCharacterReplacements);
            path = standardizeSlashes(path);
            path = replaceColons(path, illegalCharacterReplacements);
            path = removeDoubleSlashes(path);

            return path;
        }

        private static char[] invalidChars { get; } = Path.GetInvalidPathChars().Union(new[] {
                '*', '?',
                // these are weird. If you run Path.GetInvalidPathChars() in Visual Studio's "C# Interactive", then these characters are included.
                // In live code, Path.GetInvalidPathChars() does not include them
                '"', '<', '>'
            }).ToArray();
        private static string replaceInvalidChars(string path, string illegalCharacterReplacements)
            => string.Join(illegalCharacterReplacements ?? "", path.Split(invalidChars));

        private static string standardizeSlashes(string path)
            => path.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);

        private static string replaceColons(string path, string illegalCharacterReplacements)
        {
            // replace all colons except within the first 2 chars
            var builder = new System.Text.StringBuilder();
            for (var i = 0; i < path.Length; i++)
            {
                var c = path[i];
                if (i >= 2 && c == ':')
                    builder.Append(illegalCharacterReplacements);
                else
                    builder.Append(c);
            }
            return builder.ToString();
        }

        private static string removeDoubleSlashes(string path)
        {
            if (path.Length < 2)
                return path;

            // exception: don't try to condense the initial dbl bk slashes in a path. eg: \\192.168.0.1

            var remainder = path[1..];
            var dblSeparator = $"{Path.DirectorySeparatorChar}{Path.DirectorySeparatorChar}";
            while (remainder.Contains(dblSeparator))
                remainder = remainder.Replace(dblSeparator, $"{Path.DirectorySeparatorChar}");

            return path[0] + remainder;
        }

        private static string removeInvalidWhitespace_pattern { get; } = $@"[\s\.]*\{Path.DirectorySeparatorChar}\s*";
        private static Regex removeInvalidWhitespace_regex { get; } = new(removeInvalidWhitespace_pattern, RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace);

        /// <summary>no part of the path may begin or end in whitespace</summary>
        private static string removeInvalidWhitespace(string fullfilename)
        {
            // no whitespace at beginning or end
            // replace whitespace around path slashes
            //    regex (with space added for clarity)
            //    \s*  \\  \s*    =>    \
            // no ending dots. beginning dots are valid

            // regex is easier by ending with separator
            fullfilename += Path.DirectorySeparatorChar;
            fullfilename = removeInvalidWhitespace_regex.Replace(fullfilename, Path.DirectorySeparatorChar.ToString());
            // take seperator back off
            fullfilename = RemoveLastCharacter(fullfilename);

            fullfilename = removeDoubleSlashes(fullfilename);
            return fullfilename;
        }

        public static string RemoveLastCharacter(this string str) => string.IsNullOrEmpty(str) ? str : str[..^1];

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
