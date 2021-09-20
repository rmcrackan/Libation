using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using AudibleApi.Authorization;
using DataLayer;
using Dinah.Core;
using FileManager;
using InternalUtilities;
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
			//// Uncomment to see Console. Must be called before anything writes to Console.
			//// Only use while debugging. Acts erratically in the wild
			//AllocConsole();

			Application.SetHighDpiMode(HighDpiMode.SystemAware);
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

			//***********************************************//
			//                                               //
			//   do not use Configuration before this line   //
			//                                               //
			//***********************************************//
			// Migrations which must occur before configuration is loaded for the first time. Usually ones which alter the Configuration
			var config = AppScaffolding.LibationScaffolding.RunPreConfigMigrations();

			RunInstaller(config);

			// most migrations go in here
			AppScaffolding.LibationScaffolding.RunPostConfigMigrations();

			// migrations which require Forms or are long-running
			RunWindowsOnlyMigrations(config);

			MessageBoxVerboseLoggingWarning.ShowIfTrue();

#if !DEBUG
			checkForUpdate();
#endif

			AppScaffolding.LibationScaffolding.RunPostMigrationScaffolding();

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

			// if 'new user' was clicked, or if 'returning user' chose new install: show basic settings dialog
			config.Books ??= Path.Combine(defaultLibationFilesDir, "Books");
			config.InProgress ??= Configuration.WinTemp;
			config.AllowLibationFixup = true;
			config.DecryptToLossy = false;

			if (new SettingsDialog().ShowDialog() != DialogResult.OK)
			{
				CancelInstallation();
				return;
			}

			if (config.LibationSettingsAreValid)
				return;

			CancelInstallation();
		}

		private static void RunWindowsOnlyMigrations(Configuration config)
		{
			// only supported in winforms. don't move to app scaffolding
			migrate_to_v5_0_0(config);

			// long running. won't get a chance to finish in cli. don't move to app scaffolding
			migrate_to_v5_5_0(config);
		}

		#region migrate to v5.0.0 re-register device if device info not in settings
		private static void migrate_to_v5_0_0(Configuration config)
		{
			if (!config.Exists(nameof(config.AllowLibationFixup)))
				config.AllowLibationFixup = true;

			if (!File.Exists(AudibleApiStorage.AccountsSettingsFile))
				return;

			var accountsPersister = AudibleApiStorage.GetAccountsSettingsPersister();

			var accounts = accountsPersister?.AccountsSettings?.Accounts;
			if (accounts is null)
				return;

			foreach (var account in accounts)
			{
				var identity = account?.IdentityTokens;

				if (identity is null)
					continue;

				if (!string.IsNullOrWhiteSpace(identity.DeviceType) &&
					!string.IsNullOrWhiteSpace(identity.DeviceSerialNumber) &&
					!string.IsNullOrWhiteSpace(identity.AmazonAccountId))
					continue;

				var authorize = new Authorize(identity.Locale);

				try
				{
					authorize.DeregisterAsync(identity.ExistingAccessToken, identity.Cookies.ToKeyValuePair()).GetAwaiter().GetResult();
					identity.Invalidate();

					// re-registers device
					ApiExtended.CreateAsync(account, new Login.WinformLoginChoiceEager(account)).GetAwaiter().GetResult();
				}
                catch
                {
					// Don't care if it fails
                }
			}
		}
		#endregion

		#region migrate to v5.5.0. FilePaths.json => db. long running. fire and forget
		private static void migrate_to_v5_5_0(Configuration config)
			=> new System.Threading.Thread(() => migrate_to_v5_5_0_thread(config)) { IsBackground = true }.Start();
		private static void migrate_to_v5_5_0_thread(Configuration config)
		{
			try
			{
				var filePaths = Path.Combine(config.LibationFiles, "FilePaths.json");
				if (!File.Exists(filePaths))
					return;

				var fileLocations = Path.Combine(config.LibationFiles, "FileLocations.json");
				if (!File.Exists(fileLocations))
					File.Copy(filePaths, fileLocations);

				// files to be deleted at the end
				var libhackFilesToDelete = new List<string>();
				// .libhack files => errors
				var libhackFiles = Directory.EnumerateDirectories(config.Books, "*.libhack", SearchOption.AllDirectories);

				using var context = ApplicationServices.DbContexts.GetContext();
				context.Books.Load();

				var jArr = JArray.Parse(File.ReadAllText(filePaths));

				foreach (var jToken in jArr)
				{
					var asinToken = jToken["Id"];
					var fileTypeToken = jToken["FileType"];
					var pathToken = jToken["Path"];
					if (asinToken is null || fileTypeToken is null || pathToken is null ||
						asinToken.Type != JTokenType.String || fileTypeToken.Type != JTokenType.Integer || pathToken.Type != JTokenType.String)
						continue;

					var asin = asinToken.Value<string>();
					var fileType = (FileType)fileTypeToken.Value<int>();
					var path = pathToken.Value<string>();

					if (fileType == FileType.Unknown || fileType == FileType.AAXC)
						continue;

					var book = context.Books.Local.FirstOrDefault(b => b.AudibleProductId == asin);
					if (book is null)
						continue;

					// assign these strings and enums/ints unconditionally. EFCore will only update if changed
					if (fileType == FileType.PDF)
						book.UserDefinedItem.PdfStatus = LiberatedStatus.Liberated;

					if (fileType == FileType.Audio)
					{
						var lhack = libhackFiles.FirstOrDefault(f => f.ContainsInsensitive(asin));
						if (lhack is null)
							book.UserDefinedItem.BookStatus = LiberatedStatus.Liberated;
						else
						{
							book.UserDefinedItem.BookStatus = LiberatedStatus.Error;
							libhackFilesToDelete.Add(lhack);
						}
					}
				}

				context.SaveChanges();

				// only do this after save changes
				foreach (var libhackFile in libhackFilesToDelete)
					File.Delete(libhackFile);

				File.Delete(filePaths);
			}
			catch (Exception ex)
			{
				Log.Logger.Error(ex, "Error attempting to insert FilePaths into db");
			}
		}
		#endregion

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

			using var fileSelector = new SaveFileDialog { FileName = zipName, Filter = "Zip Files (*.zip)|*.zip|All files (*.*)|*.*" };
			if (fileSelector.ShowDialog() != DialogResult.OK)
				return;
			var selectedPath = fileSelector.FileName;

			try
			{
				LibationWinForms.BookLiberation.ProcessorAutomationController.DownloadFile(zipUrl, selectedPath, true);
			}
			catch (Exception ex)
			{
				MessageBoxAlertAdmin.Show("Error downloading update", "Error downloading update", ex);
			}
		}
	}
}
