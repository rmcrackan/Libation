using Dinah.Core.Collections.Generic;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

#nullable enable
namespace LibationFileManager
{
    public static class QuickFilters
    {
        static QuickFilters()
        {
            // Read file, but convert old format to new (with Name field) as necessary.
            if (!File.Exists(JsonFile))
            {
                inMemoryState = new();
                return;
            }

            try
            {
                if (JsonConvert.DeserializeObject<FilterState>(File.ReadAllText(JsonFile))
                    is FilterState inMemState)
                {
                    inMemoryState = inMemState;
                    return;
                }
            }
            catch
            {
                Serilog.Log.Logger.Information("QuickFilters.json needs upgrade");
            }

            try
            {
                if (JsonConvert.DeserializeObject<OldFilterState>(File.ReadAllText(JsonFile))
                    is OldFilterState inMemState)
                {
                    Serilog.Log.Logger.Error("Old format detected, upgrading QuickFilters.json");

                    // Copy old structure to new.
                    inMemoryState = new();
                    inMemoryState.UseDefault = inMemState.UseDefault;
                    foreach (var oldFilter in inMemState.Filters)
                        inMemoryState.Filters.Add(new NamedFilter(oldFilter, null));

                    Serilog.Log.Logger.Error($"QuickFilters.json upgraded, {inMemState.Filters?.Count ?? 0} filter(s) converted");

                    return;
                }
                Debug.Assert(false, "Should not get here, QuickFilters.json deserialization issue");
            }
            catch (Exception ex)
            {
                Serilog.Log.Logger.Error(ex, "QuickFilters.json could not be upgraded, recreating");
            }

            inMemoryState = new FilterState();
        }

        public static event EventHandler? Updated;

		public static event EventHandler? UseDefaultChanged;

        public class OldFilterState
        {
            public bool UseDefault { get; set; }
            public List<string> Filters { get; set; } = new();
        }

        public class FilterState
        {
            public bool UseDefault { get; set; }
            public List<NamedFilter> Filters { get; set; } = new();
        }

        public static string JsonFile => Path.Combine(Configuration.Instance.LibationFiles, "QuickFilters.json");


		// load json into memory. if file doesn't exist, nothing to do. save() will create if needed
		static FilterState inMemoryState { get; }

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

        // Note that records overload equality automagically, so should be able to
        // compare these the same way as comparing simple strings.
        public record NamedFilter(string Filter, string Name)
        {
            public string Filter { get; set; } = Filter;
            public string Name { get; set; } = Name;
        }

        public static IEnumerable<NamedFilter> Filters => inMemoryState.Filters.AsReadOnly();

        public static void Add(NamedFilter namedFilter)
        {
            if (string.IsNullOrWhiteSpace(namedFilter.Filter))
                return;
            namedFilter.Filter = namedFilter.Filter?.Trim() ?? null;
            namedFilter.Name = namedFilter.Name?.Trim() ?? null;

            lock (locker)
            {
                // check for duplicates
                if (inMemoryState.Filters.Select(x => x.Filter).ContainsInsensative(namedFilter.Filter))
                    return;

                inMemoryState.Filters.Add(namedFilter);
                save();
            }
        }

        public static void Remove(NamedFilter filter)
        {
            lock (locker)
            {
                inMemoryState.Filters.Remove(filter);
                save();
            }
        }

        public static void Edit(NamedFilter oldFilter, NamedFilter newFilter)
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

        public static void ReplaceAll(IEnumerable<NamedFilter> filters)
        {
            filters = filters
                .Where(f => !string.IsNullOrWhiteSpace(f.Filter))
                .Distinct();
            foreach (var filter in filters)
                filter.Filter = filter.Filter.Trim();
            lock (locker)
            {
                inMemoryState.Filters = new List<NamedFilter>(filters);
                save();
            }
        }

        private static object locker { get; } = new();

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
