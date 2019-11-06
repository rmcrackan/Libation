using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FileManager
{
    public static class FileUtility
    {
        // a replacement for File.Exists() which allows long paths
        // not needed in .net-core
        public static bool FileExists(string path)
        {
            var basic = File.Exists(path);
            if (basic)
                return true;

            // character cutoff is usually 269 but this isn't a hard number. there are edgecases which shorted the threshold
            if (path.Length < 260)
                return false;

            // try long name prefix:
            //   \\?\
            // https://blogs.msdn.microsoft.com/jeremykuhne/2016/06/21/more-on-new-net-path-handling/
            path = @"\\?\" + path;

            return File.Exists(path);
        }

        public static string GetValidFilename(string dirFullPath, string filename, string extension, params string[] metadataSuffixes)
        {
            if (string.IsNullOrWhiteSpace(dirFullPath))
                throw new ArgumentException($"{nameof(dirFullPath)} may not be null or whitespace", nameof(dirFullPath));

            // file max length = 255. dir max len = 247

            // sanitize
            filename = GetAsciiTag(filename);
            // manage length
            if (filename.Length > 50)
                filename = filename.Substring(0, 50) + "[...]";

            // append id. it is 10 or 14 char in the common cases
            if (metadataSuffixes != null && metadataSuffixes.Length > 0)
                filename += " [" + string.Join("][", metadataSuffixes) + "]";

            // this method may also be used for directory names, so no guarantee of extension
            if (!string.IsNullOrWhiteSpace(extension))
                extension = '.' + extension.Trim('.');

            // ensure uniqueness
            var fullfilename = Path.Combine(dirFullPath, filename + extension);
            var i = 0;
            while (FileExists(fullfilename))
                fullfilename = Path.Combine(dirFullPath, filename + $" ({++i})" + extension);

            return fullfilename;
        }

        public static string GetAsciiTag(string property)
        {
            if (property == null)
                return "";

            // omit characters which are invalid. EXCEPTION: change colon to underscore
            property = property.Replace(':', '_');

            // GetInvalidFileNameChars contains everything in GetInvalidPathChars plus ':', '*', '?', '\\', '/'
            foreach (var ch in Path.GetInvalidFileNameChars())
                property = property.Replace(ch.ToString(), "");
            return property;
        }
    }
}
