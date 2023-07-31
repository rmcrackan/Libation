using System;
using System.ComponentModel;
using System.IO;
using FileManager;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Serilog;
using Dinah.Core.Logging;

#nullable enable
namespace LibationFileManager
{
    public partial class Configuration
    {
        public static string AppsettingsJsonFile { get; } = getOrCreateAppsettingsFile();

		private const string LIBATION_FILES_KEY = "LibationFiles";

        [Description("Location for storage of program-created files")]
        public string LibationFiles
        {
            get
            {
				if (libationFilesPathCache is not null)
                    return libationFilesPathCache;

                // FIRST: must write here before SettingsFilePath in next step reads cache
                libationFilesPathCache = getLibationFilesSettingFromJson();

                // SECOND. before setting to json file with SetWithJsonPath, PersistentDictionary must exist
                persistentDictionary = new PersistentDictionary(SettingsFilePath);

                // Config init in ensureSerilogConfig() only happens when serilog setting is first created (prob on 1st run).
                // This Set() enforces current LibationFiles every time we restart Libation or redirect LibationFiles
                var logPath = Path.Combine(LibationFiles, "Log.log");

                // BAD: Serilog.WriteTo[1].Args
                //      "[1]" assumes ordinal position
                // GOOD: Serilog.WriteTo[?(@.Name=='File')].Args
                var jsonpath = "Serilog.WriteTo[?(@.Name=='File')].Args";

                SetWithJsonPath(jsonpath, "path", logPath, true);

                return libationFilesPathCache;
            }
        }

        private static string? libationFilesPathCache { get; set; }

		/// <summary>
		/// Try to find appsettings.json in the following locations:
		///  <list type="number">
		///		<item>
		///			<description>[App Directory]</description>
		///		</item>
		///		<item>
		///			<description>%LocalAppData%\Libation</description>
		///		</item>
		///		<item>
		///			<description>%AppData%\Libation</description>
		///		</item>
		///		<item>
		///			<description>%Temp%\Libation</description>
		///		</item>
		///  </list>   
		///    
		/// If not found, try to create it in each of the same locations in-order until successful.
		///  
		/// <para>This method must complete successfully for Libation to continue.</para>
		/// </summary>
		/// <returns>appsettings.json file path</returns>
		/// <exception cref="ApplicationException">appsettings.json could not be found or created.</exception>
		private static string getOrCreateAppsettingsFile()
		{
			const string appsettings_filename = "appsettings.json";

			//Possible appsettings.json locations, in order of preference.
			string[] possibleAppsettingsFiles = new[]
			{
				Path.Combine(ProcessDirectory, appsettings_filename),
				Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Libation", appsettings_filename),
				Path.Combine(UserProfile, appsettings_filename),
				Path.Combine(Path.GetTempPath(), "Libation", appsettings_filename)
			};

			//Try to find and validate appsettings.json in each folder
			foreach (var appsettingsFile in possibleAppsettingsFiles)
			{
				if (File.Exists(appsettingsFile))
				{
					try
					{
						var appSettings = JObject.Parse(File.ReadAllText(appsettingsFile));

						if (appSettings.ContainsKey(LIBATION_FILES_KEY)
							&& appSettings[LIBATION_FILES_KEY] is JValue jval
							&& jval.Value is string settingsPath
							&& !string.IsNullOrWhiteSpace(settingsPath))
							return appsettingsFile;
					}
					catch { }
				}
			}

			//Valid appsettings.json not found. Try to create it in each folder.
			var endingContents = new JObject { { LIBATION_FILES_KEY, UserProfile } }.ToString(Formatting.Indented);
			foreach (var appsettingsFile in possibleAppsettingsFiles)
			{
				try
				{
					File.WriteAllText(appsettingsFile, endingContents);
					return appsettingsFile;
				}
				catch(Exception ex)
				{
					Log.Logger.TryLogError(ex, $"Failed to create {appsettingsFile}");
				}
			}

			throw new ApplicationException($"Could not locate or create {appsettings_filename}");
		}

		private static string getLibationFilesSettingFromJson()
        {
            // do not check whether directory exists. special/meta directory (eg: AppDir) is valid
            // verify from live file. no try/catch. want failures to be visible
            var jObjFinal = JObject.Parse(File.ReadAllText(AppsettingsJsonFile));

			if (jObjFinal[LIBATION_FILES_KEY]?.Value<string>() is not string valueFinal)
				throw new InvalidDataException($"{LIBATION_FILES_KEY} not found in {AppsettingsJsonFile}");

            return valueFinal;
        }

        public static void SetLibationFiles(string directory)
        {
            libationFilesPathCache = null;

            var startingContents = File.ReadAllText(AppsettingsJsonFile);
            var jObj = JObject.Parse(startingContents);

            jObj[LIBATION_FILES_KEY] = directory;

            var endingContents = JsonConvert.SerializeObject(jObj, Formatting.Indented);
            if (startingContents == endingContents)
                return;

            try
            {
                // now it's set in the file again but no settings have moved yet
                File.WriteAllText(AppsettingsJsonFile, endingContents);

				Log.Logger.TryLogInformation("Libation files changed {@DebugInfo}", new { AppsettingsJsonFile, LIBATION_FILES_KEY, directory });
			}
			catch (IOException ex)
			{
                Log.Logger.TryLogError(ex, "Failed to change Libation files location {@DebugInfo}", new { AppsettingsJsonFile, LIBATION_FILES_KEY, directory });
			}
		}
    }
}
