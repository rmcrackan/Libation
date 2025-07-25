using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FileManager;
using Newtonsoft.Json;

#nullable enable
namespace LibationFileManager
{
	public static class FilePathCache
	{
		public record CacheEntry(string Id, FileType FileType, LongPath Path);

		private const string FILENAME_V2 = "FileLocationsV2.json";

		public static event EventHandler<CacheEntry>? Inserted;
		public static event EventHandler<CacheEntry>? Removed;

		private static LongPath jsonFileV2 => Path.Combine(Configuration.Instance.LibationFiles, FILENAME_V2);

		private static readonly FileCacheV2<CacheEntry> Cache = new();

		static FilePathCache()
		{
			// load json into memory. if file doesn't exist, nothing to do. save() will create if needed
			if (!File.Exists(jsonFileV2))
				return;

			try
			{
				Cache = JsonConvert.DeserializeObject<FileCacheV2<CacheEntry>>(File.ReadAllText(jsonFileV2))
					?? throw new NullReferenceException("File exists but deserialize is null. This will never happen when file is healthy.");
			}
			catch (Exception ex)
			{
				Serilog.Log.Logger.Error(ex, "Error deserializing file. Wrong format. Possibly corrupt. Deleting file. {@DebugInfo}", new { jsonFileV2 });
				lock (locker)
					File.Delete(jsonFileV2);
				return;
			}
		}

		public static bool Exists(string id, FileType type) => GetFirstPath(id, type) is not null;

		public static List<(FileType fileType, LongPath path)> GetFiles(string id)
		{
			List<CacheEntry> matchingFiles;
			lock(locker)
				matchingFiles = Cache.GetIdEntries(id);

			bool cacheChanged = false;

			//Verify all entries exist
			for (int i = 0; i < matchingFiles.Count; i++)
			{
				if (!File.Exists(matchingFiles[i].Path))
				{
					var entryToRemove = matchingFiles[i];
					matchingFiles.RemoveAt(i);
					cacheChanged |= Remove(entryToRemove);
				}
			}
			if (cacheChanged)
				save();

			return matchingFiles.Select(e => (e.FileType, e.Path)).ToList();
		}

		public static LongPath? GetFirstPath(string id, FileType type)
		{
			List<CacheEntry> matchingFiles;
			lock (locker)
				matchingFiles = Cache.GetIdEntries(id).Where(e => e.FileType == type).ToList();

			bool cacheChanged = false;
			try
			{
				//Verify entries exist, but return first matching 'type'
				for (int i = 0; i < matchingFiles.Count; i++)
				{
					if (File.Exists(matchingFiles[i].Path))
						return matchingFiles[i].Path;
					else
					{
						var entryToRemove = matchingFiles[i];
						matchingFiles.RemoveAt(i);
						cacheChanged |= Remove(entryToRemove);
					}
				}
				return null;
			}
			finally
			{
				if (cacheChanged)
					save();
			}
		}

		private static bool Remove(CacheEntry entry)
		{
			bool removed;
			lock (locker)
				removed = Cache.Remove(entry.Id, entry);
			if (removed)
			{
				Removed?.Invoke(null, entry);
				return true;
			}
			return false;
		}

		public static void Insert(string id, string path)
		{
			var type = FileTypes.GetFileTypeFromPath(path);
			Insert(new CacheEntry(id, type, path));
		}

		public static void Insert(CacheEntry entry)
		{
			lock(locker)
				Cache.Add(entry.Id, entry);
			Inserted?.Invoke(null, entry);
			save();
		}

		// cache is thread-safe and lock free. but file saving is not
		private static object locker { get; } = new object();
		private static void save()
		{
			// create json if not exists
			static void resave() => File.WriteAllText(jsonFileV2, JsonConvert.SerializeObject(Cache, Formatting.Indented));

			lock (locker)
			{
				try { resave(); }
				catch (IOException)
				{
					try { resave(); }
					catch (IOException ex)
					{
						Serilog.Log.Logger.Error(ex, $"Error saving {FILENAME_V2}");
						throw;
					}
				}
			}
		}

		private class FileCacheV2<TEntry>
		{
			[JsonProperty]
			private readonly ConcurrentDictionary<string, List<TEntry>> Dictionary = new();
			private static object lockObject = new();

			public List<TEntry> GetIdEntries(string id)
			{
				static List<TEntry> empty() => new();

				return Dictionary.TryGetValue(id, out var entries) ? entries.ToList() : empty();
			}

			public void Add(string id, TEntry entry)
			{
				Dictionary.AddOrUpdate(id, [entry], (id, entries) => { entries.Add(entry); return entries; });
			}

			public void AddRange(string id, IEnumerable<TEntry> entries)
			{
				Dictionary.AddOrUpdate(id, entries.ToList(), (id, entries) =>
				{
					entries.AddRange(entries);
					return entries;
				});
			}

			public bool Remove(string id, TEntry entry)
			{
				lock (lockObject)
				{
					if (Dictionary.TryGetValue(id, out List<TEntry>? entries))
					{
						var removed = entries?.Remove(entry) ?? false;
						if (removed && entries?.Count == 0)
						{
							Dictionary.Remove(id, out _);
						}
						return removed;
					}
					else return false;
				}
			}
		}
	}
}
