using Dinah.Core.Logging;
using FileManager;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("AppScaffolding")]
[assembly: InternalsVisibleTo("LibationUiBase.Tests")]

namespace LibationFileManager;

/// <summary>
/// Provides access to Libation's configuration and settings file locations, including methods for validating and
/// updating the Libation files directory and Settings.json file. An instance is bount to a single appsettings.json file.
/// </summary>
public class LibationFiles
{
	internal static string? s_DefaultLibationFilesDirectory;
	public static string DefaultLibationFilesDirectory => s_DefaultLibationFilesDirectory ??= Configuration.IsWindows ? Configuration.UserProfile : Configuration.LocalAppData;

	public const string LIBATION_FILES_KEY = "LibationFiles";
	public const string SETTINGS_JSON = "Settings.json";
	public const string LIBATION_FILES_DIR = "LIBATION_FILES_DIR";

	/// <summary>
	/// Directory pointed to by appsettings.json
	/// </summary>
	public LongPath Location { get; private set; }
	/// <summary>
	/// Returns true if <see cref="SettingsFilePath"/> exists and the <see cref="Configuration.Books"/> property has a non-null, non-empty value.
	/// Does not verify the existence of the <see cref="Configuration.Books"/> directory.
	/// </summary>
	public bool SettingsAreValid => SettingsFileIsValid(SettingsFilePath);
	/// <summary>
	/// Found Location of appsettings.json. This file must exist or be able to be created for Libation to start.
	/// </summary>
	internal string? AppsettingsJsonFile { get; }
	/// <summary>
	/// File path to Settings.json inside <see cref="Location"/>
	/// </summary>
	public string SettingsFilePath => Path.Combine(Location, SETTINGS_JSON);

	internal LibationFiles()
	{
		var libationFilesDir = Environment.GetEnvironmentVariable(LIBATION_FILES_DIR);
		if (Directory.Exists(libationFilesDir))
		{
			Location = libationFilesDir;
		}
		else
		{
			AppsettingsJsonFile = GetOrCreateAppsettingsFile();
			Location = GetLibationFilesFromAppsettings(AppsettingsJsonFile);
		}
	}

	internal LibationFiles(string appSettingsFile)
	{
		AppsettingsJsonFile = appSettingsFile;
		Location = GetLibationFilesFromAppsettings(AppsettingsJsonFile);
	}

	/// <summary>
	/// Set the location of the Libation Files directory, updating appsettings.json. 
	/// </summary>
	public void SetLibationFiles(LongPath libationFilesDirectory)
	{
		if (AppsettingsJsonFile is null)
		{
			Environment.SetEnvironmentVariable(LIBATION_FILES_DIR, libationFilesDirectory);
			return;
		}

		var startingContents = File.ReadAllText(AppsettingsJsonFile);
		var jObj = JObject.Parse(startingContents);

		jObj[LIBATION_FILES_KEY] = (string)(Location = libationFilesDirectory);

		var endingContents = JsonConvert.SerializeObject(jObj, Formatting.Indented);
		if (startingContents == endingContents)
			return;

		try
		{
			// now it's set in the file again but no settings have moved yet
			File.WriteAllText(AppsettingsJsonFile, endingContents);
			Log.Logger.TryLogInformation("Libation files changed {@DebugInfo}", new { AppsettingsJsonFile, LIBATION_FILES_KEY, libationFilesDirectory });
		}
		catch (Exception ex)
		{
			Log.Logger.TryLogError(ex, "Failed to change Libation files location {@DebugInfo}", new { AppsettingsJsonFile, LIBATION_FILES_KEY, libationFilesDirectory });
		}
	}

	/// <summary>
	/// Returns true if <paramref name="settingsFile"/> exists and the <see cref="Configuration.Books"/> property has a non-null, non-empty value.
	/// Does not verify the existence of the <see cref="Configuration.Books"/> directory.
	/// </summary>
	/// <param name="settingsFile">File path to the Settings.json file</param>
	public static bool SettingsFileIsValid(string settingsFile)
	{
		if (!Directory.Exists(Path.GetDirectoryName(settingsFile)) || !File.Exists(settingsFile))
			return false;

		try
		{
			var settingsJson = JObject.Parse(File.ReadAllText(settingsFile));
			return !string.IsNullOrWhiteSpace(settingsJson[nameof(Configuration.Books)]?.Value<string>());
		}
		catch (Exception ex)
		{
			Log.Logger.Error(ex, "Failed to load settings file: {SettingsFile}", settingsFile);
			try
			{
				Log.Logger.Information("Deleting invalid settings file: {SettingsFile}", settingsFile);
				FileUtility.SaferDelete(settingsFile);
				Log.Logger.Information("Creating a new, empty setting file: {SettingsFile}", settingsFile);
				try
				{
					File.WriteAllText(settingsFile, "{}");
				}
				catch (Exception createEx)
				{
					Log.Logger.Error(createEx, "Failed to create new settings file: {SettingsFile}", settingsFile);
				}
			}
			catch (Exception deleteEx)
			{
				Log.Logger.Error(deleteEx, "Failed to delete the invalid settings file: {SettingsFile}", settingsFile);
			}

			return false;
		}
	}

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
	private static string GetOrCreateAppsettingsFile()
	{
		const string appsettings_filename = "appsettings.json";

		//Possible appsettings.json locations, in order of preference.
		string[] possibleAppsettingsDirectories = new[]
		{
				Configuration.ProcessDirectory,
				Configuration.LocalAppData,
				Configuration.UserProfile,
				Configuration.WinTemp,
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
			if (!Configuration.IsWindows && dir == Configuration.ProcessDirectory)
				continue;

			var appsettingsFile = Path.Combine(dir, appsettings_filename);

			try
			{
				Directory.CreateDirectory(dir);
				File.WriteAllText(appsettingsFile, endingContents);
				return appsettingsFile;
			}
			catch (Exception ex)
			{
				Log.Logger.TryLogError(ex, $"Failed to create {appsettingsFile}");
			}
		}

		throw new ApplicationException($"Could not locate or create {appsettings_filename}");
	}

	/// <summary>
	/// Get the LibationFiles directory from appsettings.json, expanding environment variables as needed.
	/// </summary>
	/// <param name="appsettingsPath"></param>
	/// <returns></returns>
	/// <exception cref="InvalidDataException">The appsettings.json file does not contain a "LibationFiles" key</exception>
	private static string GetLibationFilesFromAppsettings(LongPath appsettingsPath)
	{
		// do not check whether directory exists. special/meta directory (eg: AppDir) is valid
		// verify from live file. no try/catch. want failures to be visible
		var jObjFinal = JObject.Parse(File.ReadAllText(appsettingsPath));

		if (jObjFinal[LIBATION_FILES_KEY]?.Value<string>() is not string libationFiles)
			throw new InvalidDataException($"{LIBATION_FILES_KEY} not found in {appsettingsPath}");

		if (Configuration.IsWindows)
		{
			libationFiles = Environment.ExpandEnvironmentVariables(libationFiles);
		}
		else
		{
			//If the shell command fails and returns null, proceed with the verbatim
			//LIBATION_FILES_KEY path and hope for the best. If Libation can't find
			//anything at this path it will set LIBATION_FILES_KEY to UserProfile
			libationFiles = runShellCommand("echo " + libationFiles) ?? libationFiles;
		}

		return libationFiles;

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
				Log.Error(e, "Failed to run shell command. {@Arguments}", psi.ArgumentList);
				return null;
			}
		}
	}
}
