using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using AudibleApi;
using AudibleApi.Authorization;
using Dinah.Core.Logging;
using FileManager;
using InternalUtilities;
using LibationWinForms;
using LibationWinForms.Dialogs;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;

namespace LibationLauncher
{
	static class Program
	{
		[STAThread]
		static void Main()
		{
			Application.SetHighDpiMode(HighDpiMode.SystemAware);
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

			// must occur before access to Configuration instance
			migrate_to_v5_2_0();

			createSettings();

			AudibleApiStorage.EnsureAccountsSettingsFileExists();

			migrate_to_v4_0_0();
			migrate_to_v5_0_0();
			
			ensureSerilogConfig();
			configureLogging();
			checkForUpdate();
			logStartupState();

			Application.Run(new Form1());
		}

		private static void createSettings()
		{
			static bool configSetupIsComplete(Configuration config)
				=> config.FilesExist
				&& !string.IsNullOrWhiteSpace(config.InProgress);

			var config = Configuration.Instance;
			if (configSetupIsComplete(config))
				return;

			var isAdvanced = false;

			var setupDialog = new SetupDialog();
			setupDialog.NoQuestionsBtn_Click += (_, __) =>
			{
				config.InProgress ??= Configuration.WinTemp;
				config.Books ??= Configuration.AppDir_Relative;
				config.AllowLibationFixup = true;
			};
			// setupDialog.BasicBtn_Click += (_, __) => // no action needed
			setupDialog.AdvancedBtn_Click += (_, __) => isAdvanced = true;
			setupDialog.ShowDialog();

			if (isAdvanced)
			{
				var dialog = new LibationFilesDialog();
				if (dialog.ShowDialog() != DialogResult.OK)
					MessageBox.Show("Libation Files location not changed");
			}

			if (configSetupIsComplete(config))
				return;

			if (new SettingsDialog().ShowDialog() == DialogResult.OK)
				return;

			MessageBox.Show("Initial set up cancelled.", "Cancelled", MessageBoxButtons.OK, MessageBoxIcon.Warning);
			Application.Exit();
			Environment.Exit(0);
		}

		#region v3 => v4 migration
		static string AccountsSettingsFileLegacy30 => Path.Combine(Configuration.Instance.LibationFiles, "IdentityTokens.json");

		private static void migrate_to_v4_0_0()
		{
			migrateLegacyIdentityFile();

			updateSettingsFile();
		}

		private static void migrateLegacyIdentityFile()
		{
			if (File.Exists(AccountsSettingsFileLegacy30))
			{
				// don't always rely on applicable POCOs. some is legacy and must be: json file => JObject
				try
				{
					updateLegacyFileWithLocale();

					var account = createAccountFromLegacySettings();
					account.DecryptKey = getDecryptKey(account);

					// the next few methods need persistence. to be a good citizen, dispose of persister at the end of current scope
					using var persister = AudibleApiStorage.GetAccountsSettingsPersister();
					persister.AccountsSettings.Add(account);
				}
				// migration is a convenience. if something goes wrong: just move on
				catch { }

				// delete legacy token file
				File.Delete(AccountsSettingsFileLegacy30);
			}
		}

		private static void updateLegacyFileWithLocale()
		{
			var legacyContents = File.ReadAllText(AccountsSettingsFileLegacy30);
			var legacyJObj = JObject.Parse(legacyContents);

			// attempt to update legacy token file with locale from settings
			if (!legacyJObj.ContainsKey("LocaleName"))
			{
				var settings = File.ReadAllText(Configuration.Instance.SettingsFilePath);
				var settingsJObj = JObject.Parse(settings);
				if (settingsJObj.TryGetValue("LocaleCountryCode", out var localeName))
				{
					// update legacy token file with locale from settings
					legacyJObj.AddFirst(new JProperty("LocaleName", localeName.Value<string>()));

					// save
					var newContents = legacyJObj.ToString(Formatting.Indented);
					File.WriteAllText(AccountsSettingsFileLegacy30, newContents);
				}
			}
		}

		private static Account createAccountFromLegacySettings()
		{
			// get required locale from settings file
			var settingsContents = File.ReadAllText(Configuration.Instance.SettingsFilePath);
			if (!JObject.Parse(settingsContents).TryGetValue("LocaleCountryCode", out var jLocale))
				return null;

			var localeName = jLocale.Value<string>();
			var locale = Localization.Get(localeName);

			var api = EzApiCreator.GetApiAsync(locale, AccountsSettingsFileLegacy30).GetAwaiter().GetResult();
			var email = api.GetEmailAsync().GetAwaiter().GetResult();

			// identity has likely been updated above. re-get contents
			var legacyContents = File.ReadAllText(AccountsSettingsFileLegacy30);

			var identity = Identity.FromJson(legacyContents);

			if (!identity.IsValid)
				return null;

			var account = new Account(email)
			{
				AccountName = $"{email} - {locale.Name}",
				LibraryScan = true,
				IdentityTokens = identity
			};

			return account;
		}

		private static string getDecryptKey(Account account)
		{
			if (!string.IsNullOrWhiteSpace(account?.DecryptKey))
				return account.DecryptKey;

			if (!File.Exists(Configuration.Instance.SettingsFilePath) || account is null)
				return "";

			var settingsContents = File.ReadAllText(Configuration.Instance.SettingsFilePath);
			if (JObject.Parse(settingsContents).TryGetValue("DecryptKey", out var jToken))
				return jToken.Value<string>() ?? "";

			return "";
		}

		private static void updateSettingsFile()
		{
			if (!File.Exists(Configuration.Instance.SettingsFilePath))
				return;

			// use JObject to remove decrypt key and locale from Settings.json
			var settingsContents = File.ReadAllText(Configuration.Instance.SettingsFilePath);
			var jObj = JObject.Parse(settingsContents);

			var jLocale = jObj.Property("LocaleCountryCode");
			var jDecryptKey = jObj.Property("DecryptKey");

			jDecryptKey?.Remove();
			jLocale?.Remove();

			if (jDecryptKey != null || jLocale != null)
			{
				var newContents = jObj.ToString(Formatting.Indented);
				File.WriteAllText(Configuration.Instance.SettingsFilePath, newContents);
			}
		}
		#endregion

		#region migrate_to_v5_0_0 re-gegister device if device info not in settings
		private static void migrate_to_v5_0_0()
		{
			var persistentDictionary = new PersistentDictionary(Configuration.Instance.SettingsFilePath);

			var config = Configuration.Instance;
			if (persistentDictionary.GetString(nameof(config.AllowLibationFixup)) is null)
            {
				persistentDictionary.Set(nameof(config.AllowLibationFixup), true);
			}

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

		#region migrate to v5.2.0 : get rid of meta-directories, combine DownloadsInProgressEnum and DecryptInProgressEnum => InProgress
		private static void migrate_to_v5_2_0()
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
		#endregion

		private static void ensureSerilogConfig()
		{
			var config = Configuration.Instance;

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
									{ "path", Path.Combine(Configuration.Instance.LibationFiles, "_Log.log") },
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

		private static void configureLogging()
		{
			var config = Configuration.Instance;

			// override path. always use current libation files
			var logPath = Path.Combine(Configuration.Instance.LibationFiles, "Log.log");
			config.SetWithJsonPath("Serilog.WriteTo[1].Args", "path", logPath);

			//// hack which achieves the same
			//configuration["Serilog:WriteTo:1:Args:path"] = logPath;

			// CONFIGURATION-DRIVEN (json)
			var configuration = new ConfigurationBuilder()
				.AddJsonFile(config.SettingsFilePath)
				.Build();
			Log.Logger = new LoggerConfiguration()
				.ReadFrom.Configuration(configuration)
				.CreateLogger();

			//// MANUAL HARD CODED
			//Log.Logger = new LoggerConfiguration()
			//  // requires: using Dinah.Core.Logging;
			//	.Enrich.WithCaller()
			//	.MinimumLevel.Information()
			//	.WriteTo.File(logPath,
			//		rollingInterval: RollingInterval.Month,
			//		outputTemplate: code_outputTemplate)
			//	.CreateLogger();

			// .Here() captures debug info via System.Runtime.CompilerServices attributes. Warning: expensive
			//var withLineNumbers_outputTemplate = "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message}{NewLine}in method {MemberName} at {FilePath}:{LineNumber}{NewLine}{Exception}{NewLine}";
			//Log.Logger.Here().Debug("Begin Libation. Debug with line numbers");
		}

		private static void checkForUpdate()
		{
			try
			{
				var gitHubClient = new Octokit.GitHubClient(new Octokit.ProductHeaderValue("Libation"));

				// https://octokitnet.readthedocs.io/en/latest/releases/
				var releases =  gitHubClient.Repository.Release.GetAll("rmcrackan", "Libation").GetAwaiter().GetResult();
				var latest = releases.First(r => !r.Draft && !r.Prerelease);

				var latestVersionString = latest.TagName.Trim('v');
				if (!Version.TryParse(latestVersionString, out var latestRelease))
					return;

				// we're up to date
				if (latestRelease <= BuildVersion)
					return;

				// we have an update
				var zip = latest.Assets.FirstOrDefault(a => a.BrowserDownloadUrl.EndsWith(".zip"));
				var zipUrl = zip?.BrowserDownloadUrl;
				if (zipUrl is null)
				{
					MessageBox.Show(latest.HtmlUrl, "New version available");
					return;
				}

				var result = MessageBox.Show($"New version available @ {latest.HtmlUrl}\r\nDownload the zip file?", "New version available", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
				if (result != DialogResult.Yes)
					return;

				using var fileSelector = new SaveFileDialog { FileName = zip.Name, Filter = "Zip Files (*.zip)|*.zip|All files (*.*)|*.*" };
				if (fileSelector.ShowDialog() != DialogResult.OK)
					return;
				var selectedPath = fileSelector.FileName;

				try
				{
					LibationWinForms.BookLiberation.ProcessorAutomationController.DownloadFile(zipUrl, selectedPath, true);
				}
				catch (Exception ex)
				{
					Error(ex, "Error downloading update");
				}
			}
			catch (Exception ex)
			{
				Error(ex, "Error checking for update");
			}
		}

		private static void Error(Exception ex, string message)
		{
			Log.Logger.Error(ex, message);
			MessageBox.Show($"{message}\r\nSee log for details");
		}

		private static void logStartupState()
		{
			var config = Configuration.Instance;

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

			// when turning on debug (and especially Verbose) to share logs, some privacy settings may not be obscured
			if (Log.Logger.IsVerboseEnabled())
				MessageBox.Show(@"
Warning: verbose logging is enabled.

This should be used for debugging only. It creates many
more logs and debug files, neither of which are as
strictly anonomous.

When you are finished debugging, it's highly recommended
to set your debug MinimumLevel to Information and restart
Libation.
".Trim(), "Verbose logging enabled", MessageBoxButtons.OK, MessageBoxIcon.Warning);
		}

		private static Version BuildVersion => System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
	}
}
