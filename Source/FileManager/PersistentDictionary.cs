using Dinah.Core.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;

namespace FileManager;

public class PersistentDictionary : IJsonBackedDictionary
{
	public string Filepath { get; }
	public bool IsReadOnly { get; }

	// optimize for strings. expectation is most settings will be strings and a rare exception will be something else
	private Dictionary<string, string?> stringCache { get; } = new();
	private Dictionary<string, object?> objectCache { get; } = new();

	// Configuration.Instance is a process-wide singleton whose properties are read and written from
	// the UI thread, BackgroundWorker callbacks and download workers simultaneously. Every cache and
	// file access below must be serialized: unsynchronized Dictionary writes corrupt the cache, and
	// unsynchronized file access lets a reader observe a half-written file.
	// This lock cannot reach a second process (the GUI and the CLI share Settings.json), which is why
	// every write goes through AtomicFileWriter: an outside reader sees either the old file or the new
	// one, never a truncated one.
	private Lock locker { get; } = new();

	public PersistentDictionary(string filepath, bool isReadOnly = false)
	{
		Filepath = filepath;
		IsReadOnly = isReadOnly;

		if (File.Exists(Filepath) || Path.GetDirectoryName(Filepath) is not string dirName)
			return;

		// will create any missing directories, incl subdirectories. if all already exist: no action
		Directory.CreateDirectory(dirName);

		if (IsReadOnly)
			return;

		createNewFile();
	}

	[return: NotNullIfNotNull(nameof(defaultValue))]
	public string? GetString(string propertyName, string? defaultValue = null)
	{
		lock (locker)
		{
			if (!stringCache.ContainsKey(propertyName))
			{
				var jObject = readFile();
				if (jObject.ContainsKey(propertyName))
					stringCache[propertyName] = jObject[propertyName]?.Value<string>();
				else
					stringCache[propertyName] = defaultValue;
			}

			return stringCache[propertyName];
		}
	}

	[return: NotNullIfNotNull(nameof(defaultValue))]
	public T? GetNonString<T>(string propertyName, T? defaultValue = default)
	{
		object? obj;
		lock (locker)
		{
			obj = getObject(propertyName);

			if (obj is null)
			{
				objectCache[propertyName] = defaultValue;
				return defaultValue;
			}
		}

		// UpCast can throw InvalidConfigurationValueException. Do it outside the lock: it neither
		// reads nor writes the cache, and callers turn the exception into a user-facing error.
		return IJsonBackedDictionary.UpCast<T>(obj, propertyName);
	}

	public object? GetObject(string propertyName)
	{
		lock (locker)
			return getObject(propertyName);
	}

	private object? getObject(string propertyName)
	{
		if (!objectCache.ContainsKey(propertyName))
		{
			var jObject = readFile();
			if (!jObject.ContainsKey(propertyName))
				return null;
			objectCache[propertyName] = jObject[propertyName]?.Value<object>();
		}

		return objectCache[propertyName];
	}

	public string? GetStringFromJsonPath(string jsonPath)
	{
		lock (locker)
		{
			if (!stringCache.ContainsKey(jsonPath))
			{
				try
				{
					var jObject = readFile();
					var token = jObject.SelectToken(jsonPath);
					if (token is null)
						return null;
					stringCache[jsonPath] = token.Value<string>();
				}
				catch
				{
					return null;
				}
			}

			return stringCache[jsonPath];
		}
	}

	public bool Exists(string propertyName)
	{
		lock (locker)
			return readFile().ContainsKey(propertyName);
	}

	public void SetString(string propertyName, string? newValue)
	{
		bool written;
		lock (locker)
		{
			// only do this check in string cache, NOT object cache
			if (stringCache.ContainsKey(propertyName) && stringCache[propertyName] == newValue)
				return;

			// set cache
			stringCache[propertyName] = newValue;

			written = writeFile(propertyName, newValue);
		}

		if (written)
			logConfigChanged(propertyName, newValue);
	}

	public void SetNonString(string propertyName, object? newValue)
	{
		bool written;
		JToken parsedNewValue;
		lock (locker)
		{
			// set cache
			objectCache[propertyName] = newValue;

			parsedNewValue = JToken.Parse(JsonConvert.SerializeObject(newValue));
			written = writeFile(propertyName, parsedNewValue);
		}

		if (written)
			logConfigChanged(propertyName, parsedNewValue.ToString());
	}

	public bool RemoveProperty(string propertyName)
	{
		if (IsReadOnly)
			return false;

		var success = false;
		try
		{
			lock (locker)
			{
				var jObject = readFile();

				if (!jObject.ContainsKey(propertyName))
					return false;

				jObject.Remove(propertyName);

				var endContents = JsonConvert.SerializeObject(jObject, Formatting.Indented);

				writeFileContents(endContents);
				success = true;
			}
			Serilog.Log.Logger.Information("Removed property. {propertyName}", propertyName);
		}
		catch { }

		return success;
	}

	/// <summary>Caller must hold <see cref="locker"/>.</summary>
	/// <returns>The file was rewritten</returns>
	private bool writeFile(string propertyName, JToken? newValue)
	{
		if (IsReadOnly)
			return false;

		// write new setting to file
		var jObject = readFile();
		var startContents = JsonConvert.SerializeObject(jObject, Formatting.Indented);

		jObject[propertyName] = newValue;
		var endContents = JsonConvert.SerializeObject(jObject, Formatting.Indented);

		if (startContents == endContents)
			return false;

		writeFileContents(endContents);
		return true;
	}

	private static void logConfigChanged(string propertyName, string? newValue)
	{
		try
		{
			Serilog.Log.Logger.Information("Config changed. {@DebugInfo}", new { propertyName, newValue = formatValueForLog(newValue) });
		}
		catch { }
	}

	/// <summary>WILL ONLY set if already present. WILL NOT create new</summary>
	/// <returns>Value was changed</returns>
	public bool SetWithJsonPath(string jsonPath, string propertyName, string? newValue, bool suppressLogging = false)
	{
		if (IsReadOnly)
			return false;

		var path = $"{jsonPath}.{propertyName}";

		try
		{
			lock (locker)
			{
				// only do this check in string cache, NOT object cache
				if (stringCache.ContainsKey(path) && stringCache[path] == newValue)
					return false;

				// set cache
				stringCache[path] = newValue;

				var jObject = readFile();
				var token = jObject.SelectToken(jsonPath);
				if (token is null || token[propertyName] is null)
					return false;

				var oldValue = token.Value<string>(propertyName);
				if (oldValue == newValue)
					return false;

				token[propertyName] = newValue;
				writeFileContents(JsonConvert.SerializeObject(jObject, Formatting.Indented));
			}
		}
		catch (Exception exDebug)
		{
			Serilog.Log.Logger.Debug(exDebug, "Silent failure");
			return false;
		}

		if (!suppressLogging)
		{
			try
			{
				var str = formatValueForLog(newValue?.ToString());
				Serilog.Log.Logger.Information("Config changed. {@DebugInfo}", new { jsonPath, propertyName, newValue = str });
			}
			catch { }
		}

		return true;
	}

	private static string formatValueForLog(string? value)
		=> value is null ? "[null]"
		: string.IsNullOrEmpty(value) ? "[empty]"
		: string.IsNullOrWhiteSpace(value) ? $"[whitespace. Length={value.Length}]"
		: value.Length > 100 ? $"[Length={value.Length}] {value[0..50]}...{value[^50..^0]}"
		: value;

	/// <summary>
	/// Replaces the settings file in one step, so a concurrent reader - including one in another
	/// Libation process - sees either the whole old file or the whole new one. Mirrors how
	/// <see cref="Dinah.Core.IO.JsonFilePersister{T}"/> saves AccountsSettings.json.
	/// <para/>
	/// Windows refuses to rename over a file while someone else holds a handle to it, and the CLI,
	/// another GUI instance or a virus scanner can all hold Settings.json for a moment, so retry
	/// before giving up and letting the caller see the failure.
	/// </summary>
	private void writeFileContents(string contents)
	{
		const int attempts = 5;
		for (var attempt = 1; ; attempt++)
		{
			try
			{
				AtomicFileWriter.WriteAllText(Filepath, contents, validateJsonTempFile);
				return;
			}
			catch (Exception ex) when (attempt < attempts && ex is IOException or UnauthorizedAccessException)
			{
				Thread.Sleep(20 * attempt);
			}
		}
	}

	/// <summary>Throws before the temp file replaces the real one, leaving the real one untouched.</summary>
	private static void validateJsonTempFile(string tempPath)
	{
		var contents = File.ReadAllText(tempPath);
		if (string.IsNullOrWhiteSpace(contents))
			throw new JsonSerializationException($"Refusing to write an empty settings file to {tempPath}");
		JToken.Parse(contents);
	}

	/// <summary>Caller must hold <see cref="locker"/>.</summary>
	private JObject readFile()
	{
		if (!File.Exists(Filepath))
		{
			var msg = "Unrecoverable error. Settings file cannot be found";
			var ex = new FileNotFoundException(msg, Filepath);
			Serilog.Log.Logger.Error(ex, msg);
			throw ex;
		}

		var settingsJsonContents = File.ReadAllText(Filepath);

		if (string.IsNullOrWhiteSpace(settingsJsonContents))
		{
			createNewFile();
			settingsJsonContents = File.ReadAllText(Filepath);
		}

		var jObject = JsonConvert.DeserializeObject<JObject>(settingsJsonContents);

		if (jObject is null)
		{
			var msg = "Unrecoverable error. Unable to read settings from Settings file";
			var ex = new NullReferenceException(msg);
			Serilog.Log.Logger.Error(ex, msg);
			throw ex;
		}

		return jObject;
	}

	private void createNewFile()
	{
		writeFileContents("{}");
	}

	public JObject GetJObject()
	{
		lock (locker)
			return readFile();
	}
}
