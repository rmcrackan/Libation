using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace FileManager
{
	public enum PictureSize { _80x80 = 80, _300x300 = 300, _500x500 = 500 }
	public class PictureCachedEventArgs : EventArgs
	{
		public PictureDefinition Definition { get; internal set; }
		public byte[] Picture { get; internal set; }
	}
	public struct PictureDefinition
	{
		public string PictureId { get; }
		public PictureSize Size { get; }

		public PictureDefinition(string pictureId, PictureSize pictureSize)
		{
			PictureId = pictureId;
			Size = pictureSize;
		}
	}
    public static class PictureStorage
    {
        // not customizable. don't move to config
        private static string ImagesDirectory { get; }
            = new DirectoryInfo(Configuration.Instance.LibationFiles).CreateSubdirectory("Images").FullName;

		private static string getPath(PictureDefinition def)
			=> Path.Combine(ImagesDirectory, $"{def.PictureId}{def.Size}.jpg");

		static PictureStorage()
		{
			new Task(BackgroundDownloader, TaskCreationOptions.LongRunning)
			.Start();
		}

		public static event EventHandler<PictureCachedEventArgs> PictureCached;

		private static BlockingCollection<PictureDefinition> DownloadQueue { get; } = new BlockingCollection<PictureDefinition>();
		private static object cacheLocker { get; } = new object();
		private static Dictionary<PictureDefinition, byte[]> cache { get; } = new Dictionary<PictureDefinition, byte[]>();
		private static Dictionary<PictureSize, byte[]> defaultImages { get; } = new Dictionary<PictureSize, byte[]>();
		public static (bool isDefault, byte[] bytes) GetPicture(PictureDefinition def)
		{
			lock (cacheLocker)
			{
				if (cache.ContainsKey(def))
					return (false, cache[def]);

				var path = getPath(def);

				if (File.Exists(path))
				{
					cache[def] = File.ReadAllBytes(path);
					return (false, cache[def]);
				}

				DownloadQueue.Add(def);
				return (true, getDefaultImage(def.Size));
			}
		}

		public static byte[] GetPictureSynchronously(PictureDefinition def)
		{
			lock (cacheLocker)
			{
				if (!cache.ContainsKey(def) || cache[def] == null)
				{
					var path = getPath(def);
					byte[] bytes;

					if (File.Exists(path))
						bytes = File.ReadAllBytes(path);
					else
					{
						bytes = downloadBytes(def);
						saveFile(def, bytes);
					}

					cache[def] = bytes;
				}
				return cache[def];
			}
		}

		public static void SetDefaultImage(PictureSize pictureSize, byte[] bytes)
			=> defaultImages[pictureSize] = bytes;
		private static byte[] getDefaultImage(PictureSize size)
			=> defaultImages.ContainsKey(size)
			? defaultImages[size]
			: new byte[0];

		static void BackgroundDownloader()
		{
			while (!DownloadQueue.IsCompleted)
			{
				if (!DownloadQueue.TryTake(out var def, System.Threading.Timeout.InfiniteTimeSpan))
					continue;

				var bytes = downloadBytes(def);
				saveFile(def, bytes);
				lock (cacheLocker)
					cache[def] = bytes;

				PictureCached?.Invoke(nameof(PictureStorage), new PictureCachedEventArgs { Definition = def, Picture = bytes });
			}
		}

		private static HttpClient imageDownloadClient { get; } = new HttpClient();
		private static byte[] downloadBytes(PictureDefinition def)
		{
			var sz = (int)def.Size;
			return imageDownloadClient.GetByteArrayAsync("ht" + $"tps://images-na.ssl-images-amazon.com/images/I/{def.PictureId}._SL{sz}_.jpg").Result;
		}

		private static void saveFile(PictureDefinition def, byte[] bytes)
		{
			var path = getPath(def);
			File.WriteAllBytes(path, bytes);
		}
	}
}