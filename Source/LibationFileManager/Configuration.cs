using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Dinah.Core;
using FileManager;
using Newtonsoft.Json.Linq;

#nullable enable
namespace LibationFileManager
{
	public partial class Configuration : PropertyChangeFilter
	{
		/// <summary>
		/// Returns true if <see cref="SettingsFilePath"/> exists and the <see cref="Books"/> property has a non-null, non-empty value.
		/// Does not verify the existence of the <see cref="Books"/> directory.
		/// </summary>
		public bool LibationSettingsAreValid => SettingsFileIsValid(SettingsFilePath);

		/// <summary>
		/// Returns true if <paramref name="settingsFile"/> exists and the <see cref="Books"/> property has a non-null, non-empty value.
		/// Does not verify the existence of the <see cref="Books"/> directory.
		/// </summary>
		/// <param name="settingsFile">File path to the settings JSON file</param>
		public static bool SettingsFileIsValid(string settingsFile)
		{
			if (!Directory.Exists(Path.GetDirectoryName(settingsFile)) || !File.Exists(settingsFile))
				return false;

			try
			{
				var settingsJson = JObject.Parse(File.ReadAllText(settingsFile));
				return !string.IsNullOrWhiteSpace(settingsJson[nameof(Books)]?.Value<string>());
			}
			catch (Exception ex)
			{
				Serilog.Log.Logger.Error(ex, "Failed to load settings file: {@SettingsFile}", settingsFile);
				try
				{
					Serilog.Log.Logger.Information("Deleting invalid settings file: {@SettingsFile}", settingsFile);
					FileUtility.SaferDelete(settingsFile);
					Serilog.Log.Logger.Information("Creating a new, empty setting file: {@SettingsFile}", settingsFile);
					try
					{
						File.WriteAllText(settingsFile, "{}");
					}
					catch (Exception createEx)
					{
						Serilog.Log.Logger.Error(createEx, "Failed to create new settings file: {@SettingsFile}", settingsFile);
					}
				}
				catch (Exception deleteEx)
				{
					Serilog.Log.Logger.Error(deleteEx, "Failed to delete the invalid settings file: {@SettingsFile}", settingsFile);
				}

				return false;
			}
		}

		#region singleton stuff

		private static readonly Configuration s_SingletonInstance = new();
		public static Configuration Instance { get; private set; } = s_SingletonInstance;

		public static Configuration CreateMockInstance()
		{
#if !DEBUG
			throw new InvalidOperationException("CreateMockInstance should only be called in design mode.");
#endif
			var mockInstance = new Configuration() { persistentDictionary = new MockPersistentDictionary() };
			mockInstance.SetString("Light", "ThemeVariant");
			Instance = mockInstance;
			return mockInstance;
		}
		public static void RestoreSingletonInstance()
		{
			Instance = s_SingletonInstance;
		}

		private Configuration() { }
		#endregion
	}
}
