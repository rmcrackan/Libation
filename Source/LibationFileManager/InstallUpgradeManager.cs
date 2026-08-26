using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LibationFileManager;

public readonly record struct UpgradeVerificationResult(
	bool Success,
	IReadOnlyList<string> FailedFiles,
	string Summary);

public sealed record UpgradeRecoveryResult(
	bool RolledBack,
	string Title,
	string Message,
	IReadOnlyList<string> FailedFiles);

/// <summary>
/// Backups, verifies, and rolls back flat zip overlay upgrades (Windows ZipExtractor flow).
/// </summary>
public static class InstallUpgradeManager
{
	public const string UpgradeStateFolderName = ".libation-upgrade";
	public const string PendingStateFileName = "pending.json";
	public const string BackupFolderName = "backup";

	/// <summary>Suffix for a loaded assembly moved out of the way so its replacement can be written.</summary>
	public const string DisplacedFileSuffix = ".libation-old";

	public const string LibationUiBaseIntegrityTypeName = "LibationUiBase.ShowBadBookDialogAsyncDelegate";

	private static readonly JsonSerializerOptions JsonOptions = new()
	{
		WriteIndented = true,
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
		DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
	};

	/// <summary>
	/// Files an overlay upgrade must have replaced for Libation to start at all, so they are both backed up
	/// and hash-verified.
	/// <para/>
	/// The bottom four were added after issue #2001: Libation cannot reach its own crash dialog without
	/// them, yet they were absent from this list, so an overlay that left a stale <c>Serilog.dll</c> behind
	/// still passed verification and the pending marker was cleared as a success.
	/// </summary>
	private static readonly string[] AlwaysCriticalFileNames =
	[
		"LibationUiBase.dll",
		"LibationFileManager.dll",
		"AppScaffolding.dll",
		"Microsoft.EntityFrameworkCore.Sqlite.dll",
		"Serilog.dll",
		"Dinah.Core.dll",
		"FileManager.dll",
		"Newtonsoft.Json.dll",
	];

	private static FatalStartupMessage? s_StartupRecoveryAlert;

	public static FatalStartupMessage? TakeStartupRecoveryAlert()
	{
		var alert = s_StartupRecoveryAlert;
		s_StartupRecoveryAlert = null;
		return alert;
	}

	public static string GetUpgradeStateDirectory(string installDirectory)
		=> Path.Combine(installDirectory, UpgradeStateFolderName);

	public static string GetPendingStatePath(string installDirectory)
		=> Path.Combine(GetUpgradeStateDirectory(installDirectory), PendingStateFileName);

	public static string GetBackupDirectory(string installDirectory)
		=> Path.Combine(GetUpgradeStateDirectory(installDirectory), BackupFolderName);

	/// <summary>
	/// Snapshot critical install files and record expected post-upgrade hashes from the upgrade zip.
	/// Call immediately before launching ZipExtractor.
	/// </summary>
	public static void PrepareForUpgrade(string installDirectory, string upgradeBundlePath, Version targetVersion)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(installDirectory);
		ArgumentException.ThrowIfNullOrWhiteSpace(upgradeBundlePath);
		ArgumentNullException.ThrowIfNull(targetVersion);

		if (!Directory.Exists(installDirectory))
			throw new DirectoryNotFoundException($"Install directory not found: {installDirectory}");
		if (!File.Exists(upgradeBundlePath))
			throw new FileNotFoundException("Upgrade bundle not found.", upgradeBundlePath);

		var criticalFiles = GetCriticalFileNames(installDirectory);
		var expectedHashes = BuildExpectedHashesFromZip(upgradeBundlePath, criticalFiles);

		var stateDirectory = GetUpgradeStateDirectory(installDirectory);
		var backupDirectory = GetBackupDirectory(installDirectory);

		if (Directory.Exists(stateDirectory))
			Directory.Delete(stateDirectory, recursive: true);

		Directory.CreateDirectory(backupDirectory);

		var backedUpFiles = new List<string>();
		foreach (var fileName in criticalFiles)
		{
			var sourcePath = Path.Combine(installDirectory, fileName);
			if (!File.Exists(sourcePath))
				continue;

			var backupPath = Path.Combine(backupDirectory, fileName);
			Directory.CreateDirectory(Path.GetDirectoryName(backupPath)!);
			File.Copy(sourcePath, backupPath, overwrite: true);
			backedUpFiles.Add(fileName);
		}

		var pending = new PendingUpgradeState
		{
			TargetVersion = targetVersion.ToString(),
			UpgradeBundlePath = upgradeBundlePath,
			StartedUtc = DateTime.UtcNow,
			InstallDirectory = installDirectory,
			BackedUpFiles = backedUpFiles,
			ExpectedFileHashesSha256 = expectedHashes,
		};

		var pendingPath = GetPendingStatePath(installDirectory);
		File.WriteAllText(pendingPath, JsonSerializer.Serialize(pending, JsonOptions));

		StartupLog.Information(
			$"Prepared in-app upgrade to {targetVersion}. Backed up {backedUpFiles.Count} files to {backupDirectory}. Expecting {expectedHashes.Count} install files to match the upgrade package.");
	}

	/// <summary>
	/// If a previous upgrade left a pending marker, verify the install folder and roll back on failure.
	/// Call at startup before loading UI assemblies.
	/// </summary>
	public static UpgradeRecoveryResult? RecoverPendingUpgradeIfNeeded(string installDirectory)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(installDirectory);

		// A previous run's rollback left these behind because it could not delete a file it still had open.
		DeleteDisplacedFiles(installDirectory);

		var pendingPath = GetPendingStatePath(installDirectory);
		if (!File.Exists(pendingPath))
			return null;

		PendingUpgradeState pending;
		try
		{
			pending = JsonSerializer.Deserialize<PendingUpgradeState>(File.ReadAllText(pendingPath), JsonOptions)
				?? throw new InvalidDataException("Pending upgrade state was empty.");
		}
		catch (Exception ex)
		{
			StartupLog.Error(ex, $"Could not read pending upgrade state at {pendingPath}. Attempting emergency rollback.");
			return RollbackAndReport(installDirectory, pendingPath, null, ["Could not read pending upgrade state."], ex.Message);
		}

		var verification = VerifyInstallMatchesUpgrade(installDirectory, pending.ExpectedFileHashesSha256);
		if (verification.Success)
		{
			CompleteUpgrade(installDirectory);
			StartupLog.Information($"In-app upgrade to {pending.TargetVersion} verified successfully at startup.");
			return null;
		}

		StartupLog.Error(
			$"Incomplete in-app upgrade detected at startup. Target version {pending.TargetVersion}. {verification.Summary}");

		return RollbackAndReport(installDirectory, pendingPath, pending, verification.FailedFiles, verification.Summary);
	}

	public static UpgradeVerificationResult VerifyInstallMatchesUpgrade(
		string installDirectory,
		IReadOnlyDictionary<string, string>? expectedFileHashesSha256 = null)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(installDirectory);

		expectedFileHashesSha256 ??= TryReadPendingExpectedHashes(installDirectory);
		if (expectedFileHashesSha256 is null || expectedFileHashesSha256.Count == 0)
			return new UpgradeVerificationResult(true, [], "No pending upgrade verification manifest.");

		var failedFiles = new List<string>();
		foreach (var (fileName, expectedHash) in expectedFileHashesSha256)
		{
			var installPath = Path.Combine(installDirectory, fileName);
			if (!File.Exists(installPath))
			{
				failedFiles.Add($"{fileName}: missing from install folder");
				continue;
			}

			var actualHash = ComputeSha256Hex(installPath);
			if (!string.Equals(actualHash, expectedHash, StringComparison.OrdinalIgnoreCase))
				failedFiles.Add($"{fileName}: on-disk content does not match upgrade package (file was not replaced)");
		}

		var typeCheckFailure = VerifyLibationUiBaseIntegrityType(installDirectory);
		if (typeCheckFailure is not null)
			failedFiles.Add(typeCheckFailure);

		if (failedFiles.Count == 0)
			return new UpgradeVerificationResult(true, failedFiles, "Install folder matches upgrade package.");

		var summary =
			$"Upgrade integrity check failed for {failedFiles.Count} item(s):{Environment.NewLine}"
			+ string.Join(Environment.NewLine, failedFiles.Select(f => $"  - {f}"));

		return new UpgradeVerificationResult(false, failedFiles, summary);
	}

	public static void RollbackAfterFailedUpgrade(string installDirectory, string reason)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(installDirectory);
		ArgumentException.ThrowIfNullOrWhiteSpace(reason);

		var pending = TryReadPendingState(installDirectory);
		var failedFiles = new[] { reason };
		RollbackAndReport(installDirectory, GetPendingStatePath(installDirectory), pending, failedFiles, reason);
	}

	public static UpgradeRecoveryResult TryEmergencyRollback(string installDirectory)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(installDirectory);

		var backupDirectory = GetBackupDirectory(installDirectory);
		if (!Directory.Exists(backupDirectory))
			return new UpgradeRecoveryResult(false, string.Empty, string.Empty, []);

		var pending = TryReadPendingState(installDirectory);
		return RollbackAndReport(
			installDirectory,
			GetPendingStatePath(installDirectory),
			pending,
			["Emergency rollback triggered by startup assembly load failure."],
			"Startup assembly load failure.");
	}

	public static void CompleteUpgrade(string installDirectory)
	{
		DeleteDisplacedFiles(installDirectory);

		var stateDirectory = GetUpgradeStateDirectory(installDirectory);
		if (!Directory.Exists(stateDirectory))
			return;

		try
		{
			Directory.Delete(stateDirectory, recursive: true);
		}
		catch (Exception ex)
		{
			StartupLog.Warning(ex, $"Could not delete upgrade state directory {stateDirectory}");
		}
	}

	public static IReadOnlyList<string> GetCriticalFileNames(string installDirectory)
	{
		var files = new HashSet<string>(AlwaysCriticalFileNames, StringComparer.OrdinalIgnoreCase);

		var mainExecutable = Path.GetFileName(Environment.ProcessPath ?? string.Empty);
		if (!string.IsNullOrWhiteSpace(mainExecutable))
			files.Add(mainExecutable);

		if (Directory.Exists(installDirectory))
		{
			foreach (var configApp in Directory.EnumerateFiles(installDirectory, "*ConfigApp.dll"))
				files.Add(Path.GetFileName(configApp));

			if (File.Exists(Path.Combine(installDirectory, "ZipExtractor.exe")))
				files.Add("ZipExtractor.exe");
		}

		return files.OrderBy(f => f, StringComparer.OrdinalIgnoreCase).ToArray();
	}

	private static UpgradeRecoveryResult RollbackAndReport(
		string installDirectory,
		string pendingPath,
		PendingUpgradeState? pending,
		IReadOnlyList<string> failedFiles,
		string summary)
	{
		var restoredFiles = RestoreFromBackup(installDirectory);

		StartupLog.Error(
			$"In-app upgrade failed. Rolled back {restoredFiles.Count} file(s) in {installDirectory}. {summary}");

		try
		{
			if (File.Exists(pendingPath))
				File.Delete(pendingPath);
		}
		catch (Exception ex)
		{
			StartupLog.Warning(ex, $"Could not delete pending upgrade state at {pendingPath}");
		}

		var targetVersion = pending?.TargetVersion ?? "unknown";
		var title = "In-app upgrade failed -- Libation was restored";
		var message = $"""
			Libation attempted an in-app upgrade to version {targetVersion}, but one or more install files were not updated correctly.

			Libation restored your previous install files from backup so you can continue using the app.

			Details:
			{summary}

			Install folder:
			{installDirectory}

			Your library database, accounts, and settings are stored separately and were not changed.

			To upgrade safely:
			1. Quit Libation completely.
			2. Download the latest release zip from GitHub.
			3. Extract it to a new folder (do not copy files on top of this install folder).
			4. Run Libation from the new folder.

			More help:
			{StartupAssemblyBootstrap.TroubleshootIncompleteUpgradeUrl}
			""";

		s_StartupRecoveryAlert = new FatalStartupMessage(title, message);
		return new UpgradeRecoveryResult(true, title, message, failedFiles);
	}

	private static List<string> RestoreFromBackup(string installDirectory)
	{
		var backupDirectory = GetBackupDirectory(installDirectory);
		var restoredFiles = new List<string>();

		if (!Directory.Exists(backupDirectory))
			return restoredFiles;

		foreach (var backupFile in Directory.EnumerateFiles(backupDirectory, "*", SearchOption.AllDirectories))
		{
			var relativePath = Path.GetRelativePath(backupDirectory, backupFile);
			var targetPath = Path.Combine(installDirectory, relativePath);
			Directory.CreateDirectory(Path.GetDirectoryName(targetPath)!);
			ReplaceInstallFile(backupFile, targetPath);
			restoredFiles.Add(relativePath);

			StartupLog.Information($"Upgrade rollback restored {relativePath}");
		}

		return restoredFiles;
	}

	/// <summary>
	/// Puts <paramref name="source"/> at <paramref name="targetPath"/> even when this process has already
	/// loaded the file it is replacing.
	/// <para/>
	/// Every backed-up file is an assembly, and by the time startup recovery runs, at least
	/// LibationFileManager and AppScaffolding are loaded and memory-mapped. Writing over a mapped file
	/// in place corrupts the mapping: <c>File.Copy(overwrite: true)</c> segfaulted the process outright on
	/// Linux, and Windows denies the write, so the rollback could never finish. Renaming is permitted on
	/// both, because .NET opens assemblies with <c>FileShare.Delete</c>, and the mapping keeps working off
	/// the moved inode until the process exits. See issue #2001.
	/// </summary>
	private static void ReplaceInstallFile(string source, string targetPath)
	{
		if (File.Exists(targetPath))
		{
			var displaced = targetPath + DisplacedFileSuffix;
			TryDelete(displaced);
			File.Move(targetPath, displaced, overwrite: true);
		}

		File.Copy(source, targetPath, overwrite: true);
	}

	/// <summary>
	/// Removes the files a previous rollback moved aside. Their replacements are on disk and the process
	/// that was holding them has exited, so nothing needs them any more.
	/// <para/>
	/// Top level only: every backed-up name comes from <see cref="GetCriticalFileNames"/>, which yields
	/// bare file names, so a displaced file can only ever sit next to the executable. That keeps this cheap
	/// enough to run on every startup, which it has to, because the rollback that creates these files also
	/// deletes the pending marker that would otherwise signal there is cleaning up to do.
	/// </summary>
	private static void DeleteDisplacedFiles(string installDirectory)
	{
		try
		{
			if (!Directory.Exists(installDirectory))
				return;

			foreach (var displaced in Directory.EnumerateFiles(installDirectory, $"*{DisplacedFileSuffix}", SearchOption.TopDirectoryOnly))
				TryDelete(displaced);
		}
		catch (Exception ex)
		{
			StartupLog.Warning(ex, $"Could not clean up displaced install files in {installDirectory}");
		}
	}

	private static void TryDelete(string path)
	{
		try
		{
			if (File.Exists(path))
				File.Delete(path);
		}
		catch (Exception ex)
		{
			// Still held by something, or not ours to delete. It is inert either way.
			StartupLog.Debug(ex, $"Could not delete {path}");
		}
	}

	private static Dictionary<string, string> BuildExpectedHashesFromZip(string upgradeBundlePath, IReadOnlyList<string> criticalFileNames)
	{
		var expected = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

		using var zip = ZipFile.OpenRead(upgradeBundlePath);
		foreach (var fileName in criticalFileNames)
		{
			var entry = zip.GetEntry(fileName)
				?? zip.Entries.FirstOrDefault(e => string.Equals(Path.GetFileName(e.FullName), fileName, StringComparison.OrdinalIgnoreCase));

			if (entry is null)
			{
				StartupLog.Warning($"Upgrade package does not contain expected file {fileName}");
				continue;
			}

			using var entryStream = entry.Open();
			expected[fileName] = ComputeSha256Hex(entryStream);
		}

		if (expected.Count == 0)
			throw new InstallUpgradeIntegrityException("Upgrade package does not contain any verifiable install files.");

		return expected;
	}

	private static string ComputeSha256Hex(string path)
	{
		using var stream = File.OpenRead(path);
		return ComputeSha256Hex(stream);
	}

	private static string ComputeSha256Hex(Stream stream)
	{
		var hash = SHA256.HashData(stream);
		return Convert.ToHexString(hash);
	}

	private static PendingUpgradeState? TryReadPendingState(string installDirectory)
	{
		var pendingPath = GetPendingStatePath(installDirectory);
		if (!File.Exists(pendingPath))
			return null;

		try
		{
			return JsonSerializer.Deserialize<PendingUpgradeState>(File.ReadAllText(pendingPath), JsonOptions);
		}
		catch
		{
			return null;
		}
	}

	private static IReadOnlyDictionary<string, string>? TryReadPendingExpectedHashes(string installDirectory)
		=> TryReadPendingState(installDirectory)?.ExpectedFileHashesSha256;

	private static string? VerifyLibationUiBaseIntegrityType(string installDirectory)
	{
		var uiBasePath = Path.Combine(installDirectory, "LibationUiBase.dll");
		if (!File.Exists(uiBasePath))
			return "LibationUiBase.dll: missing from install folder";

		try
		{
			var alreadyLoaded = AppDomain.CurrentDomain
				.GetAssemblies()
				.FirstOrDefault(a => string.Equals(a.GetName().Name, "LibationUiBase", StringComparison.OrdinalIgnoreCase));

			var assembly = alreadyLoaded ?? Assembly.LoadFrom(uiBasePath);
			var integrityType = assembly.GetType(LibationUiBaseIntegrityTypeName, throwOnError: false, ignoreCase: false);
			if (integrityType is null)
				return $"{LibationUiBaseIntegrityTypeName}: missing from LibationUiBase.dll (install files are from mixed versions)";

			return null;
		}
		catch (BadImageFormatException)
		{
			// Non-assembly test doubles and corrupt files are covered by hash verification.
			return null;
		}
		catch (FileLoadException)
		{
			return null;
		}
		catch (Exception ex)
		{
			return $"LibationUiBase.dll: could not verify required type ({ex.Message})";
		}
	}

	private sealed class PendingUpgradeState
	{
		public string TargetVersion { get; set; } = string.Empty;
		public string UpgradeBundlePath { get; set; } = string.Empty;
		public DateTime StartedUtc { get; set; }
		public string InstallDirectory { get; set; } = string.Empty;
		public List<string> BackedUpFiles { get; set; } = [];
		public Dictionary<string, string> ExpectedFileHashesSha256 { get; set; } = new(StringComparer.OrdinalIgnoreCase);
	}
}
