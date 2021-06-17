using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dinah.Core.Collections.Immutable;
using Newtonsoft.Json;

namespace FileManager
{
	public static class FilePathCache
    {
        internal class CacheEntry
        {
            public string Id { get; set; }
            public FileType FileType { get; set; }
            public string Path { get; set; }
        }

        static Cache<CacheEntry> cache { get; } = new Cache<CacheEntry>();

        public static string JsonFile => Path.Combine(Configuration.Instance.LibationFiles, "FilePaths.json");

        static FilePathCache()
        {
			// load json into memory. if file doesn't exist, nothing to do. save() will create if needed
			if (File.Exists(JsonFile))
			{
				var list = JsonConvert.DeserializeObject<List<CacheEntry>>(File.ReadAllText(JsonFile));
				cache = new Cache<CacheEntry>(list);
			}
        }

        public static bool Exists(string id, FileType type) => GetPath(id, type) != null;

        public static string GetPath(string id, FileType type)
        {
            var entry = cache.SingleOrDefault(i => i.Id == id && i.FileType == type);

            if (entry == null)
                return null;

            if (!File.Exists(entry.Path))
            {
                remove(entry);
                return null;
            }

            return entry.Path;
        }

        private static void remove(CacheEntry entry)
		{
			cache.Remove(entry);
			save();
		}

        public static void Upsert(string id, FileType type, string path)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException("Cannot add path to cache. File not found");

			var entry = cache.SingleOrDefault(i => i.Id == id && i.FileType == type);

			if (entry is null)
				cache.Add(new CacheEntry { Id = id, FileType = type, Path = path });
			else
				entry.Path = path;

			save();
		}

		// cache is thread-safe and lock free. but file saving is not
		private static object locker { get; } = new object();
		private static void save()
		{
			// create json if not exists
			static void resave() => File.WriteAllText(JsonFile, JsonConvert.SerializeObject(cache.ToList(), Formatting.Indented));

			lock (locker)
			{
				try { resave(); }
				catch (IOException)
				{
					try { resave(); }
					catch (IOException ex)
					{
						Serilog.Log.Logger.Error(ex, "Error saving FilePaths.json");
						throw;
					}
				}
			}
        }
    }
}
