using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using AudibleApi;
using AudibleApi.Authorization;
using Dinah.Core.IO;
using Dinah.Core.Logging;
using FileManager;
using InternalUtilities;
using LibationWinForms;
using LibationWinForms.Dialogs;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;

namespace LibationLauncher
{
	static class Program
	{
		[System.Runtime.InteropServices.DllImport("kernel32.dll", SetLastError = true)]
		[return: System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.Bool)]
		static extern bool AllocConsole();

		[STAThread]
		static void Main()
		{
			//// uncomment to see Console. MUST be called before anything writes to Console. Might only work from VS
			//AllocConsole();

			Application.SetHighDpiMode(HighDpiMode.SystemAware);
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

			// must occur before access to Configuration instance
			migrate_to_v5_2_0__pre_config();


			//***********************************************//
			//                                               //
			//   do not use Configuration before this line   //
			//                                               //
			//***********************************************//


			var config = Configuration.Instance;

			createSettings(config);

			AudibleApiStorage.EnsureAccountsSettingsFileExists();

			migrate_to_v5_0_0(config);
			migrate_to_v5_2_0__post_config(config);
			migrate_to_v5_3_10(config);

			ensureSerilogConfig(config);
			configureLogging(config);
			logStartupState(config);
			checkForUpdate(config);

			Application.Run(new Form1());
		}

		private static void createSettings(Configuration config)
		{
			// all returns should be preceded by either:
			// - if config.LibationSettingsAreValid
			// - error message, Exit()

			static void CancelInstallation()
			{
				MessageBox.Show("Initial set up cancelled.", "Cancelled", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				Application.Exit();
				Environment.Exit(0);
			}

			if (config.LibationSettingsAreValid)
				return;

			var defaultLibationFilesDir = Configuration.UserProfile;

			// check for existing settigns in default location
			var defaultSettingsFile = Path.Combine(defaultLibationFilesDir, "Settings.json");
			if (Configuration.SettingsFileIsValid(defaultSettingsFile))
				config.SetLibationFiles(defaultLibationFilesDir);

			if (config.LibationSettingsAreValid)
				return;

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
				MessageBox.Show(
					$"No valid settings were found at this location.\r\nWould you like to create a new install settings in this folder?\r\n\r\n{libationFilesDialog.SelectedDirectory}",
					"New install?",
					MessageBoxButtons.YesNo,
					MessageBoxIcon.Question);

				if (libationFilesDialog.ShowDialog() != DialogResult.Yes)
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

		#region migrate to v5.0.0: re-register device if device info not in settings
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

					var api = AudibleApiActions.GetApiAsync(new LibationWinForms.Login.WinformResponder(account), account).GetAwaiter().GetResult();
				}
                catch
                {
					// Don't care if it fails
                }
			}
		}
		#endregion

		#region migrate to v5.2.0
		// get rid of meta-directories, combine DownloadsInProgressEnum and DecryptInProgressEnum => InProgress
		private static void migrate_to_v5_2_0__pre_config()
		{
			{
				var settingsKey = "DownloadsInProgressEnum";
				if (UNSAFE_MigrationHelper.Settings_TryGet(settingsKey, out var value))
				{
					UNSAFE_MigrationHelper.Settings_Delete(settingsKey);
					UNSAFE_MigrationHelper.Settings_Insert("InProgress", translatePath(value));
				}
			}

			{
				UNSAFE_MigrationHelper.Settings_Delete("DecryptInProgressEnum");
			}

			{ // appsettings.json
				var appSettingsKey = UNSAFE_MigrationHelper.LIBATION_FILES_KEY;
				if (UNSAFE_MigrationHelper.APPSETTINGS_TryGet(appSettingsKey, out var value))
					UNSAFE_MigrationHelper.APPSETTINGS_Update(appSettingsKey, translatePath(value));
			}
		}

		private static string translatePath(string path)
			=> path switch
			{
				"AppDir" => @".\LibationFiles",
				"MyDocs" => Path.GetFullPath(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "LibationFiles")),
				"UserProfile" => Path.GetFullPath(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Libation")),
				"WinTemp" => Path.GetFullPath(Path.Combine(Path.GetTempPath(), "Libation")),
				_ => path
			};

		private static void migrate_to_v5_2_0__post_config(Configuration config)
		{
			if (!config.Exists(nameof(config.AllowLibationFixup)))
				config.AllowLibationFixup = true;

			if (!config.Exists(nameof(config.DecryptToLossy)))
				config.DecryptToLossy = false;
		}
		#endregion

		#region migrate to v5.3.10: rename BookTags.json to UserDefinedItems.json
		private static void migrate_to_v5_3_10(Configuration config)
		{
			var oldPath = Path.Combine(config.LibationFiles, "BookTags.json");

			if (File.Exists(oldPath))
			{
				var newPath = Path.Combine(config.LibationFiles, "UserDefinedItems.json");
				File.Move(oldPath, newPath);
			}
		}
		#endregion

		private static void ensureSerilogConfig(Configuration config)
		{
			if (config.GetObject("Serilog") != null)
				return;

			// "Serilog": {
			//   "MinimumLevel": "Information"
			//   "WriteTo": [
			//     {
			//       "Name": "Console"
			//     },
			//     {
			//       "Name": "File",
			//       "Args": {
			//         "rollingInterval": "Day",
			//         "outputTemplate": ...
			//       }
			//     }
			//   ],
			//   "Using": [ "Dinah.Core" ],
			//   "Enrich": [ "WithCaller" ]
			// }
			var serilogObj = new JObject
			{
				{ "MinimumLevel", "Information" },
				{ "WriteTo", new JArray
					{
						new JObject { {"Name", "Console" } },
						new JObject
						{
							{ "Name", "File" },
							{ "Args",
								new JObject
								{
									// for this sink to work, a path must be provided. we override this below
									{ "path", Path.Combine(config.LibationFiles, "_Log.log") },
									{ "rollingInterval", "Month" },
									// Serilog template formatting examples
									// - default:                    "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
									//   output example:             2019-11-26 08:48:40.224 -05:00 [DBG] Begin Libation
									// - with class and method info: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] (at {Caller}) {Message:lj}{NewLine}{Exception}";
									//   output example:             2019-11-26 08:48:40.224 -05:00 [DBG] (at LibationWinForms.Program.init()) Begin Libation
									{ "outputTemplate", "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] (at {Caller}) {Message:lj}{NewLine}{Exception}" }
								}
							}
						}
					}
				},
				{ "Using", new JArray{ "Dinah.Core" } }, // dll's name, NOT namespace
				{ "Enrich", new JArray{ "WithCaller" } },
			};
			config.SetObject("Serilog", serilogObj);
		}

		// to restore original: Console.SetOut(origOut);
		private static TextWriter origOut { get; } = Console.Out;

		private static void configureLogging(Configuration config)
		{
			config.ConfigureLogging();

			// Fwd Console to serilog.
			// Serilog also write to Console (should probably change this) so it might be asking for trouble.
			// SerilogTextWriter needs to be more robust and tested. Esp the Write() methods.
			// Empirical testing so far has shown no issues.
			Console.SetOut(new MultiTextWriter(origOut, new SerilogTextWriter()));

			// .Here() captures debug info via System.Runtime.CompilerServices attributes. Warning: expensive
			//var withLineNumbers_outputTemplate = "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message}{NewLine}in method {MemberName} at {FilePath}:{LineNumber}{NewLine}{Exception}{NewLine}";
			//Log.Logger.Here().Debug("Begin Libation. Debug with line numbers");
		}

		private static void logStartupState(Configuration config)
		{
			// begin logging session with a form feed
			Log.Logger.Information("\r\n\f");
			Log.Logger.Information("Begin Libation. {@DebugInfo}", new
			{
				Version = BuildVersion.ToString(),

				LogLevel_Verbose_Enabled = Log.Logger.IsVerboseEnabled(),
				LogLevel_Debug_Enabled = Log.Logger.IsDebugEnabled(),
				LogLevel_Information_Enabled = Log.Logger.IsInformationEnabled(),
				LogLevel_Warning_Enabled = Log.Logger.IsWarningEnabled(),
				LogLevel_Error_Enabled = Log.Logger.IsErrorEnabled(),
				LogLevel_Fatal_Enabled = Log.Logger.IsFatalEnabled(),

				config.LibationFiles,
				AudibleFileStorage.BooksDirectory,

				config.InProgress,

				DownloadsInProgressDir = AudibleFileStorage.DownloadsInProgress,
				DownloadsInProgressFiles = Directory.EnumerateFiles(AudibleFileStorage.DownloadsInProgress).Count(),

				DecryptInProgressDir = AudibleFileStorage.DecryptInProgress,
				DecryptInProgressFiles = Directory.EnumerateFiles(AudibleFileStorage.DecryptInProgress).Count(),
			});

			MessageBoxVerboseLoggingWarning.ShowIfTrue();
		}

		private static void checkForUpdate(Configuration config)
		{
			string zipUrl;
			string selectedPath;

			try
			{
				// timed out
				var latest = getLatestRelease(TimeSpan.FromSeconds(10));
				if (latest is null)
					return;

				var latestVersionString = latest.TagName.Trim('v');
				if (!Version.TryParse(latestVersionString, out var latestRelease))
					return;

				// we're up to date
				if (latestRelease <= BuildVersion)
					return;

				// we have an update
				var zip = latest.Assets.FirstOrDefault(a => a.BrowserDownloadUrl.EndsWith(".zip"));
				zipUrl = zip?.BrowserDownloadUrl;

				Log.Logger.Information("Update available: {@DebugInfo}", new {
					latestRelease = latestRelease.ToString(),
					latest.HtmlUrl,
					zipUrl
				});

				if (zipUrl is null)
				{
					MessageBox.Show(latest.HtmlUrl, "New version available");
					return;
				}

				var result = MessageBox.Show($"New version available @ {latest.HtmlUrl}\r\nDownload the zip file?", "New version available", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
				if (result != DialogResult.Yes)
					return;

				using var fileSelector = new SaveFileDialog { FileName = zip.Name, Filter = "Zip Files (*.zip)|*.zip|All files (*.*)|*.*" };
				if (fileSelector.ShowDialog() != DialogResult.OK)
					return;
				selectedPath = fileSelector.FileName;
			}
			catch (AggregateException aggEx)
			{
				Log.Logger.Error(aggEx, "Checking for new version too often");
				return;
			}
			catch (Exception ex)
			{
				MessageBoxAlertAdmin.Show("Error checking for update", "Error checking for update", ex);
				return;
			}

			try
			{
				LibationWinForms.BookLiberation.ProcessorAutomationController.DownloadFile(zipUrl, selectedPath, true);
			}
			catch (Exception ex)
			{
				MessageBoxAlertAdmin.Show("Error downloading update", "Error downloading update", ex);
			}
		}

		private static Octokit.Release getLatestRelease(TimeSpan timeout)
		{
			var task = System.Threading.Tasks.Task.Run(() => getLatestRelease());
			if (task.Wait(timeout))
				return task.Result;

			Log.Logger.Information("Timed out");
			return null;
		}
		private static Octokit.Release getLatestRelease()
		{
			var gitHubClient = new Octokit.GitHubClient(new Octokit.ProductHeaderValue("Libation"));

			// https://octokitnet.readthedocs.io/en/latest/releases/
			var releases = gitHubClient.Repository.Release.GetAll("rmcrackan", "Libation").GetAwaiter().GetResult();
			var latest = releases.First(r => !r.Draft && !r.Prerelease);
			return latest;
		}

		private static Version BuildVersion => System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
	}
}
