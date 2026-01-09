using ApplicationServices;
using AudibleUtilities;
using Dinah.Core.IO;
using Dinah.Core.Logging;
using LibationFileManager;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

namespace AppScaffolding
{
    public enum ReleaseIdentifier
	{
		None,
		WindowsClassic = OS.Windows | Variety.Classic | Architecture.X64,
		WindowsAvalonia = OS.Windows | Variety.Chardonnay | Architecture.X64,
		LinuxAvalonia = OS.Linux | Variety.Chardonnay | Architecture.X64,
		MacOSAvalonia = OS.MacOS | Variety.Chardonnay | Architecture.X64,
		LinuxAvalonia_Arm64 = OS.Linux | Variety.Chardonnay | Architecture.Arm64,
		MacOSAvalonia_Arm64 = OS.MacOS | Variety.Chardonnay | Architecture.Arm64,
		WindowsAvalonia_Arm64 = OS.Windows | Variety.Chardonnay | Architecture.Arm64,
	}

	// I know I'm taking the wine metaphor a bit far by naming this "Variety", but I don't know what else to call it
	[Flags]
	public enum Variety
	{
		None,
		Classic = 0x10000,
		Chardonnay = 0x20000,
	}

	public static class LibationScaffolding
	{
		public const string RepositoryUrl = "ht" + "tps://github.com/rmcrackan/Libation";
		public const string WebsiteUrl = "ht" + "tps://getlibation.com";
		public const string RepositoryLatestUrl = "ht" + "tps://github.com/rmcrackan/Libation/releases/latest";
		public static ReleaseIdentifier ReleaseIdentifier { get; private set; }
		public static Variety Variety { get; private set; }

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

			Configuration.SetLibationVersion(BuildVersion);

			//***********************************************//
			//                                               //
			//   do not use Configuration before this line   //
			//                                               //
			//***********************************************//
			return Configuration.Instance;
		}

		/// <summary>most migrations go in here</summary>
		public static void RunPostConfigMigrations(Configuration config, bool ephemeralSettings = false)
		{
			if (ephemeralSettings)
			{
				var settings = JObject.Parse(File.ReadAllText(config.LibationFiles.SettingsFilePath));
				config.LoadEphemeralSettings(settings);
			}
			else
			{
				config.LoadPersistentSettings(config.LibationFiles.SettingsFilePath);
			}
			DeleteOpenSqliteFiles(config);
			AudibleApiStorage.EnsureAccountsSettingsFileExists();

			//
			// migrations go below here
			//

			Migrations.migrate_to_v6_6_9(config);
            Migrations.migrate_to_v11_5_0(config);
			Migrations.migrate_to_v11_6_5(config);
			Migrations.migrate_to_v12_0_1(config);
		}

		/// <summary>
		/// Delete shared memory and write-ahead log SQLite database files which may prevent access to the database.
		/// These file may or may not cause libation to hang on CreateContext,
		/// so try our luck by swallowing any exceptions and continuing.
		/// </summary>
		private static void DeleteOpenSqliteFiles(Configuration config)
		{
			var walFile = SqliteStorage.DatabasePath + "-wal";
			var shmFile = SqliteStorage.DatabasePath + "-shm";
			if (File.Exists(walFile))
			{
				try
				{
					FileManager.FileUtility.SaferDelete(walFile);
				}
				catch(Exception ex)
				{
					Log.Logger.Warning(ex, "Could not delete SQLite WAL file: {WalFile}", walFile);
				}
			}
			if (File.Exists(shmFile))
			{
				try
				{
					FileManager.FileUtility.SaferDelete(shmFile);
				}
				catch (Exception ex)
				{
					Log.Logger.Warning(ex, "Could not delete SQLite SHM file: {ShmFile}", shmFile);
				}
			}
		}
		static bool migrationsRun = false;

		/// <summary>Initialize logging. Wire-up events. Run after migration</summary>
		public static void RunPostMigrationScaffolding(Variety variety, Configuration config)
		{
			if (System.Threading.Interlocked.CompareExchange(ref migrationsRun, true, false))
				return;
			Variety = Enum.IsDefined(variety) ? variety : Variety.None;

			var releaseID = (ReleaseIdentifier)((int)variety | (int)Configuration.OS | (int)RuntimeInformation.ProcessArchitecture);

			ReleaseIdentifier = Enum.IsDefined(releaseID) ? releaseID : ReleaseIdentifier.None;

			ensureSerilogConfig(config);
			configureLogging(config);
			logStartupState(config);

			// all else should occur after logging

			wireUpSystemEvents(config);
		}

		private static void ensureSerilogConfig(Configuration config)
		{
			if (config.GetObject("Serilog") is JObject serilog)
			{
				bool fileChanged = false;
				if (serilog.SelectToken("$.WriteTo[?(@.Name == 'ZipFile')]", false) is JObject zipFileSink)
				{
					zipFileSink["Name"] = "File";
					fileChanged = true;
				}
				var hooks = typeof(FileSinkHook).AssemblyQualifiedName;
				if (serilog.SelectToken("$.WriteTo[?(@.Name == 'File')].Args", false) is JObject fileSinkArgs
					&& fileSinkArgs["hooks"]?.Value<string>() != hooks)
				{
					fileSinkArgs["hooks"] = hooks;
					fileChanged = true;
				}

				if (fileChanged)
					config.SetNonString(serilog.DeepClone(), "Serilog");
				return;
			}

			var serilogObj = new JObject
			{
				{ "MinimumLevel", "Information" },
				{ "WriteTo", new JArray
					{
						// ABOUT SINKS
						// Only File sink is currently used. By user request (June 2024) others packages are included for experimental use.

						// new JObject { {"Name", "Console" } }, // this has caused more problems than it's solved
						new JObject
						{
							{ "Name", "File" },
							{ "Args",
								new JObject
								{
									// for this sink to work, a path must be provided. we override this below
									{ "path", Path.Combine(config.LibationFiles.Location, "Log.log") },
									{ "rollingInterval", "Month" },
									// Serilog template formatting examples
									// - default:                    "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
									//   output example:             2019-11-26 08:48:40.224 -05:00 [DBG] Begin Libation
									// - with class and method info: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] (at {Caller}) {Message:lj}{NewLine}{Exception}";
									//   output example:             2019-11-26 08:48:40.224 -05:00 [DBG] (at LibationWinForms.Program.init()) Begin Libation
									// {Properties:j} needed for expanded exception logging
									{ "outputTemplate", "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] (at {Caller}) {Message:lj}{NewLine}{Exception} {Properties:j}" },
									{ "hooks", typeof(FileSinkHook).AssemblyQualifiedName }, // for FileSinkHook
								}
							}
						}
					}
				},
				// better exception logging with: Serilog.Exceptions library -- WithExceptionDetails
				{ "Using", new JArray{ "Dinah.Core", "Serilog.Exceptions" } }, // dll's name, NOT namespace
				{ "Enrich", new JArray{ "WithCaller", "WithExceptionDetails" } },
			};
			config.SetNonString(serilogObj, "Serilog");
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

#nullable enable
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

			static int fileCount(FileManager.LongPath? longPath)
            {
				if (longPath is null)
					return -1;
				try { return FileManager.FileUtility.SaferEnumerateFiles(longPath).Count(); }
                catch { return -1; }
            }

            Log.Logger.Information("Begin. {@DebugInfo}", new
			{
				AppName = EntryAssembly.GetName().Name,
				Version = BuildVersion.ToString(),
				ReleaseIdentifier,
				Configuration.OS,
                Environment.OSVersion,
                InteropFactory.InteropFunctionsType,
                Mode = mode,
				LogLevel_Verbose_Enabled = Log.Logger.IsVerboseEnabled(),
				LogLevel_Debug_Enabled = Log.Logger.IsDebugEnabled(),
				LogLevel_Information_Enabled = Log.Logger.IsInformationEnabled(),
				LogLevel_Warning_Enabled = Log.Logger.IsWarningEnabled(),
				LogLevel_Error_Enabled = Log.Logger.IsErrorEnabled(),
				LogLevel_Fatal_Enabled = Log.Logger.IsFatalEnabled(),

                config.AutoScan,
                config.BetaOptIn,
                config.UseCoverAsFolderIcon,
                config.LibationFiles,
				AudibleFileStorage.BooksDirectory,

				config.InProgress,

				AudibleFileStorage.DownloadsInProgressDirectory,
				DownloadsInProgressFiles = fileCount(AudibleFileStorage.DownloadsInProgressDirectory),

				AudibleFileStorage.DecryptInProgressDirectory,
				DecryptInProgressFiles = fileCount(AudibleFileStorage.DecryptInProgressDirectory),

                disableIPv6 = AppContext.TryGetSwitch("System.Net.DisableIPv6", out bool disableIPv6Value),
			});

			if (InteropFactory.InteropFunctionsType is null)
                Serilog.Log.Logger.Warning("WARNING: OSInteropProxy.InteropFunctionsType is null");
        }
#nullable restore
		private static void wireUpSystemEvents(Configuration configuration)
		{
			LibraryCommands.LibrarySizeChanged += (object _, List<DataLayer.LibraryBook> libraryBooks)
				=> SearchEngineCommands.FullReIndex(libraryBooks);

			LibraryCommands.BookUserDefinedItemCommitted += (_, books)
				=> SearchEngineCommands.UpdateBooks(books);
		}

		public static UpgradeProperties GetLatestRelease()
		{
			// timed out
			(var version, var latest, var zip) = getLatestRelease(TimeSpan.FromSeconds(10));

			if (version is null || latest is null || zip is null)
				return null;

			// we have an update
			var zipUrl = zip?.BrowserDownloadUrl;

			Log.Logger.Information("Update available: {@DebugInfo}", new
			{
				latestRelease = version.ToString(),
				latest.HtmlUrl,
				zipUrl
			});

			return new(zipUrl, latest.HtmlUrl, zip.Name, version, latest.Body);
		}
		private static (Version releaseVersion, Octokit.Release, Octokit.ReleaseAsset) getLatestRelease(TimeSpan timeout)
		{
			try
			{
				var task = getLatestRelease();
				if (task.Wait(timeout))
					return task.Result;

				Log.Logger.Information("Timed out");
			}
			catch (AggregateException aggEx)
			{
				Log.Logger.Error(aggEx, "Checking for new version too often");
			}
			return (null, null, null);
		}
		private static async System.Threading.Tasks.Task<(Version releaseVersion, Octokit.Release, Octokit.ReleaseAsset)> getLatestRelease()
		{
			const string ownerAccount = "rmcrackan";
			const string repoName = "Libation";

			var gitHubClient = new Octokit.GitHubClient(new Octokit.ProductHeaderValue(repoName));

			//https://docs.github.com/en/rest/releases/releases?apiVersion=2022-11-28#get-the-latest-release
			var latestRelease = await gitHubClient.Repository.Release.GetLatest(ownerAccount, repoName);

			//Ensure that latest release is greater than the current version
			var latestVersionString = latestRelease.TagName.Trim('v');
			if (!Version.TryParse(latestVersionString, out var releaseVersion) || releaseVersion <= BuildVersion)
				return (null, null, null);

			//Download the release index
			var bts = await gitHubClient.Repository.Content.GetRawContent(ownerAccount, repoName, ".releaseindex.json");
			var releaseIndex = JObject.Parse(System.Text.Encoding.ASCII.GetString(bts));

			string regexPattern;

			try
			{
				regexPattern = releaseIndex.Value<string>(InteropFactory.Create().ReleaseIdString);
			}
			catch
			{
				regexPattern = releaseIndex.Value<string>(ReleaseIdentifier.ToString());
			}

			var regex = new System.Text.RegularExpressions.Regex(regexPattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase);

			return (releaseVersion, latestRelease, latestRelease?.Assets?.FirstOrDefault(a => regex.IsMatch(a.Name)));
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

        class FilterState_6_6_9
        {
            public bool UseDefault { get; set; }
            public List<string> Filters { get; set; } = new();
        }

		public static void migrate_to_v12_0_1(Configuration config)
		{
#nullable enable
			//Migrate from version 1 file cache to the dictionary-based version 2 cache
			const string FILENAME_V1 = "FileLocations.json";
			const string FILENAME_V2 = "FileLocationsV2.json";

			var jsonFileV1 = Path.Combine(Configuration.Instance.LibationFiles.Location, FILENAME_V1);
			var jsonFileV2 = Path.Combine(Configuration.Instance.LibationFiles.Location, FILENAME_V2);

			if (!File.Exists(jsonFileV2) && File.Exists(jsonFileV1))
			{
				try
				{
					//FilePathCache loads the cache in its static constructor,
					//so perform migration without using FilePathCache.CacheEntry
					if (JArray.Parse(File.ReadAllText(jsonFileV1)) is not JArray v1Cache || v1Cache.Count == 0)
						return;

					Dictionary<string, JArray> cache = new();

					//Convert to c# objects to speed up searching by ID inside the iterator 
					var allItems
						= v1Cache
						.Select(i => new
						{
							Id = i["Id"]?.Value<string>(),
							Path = i["Path"]?["Path"]?.Value<string>()
						}).Where(i => i.Id != null)
						.ToArray();

					foreach (var id in allItems.Select(i => i.Id).OfType<string>().Distinct())
					{
						//Use this opportunity to purge non-existent files and re-classify file types
						//(due to *.aax files previously not being classified as FileType.AAXC)
						var items = allItems
							.Where(i => i.Id == id && File.Exists(i.Path))
							.Select(i => new JObject
							{
								{ "Id", i.Id },
								{ "FileType", (int)FileTypes.GetFileTypeFromPath(i.Path) },
								{ "Path", new JObject{ { "Path", i.Path } } }
							})
							.ToArray();

						if (items.Length == 0)
							continue;

						cache[id] = new JArray(items);
					}

					var cacheJson = new JObject { { "Dictionary", JObject.FromObject(cache) } };
					var cacheFileText = cacheJson.ToString(Formatting.Indented);

					void migrate()
					{
						File.WriteAllText(jsonFileV2, cacheFileText);
						File.Delete(jsonFileV1);
					}

					try { migrate(); }
					catch (IOException)
					{
						try { migrate(); }
						catch (IOException)
						{
							migrate();
						}
					}
				}
				catch { /* eat */ }
			}
#nullable restore
		}

		public static void migrate_to_v11_6_5(Configuration config)
		{
			//Settings migration for unsupported sample rates (#1116)
			if (config.MaxSampleRate < AAXClean.SampleRate.Hz_8000)
				config.MaxSampleRate = AAXClean.SampleRate.Hz_8000;
			else if (config.MaxSampleRate > AAXClean.SampleRate.Hz_48000)
				config.MaxSampleRate = AAXClean.SampleRate.Hz_48000;
		}

		public static void migrate_to_v11_5_0(Configuration config)
        {
            // Read file, but convert old format to new (with Name field) as necessary.
            if (!File.Exists(QuickFilters.JsonFile))
            {
                QuickFilters.InMemoryState = new();
                return;
            }

            try
            {
                if (JsonConvert.DeserializeObject<QuickFilters.FilterState>(File.ReadAllText(QuickFilters.JsonFile))
                    is QuickFilters.FilterState inMemState)
                {
                    QuickFilters.InMemoryState = inMemState;
                    return;
                }
            }
            catch
            {
                // Eat
            }

            try
            {
                if (JsonConvert.DeserializeObject<FilterState_6_6_9>(File.ReadAllText(QuickFilters.JsonFile))
                    is FilterState_6_6_9 inMemState)
                {
                    // Copy old structure to new.
                    QuickFilters.InMemoryState = new();
                    QuickFilters.InMemoryState.UseDefault = inMemState.UseDefault;
                    foreach (var oldFilter in inMemState.Filters)
                        QuickFilters.InMemoryState.Filters.Add(new QuickFilters.NamedFilter(oldFilter, null));

                    return;
                }
                Debug.Assert(false, "Should not get here, QuickFilters.json deserialization issue");
            }
            catch
            {
				// Eat
            }
        }
    }
}
