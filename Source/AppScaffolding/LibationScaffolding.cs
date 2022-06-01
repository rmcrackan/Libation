using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using ApplicationServices;
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
			// // outdated. kept here as an example of what belongs in this area
			// // Migrations.migrate_to_v5_2_0__pre_config();

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
			PopulateMissingConfigValues(config);

			//
			// migrations go below here
			//

			Migrations.migrate_to_v6_6_9(config);
		}

		public static void PopulateMissingConfigValues(Configuration config)
		{
			config.InProgress ??= Configuration.WinTemp;

			if (!config.Exists(nameof(config.AllowLibationFixup)))
				config.AllowLibationFixup = true;

			if (!config.Exists(nameof(config.CreateCueSheet)))
				config.CreateCueSheet = true;

			if (!config.Exists(nameof(config.RetainAaxFile)))
				config.RetainAaxFile = false;

			if (!config.Exists(nameof(config.SplitFilesByChapter)))
				config.SplitFilesByChapter = false;

			if (!config.Exists(nameof(config.StripUnabridged)))
				config.StripUnabridged = false;

			if (!config.Exists(nameof(config.StripAudibleBrandAudio)))
				config.StripAudibleBrandAudio = false;

			if (!config.Exists(nameof(config.DecryptToLossy)))
				config.DecryptToLossy = false;

			if (!config.Exists(nameof(config.LameTargetBitrate)))
				config.LameTargetBitrate = false;			
			
			if (!config.Exists(nameof(config.LameDownsampleMono)))
				config.LameDownsampleMono = true;
			
			if (!config.Exists(nameof(config.LameBitrate)))
				config.LameBitrate = 64;
			
			if (!config.Exists(nameof(config.LameConstantBitrate)))
				config.LameConstantBitrate = false;
			
			if (!config.Exists(nameof(config.LameMatchSourceBR)))
				config.LameMatchSourceBR = true;
			
			if (!config.Exists(nameof(config.LameVBRQuality)))
				config.LameVBRQuality = 2;

			if (!config.Exists(nameof(config.BadBook)))
				config.BadBook = Configuration.BadBookAction.Ask;

			if (!config.Exists(nameof(config.ShowImportedStats)))
				config.ShowImportedStats = true;

			if (!config.Exists(nameof(config.ImportEpisodes)))
				config.ImportEpisodes = true;

			if (!config.Exists(nameof(config.DownloadEpisodes)))
				config.DownloadEpisodes = true;

			if (!config.Exists(nameof(config.FolderTemplate)))
				config.FolderTemplate = Templates.Folder.DefaultTemplate;

			if (!config.Exists(nameof(config.FileTemplate)))
				config.FileTemplate = Templates.File.DefaultTemplate;

			if (!config.Exists(nameof(config.ChapterFileTemplate)))
				config.ChapterFileTemplate = Templates.ChapterFile.DefaultTemplate;

			if (!config.Exists(nameof(config.AutoScan)))
				config.AutoScan = true;

			if (!config.Exists(nameof(config.GridColumnsVisibilities)))
				config.GridColumnsVisibilities = new Dictionary<string, bool>();

			if (!config.Exists(nameof(config.GridColumnsDisplayIndices)))
				config.GridColumnsDisplayIndices = new Dictionary<string, int>();

			if (!config.Exists(nameof(config.GridColumnsWidths)))
				config.GridColumnsWidths = new Dictionary<string, int>();

			if (!config.Exists(nameof(config.DownloadCoverArt)))
				config.DownloadCoverArt = true;

			if (!config.Exists(nameof(config.AutoDownloadEpisodes)))
				config.AutoDownloadEpisodes = false;
		}

		/// <summary>Initialize logging. Wire-up events. Run after migration</summary>
		public static void RunPostMigrationScaffolding(Configuration config)
		{
			ensureSerilogConfig(config);
			configureLogging(config);
			logStartupState(config);

			wireUpSystemEvents(config);
		}

		private static void ensureSerilogConfig(Configuration config)
		{
			if (config.GetObject("Serilog") is not null)
				return;

			var serilogObj = new JObject
			{
				{ "MinimumLevel", "Information" },
				{ "WriteTo", new JArray
					{
						// new JObject { {"Name", "Console" } }, // this has caused more problems than it's solved
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
									// {Properties:j} needed for expanded exception logging
									{ "outputTemplate", "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] (at {Caller}) {Message:lj}{NewLine}{Exception} {Properties:j}" }
								}
							}
						}
					}
				},
				// better exception logging with: Serilog.Exceptions library -- WithExceptionDetails
				{ "Using", new JArray{ "Dinah.Core", "Serilog.Exceptions" } }, // dll's name, NOT namespace
				{ "Enrich", new JArray{ "WithCaller", "WithExceptionDetails" } },
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
			// If Serilog also writes to Console, this might be asking for trouble. ie: infinite loops.
			// To use that way, SerilogTextWriter needs to be more robust and tested. Esp the Write() methods.
			// However, empirical testing so far has shown no issues.
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
#if DEBUG
			var mode = "Debug";
#else
			var mode = "Release";
#endif
			if (System.Diagnostics.Debugger.IsAttached)
				mode += " (Debugger attached)";

			// begin logging session with a form feed
			Log.Logger.Information("\r\n\f");
			Log.Logger.Information("Begin. {@DebugInfo}", new
			{
				AppName = EntryAssembly.GetName().Name,
				Version = BuildVersion.ToString(),
				Mode = mode,
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
				DownloadsInProgressFiles = FileManager.FileUtility.SaferEnumerateFiles(AudibleFileStorage.DownloadsInProgressDirectory).Count(),

				AudibleFileStorage.DecryptInProgressDirectory,
				DecryptInProgressFiles = FileManager.FileUtility.SaferEnumerateFiles(AudibleFileStorage.DecryptInProgressDirectory).Count(),
			});
		}

		private static void wireUpSystemEvents(Configuration configuration)
		{
			LibraryCommands.LibrarySizeChanged += (_, __) => SearchEngineCommands.FullReIndex();
			LibraryCommands.BookUserDefinedItemCommitted += (_, books) => SearchEngineCommands.UpdateBooks(books);
		}

		public static UpgradeProperties GetLatestRelease()
		{
			// timed out
			var latest = getLatestRelease(TimeSpan.FromSeconds(10));
			if (latest is null)
				return null;

			var latestVersionString = latest.TagName.Trim('v');
			if (!Version.TryParse(latestVersionString, out var latestRelease))
				return null;

			// we're up to date
			if (latestRelease <= BuildVersion)
				return null;

			// we have an update
			var zip = latest.Assets.FirstOrDefault(a => a.BrowserDownloadUrl.EndsWith(".zip"));
			var zipUrl = zip?.BrowserDownloadUrl;

			Log.Logger.Information("Update available: {@DebugInfo}", new
			{
				latestRelease = latestRelease.ToString(),
				latest.HtmlUrl,
				zipUrl
			});

			return new(zipUrl, latest.HtmlUrl, zip.Name, latestRelease);
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
		public static void migrate_to_v6_6_9(Configuration config)
		{
			var writeToPath = $"Serilog.WriteTo";

			// remove WriteTo[].Name == Console
			{
				if (UNSAFE_MigrationHelper.Settings_TryGetArrayLength(writeToPath, out var length1))
				{
					for (var i = length1 - 1; i >= 0; i--)
					{
						var exists = UNSAFE_MigrationHelper.Settings_TryGetFromJsonPath($"{writeToPath}[{i}].Name", out var value);

						if (exists && value == "Console")
							UNSAFE_MigrationHelper.Settings_RemoveFromArray(writeToPath, i);
					}
				}
			}

			// add Serilog.Exceptions -- WithExceptionDetails
			{
				// outputTemplate should contain "{Properties:j}"
				{
					// re-calculate. previous loop may have changed the length
					if (UNSAFE_MigrationHelper.Settings_TryGetArrayLength(writeToPath, out var length2))
					{
						var propertyName = "outputTemplate";
						for (var i = 0; i < length2; i++)
						{
							var jsonPath = $"{writeToPath}[{i}].Args";
							var exists = UNSAFE_MigrationHelper.Settings_TryGetFromJsonPath($"{jsonPath}.{propertyName}", out var value);

							var newValue = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] (at {Caller}) {Message:lj}{NewLine}{Exception} {Properties:j}";

							if (exists && value != newValue)
								UNSAFE_MigrationHelper.Settings_SetWithJsonPath(jsonPath, propertyName, newValue);
						}
					}
				}

				// Serilog.Using must include "Serilog.Exceptions"
				UNSAFE_MigrationHelper.Settings_AddUniqueToArray("Serilog.Using", "Serilog.Exceptions");

				// Serilog.Enrich must include "WithExceptionDetails"
				UNSAFE_MigrationHelper.Settings_AddUniqueToArray("Serilog.Enrich", "WithExceptionDetails");
			}
		}
	}
}
