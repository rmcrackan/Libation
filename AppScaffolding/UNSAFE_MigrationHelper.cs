using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dinah.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AppScaffolding
{
	/// <summary>
	/// 
	/// 
	///      directly manipulates settings files without going through domain logic.
	///      
	///      for migrations only. use with caution.
	/// 
	/// 
	/// </summary>
	internal static class UNSAFE_MigrationHelper
	{
		#region appsettings.json
		private static string APPSETTINGS_JSON { get; } = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location), "appsettings.json");

		public static bool APPSETTINGS_Json_Exists => File.Exists(APPSETTINGS_JSON);

		public static bool APPSETTINGS_TryGet(string key, out string value)
		{
			bool success = false;
			JToken val = null;

			process_APPSETTINGS_Json(jObj => success = jObj.TryGetValue(key, out val), false);

			value = success ? val.Value<string>() : null;
			return success;
		}

		/// <summary>only insert if not exists</summary>
		public static void APPSETTINGS_Insert(string key, string value)
			=> process_APPSETTINGS_Json(jObj => jObj.TryAdd(key, value));

		/// <summary>only update if exists</summary>
		public static void APPSETTINGS_Update(string key, string value)
			=> process_APPSETTINGS_Json(jObj => {
				if (jObj.ContainsKey(key))
					jObj[key] = value;
			});

		/// <summary>only delete if exists</summary>
		public static void APPSETTINGS_Delete(string key)
			=> process_APPSETTINGS_Json(jObj => {
				if (jObj.ContainsKey(key))
					jObj.Remove(key);
			});

		/// <param name="save">True: save if contents changed. False: no not attempt save</param>
		private static void process_APPSETTINGS_Json(Action<JObject> action, bool save = true)
		{
			// only insert if not exists
			if (!APPSETTINGS_Json_Exists)
				return;

			var startingContents = File.ReadAllText(APPSETTINGS_JSON);

			JObject jObj;
			try
			{
				jObj = JObject.Parse(startingContents);
			}
			catch
			{
				return;
			}

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
		private const string SETTINGS_JSON = "Settings.json";

		public static string SettingsJsonPath
		{
			get
			{
				var success = APPSETTINGS_TryGet(LIBATION_FILES_KEY, out var value);
				return !success || value is null ? null : Path.Combine(value, SETTINGS_JSON);
			}
		}
		public static bool SettingsJson_Exists => SettingsJsonPath is not null && File.Exists(SettingsJsonPath);

		public static bool Settings_TryGet(string key, out string value)
		{
			bool success = false;
			JToken val = null;

			process_SettingsJson(jObj => success = jObj.TryGetValue(key, out val), false);

			value = success ? val.Value<string>() : null;
			return success;
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

			JObject jObj;
			try
			{
				jObj = JObject.Parse(startingContents);
			}
			catch
			{
				return;
			}

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
