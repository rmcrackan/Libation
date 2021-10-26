using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using AudibleUtilities;
using Dinah.Core;
using Dinah.Core.IO;
using Dinah.Core.Logging;
using LibationFileManager;
using Newtonsoft.Json.Linq;
using Serilog;

namespace AppScaffolding
{
	public static class LibationScaffolding
	{
		// AppScaffolding
		private static Assembly _executingAssembly;
		private static Assembly ExecutingAssembly
			=> _executingAssembly ??= Assembly.GetExecutingAssembly();

		// LibationWinForms or LibationCli
		private static Assembly _entryAssembly;
		private static Assembly EntryAssembly
			=> _entryAssembly ??= Assembly.GetEntryAssembly();

		// previously: System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
		private static Version _buildVersion;
		public static Version BuildVersion
			=> _buildVersion
			??= new[] { ExecutingAssembly.GetName(), EntryAssembly.GetName() }
			.Max(a => a.Version);

		/// <summary>Run migrations before loading Configuration for the first time. Then load and return Configuration</summary>
		public static Configuration RunPreConfigMigrations()
		{
			// must occur before access to Configuration instance
			Migrations.migrate_to_v5_2_0__pre_config();

			//***********************************************//
			//                                               //
			//   do not use Configuration before this line   //
			//                                               //
			//***********************************************//
			return Configuration.Instance;
		}

		/// <summary>most migrations go in here</summary>
		public static void RunPostConfigMigrations(Configuration config)
		{
			AudibleApiStorage.EnsureAccountsSettingsFileExists();

			//
			// migrations go below here
			//

			Migrations.migrate_to_v5_2_0__post_config(config);
			Migrations.migrate_to_v5_7_1(config);
			Migrations.migrate_to_v6_1_2(config);
			Migrations.migrate_to_v6_2_0(config);
			Migrations.migrate_to_v6_2_9(config);
		}

		/// <summary>Initialize logging. Run after migration</summary>
		public static void RunPostMigrationScaffolding(Configuration config)
		{
			ensureSerilogConfig(config);
			configureLogging(config);
			logStartupState(config);
		}

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

			// capture most Console.WriteLine() and write to serilog. See below tests for details.
			// Some dependencies print helpful info via Console.WriteLine. We'd like to log it.
			//
			// Serilog also writes to Console so this might be asking for trouble. ie: infinite loops.
			// SerilogTextWriter needs to be more robust and tested. Esp the Write() methods.
			// Empirical testing so far has shown no issues.
			Console.SetOut(new MultiTextWriter(origOut, new SerilogTextWriter()));

			#region Console => Serilog tests
			/*
			// all below apply to "Console." and "Console.Out."

			// captured
			Console.WriteLine("str");
			Console.WriteLine(new { a = "anon" });
			Console.WriteLine("{0}", "format");
			Console.WriteLine("{0}{1}", "zero|", "one");
			Console.WriteLine("{0}{1}{2}", "zero|", "one|", "two");
			Console.WriteLine("{0}", new object[] { "arr" });

			// not captured
			Console.WriteLine();
			Console.WriteLine(true);
			Console.WriteLine('0');
			Console.WriteLine(1);
			Console.WriteLine(2m);
			Console.WriteLine(3f);
			Console.WriteLine(4d);
			Console.WriteLine(5L);
			Console.WriteLine((uint)6);
			Console.WriteLine((ulong)7);

			Console.Write("str");
			Console.Write(true);
			Console.Write('0');
			Console.Write(1);
			Console.Write(2m);
			Console.Write(3f);
			Console.Write(4d);
			Console.Write(5L);
			Console.Write((uint)6);
			Console.Write((ulong)7);
			Console.Write(new { a = "anon" });
			Console.Write("{0}", "format");
			Console.Write("{0}{1}", "zero|", "one");
			Console.Write("{0}{1}{2}", "zero|", "one|", "two");
			Console.Write("{0}", new object[] { "arr" });
			*/
			#endregion

			// .Here() captures debug info via System.Runtime.CompilerServices attributes. Warning: expensive
			//var withLineNumbers_outputTemplate = "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message}{NewLine}in method {MemberName} at {FilePath}:{LineNumber}{NewLine}{Exception}{NewLine}";
			//Log.Logger.Here().Debug("Begin Libation. Debug with line numbers");
		}

		private static void logStartupState(Configuration config)
		{
			// begin logging session with a form feed
			Log.Logger.Information("\r\n\f");
			Log.Logger.Information("Begin. {@DebugInfo}", new
			{
				AppName = EntryAssembly.GetName().Name,
				Version = BuildVersion.ToString(),
#if DEBUG
				Mode = "Debug",
#else
				Mode = "Release",
#endif

				LogLevel_Verbose_Enabled = Log.Logger.IsVerboseEnabled(),
				LogLevel_Debug_Enabled = Log.Logger.IsDebugEnabled(),
				LogLevel_Information_Enabled = Log.Logger.IsInformationEnabled(),
				LogLevel_Warning_Enabled = Log.Logger.IsWarningEnabled(),
				LogLevel_Error_Enabled = Log.Logger.IsErrorEnabled(),
				LogLevel_Fatal_Enabled = Log.Logger.IsFatalEnabled(),

				config.LibationFiles,
				AudibleFileStorage.BooksDirectory,

				config.InProgress,

				AudibleFileStorage.DownloadsInProgressDirectory,
				DownloadsInProgressFiles = Directory.EnumerateFiles(AudibleFileStorage.DownloadsInProgressDirectory).Count(),

				AudibleFileStorage.DecryptInProgressDirectory,
				DecryptInProgressFiles = Directory.EnumerateFiles(AudibleFileStorage.DecryptInProgressDirectory).Count(),
			});
		}

		public static (bool hasUpgrade, string zipUrl, string htmlUrl, string zipName) GetLatestRelease()
		{
			(bool, string, string, string) isFalse = (false, null, null, null);

			// timed out
			var latest = getLatestRelease(TimeSpan.FromSeconds(10));
			if (latest is null)
				return isFalse;

			var latestVersionString = latest.TagName.Trim('v');
			if (!Version.TryParse(latestVersionString, out var latestRelease))
				return isFalse;

			// we're up to date
			if (latestRelease <= BuildVersion)
				return isFalse;

			// we have an update
			var zip = latest.Assets.FirstOrDefault(a => a.BrowserDownloadUrl.EndsWith(".zip"));
			var zipUrl = zip?.BrowserDownloadUrl;

			Log.Logger.Information("Update available: {@DebugInfo}", new
			{
				latestRelease = latestRelease.ToString(),
				latest.HtmlUrl,
				zipUrl
			});

			return (true, zipUrl, latest.HtmlUrl, zip.Name);
		}
		private static Octokit.Release getLatestRelease(TimeSpan timeout)
		{
			try
			{
				var task = System.Threading.Tasks.Task.Run(() => getLatestRelease());
				if (task.Wait(timeout))
					return task.Result;

				Log.Logger.Information("Timed out");
			}
			catch (AggregateException aggEx)
			{
				Log.Logger.Error(aggEx, "Checking for new version too often");
			}
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
	}

	internal static class Migrations
	{
		#region migrate to v5.2.0
		// get rid of meta-directories, combine DownloadsInProgressEnum and DecryptInProgressEnum => InProgress
		public static void migrate_to_v5_2_0__pre_config()
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

		public static void migrate_to_v5_2_0__post_config(Configuration config)
		{
			if (!config.Exists(nameof(config.AllowLibationFixup)))
				config.AllowLibationFixup = true;

			if (!config.Exists(nameof(config.DecryptToLossy)))
				config.DecryptToLossy = false;
		}
		#endregion

		// add config.BadBook
		public static void migrate_to_v5_7_1(Configuration config)
		{
			if (!config.Exists(nameof(config.BadBook)))
				config.BadBook = Configuration.BadBookAction.Ask;
		}

		// add config.DownloadEpisodes , config.ImportEpisodes
		public static void migrate_to_v6_1_2(Configuration config)
		{
			if (!config.Exists(nameof(config.DownloadEpisodes)))
				config.DownloadEpisodes = true;

			if (!config.Exists(nameof(config.ImportEpisodes)))
				config.ImportEpisodes = true;
		}

		// add config.SplitFilesByChapter
		public static void migrate_to_v6_2_0(Configuration config)
		{
			if (!config.Exists(nameof(config.SplitFilesByChapter)))
				config.SplitFilesByChapter = false;
		}

		// add file naming templates
		public static void migrate_to_v6_2_9(Configuration config)
		{
			if (!config.Exists(nameof(config.FolderTemplate)))
				config.FolderTemplate = Templates.Folder.DefaultTemplate;

			if (!config.Exists(nameof(config.FileTemplate)))
				config.FileTemplate = Templates.File.DefaultTemplate;

			if (!config.Exists(nameof(config.ChapterFileTemplate)))
				config.ChapterFileTemplate = Templates.ChapterFile.DefaultTemplate;
		}
	}
}
