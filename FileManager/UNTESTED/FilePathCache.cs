using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        static List<CacheEntry> inMemoryCache { get; } = new List<CacheEntry>();

        public static string JsonFile => Path.Combine(Configuration.Instance.LibationFiles, "FilePaths.json");

        static FilePathCache()
        {
            // load json into memory. if file doesn't exist, nothing to do. save() will create if needed
            if (FileUtility.FileExists(JsonFile))
                inMemoryCache = JsonConvert.DeserializeObject<List<CacheEntry>>(File.ReadAllText(JsonFile));
        }

        public static bool Exists(string id, FileType type) => GetPath(id, type) != null;

        public static string GetPath(string id, FileType type)
        {
            var entry = inMemoryCache.SingleOrDefault(i => i.Id == id && i.FileType == type);

            if (entry == null)
                return null;

            if (!FileUtility.FileExists(entry.Path))
            {
                remove(entry);
                return null;
            }

            return entry.Path;
        }

        private static object locker { get; } = new object();

        private static void remove(CacheEntry entry)
        {
            lock (locker)
            {
                inMemoryCache.Remove(entry);
                save();
            }
        }

        public static void Upsert(string id, FileType type, string path)
        {
            if (!FileUtility.FileExists(path))
                throw new FileNotFoundException("Cannot add path to cache. File not found");

            lock (locker)
            {
                var entry = inMemoryCache.SingleOrDefault(i => i.Id == id && i.FileType == type);
                if (entry != null)
                    entry.Path = path;
                else
                {
                    entry = new CacheEntry { Id = id, FileType = type, Path = path };
                    inMemoryCache.Add(entry);
                }
                save();
            }
        }

        // ONLY call this within lock()
        private static void save()
        {
            // create json if not exists
            void resave() => File.WriteAllText(JsonFile, JsonConvert.SerializeObject(inMemoryCache, Formatting.Indented));
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
