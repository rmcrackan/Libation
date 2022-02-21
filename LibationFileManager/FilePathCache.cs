using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dinah.Core.Collections.Immutable;
using Newtonsoft.Json;

namespace LibationFileManager
{
	public static class FilePathCache
	{
		public record CacheEntry(string Id, FileType FileType, string Path);

		private const string FILENAME = "FileLocations.json";

		public static event EventHandler<CacheEntry> Inserted;
		public static event EventHandler<CacheEntry> Removed;

		private static Cache<CacheEntry> cache { get; } = new Cache<CacheEntry>();

		private static string jsonFile => Path.Combine(Configuration.Instance.LibationFiles, FILENAME);

        static FilePathCache()
        {
			// load json into memory. if file doesn't exist, nothing to do. save() will create if needed
			if (File.Exists(jsonFile))
			{
				var list = JsonConvert.DeserializeObject<List<CacheEntry>>(File.ReadAllText(jsonFile));

				// file exists but deser is null. this will never happen when file is healthy
				if (list is null)
				{
					lock (locker)
					{
						Serilog.Log.Logger.Error("Error deserializing file. Wrong format. Possibly corrupt. Deleting file. {@DebugInfo}", new { jsonFile });
						File.Delete(jsonFile);
						return;
					}
				}

				cache = new Cache<CacheEntry>(list);
			}
        }

        public static bool Exists(string id, FileType type) => GetFirstPath(id, type) != null;

		public static List<(FileType fileType, string path)> GetFiles(string id)
			=> getEntries(entry => entry.Id == id)
			.Select(entry => (entry.FileType, entry.Path))
			.ToList();

		public static string GetFirstPath(string id, FileType type)
			=> getEntries(entry => entry.Id == id && entry.FileType == type)
			?.FirstOrDefault()
			?.Path;

		private static List<CacheEntry> getEntries(Func<CacheEntry, bool> predicate)
		{
			var entries = cache.Where(predicate).ToList();
			if (entries is null || !entries.Any())
				return null;

			remove(entries.Where(e => !File.Exists(e.Path)).ToList());

			return entries;
		}

		private static void remove(List<CacheEntry> entries)
		{
			if (entries is null)
				return;

			lock (locker)
			{
				foreach (var entry in entries)
				{
					cache.Remove(entry);
					Removed?.Invoke(null, entry);
				}
				save();
			}
		}

        public static void Insert(string id, string path)
		{
			var type = FileTypes.GetFileTypeFromPath(path);
			var entry = new CacheEntry(id, type, path);
			cache.Add(entry);
			Inserted?.Invoke(null, entry);
			save();
		}

		// cache is thread-safe and lock free. but file saving is not
		private static object locker { get; } = new object();
		private static void save()
		{
			// create json if not exists
			static void resave() => File.WriteAllText(jsonFile, JsonConvert.SerializeObject(cache.ToList(), Formatting.Indented));

			lock (locker)
			{
				try { resave(); }
				catch (IOException)
				{
					try { resave(); }
					catch (IOException ex)
					{
						Serilog.Log.Logger.Error(ex, $"Error saving {FILENAME}");
						throw;
					}
				}
			}
        }
    }
}
