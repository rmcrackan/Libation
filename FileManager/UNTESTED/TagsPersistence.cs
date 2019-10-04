using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace FileManager
{
    /// <summary>
    /// Tags must also be stored in db for search performance. Stored in json file to survive a db reset.
    /// json is only read when a product is first loaded
    /// json is only written to when tags are edited
    /// json access is infrequent and one-off
    /// all other reads happen against db. No volitile storage
    /// </summary>
    public static class TagsPersistence
    {
        public static string TagsFile => Path.Combine(Configuration.Instance.LibationFiles, "BookTags.json");

        private static object locker { get; } = new object();

        public static void Save(string productId, string tags)
			=> System.Threading.Tasks.Task.Run(() => save_fireAndForget(productId, tags));

		private static void save_fireAndForget(string productId, string tags)
		{
			lock (locker)
			{
				// get all
				var allDictionary = retrieve();

				// update/upsert tag list
				allDictionary[productId] = tags;

				// re-save:
				//   this often fails the first time with
				//     The requested operation cannot be performed on a file with a user-mapped section open.
				//   2nd immediate attempt failing was rare. So I added sleep. We'll see...
				void resave() => File.WriteAllText(TagsFile, JsonConvert.SerializeObject(allDictionary, Formatting.Indented));
				try { resave(); }
				catch (IOException debugEx)
				{
					// 1000 was always reliable but very slow. trying other values
					var waitMs = 100;

					System.Threading.Thread.Sleep(waitMs);
					resave();
				}
			}
		}

		public static string GetTags(string productId)
        {
            var dic = retrieve();
            return dic.ContainsKey(productId) ? dic[productId] : null;
        }

        private static Dictionary<string, string> retrieve()
        {
            if (!FileUtility.FileExists(TagsFile))
                return new Dictionary<string, string>();
			lock (locker)
				return JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(TagsFile));
        }
    }
}
