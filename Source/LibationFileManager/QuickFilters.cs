using Dinah.Core.Collections.Generic;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace LibationFileManager;

public static class QuickFilters
{
	public static event EventHandler? Updated;

	public static event EventHandler? UseDefaultChanged;

	public class FilterState
	{
		public bool UseDefault { get; set; }
		public List<NamedFilter> Filters { get; set; } = new();
	}

	public static string JsonFile => Path.Combine(Configuration.Instance.LibationFiles.Location, "QuickFilters.json");


	// Lazily loaded from JsonFile on first access. If the file doesn't exist, starts empty; save() will create it if needed.
	private static FilterState? inMemoryState;
	private static FilterState InMemoryState
	{
		get
		{
			lock (locker)
				return inMemoryState ??= LoadFromFile(JsonFile);
		}
	}

	/// <summary>Discard the in-memory state so the next access reloads from disk. For testing.</summary>
	internal static void Reset()
	{
		lock (locker)
			inMemoryState = null;
	}

	internal static FilterState LoadFromFile(string jsonFile)
	{
		try
		{
			if (!File.Exists(jsonFile))
				return new();

			var json = File.ReadAllText(jsonFile);

			try
			{
				if (JsonConvert.DeserializeObject<FilterState>(json) is FilterState state)
					return state;
			}
			catch
			{
				// fall through and try the legacy format
			}

			if (JsonConvert.DeserializeObject<FilterState_v11_4>(json) is FilterState_v11_4 legacyState)
			{
				var state = new FilterState { UseDefault = legacyState.UseDefault };
				foreach (var filter in legacyState.Filters)
					state.Filters.Add(new NamedFilter(filter, null));
				return state;
			}
		}
		catch (Exception ex)
		{
			Serilog.Log.Logger.Error(ex, "Error loading QuickFilters.json");
		}
		return new();
	}

	// Format used until v11.5.0: filters were plain strings without names
	private class FilterState_v11_4
	{
		public bool UseDefault { get; set; }
		public List<string> Filters { get; set; } = new();
	}

	public static bool UseDefault
	{
		get => InMemoryState.UseDefault;
		set
		{
			if (UseDefault == value)
				return;

			lock (locker)
			{
				InMemoryState.UseDefault = value;
				save(false);
			}

			UseDefaultChanged?.Invoke(null, EventArgs.Empty);
		}
	}

	// Note that records overload equality automagically, so should be able to
	// compare these the same way as comparing simple strings.
	public record NamedFilter(string Filter, string? Name)
	{
		public string Filter { get; set; } = Filter;
		public string? Name { get; set; } = Name;
	}

	public static IEnumerable<NamedFilter> Filters
		=> InMemoryState.Filters.AsReadOnly();

	public static void Add(NamedFilter namedFilter)
	{
		if (namedFilter == null)
			throw new ArgumentNullException(nameof(namedFilter));

		if (string.IsNullOrWhiteSpace(namedFilter.Filter))
			return;
		namedFilter.Filter = namedFilter.Filter?.Trim() ?? string.Empty;
		namedFilter.Name = namedFilter.Name?.Trim() ?? null;

		lock (locker)
		{
			// check for duplicates
			if (InMemoryState.Filters.Select(x => x.Filter).ContainsInsensative(namedFilter.Filter))
				return;

			InMemoryState.Filters.Add(namedFilter);
			save();
		}
	}

	public static void Remove(NamedFilter filter)
	{
		lock (locker)
		{
			InMemoryState.Filters.Remove(filter);
			save();
		}
	}

	public static void Edit(NamedFilter oldFilter, NamedFilter newFilter)
	{
		lock (locker)
		{
			if (InMemoryState.Filters.IndexOf(oldFilter) < 0)
				return;

			InMemoryState.Filters = InMemoryState.Filters.Select(f => f == oldFilter ? newFilter : f).ToList();

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
			InMemoryState.Filters = new List<NamedFilter>(filters);
			save();
		}
	}

	private static Lock locker { get; } = new();

	// ONLY call this within lock()
	private static void save(bool invokeUpdatedEvent = true)
	{
		// create json if not exists
		void resave() => File.WriteAllText(JsonFile, JsonConvert.SerializeObject(InMemoryState, Formatting.Indented));
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
