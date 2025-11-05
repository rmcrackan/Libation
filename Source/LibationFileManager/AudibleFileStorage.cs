using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Dinah.Core;
using System.Threading.Tasks;
using System.Threading;
using FileManager;
using AaxDecrypter;

#nullable enable
namespace LibationFileManager
{
	public abstract class AudibleFileStorage
	{
		protected abstract LongPath? GetFilePathCustom(string productId);
		protected abstract List<LongPath> GetFilePathsCustom(string productId);

		#region static
		public static LongPath DownloadsInProgressDirectory => Directory.CreateDirectory(Path.Combine(Configuration.Instance.InProgress, "DownloadsInProgress")).FullName;
		public static LongPath DecryptInProgressDirectory => Directory.CreateDirectory(Path.Combine(Configuration.Instance.InProgress, "DecryptInProgress")).FullName;

		static AudibleFileStorage()
		{
			//Clean up any partially-decrypted files from previous Libation instances.
			//Do not clean DownloadsInProgressDirectory. Those files are resumable.
			try
            {
                foreach (var tempFile in FileUtility.SaferEnumerateFiles(DecryptInProgressDirectory))
                    FileUtility.SaferDelete(tempFile);
            }
            catch (Exception ex)
            {
                // since this is a static constructor which may be called very early, do not assume Serilog is initialized
                try { Serilog.Log.Error(ex, "Error cleaning up partially-decrypted files"); }
				catch { }
            }
		}


		private static AaxcFileStorage AAXC { get; } = new AaxcFileStorage();
		public static bool AaxcExists(string productId) => AAXC.Exists(productId);

		public static AudioFileStorage Audio { get; } = new AudioFileStorage();

		/// <summary>
		/// The fully-qualified Books durectory path if the directory exists, otherwise null.
		/// </summary>
		public static LongPath? BooksDirectory
		{
			get
			{
				if (string.IsNullOrWhiteSpace(Configuration.Instance.Books))
					return null;
				try
				{
					return Directory.CreateDirectory(Configuration.Instance.Books)?.FullName;
				}
				catch (Exception ex)
				{
					Serilog.Log.Error(ex, "Error creating Books directory: {@BooksDirectory}", Configuration.Instance.Books);
					return null;
				}
			}
		}
		#endregion

		#region instance
		private FileType FileType { get; }
		private string regexTemplate { get; }

		protected AudibleFileStorage(FileType fileType)
		{
			FileType = fileType;

			var extAggr = FileTypes.GetExtensions(FileType).Aggregate((a, b) => $"{a}|{b}");
			regexTemplate = $@"{{0}}.*?\.({extAggr})$";
		}

		protected LongPath? GetFilePath(string productId)
		{
			// primary lookup
			var cachedFile = FilePathCache.GetFirstPath(productId, FileType);
			if (cachedFile is not null && File.Exists(cachedFile))
				return cachedFile;

			// secondary lookup attempt
			var firstOrNull = GetFilePathCustom(productId);
			if (firstOrNull is not null)
				FilePathCache.Insert(productId, firstOrNull);

			return firstOrNull;
		}

		public List<LongPath> GetPaths(string productId)
			=> GetFilePathsCustom(productId);

		protected Regex GetBookSearchRegex(string productId)
		{
			var pattern = string.Format(regexTemplate, productId);
			return new Regex(pattern, RegexOptions.IgnoreCase);
		}
		#endregion
	}

	internal class AaxcFileStorage : AudibleFileStorage
	{
		internal AaxcFileStorage() : base(FileType.AAXC) { }

		protected override LongPath? GetFilePathCustom(string productId)
			=> GetFilePathsCustom(productId).FirstOrDefault();

		protected override List<LongPath> GetFilePathsCustom(string productId)
		{
			var regex = GetBookSearchRegex(productId);
			return FileUtility
				.SaferEnumerateFiles(DownloadsInProgressDirectory, "*.*", SearchOption.AllDirectories)
				.Where(s => regex.IsMatch(s)).ToList();
		}

		public bool Exists(string productId) => GetFilePath(productId) is not null;
	}

	public class AudioFileStorage : AudibleFileStorage
	{
		internal AudioFileStorage() : base(FileType.Audio)
			=> BookDirectoryFiles ??= newBookDirectoryFiles();

		private static BackgroundFileSystem? BookDirectoryFiles { get; set; }
		private static Lock bookDirectoryFilesLocker { get; } = new();
		private static EnumerationOptions enumerationOptions { get; } = new()
		{
			RecurseSubdirectories = true,
			IgnoreInaccessible = true,
			AttributesToSkip = FileAttributes.Hidden,
		};

		protected override LongPath? GetFilePathCustom(string productId)
			=> GetFilePathsCustom(productId).FirstOrDefault();

		private static BackgroundFileSystem? newBookDirectoryFiles()
			=> BooksDirectory is LongPath books ? new BackgroundFileSystem(books, "*.*", SearchOption.AllDirectories)
			: null;

		protected override List<LongPath> GetFilePathsCustom(string productId)
		{
			// If user changed the BooksDirectory: reinitialize
			lock (bookDirectoryFilesLocker)
			{
				if (BooksDirectory != BookDirectoryFiles?.RootDirectory)
				{
					BookDirectoryFiles?.Dispose();
					BookDirectoryFiles = newBookDirectoryFiles();
				}
			}

			var regex = GetBookSearchRegex(productId);
			var diskFiles = BookDirectoryFiles?.FindFiles(regex) ?? [];

			//Find all extant files matching the productId
			//using both the file system and the file path cache
			return
				FilePathCache
				.GetFiles(productId)
				.Where(c => c.fileType == FileType.Audio && File.Exists(c.path))
				.Select(c => c.path)
				.Union(diskFiles)
				.ToList();
		}

		public void Refresh()
		{
			lock (bookDirectoryFilesLocker)
			{
				BookDirectoryFiles ??= newBookDirectoryFiles();
			}

			BookDirectoryFiles?.RefreshFiles();
		}

		public LongPath? GetPath(string productId) => GetFilePath(productId);

		public static async IAsyncEnumerable<FilePathCache.CacheEntry> FindAudiobooksAsync(LongPath searchDirectory, [EnumeratorCancellation] CancellationToken cancellationToken)
		{
			ArgumentValidator.EnsureNotNull(searchDirectory, nameof(searchDirectory));

			foreach (LongPath path in Directory.EnumerateFiles(searchDirectory, "*.*", enumerationOptions))
			{
				if (cancellationToken.IsCancellationRequested)
					yield break;

				if (getFormatByExtension(path) is not OutputFormat format)
					continue;

				FilePathCache.CacheEntry? audioFile = default;

				try
				{
					if (format is OutputFormat.M4b)
					{
						var tags = await Task.Run(() => AAXClean.AppleTags.FromFile(path));

						if (tags?.Asin is not null)
							audioFile = new FilePathCache.CacheEntry(tags.Asin, FileType.Audio, path);
					}
					else
					{
						using var fileStream = File.OpenRead(path);
						var id3 = await Task.Run(() => NAudio.Lame.ID3.Id3Tag.Create(fileStream));

						var asin = id3?.Children
							.OfType<NAudio.Lame.ID3.TXXXFrame>()
							.FirstOrDefault(f => f.FieldName == "AUDIBLE_ASIN")
							?.FieldValue;

						if (!string.IsNullOrWhiteSpace(asin))
							audioFile = new FilePathCache.CacheEntry(asin, FileType.Audio, path);
					}
				}
				catch (Exception ex)
				{
					Serilog.Log.Error(ex, "Error checking for asin in {@file}", path);
				}
				finally
				{
					GC.Collect(GC.MaxGeneration, GCCollectionMode.Aggressive, true, true);
				}

				if (audioFile is not null)
					yield return audioFile;
			}

			static OutputFormat? getFormatByExtension(string path)
			{
				var ext = Path.GetExtension(path).ToLower();

				return ext == ".mp3" ? OutputFormat.Mp3
					: ext == ".m4b" ? OutputFormat.M4b
					: null;
			}
		}
	}
}
