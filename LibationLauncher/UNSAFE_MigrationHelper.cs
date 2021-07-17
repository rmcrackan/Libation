using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dinah.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LibationLauncher
{
	/// <summary>for migrations only. directly manipulatings settings files without going through domain logic</summary>
	internal static class UNSAFE_MigrationHelper
	{
		#region appsettings.json
		public const string APPSETTINGS_JSON = "appsettings.json";

		public static bool AppSettingsJson_Exists => File.Exists(APPSETTINGS_JSON);

		public static string AppSettings_Get(string key)
		{
			bool success = false;
			JToken val = null;

			process_AppSettingsJson(jObj => success = jObj.TryGetValue(key, out val), false);

			if (success)
				return val.Value<string>();
			return null;
		}

		/// <summary>only insert if not exists</summary>
		public static void AppSettings_Insert(string key, string value)
			=> process_AppSettingsJson(jObj => jObj.TryAdd(key, value));

		/// <summary>only update if exists</summary>
		public static void AppSettings_Update(string key, string value)
			=> process_AppSettingsJson(jObj => {
				if (jObj.ContainsKey(key))
					jObj[key] = value;
			});

		/// <summary>only delete if exists</summary>
		public static void AppSettings_Delete(string key)
			=> process_AppSettingsJson(jObj => {
				if (jObj.ContainsKey(key))
					jObj.Remove(key);
			});

		/// <param name="save">True: save if contents changed. False: no not attempt save</param>
		private static void process_AppSettingsJson(Action<JObject> action, bool save = true)
		{
			// only insert if not exists
			if (!AppSettingsJson_Exists)
				return;

			var startingContents = File.ReadAllText(APPSETTINGS_JSON);
			var jObj = JObject.Parse(startingContents);

			action(jObj);

			if (!save)
				return;

			// only save if different
			var endingContents_indented = jObj.ToString(Formatting.Indented);
			var endingContents_compact = jObj.ToString(Formatting.None);
			if (startingContents.EqualsInsensitive(endingContents_indented) || startingContents.EqualsInsensitive(endingContents_compact))
				return;

			File.WriteAllText(APPSETTINGS_JSON, endingContents_indented);
			System.Threading.Thread.Sleep(100);
		}
		#endregion

		#region Settings.json
		public const string LIBATION_FILES_KEY = "LibationFiles";
		public const string SETTINGS_JSON = "Settings.json";

		public static string SettingsJsonPath
		{
			get
			{
				var value = AppSettings_Get(LIBATION_FILES_KEY);
				return value is null ? null : Path.Combine(value, SETTINGS_JSON);
			}
		}
		public static bool SettingsJson_Exists => SettingsJsonPath is not null && File.Exists(SettingsJsonPath);

		public static string Settings_Get(string key)
		{
			bool success = false;
			JToken val = null;

			process_SettingsJson(jObj => success = jObj.TryGetValue(key, out val), false);

			if (success)
				return val.Value<string>();
			return null;
		}

		/// <summary>only insert if not exists</summary>
		public static void Settings_Insert(string key, string value)
			=> process_SettingsJson(jObj => jObj.TryAdd(key, value));

		/// <summary>only update if exists</summary>
		public static void Settings_Update(string key, string value)
			=> process_SettingsJson(jObj => {
				if (jObj.ContainsKey(key))
					jObj[key] = value;
			});

		/// <summary>only delete if exists</summary>
		public static void Settings_Delete(string key)
			=> process_SettingsJson(jObj => {
				if (jObj.ContainsKey(key))
					jObj.Remove(key);
			});

		/// <param name="save">True: save if contents changed. False: no not attempt save</param>
		private static void process_SettingsJson(Action<JObject> action, bool save = true)
		{
			// only insert if not exists
			if (!SettingsJson_Exists)
				return;

			var startingContents = File.ReadAllText(SettingsJsonPath);
			var jObj = JObject.Parse(startingContents);

			action(jObj);

			if (!save)
				return;

			// only save if different
			var endingContents_indented = jObj.ToString(Formatting.Indented);
			var endingContents_compact = jObj.ToString(Formatting.None);
			if (startingContents.EqualsInsensitive(endingContents_indented) || startingContents.EqualsInsensitive(endingContents_compact))
				return;

			File.WriteAllText(SettingsJsonPath, endingContents_indented);
			System.Threading.Thread.Sleep(100);
		}
		#endregion
	}
}
