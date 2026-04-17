using FileManager;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace LibationFileManager;

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
			var path = getPath(def);

			// Disk is authoritative. Ignore in-memory cache when the file is missing so a later
			// successful download (or a CDN that omits Content-Length) is not blocked by a stale placeholder.
			if (File.Exists(path))
			{
				cache[def] = File.ReadAllBytes(path);
				return cache[def];
			}

			if (cache.ContainsKey(def))
				cache.Remove(def);

			var bytes = downloadBytes(def, cancellationToken);
			if (File.Exists(path))
				cache[def] = bytes;
			return bytes;
		}
	}

	public static void SetDefaultImage(PictureSize pictureSize, byte[] bytes)
		=> defaultImages[pictureSize] = bytes;
	public static byte[] GetDefaultImage(PictureSize size)
		=> defaultImages.TryGetValue(size, out byte[]? value) ? value : [];

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

	private const long MaxPictureDownloadBytes = 25 * 1024 * 1024;

	private static byte[] downloadBytes(PictureDefinition def, CancellationToken cancellationToken = default)
	{
		if (def.PictureId is null)
			return GetDefaultImage(def.Size);

		try
		{
			var sizeStr = def.Size == PictureSize.Native ? "" : $"._SL{(int)def.Size}_";

			using var requestMessage = new HttpRequestMessage(HttpMethod.Get, "ht" + $"tps://images-na.ssl-images-amazon.com/images/I/{def.PictureId}{sizeStr}.jpg");
			using var response = imageDownloadClient.Send(requestMessage, HttpCompletionOption.ResponseHeadersRead, cancellationToken).EnsureSuccessStatusCode();

			byte[] bytes;
			if (response.Content.Headers.ContentLength is long knownSize && knownSize >= 0)
			{
				if (knownSize == 0 || knownSize > MaxPictureDownloadBytes)
					return GetDefaultImage(def.Size);

				bytes = new byte[knownSize];
				using (var respStream = response.Content.ReadAsStream(cancellationToken))
					respStream.ReadExactly(bytes);
			}
			else
			{
				// Chunked responses often omit Content-Length; the previous implementation treated that as failure,
				// left no file on disk, and callers that opened the expected path then broke (e.g. folder icons).
				using var respStream = response.Content.ReadAsStream(cancellationToken);
				using var ms = new MemoryStream();
				var buffer = new byte[81920];
				int read;
				while ((read = respStream.Read(buffer, 0, buffer.Length)) > 0)
				{
					if (ms.Length + read > MaxPictureDownloadBytes)
						return GetDefaultImage(def.Size);
					ms.Write(buffer, 0, read);
				}

				bytes = ms.ToArray();
				if (bytes.Length == 0)
					return GetDefaultImage(def.Size);
			}

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