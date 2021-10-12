using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dinah.Core;

namespace FileManager
{
    public static class FileUtility
    {
        private const int MAX_FILENAME_LENGTH = 255;
        private const int MAX_DIRECTORY_LENGTH = 247;

//public static string GetValidFilename(string template, Dictionary<string, object> parameters) { }
        public static string GetValidFilename(string dirFullPath, string filename, string extension, params string[] metadataSuffixes)
        {
            if (string.IsNullOrWhiteSpace(dirFullPath))
                throw new ArgumentException($"{nameof(dirFullPath)} may not be null or whitespace", nameof(dirFullPath));

            filename ??= "";

            // sanitize. omit invalid characters. exception: colon => underscore
            filename = filename.Replace(':', '_');
            filename = PathLib.ToPathSafeString(filename);

            // manage length
            if (filename.Length > 50)
                filename = filename.Substring(0, 50) + "[...]";

            // append metadata
            if (metadataSuffixes != null && metadataSuffixes.Length > 0)
                filename += " [" + string.Join("][", metadataSuffixes) + "]";

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
            SafeMove(source, destination);
            return destination;
        }

        public static string GetValidFilename(string path)
        {
            // TODO: destination must be valid path. Use: " (#)" when needed
            return path;
		}

        /// <summary>Delete file. No error when file does not exist.
        /// Exceptions are logged, not thrown.</summary>
        /// <param name="source">File to delete</param>
        public static void SafeDelete(string source)
        {
            if (!File.Exists(source))
                return;

            while (true)
            {
                try
                {
                    File.Delete(source);
                    Serilog.Log.Logger.Information($"File successfully deleted: {source}");
                    break;
                }
                catch (Exception e)
                {
					System.Threading.Thread.Sleep(100);
                    Serilog.Log.Logger.Error(e, $"Failed to delete: {source}");
                }
            }
        }

        /// <summary>
		/// Moves a specified file to a new location, providing the option to specify a newfile name.
		/// Exceptions are logged, not thrown.
		/// </summary>
		/// <param name="source">The name of the file to move. Can include a relative or absolute path.</param>
		/// <param name="target">The new path and name for the file.</param>
		public static void SafeMove(string source, string target)
        {
            while (true)
            {
                try
                {
                    if (File.Exists(source))
                    {
                        SafeDelete(target);
                        File.Move(source, target);
                        Serilog.Log.Logger.Information($"File successfully moved from '{source}' to '{target}'");
                    }

                    break;
                }
                catch (Exception e)
                {
					System.Threading.Thread.Sleep(100);
                    Serilog.Log.Logger.Error(e, $"Failed to move '{source}' to '{target}'");
                }
            }
        }
    }
}
