using Dinah.Core;
using Dinah.Core.Logging;
using LibationFileManager;
using LibationUiBase.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;
using System;
using System.IO;
using System.Threading.Tasks;

namespace LibationUiBase;

/// <summary>
/// Contains the results of a, initial setup prompt.
/// </summary>
public interface ILibationSetup
{
	public bool IsNewUser { get; }
	public bool IsReturningUser { get; }
}

/// <summary>
/// Contains the results of a Libation Files install location selection prompt.
/// </summary>
public interface ILibationInstallLocation
{
	public string? SelectedDirectory { get; }
}

/// <summary>
/// Provides configuration and delegates for running the Libation setup process, including user prompts for initial
/// setup and selecting installation locations.
/// </summary>
/// <remarks>LibationSetup encapsulates the logic required to ensure that Libation is properly configured before
/// use. This class is used at application startup to ensure that all required settings are present and valid
/// before proceeding.</remarks>
public class LibationSetup
{
	/// <summary> Asynchronous delegate to show the setup prompt </summary>
	public Func<Task<ILibationSetup>>? SetupPromptAsync { get; init; }
	/// <summary> Asynchronous delegate to show the Libation Files selection dialog prompt </summary>
	public Func<Task<ILibationInstallLocation?>>? SelectFolderPromptAsync { get; init; }
	/// <summary> Synchronous delegate to show the setup prompt </summary>
	public Func<ILibationSetup>? SetupPrompt { get; init; }
	/// <summary> Synchronous delegate to show the Libation Files selection dialog prompt </summary>
	public Func<ILibationInstallLocation?>? SelectFolderPrompt { get; init; }

	private LibationFiles Files { get; }
	public LibationSetup(LibationFiles libationFiles)
	{
		Files = libationFiles;
	}

	/// <summary>
	/// Runs Libation setup if needed.
	/// Verifies that 
	/// </summary>
	/// <returns></returns>
	/// <exception cref="InvalidOperationException"></exception>
	public async Task<bool> RunSetupIfNeededAsync()
	{
		// all returns should be preceded by either:
		// - if libationFiles.LibationSettingsAreValid
		// - OnCancelled()
		if (Files.SettingsAreValid)
			return true;

		// check for existing settings in default location
		// First check if file exists so that, if it doesn't, we don't
		// overwrite user's LibationFiles setting in appsettings.json
		var defaultSettingsFile = Path.Combine(LibationFiles.DefaultLibationFilesDirectory, LibationFiles.SETTINGS_JSON);
		if (File.Exists(defaultSettingsFile) && LibationFiles.SettingsFileIsValid(defaultSettingsFile))
		{
			Files.SetLibationFiles(LibationFiles.DefaultLibationFilesDirectory);

			if (Files.SettingsAreValid)
				return true;
		}

		var setupResult
			= SetupPromptAsync is not null ? await SetupPromptAsync()
			: SetupPrompt is not null ? SetupPrompt()
			: throw new InvalidOperationException("No setup prompt provided");

		if (setupResult.IsNewUser)
		{
			return await CreateDefaultSettingsAsync();
		}
		else if (setupResult.IsReturningUser)
		{
			var chooseFolderResult
				= SelectFolderPromptAsync is not null ? await SelectFolderPromptAsync()
				: SelectFolderPrompt is not null ? SelectFolderPrompt()
				: throw new InvalidOperationException("No select folder prompt provided");

			if (string.IsNullOrWhiteSpace(chooseFolderResult?.SelectedDirectory))
				return false;

			Files.SetLibationFiles(chooseFolderResult.SelectedDirectory);
			if (Files.SettingsAreValid)
				return true;

			// path did not result in valid settings
			var continueResult = await MessageBoxBase.ShowAsyncImpl(null,
				$"""
				No valid settings were found at this location.				
				Would you like to create a new install settings in this folder?
				
				{chooseFolderResult.SelectedDirectory}
				""",
				"New install?",
				MessageBoxButtons.YesNo,
				MessageBoxIcon.Question,
				MessageBoxDefaultButton.Button1);

			return continueResult == DialogResult.Yes && await CreateDefaultSettingsAsync();
		}
		else
		{
			return false;
		}
	}

	private async Task<bool> CreateDefaultSettingsAsync()
	{
		if (!TryCreateDirectory())
		{
			var result = await MessageBoxBase.ShowAsyncImpl(null,
					$"""
					Could not create the Libation Settings folder at:
					{Files.Location.Path}
					
					Would you like to create a new install settings in this folder?
					{LibationFiles.DefaultLibationFilesDirectory}
					""",
					"Error Creating Libation Settings",
					MessageBoxButtons.YesNo,
					MessageBoxIcon.Question,
					MessageBoxDefaultButton.Button1);

			if (result is not DialogResult.Yes)
				return false;

			Files.SetLibationFiles(LibationFiles.DefaultLibationFilesDirectory);
			//We should never not be able to access DefaultLibationFilesDirectory.
			//If we can't write here, something is very wrong and we shouldn't even try to continue.
			if (!TryCreateDirectory())
			{
				await MessageBoxBase.ShowAsyncImpl(null,
					$"""
					An error occurred while creating default settings folder:
					{LibationFiles.DefaultLibationFilesDirectory}
					""",
					"Error Creating Libation Settings",
					MessageBoxButtons.OK,
					MessageBoxIcon.Error,
					MessageBoxDefaultButton.Button1);
				return false;
			}
		}

		if (Files.SettingsAreValid)
			return true;

		try
		{
			WriteDefaultSettingsFile(Files.SettingsFilePath);
			return Files.SettingsAreValid;
		}
		catch (Exception ex)
		{
			// We are able to create the LibationFiles directory, but we can't write a settings file in it.
			// Examples of this is the root of a system drive (C:\)
			Log.Logger.TryLogError(ex, $"Failed to create {LibationFiles.SETTINGS_JSON} in {Files.Location}");


			if (!Files.Location.PathWithoutPrefix.Equals(LibationFiles.DefaultLibationFilesDirectory, StringComparison.OrdinalIgnoreCase))
			{
				var result = await MessageBoxBase.ShowAsyncImpl(null,
					$"""
					Could not create the Libation Settings file at:
					{Files.SettingsFilePath}
					
					Would you like to create a new install settings in this folder?
					{LibationFiles.DefaultLibationFilesDirectory}
					""",
					"Error Creating Libation Settings",
					MessageBoxButtons.YesNo,
					MessageBoxIcon.Question,
					MessageBoxDefaultButton.Button1);

				if (result is not DialogResult.Yes)
					return false;

				// Try again in the default location
				Log.Logger.TryLogInformation($"Changing {LibationFiles.LIBATION_FILES_KEY} to {LibationFiles.DefaultLibationFilesDirectory}");
				Files.SetLibationFiles(LibationFiles.DefaultLibationFilesDirectory);
				return await CreateDefaultSettingsAsync();
			}
			else
			{
				await MessageBoxBase.ShowAsyncImpl(null,
					$"""
					An error occurred while creating default settings file in:
					{LibationFiles.DefaultLibationFilesDirectory}
					""",
					"Error Creating Libation Settings",
					MessageBoxButtons.OK,
					MessageBoxIcon.Error,
					MessageBoxDefaultButton.Button1);
				return false;
			}
		}
	}

	private bool TryCreateDirectory()
	{
		try
		{
			Directory.CreateDirectory(Files.Location);
			return true;
		}
		catch (Exception ex)
		{
			Log.Logger.TryLogError(ex, $"Failed to create {LibationFiles.LIBATION_FILES_KEY} directory at {Files.Location}");
			return false;
		}
	}

	private void WriteDefaultSettingsFile(string settingsFilePath)
	{
		var booksParent = Configuration.IsWindows ? Files.Location.Path : Configuration.MyMusic;
		var jObj = new JObject
		{
			{ nameof(Configuration.Books), Path.Combine(booksParent, nameof(Configuration.Books)) }
		};
		var contents = JsonConvert.SerializeObject(jObj, Formatting.Indented);
		File.WriteAllText(settingsFilePath, contents);
	}
}
