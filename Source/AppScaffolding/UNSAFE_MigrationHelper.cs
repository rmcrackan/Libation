using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dinah.Core;
using LibationFileManager;
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
	public static class UNSAFE_MigrationHelper
	{
		public static string SettingsDirectory
			=> !APPSETTINGS_TryGet(LibationFiles.LIBATION_FILES_KEY, out var value) || value is null
			? null
			: value;

		#region appsettings.json

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
			var startingContents = File.ReadAllText(Configuration.Instance.LibationFiles.AppsettingsJsonFile);

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

			File.WriteAllText(Configuration.Instance.LibationFiles.AppsettingsJsonFile, endingContents_indented);
			System.Threading.Thread.Sleep(100);
		}
		#endregion
		#region Settings.json

		public static string SettingsJsonPath => SettingsDirectory is null ? null : Path.Combine(SettingsDirectory, LibationFiles.SETTINGS_JSON);
		public static bool SettingsJson_Exists => SettingsJsonPath is not null && File.Exists(SettingsJsonPath);

		public static bool Settings_TryGet(string key, out string value)
		{
			bool success = false;
			JToken val = null;

			process_SettingsJson(jObj => success = jObj.TryGetValue(key, out val), false);

			value = success ? val.Value<string>() : null;
			return success;
		}

		public static bool Settings_JsonPathIsType(string jsonPath, JTokenType jTokenType)
		{
			JToken val = null;

			process_SettingsJson(jObj => val = jObj.SelectToken(jsonPath), false);

			return val?.Type == jTokenType;
		}

		public static bool Settings_TryGetFromJsonPath(string jsonPath, out string value)
		{
			JToken val = null;

			process_SettingsJson(jObj => val = jObj.SelectToken(jsonPath), false);

			if (val?.Type == JTokenType.String)
			{
				value = val.Value<string>();
				return true;
			}
			else
			{
				value = null;
				return false;
			}
		}

		public static void Settings_SetWithJsonPath(string jsonPath, string propertyName, string newValue)
		{
			if (!Settings_TryGetFromJsonPath($"{jsonPath}.{propertyName}", out _))
				return;

			process_SettingsJson(jObj =>
			{
				var token = jObj.SelectToken(jsonPath);
				if (token is null
				|| token is not JObject o
				|| o[propertyName] is null)
					return;

				var oldValue = token.Value<string>(propertyName);
				if (oldValue != newValue)
					token[propertyName] = newValue;
			});
		}

		public static bool Settings_TryGetArrayLength(string jsonPath, out int length)
		{
			length = 0;

			if (!Settings_JsonPathIsType(jsonPath, JTokenType.Array))
				return false;

			JArray array = null;
			process_SettingsJson(jObj => array = (JArray)jObj.SelectToken(jsonPath));

			length = array.Count;
			return true;
		}

		public static void Settings_AddToArray(string jsonPath, string newValue)
		{
			if (!Settings_JsonPathIsType(jsonPath, JTokenType.Array))
				return;

			process_SettingsJson(jObj =>
			{
				var array = (JArray)jObj.SelectToken(jsonPath);
				array.Add(newValue);
			});
		}

		/// <summary>Do not add if already exists</summary>
		public static void Settings_AddUniqueToArray(string arrayPath, string newValue)
		{
			if (!Settings_TryGetArrayLength(arrayPath, out var qty))
				return;

			for (var i = 0; i < qty; i++)
			{
				var exists = Settings_TryGetFromJsonPath($"{arrayPath}[{i}]", out var value);
				if (exists && value == newValue)
					return;
			}

			Settings_AddToArray(arrayPath, newValue);
		}

		/// <summary>only remove if not exists</summary>
		public static void Settings_RemoveFromArray(string jsonPath, int position)
		{
			if (!Settings_JsonPathIsType(jsonPath, JTokenType.Array))
				return;

			process_SettingsJson(jObj =>
			{
				var array = (JArray)jObj.SelectToken(jsonPath);
				if (position < array.Count)
					array.RemoveAt(position);
			});
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
		#region LibationContext.db
		public const string LIBATION_CONTEXT = "LibationContext.db";
		public static string DatabaseFile => SettingsDirectory is null ? null : Path.Combine(SettingsDirectory, LIBATION_CONTEXT);
		public static bool DatabaseFile_Exists => DatabaseFile is not null && File.Exists(DatabaseFile);
		#endregion
	}
}
