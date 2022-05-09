using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using AudibleApi.Authorization;
using AudibleUtilities;
using DataLayer;
using Dinah.Core;
using LibationFileManager;
using LibationWinForms.Dialogs;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using Serilog;

namespace LibationWinForms
{
	static class Program
	{
		[System.Runtime.InteropServices.DllImport("kernel32.dll", SetLastError = true)]
		[return: System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.Bool)]
		static extern bool AllocConsole();

		[STAThread]
		static void Main()
		{
			try
			{
				//// Uncomment to see Console. Must be called before anything writes to Console.
				//// Only use while debugging. Acts erratically in the wild
				//AllocConsole();

				ApplicationConfiguration.Initialize();

				//***********************************************//
				//                                               //
				//   do not use Configuration before this line   //
				//                                               //
				//***********************************************//
				// Migrations which must occur before configuration is loaded for the first time. Usually ones which alter the Configuration
				var config = AppScaffolding.LibationScaffolding.RunPreConfigMigrations();

				// do this as soon as possible (post-config)
				RunInstaller(config);

				// most migrations go in here
				AppScaffolding.LibationScaffolding.RunPostConfigMigrations(config);

				// migrations which require Forms or are long-running
				RunWindowsOnlyMigrations(config);

				MessageBoxVerboseLoggingWarning.ShowIfTrue();

#if !DEBUG
				checkForUpdate();
#endif

				AppScaffolding.LibationScaffolding.RunPostMigrationScaffolding(config);
			}
			catch (Exception ex)
			{
				var title = "Fatal error, pre-logging";
				var body = "An unrecoverable error occurred. Since this error happened before logging could be initialized, this error can not be written to the log file.";
				try
				{
					MessageBoxAlertAdmin.Show(body, title, ex);
				}
				catch
				{
					MessageBox.Show($"{body}\r\n\r\n{ex.Message}\r\n\r\n{ex.StackTrace}", title, MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
				return;
			}

			Application.Run(new Form1());
		}

		private static void RunInstaller(Configuration config)
		{
			// all returns should be preceded by either:
			// - if config.LibationSettingsAreValid
			// - error message, Exit()

			if (config.LibationSettingsAreValid)
				return;

			var defaultLibationFilesDir = Configuration.UserProfile;

			// check for existing settigns in default location
			var defaultSettingsFile = Path.Combine(defaultLibationFilesDir, "Settings.json");
			if (Configuration.SettingsFileIsValid(defaultSettingsFile))
				config.SetLibationFiles(defaultLibationFilesDir);

			if (config.LibationSettingsAreValid)
				return;

			static void CancelInstallation()
			{
				MessageBox.Show("Initial set up cancelled.", "Cancelled", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				Application.Exit();
				Environment.Exit(0);
			}

			var setupDialog = new SetupDialog();
			if (setupDialog.ShowDialog() != DialogResult.OK)
			{
				CancelInstallation();
				return;
			}

			if (setupDialog.IsNewUser)
				config.SetLibationFiles(defaultLibationFilesDir);
			else if (setupDialog.IsReturningUser)
			{
				var libationFilesDialog = new LibationFilesDialog();

				if (libationFilesDialog.ShowDialog() != DialogResult.OK)
				{
					CancelInstallation();
					return;
				}

				config.SetLibationFiles(libationFilesDialog.SelectedDirectory);
				if (config.LibationSettingsAreValid)
					return;

				// path did not result in valid settings
				var continueResult = MessageBox.Show(
					$"No valid settings were found at this location.\r\nWould you like to create a new install settings in this folder?\r\n\r\n{libationFilesDialog.SelectedDirectory}",
					"New install?",
					MessageBoxButtons.YesNo,
					MessageBoxIcon.Question);

				if (continueResult != DialogResult.Yes)
				{
					CancelInstallation();
					return;
				}
			}

			// INIT DEFAULT SETTINGS
			// if 'new user' was clicked, or if 'returning user' chose new install: show basic settings dialog
			config.Books ??= Path.Combine(defaultLibationFilesDir, "Books");
			AppScaffolding.LibationScaffolding.PopulateMissingConfigValues(config);

			if (new SettingsDialog().ShowDialog() != DialogResult.OK)
			{
				CancelInstallation();
				return;
			}

			if (config.LibationSettingsAreValid)
				return;

			CancelInstallation();
		}

		/// <summary>migrations which require Forms or are long-running</summary>
		private static void RunWindowsOnlyMigrations(Configuration config)
		{
			// examples:
			// - only supported in winforms. don't move to app scaffolding
			// - long running. won't get a chance to finish in cli. don't move to app scaffolding
		}

		private static void checkForUpdate()
		{
			string zipUrl;
			string htmlUrl;
			string zipName;
			try
			{
				bool hasUpgrade;
				(hasUpgrade, zipUrl, htmlUrl, zipName) = AppScaffolding.LibationScaffolding.GetLatestRelease();

				if (!hasUpgrade)
					return;
			}
			catch (Exception ex)
			{
				MessageBoxAlertAdmin.Show("Error checking for update", "Error checking for update", ex);
				return;
			}

			if (zipUrl is null)
			{
				MessageBox.Show(htmlUrl, "New version available");
				return;
			}

			var result = MessageBox.Show($"New version available @ {htmlUrl}\r\nDownload the zip file?", "New version available", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
			if (result != DialogResult.Yes)
				return;

			try
			{
				using var fileSelector = new SaveFileDialog { FileName = zipName, Filter = "Zip Files (*.zip)|*.zip|All files (*.*)|*.*" };
				if (fileSelector.ShowDialog() != DialogResult.OK)
					return;
				var selectedPath = fileSelector.FileName;

				BookLiberation.ProcessorAutomationController.DownloadFile(zipUrl, selectedPath, true);
			}
			catch (Exception ex)
			{
				MessageBoxAlertAdmin.Show("Error downloading update", "Error downloading update", ex);
			}
		}
	}
}
