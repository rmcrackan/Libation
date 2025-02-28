using System;
using System.ComponentModel;
using System.IO;
using FileManager;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Serilog;
using Dinah.Core.Logging;
using System.Diagnostics;

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
				if (LibationSettingsDirectory is not null)
                    return LibationSettingsDirectory;

                // FIRST: must write here before SettingsFilePath in next step reads cache
                LibationSettingsDirectory = getLibationFilesSettingFromJson();

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

                return LibationSettingsDirectory;
            }
        }

		/// <summary>
		/// Directory pointed to by appsettings.json
		/// </summary>
        private static string? LibationSettingsDirectory { get; set; }

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
			string[] possibleAppsettingsDirectories = new[]
			{
				ProcessDirectory,
				LocalAppData,
				UserProfile,
				Path.Combine(Path.GetTempPath(), "Libation")
			};

			//Try to find and validate appsettings.json in each folder
			foreach (var dir in possibleAppsettingsDirectories)
			{
				var appsettingsFile = Path.Combine(dir, appsettings_filename);

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
			var endingContents = new JObject { { LIBATION_FILES_KEY, DefaultLibationFilesDirectory } }.ToString(Formatting.Indented);

			foreach (var dir in possibleAppsettingsDirectories)
			{
				//Don't try to create appsettings.json in the program files directory on *.nix systems.
				//However, still _look_ for one there for backwards compatibility with previous installations
				if (!IsWindows && dir == ProcessDirectory)
					continue;

				var appsettingsFile = Path.Combine(dir, appsettings_filename);

				try
				{
					Directory.CreateDirectory(dir);
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

			if (IsWindows)
			{
				valueFinal = Environment.ExpandEnvironmentVariables(valueFinal);
			}
			else
			{
				//If the shell command fails and returns null, proceed with the verbatim
				//LIBATION_FILES_KEY path and hope for the best. If Libation can't find
				//anything at this path it will set LIBATION_FILES_KEY to UserProfile
				valueFinal = runShellCommand("echo " + valueFinal) ?? valueFinal;
			}

			return valueFinal;

			static string? runShellCommand(string command)
			{
				var psi = new ProcessStartInfo
				{
					FileName = "/bin/sh",
					RedirectStandardOutput = true,
					UseShellExecute = false,
					CreateNoWindow = true,
					ArgumentList =
					{
						"-c",
						command
					}
				};

				try
				{
					var proc = Process.Start(psi);
					proc?.WaitForExit();
					return proc?.StandardOutput?.ReadToEnd()?.Trim();
				}
				catch (Exception e)
				{
					Serilog.Log.Error(e, "Failed to run shell command. {Arguments}", psi.ArgumentList);
					return null;
				}
			}
		}

        public static void SetLibationFiles(string directory)
        {
            LibationSettingsDirectory = null;

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
