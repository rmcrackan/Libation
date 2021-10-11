using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dinah.Core.Collections.Generic;
using Newtonsoft.Json;

namespace LibationFileManager
{
    public static class QuickFilters
    {
        internal class FilterState
        {
            public bool UseDefault { get; set; }
            public List<string> Filters { get; set; } = new List<string>();
        }

        static FilterState inMemoryState { get; } = new FilterState();

        public static string JsonFile => Path.Combine(Configuration.Instance.LibationFiles, "QuickFilters.json");

        static QuickFilters()
        {
            // load json into memory. if file doesn't exist, nothing to do. save() will create if needed
            if (File.Exists(JsonFile))
                inMemoryState = JsonConvert.DeserializeObject<FilterState>(File.ReadAllText(JsonFile));
        }

        public static bool UseDefault
        {
            get => inMemoryState.UseDefault;
            set
            {
                lock (locker)
                {
                    inMemoryState.UseDefault = value;
                    save();
                }
            }
        }

        public static IEnumerable<string> Filters => inMemoryState.Filters.AsReadOnly();

        public static void Add(string filter)
        {
            if (string.IsNullOrWhiteSpace(filter))
                return;
            filter = filter.Trim();

            lock (locker)
            {
                // check for duplicate
                if (inMemoryState.Filters.ContainsInsensative(filter))
                    return;

                inMemoryState.Filters.Add(filter);
                save();
            }
        }

        public static void Remove(string filter)
        {
            lock (locker)
            {
                inMemoryState.Filters.Remove(filter);
                save();
            }
        }

        public static void Edit(string oldFilter, string newFilter)
        {
            lock (locker)
            {
                var index = inMemoryState.Filters.IndexOf(oldFilter);
                if (index < 0)
                    return;

                inMemoryState.Filters = inMemoryState.Filters.Select(f => f == oldFilter ? newFilter : f).ToList();

                save();
            }
        }

        public static void ReplaceAll(IEnumerable<string> filters)
        {
            filters = filters
                .Where(f => !string.IsNullOrWhiteSpace(f))
                .Distinct()
                .Select(f => f.Trim());
            lock (locker)
            {
                inMemoryState.Filters = new List<string>(filters);
                save();
            }
        }

        private static object locker { get; } = new object();

        // ONLY call this within lock()
        private static void save()
        {
            // create json if not exists
            void resave() => File.WriteAllText(JsonFile, JsonConvert.SerializeObject(inMemoryState, Formatting.Indented));
            try { resave(); }
            catch (IOException)
            {
                try { resave(); }
				catch (IOException ex)
				{
					Serilog.Log.Logger.Error(ex, "Error saving QuickFilters.json");
					throw;
				}
			}
        }
    }
}
