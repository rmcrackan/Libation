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

namespace AppScaffolding;

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
	public const string DonateUrl = WebsiteUrl + "/donate";
	public const string RepositoryLatestUrl = "ht" + "tps://github.com/rmcrackan/Libation/releases/latest";
	/// <summary>Documentation for naming template syntax</summary>
	public const string NamingTemplatesDocUrl = WebsiteUrl + "/docs/features/naming-templates";
	public static ReleaseIdentifier ReleaseIdentifier { get; private set; }
	public static Variety Variety { get; private set; }

	// AppScaffolding
	private static Assembly? _executingAssembly;
	private static Assembly ExecutingAssembly
		=> _executingAssembly ??= Assembly.GetExecutingAssembly();

	// LibationWinForms or LibationCli
	private static Assembly? _entryAssembly;
	private static Assembly? EntryAssembly
		=> _entryAssembly ??= Assembly.GetEntryAssembly();

	// previously: System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
	private static Version? _buildVersion;
	public static Version? BuildVersion
		=> _buildVersion ??= new[] { ExecutingAssembly.GetName(), EntryAssembly?.GetName() }.Max(a => a?.Version);

	/// <summary>Run migrations before loading Configuration for the first time. Then load and return Configuration</summary>
	public static Configuration RunPreConfigMigrations()
	{
		// must occur before access to Configuration instance
		// // outdated. kept here as an example of what belongs in this area
		// // Migrations.migrate_to_v5_2_0__pre_config();

		InstallFolderUnblock.TryUnblockProcessDirectoryIfWindows();

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
		config.ValidateEnumSettings();
		DeleteOpenSqliteFiles(config);
		AudibleApiStorage.EnsureAccountsSettingsFileExists();
		IdentityTokenStorageWiring.Apply(config);

		//
		// migrations go below here
		//

		Migrations.migrate_to_v12_0_1(config);
	}

	/// <summary>
	/// Delete shared memory and write-ahead log SQLite database files which may prevent access to the database.
	/// These file may or may not cause libation to hang on CreateContext,
	/// so try our luck by swallowing any exceptions and continuing.
	/// </summary>
	private static void DeleteOpenSqliteFiles(Configuration config)
	{
		var dbFile = SqliteStorage.DatabasePath;
		var walFile = dbFile + "-wal";
		var shmFile = dbFile + "-shm";

		// If another Libation process currently has the database open, its WAL/SHM are live state.
		// Deleting them would corrupt that process's database (and ours), so leave everything alone.
		// See issue #1931.
		if (IsFileLockedByAnotherProcess(dbFile))
		{
			Log.Logger.Information("Skipping SQLite WAL/SHM cleanup: the database appears to be open in another process.");
			return;
		}

		// A non-empty WAL from a previous run that ended abruptly can hold committed-but-uncheckpointed
		// transactions. Deleting it would silently discard that data; instead, leave it in place and let
		// SQLite recover it when the database is next opened. Only remove an already-checkpointed WAL.
		if (File.Exists(walFile))
		{
			if (WalHoldsUnrecoveredTransactions(walFile))
				Log.Logger.Information("Leaving SQLite WAL in place so SQLite can recover it on open: {WalFile}", walFile);
			else
				FileManager.FileUtility.TrySaferDelete(walFile);
		}

		// The SHM (shared-memory index) is rebuilt from the WAL/database, so it is safe to remove once
		// no other process holds the database.
		if (File.Exists(shmFile))
			FileManager.FileUtility.TrySaferDelete(shmFile);
	}

	/// <summary>True if <paramref name="path"/> exists and cannot be opened exclusively, i.e. another process holds it.</summary>
	private static bool IsFileLockedByAnotherProcess(string path)
	{
		if (!File.Exists(path))
			return false;

		try
		{
			using var _ = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
			return false;
		}
		catch (IOException)
		{
			return true;
		}
		catch (UnauthorizedAccessException)
		{
			// Can't prove another process holds it, but we also can't safely touch it. Be conservative.
			return true;
		}
	}

	/// <summary>
	/// True if the WAL may contain committed transactions SQLite still needs to recover. A WAL longer
	/// than its 32-byte header may hold frames; treat any such file as unrecovered so it is preserved.
	/// </summary>
	private static bool WalHoldsUnrecoveredTransactions(string walFile)
	{
		const long walHeaderSize = 32;
		try
		{
			return new FileInfo(walFile).Length > walHeaderSize;
		}
		catch (Exception ex)
		{
			// If we can't measure it, assume it matters and keep it rather than risk data loss.
			Log.Logger.Warning(ex, "Could not inspect SQLite WAL file; leaving it in place: {WalFile}", walFile);
			return true;
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
		=> config.EnsureSerilogConfig();

	// to restore original: Console.SetOut(origOut);
	private static TextWriter origOut { get; } = Console.Out;

	private static void configureLogging(Configuration config)
	{
		config.ConfigureLogging();
		Log.Information(
			"Paths: LibationFiles={LibationFiles} AppsettingsJson={AppsettingsJson} SQLiteDb={SqliteDb}",
			config.LibationFiles.Location,
			config.LibationFiles.AppsettingsJsonFile ?? "(null, LIBATION_FILES_DIR may be set)",
			Path.Combine(config.LibationFiles.Location, "LibationContext.db"));
		DbContexts.TryEmitPendingInitialDatabaseStatistics();

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
		if (Debugger.IsAttached)
			mode += " (Debugger attached)";

		// begin logging session with a form feed
		Log.Logger.Information("\r\n\f");

		// -1 means the count could not be taken. Listing a directory no longer throws when it stops being
		// readable partway through, so an incomplete walk has to be asked about rather than caught: a count
		// that silently means "as many as I managed to read" is worse than no count in a bug report.
		static int fileCount(FileManager.LongPath? longPath)
		{
			if (longPath is null)
				return -1;

			var complete = true;
			try
			{
				var count = FileManager.FileUtility.SaferEnumerateFiles(longPath, onIncomplete: _ => complete = false).Count();
				return complete ? count : -1;
			}
			catch { return -1; }
		}

		Log.Logger.Information("Begin. {@DebugInfo}", new
		{
			AppName = EntryAssembly?.GetName().Name,
			Version = BuildVersion?.ToString(),
			ReleaseIdentifier,
			Configuration.OS,
			Environment.OSVersion,
			InteropFactory.InteropFunctionsType,
			// A file Windows refused to load, or a half-applied upgrade, is usually explained by one
			// of these three, and none of them is visible anywhere else in a bug report.
			InstallFolder = Configuration.ProcessDirectory,
			ApplicationControl = ApplicationControlPolicy.GetState(),
			InstallFolderCloudSync = CloudSyncedFolders.GetSyncStatus(Configuration.ProcessDirectory),
			Mode = mode,
			LogLevel_Verbose_Enabled = Log.Logger.IsVerboseEnabled(),
			LogLevel_Debug_Enabled = Log.Logger.IsDebugEnabled(),
			LogLevel_Information_Enabled = Log.Logger.IsInformationEnabled(),
			LogLevel_Warning_Enabled = Log.Logger.IsWarningEnabled(),
			LogLevel_Error_Enabled = Log.Logger.IsErrorEnabled(),
			LogLevel_Fatal_Enabled = Log.Logger.IsFatalEnabled(),

			config.AutoScan,
			// These silently exclude titles from every scan, so a log without them can't explain a missing book
			config.ImportEpisodes,
			config.ImportPlusTitles,
			config.DownloadEpisodes,
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

	private static void wireUpSystemEvents(Configuration configuration)
	{
		LibraryCommands.LibrarySizeChanged += (_, libraryBooks)
			=> SearchEngineCommands.OnLibrarySizeChanged(libraryBooks);

		LibraryCommands.BookUserDefinedItemCommitted += (_, books)
			=> SearchEngineCommands.OnBookUserDefinedItemCommitted(books);
	}

	public static VersionCheckResult GetLatestRelease()
	{
		var (version, latest, zip, checkSucceeded, definitive) = getLatestRelease(TimeSpan.FromSeconds(10));

		if (!checkSucceeded || !definitive)
			return new VersionCheckResult(VersionCheckOutcome.UnableToDetermine);

		if (version is null || latest is null || zip is null)
			return new VersionCheckResult(VersionCheckOutcome.UpToDate);

		// we have an update
		var zipUrl = zip.BrowserDownloadUrl;

		Log.Logger.Information("Update available: {@DebugInfo}", new
		{
			latestRelease = version.ToString(),
			latest.HtmlUrl,
			zipUrl
		});

		return new VersionCheckResult(VersionCheckOutcome.UpdateAvailable, new UpgradeProperties(zipUrl, latest.HtmlUrl, zip.Name, version, latest.Body));
	}
	private static (Version? releaseVersion, Octokit.Release?, Octokit.ReleaseAsset? zip, bool checkSucceeded, bool definitive) getLatestRelease(TimeSpan timeout)
	{
		try
		{
			var task = getLatestReleaseAsync();
			if (task.Wait(timeout))
				return (task.Result.releaseVersion, task.Result.latest, task.Result.zip, true, task.Result.definitive);

			Log.Logger.Information("Version check timed out");
		}
		catch (AggregateException aggEx)
		{
			var inner = aggEx.InnerException;
			if (inner?.Message?.Contains("API rate limit", StringComparison.OrdinalIgnoreCase) == true)
				Log.Logger.Error(aggEx, "Checking for new version too often");
			else
				Log.Logger.Error(aggEx, "Version check failed. Agg exception");
		}
		catch (Exception ex)
		{
			// different text to make it easier to identify in logs, vs the AggregateException case above
			Log.Logger.Error(ex, "Version check failed. General exception");
		}
		return (null, null, null, false, false);
	}
	private static async System.Threading.Tasks.Task<(Version? releaseVersion, Octokit.Release? latest, Octokit.ReleaseAsset? zip, bool definitive)> getLatestReleaseAsync()
	{
		const string ownerAccount = "rmcrackan";
		const string repoName = "Libation";

		var gitHubClient = new Octokit.GitHubClient(new Octokit.ProductHeaderValue(repoName));

		//https://docs.github.com/en/rest/releases/releases?apiVersion=2022-11-28#get-the-latest-release
		var latestRelease = await gitHubClient.Repository.Release.GetLatest(ownerAccount, repoName);

		//Ensure that latest release is greater than the current version
		var latestVersionString = latestRelease.TagName.Trim('v');
		if (!Version.TryParse(latestVersionString, out var releaseVersion) || releaseVersion <= BuildVersion)
			return (null, null, null, true);

		//Download the release index
		var bts = await gitHubClient.Repository.Content.GetRawContent(ownerAccount, repoName, ".releaseindex.json");
		var releaseIndex = JObject.Parse(System.Text.Encoding.ASCII.GetString(bts));

		string? regexPattern;
		string? releaseIdString = null;

		try
		{
			releaseIdString = InteropFactory.Create().ReleaseIdString;
			regexPattern = releaseIndex.Value<string>(releaseIdString);
		}
		catch
		{
			regexPattern = null;
		}
		if (string.IsNullOrEmpty(regexPattern) && Configuration.IsLinux)
		{
			var baseId = ReleaseIdentifier.ToString();
			regexPattern = releaseIndex.Value<string>($"{baseId}_RPM")
				?? releaseIndex.Value<string>($"{baseId}_DEB");
			releaseIdString ??= $"{baseId}_RPM";
		}
		if (string.IsNullOrEmpty(regexPattern))
			regexPattern = releaseIndex.Value<string>(ReleaseIdentifier.ToString());
		if (string.IsNullOrEmpty(regexPattern))
		{
			Log.Logger.Warning("Release index has no entry for this platform (ReleaseIdentifier: {ReleaseId}, ReleaseIdString: {ReleaseIdString}). Version check inconclusive.", ReleaseIdentifier, releaseIdString);
			return (null, null, null, false);
		}

		var regex = new System.Text.RegularExpressions.Regex(regexPattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
		var zip = latestRelease?.Assets?.FirstOrDefault(a => regex.IsMatch(a.Name));

		if (zip is not null && !string.IsNullOrEmpty(releaseIdString))
			Log.Logger.Information("Update asset matched using {ReleaseIdString}: {AssetName}", releaseIdString, zip.Name);

		return (releaseVersion, latestRelease, zip, true);
	}
}

internal static class Migrations
{
	public static void migrate_to_v12_0_1(Configuration config)
	{
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

				Dictionary<string, JArray> cache = [];

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
							{ "FileType", (int)FileTypes.GetFileTypeFromPath(i.Path!) },
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
	}
}
