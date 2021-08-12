using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;

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

		private static System.Timers.Timer timer { get; }
		static PictureStorage()
		{
			timer = new System.Timers.Timer(700)
			{
				AutoReset = true,
				Enabled = true
			};
			timer.Elapsed += (_, __) => timerDownload();
		}

		public static event EventHandler<PictureCachedEventArgs> PictureCached;

		private static Dictionary<PictureDefinition, byte[]> cache { get; } = new Dictionary<PictureDefinition, byte[]>();
		public static (bool isDefault, byte[] bytes) GetPicture(PictureDefinition def)
		{
			if (!cache.ContainsKey(def))
			{
				var path = getPath(def);
				cache[def]
					= File.Exists(path)
					? File.ReadAllBytes(path)
					: null;
			}
			return (cache[def] == null, cache[def] ?? getDefaultImage(def.Size));
		}

		public static byte[] GetPictureSynchronously(PictureDefinition def)
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

		private static Dictionary<PictureSize, byte[]> defaultImages { get; } = new Dictionary<PictureSize, byte[]>();
		public static void SetDefaultImage(PictureSize pictureSize, byte[] bytes)
			=> defaultImages[pictureSize] = bytes;
		private static byte[] getDefaultImage(PictureSize size)
			=> defaultImages.ContainsKey(size)
			? defaultImages[size]
			: new byte[0];

		// necessary to avoid IO errors. ReadAllBytes and WriteAllBytes can conflict in some cases, esp when debugging
		private static bool isProcessing;
		private static void timerDownload()
		{
			// must live outside try-catch, else 'finally' can reset another thread's lock
			if (isProcessing)
				return;

			try
			{
				isProcessing = true;

				var def = cache
					.Where(kvp => kvp.Value is null)
					.Select(kvp => kvp.Key)
					// 80x80 should be 1st since it's enum value == 0
					.OrderBy(d => d.PictureId)
					.FirstOrDefault();

				// no more null entries. all requsted images are cached
				if (string.IsNullOrWhiteSpace(def.PictureId))
					return;

				var bytes = downloadBytes(def);
				saveFile(def, bytes);
				cache[def] = bytes;

				PictureCached?.Invoke(nameof(PictureStorage), new PictureCachedEventArgs { Definition = def, Picture = bytes });
			}
			finally
			{
				isProcessing = false;
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
