using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Polly;
using Polly.Retry;

namespace FileManager
{
    /// <summary>
    /// Tags must also be stored in db for search performance. Stored in json file to survive a db reset.
    /// json is only read when a product is first loaded into the db
    /// json is only written to when tags are edited
    /// json access is infrequent and one-off
    /// </summary>
    public static class TagsPersistence
    {
		private static string TagsFile => Path.Combine(Configuration.Instance.LibationFiles, "BookTags.json");

        private static object locker { get; } = new object();

		// if failed, retry only 1 time after a wait of 100 ms
		// 1st save attempt sometimes fails with
		//   The requested operation cannot be performed on a file with a user-mapped section open.
		private static RetryPolicy policy { get; }
			= Policy.Handle<Exception>()
			.WaitAndRetry(new[] { TimeSpan.FromMilliseconds(100) });

		public static void Save(IEnumerable<(string productId, string tags)> tagsCollection)
		{
			ensureCache();

			// on initial reload, there's a huge benefit to adding to cache individually then updating the file only once
			foreach ((string productId, string tags) in tagsCollection)
				cache[productId] = tags;

			lock (locker)
				policy.Execute(() => File.WriteAllText(TagsFile, JsonConvert.SerializeObject(cache, Formatting.Indented)));
		}

		private static Dictionary<string, string> cache;

		public static string GetTags(string productId)
        {
			ensureCache();

			cache.TryGetValue(productId, out string value);
			return value;
        }

		private static void ensureCache()
		{
			if (cache is null)
				lock (locker)
					cache = !File.Exists(TagsFile)
						? new Dictionary<string, string>()
						: JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(TagsFile));
		}
	}
}
