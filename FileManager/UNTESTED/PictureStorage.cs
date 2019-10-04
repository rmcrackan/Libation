using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FileManager
{
    /// <summary>
    /// Files are small. Entire file is read from disk every time. No volitile storage. Paths are well known
    /// </summary>
    public static class PictureStorage
    {
        public enum PictureSize { _80x80, _300x300, _500x500 }

        // not customizable. don't move to config
        private static string ImagesDirectory { get; }
            = new DirectoryInfo(Configuration.Instance.LibationFiles).CreateSubdirectory("Images").FullName;

        private static string getPath(string pictureId, PictureSize size)
            => Path.Combine(ImagesDirectory, $"{pictureId}{size}.jpg");

        public static byte[] GetImage(string pictureId, PictureSize size)
        {
            var path = getPath(pictureId, size);
            if (!FileUtility.FileExists(path))
                DownloadImages(pictureId);

            return File.ReadAllBytes(path);
        }

        public static void DownloadImages(string pictureId)
        {
            var path80 = getPath(pictureId, PictureSize._80x80);
            var path300 = getPath(pictureId, PictureSize._300x300);
            var path500 = getPath(pictureId, PictureSize._500x500);

            int retry = 0;
            do
            {
                try
                {
                    using (var webClient = new System.Net.WebClient())
                    {
                        // download any that don't exist
                        {
                            if (!FileUtility.FileExists(path80))
                            {
                                var bytes = webClient.DownloadData(
                                    "https://images-na.ssl-images-amazon.com/images/I/" + pictureId + "._SL80_.jpg");
                                File.WriteAllBytes(path80, bytes);
                            }
                        }

                        {
                            if (!FileUtility.FileExists(path300))
                            {
                                var bytes = webClient.DownloadData(
                                    "https://images-na.ssl-images-amazon.com/images/I/" + pictureId + "._SL300_.jpg");
                                File.WriteAllBytes(path300, bytes);
                            }
                        }

                        {
                            if (!FileUtility.FileExists(path500))
                            {
                                var bytes = webClient.DownloadData(
                                    "https://m.media-amazon.com/images/I/" + pictureId + "._SL500_.jpg");
                                File.WriteAllBytes(path500, bytes);
                            }
                        }

                        break;
                    }
                }
                catch { retry++; }
            }
            while (retry < 3);
        }
    }
}
