using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dinah.Core.Collections.Generic;
using Newtonsoft.Json;

#nullable enable
namespace LibationFileManager
{
    public static class QuickFilters
    {
        public static event EventHandler? Updated;

		public static event EventHandler? UseDefaultChanged;

		internal class FilterState
        {
            public bool UseDefault { get; set; }
            public List<string> Filters { get; set; } = new List<string>();
        }

        public static string JsonFile => Path.Combine(Configuration.Instance.LibationFiles, "QuickFilters.json");


		// load json into memory. if file doesn't exist, nothing to do. save() will create if needed
		static FilterState inMemoryState { get; }
            = File.Exists(JsonFile) && JsonConvert.DeserializeObject<FilterState>(File.ReadAllText(JsonFile)) is FilterState inMemState
            ? inMemState
            : new FilterState();

        public static bool UseDefault
        {
            get => inMemoryState.UseDefault;
            set
            {
                if (UseDefault == value)
                    return;

                lock (locker)
                {
                    inMemoryState.UseDefault = value;
                    save(false);
                }

                UseDefaultChanged?.Invoke(null, EventArgs.Empty);
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
        private static void save(bool invokeUpdatedEvent = true)
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

            if (invokeUpdatedEvent)
                Updated?.Invoke(null, EventArgs.Empty);
        }
    }
}
