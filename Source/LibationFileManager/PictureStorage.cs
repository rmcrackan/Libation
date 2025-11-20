using FileManager;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

#nullable enable
namespace LibationFileManager
{
	public enum PictureSize { Native, _80x80 = 80, _300x300 = 300, _500x500 = 500 }
	public class PictureCachedEventArgs : EventArgs
	{
		public PictureDefinition Definition { get; }
		public byte[] Picture { get; }

		internal PictureCachedEventArgs(PictureDefinition definition, byte[] picture)
		{
			Definition = definition;
			Picture = picture;
		}
	}
	public struct PictureDefinition : IEquatable<PictureDefinition>
	{
		public string PictureId { get; init; }
		public PictureSize Size { get; init; }

		public PictureDefinition(string pictureId, PictureSize pictureSize)
		{
			PictureId = pictureId;
			Size = pictureSize;
		}

		public bool Equals(PictureDefinition other)
		{
			return PictureId == other.PictureId && Size == other.Size;
		}
	}
    public static class PictureStorage
    {
        // not customizable. don't move to config
        private static string ImagesDirectory { get; }
            = new DirectoryInfo(Configuration.Instance.LibationFiles.Location).CreateSubdirectoryEx("Images").FullName;

		private static string getPath(PictureDefinition def)
			=> Path.Combine(ImagesDirectory, $"{def.PictureId}{def.Size}.jpg");

		static PictureStorage()
		{
			new Task(BackgroundDownloader, TaskCreationOptions.LongRunning)
			.Start();
		}

		public static event EventHandler<PictureCachedEventArgs>? PictureCached;

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
				return (true, GetDefaultImage(def.Size));
			}
		}

		public static string GetPicturePathSynchronously(PictureDefinition def, CancellationToken cancellationToken = default)
        {
			GetPictureSynchronously(def, cancellationToken);
			return getPath(def);
		}

		public static byte[] GetPictureSynchronously(PictureDefinition def, CancellationToken cancellationToken = default)
		{
			lock (cacheLocker)
			{
				if (!cache.ContainsKey(def) || cache[def] is null)
				{
					var path = getPath(def);
					var bytes
						= File.Exists(path)
						? File.ReadAllBytes(path)
						: downloadBytes(def, cancellationToken);
					cache[def] = bytes;
				}
				return cache[def];
			}
		}

		public static void SetDefaultImage(PictureSize pictureSize, byte[] bytes)
			=> defaultImages[pictureSize] = bytes;
		public static byte[] GetDefaultImage(PictureSize size)
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
				lock (cacheLocker)
					cache[def] = bytes;

				PictureCached?.Invoke(nameof(PictureStorage), new PictureCachedEventArgs(def, bytes));
			}
		}

		private static HttpClient imageDownloadClient { get; } = new HttpClient();
		private static byte[] downloadBytes(PictureDefinition def, CancellationToken cancellationToken = default)
		{
			if (def.PictureId is null)
				return GetDefaultImage(def.Size);

			try
			{
				var sizeStr = def.Size == PictureSize.Native ? "" : $"._SL{(int)def.Size}_";

				using var requestMessage = new HttpRequestMessage(HttpMethod.Get, "ht" + $"tps://images-na.ssl-images-amazon.com/images/I/{def.PictureId}{sizeStr}.jpg");
				using var response = imageDownloadClient.Send(requestMessage, HttpCompletionOption.ResponseHeadersRead, cancellationToken).EnsureSuccessStatusCode();
				
				if (response.Content.Headers.ContentLength is not long size)
					return GetDefaultImage(def.Size);

				var bytes = new byte[size];
				using var respStream = response.Content.ReadAsStream(cancellationToken);
				respStream.ReadExactly(bytes);

				// save image file. make sure to not save default image
				var path = getPath(def);
				File.WriteAllBytes(path, bytes);

				return bytes;
			}
			catch
			{
				return GetDefaultImage(def.Size);
			}
		}
	}
}