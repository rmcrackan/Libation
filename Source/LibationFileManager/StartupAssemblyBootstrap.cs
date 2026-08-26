using FileManager;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace LibationFileManager;

/// <summary>
/// Ensures OS interop assembly resolution and required dependency files are ready before background library load.
/// </summary>
public static class StartupAssemblyBootstrap
{
	public const string EntityFrameworkCoreSqliteAssemblyFileName = "Microsoft.EntityFrameworkCore.Sqlite.dll";
	public const int ApplicationControlBlockedHResult = unchecked((int)0x800711C7);
	public const string TroubleshootApplicationControlUrl = "https://getlibation.com/docs/advanced/troubleshoot#windows-smart-app-control-and-in-app-upgrades";
	internal const string TroubleshootIncompleteUpgradeUrl = "https://getlibation.com/docs/advanced/troubleshoot#windows-incomplete-in-app-upgrade";

	/// <summary>
	/// Registers <see cref="InteropFactory"/> assembly resolution and verifies required install-folder assemblies exist.
	/// Call after <see cref="RecoverFromIncompleteUpgradeIfNeeded"/> and before <c>Task.Run</c> loads the library.
	/// </summary>
	public static void PrepareForBackgroundDataAccess()
	{
		_ = InteropFactory.InteropFunctionsType;
		ValidateEntityFrameworkCoreSqlitePresent();
		TrySyncWindowsInstallMetadata();
	}

	/// <summary>
	/// If a zip overlay upgrade was interrupted or incomplete, verify install files and roll back before continuing startup.
	/// Call once immediately after <c>RunPreConfigMigrations</c>, before assigning UI assembly hooks such as
	/// <c>BadBookActionDialogBase.ShowAsyncImpl</c>.
	/// </summary>
	/// <returns>
	/// The message to show when a rollback replaced install files, in which case the caller must show it and
	/// quit rather than continue. This process has already loaded the assemblies that were just swapped out
	/// from under it, so what is in memory no longer matches what is on disk. Null when there was nothing to
	/// recover, which is the normal case.
	/// </returns>
	public static FatalStartupMessage? RecoverFromIncompleteUpgradeIfNeeded()
	{
		try
		{
			var recovery = InstallUpgradeManager.RecoverPendingUpgradeIfNeeded(Configuration.ProcessDirectory);
			if (recovery?.RolledBack != true)
				return null;

			InstallUpgradeManager.TakeStartupRecoveryAlert();
			return new FatalStartupMessage(
				recovery.Title,
				recovery.Message + Environment.NewLine + Environment.NewLine + "Libation will close now. Please start it again.");
		}
		catch (Exception ex)
		{
			StartupLog.Error(ex, "Failed while recovering from a pending in-app upgrade");
			return null;
		}
	}

	private static void TrySyncWindowsInstallMetadata()
	{
		if (!Configuration.IsWindows || InteropFactory.InteropFunctionsType is null)
			return;

		try
		{
			InteropFactory.Create().TrySyncInstallMetadata();
		}
		catch (Exception ex)
		{
			StartupLog.Warning(ex, "Could not run install metadata sync at startup");
		}
	}

	public static string GetLibraryLoadFailureMessage() =>
		$"""
		Libation could not load its database components (Entity Framework Core for SQLite).

		This often happens after an incomplete in-app upgrade. Quit Libation completely, then install a fresh copy of the latest release to a new folder (do not overlay files on top of the old install).

		If the error mentions an Application Control policy or Smart App Control, see:
		{TroubleshootApplicationControlUrl}

		Install folder:
		{Configuration.ProcessDirectory}

		Expected file:
		{Path.Combine(Configuration.ProcessDirectory, EntityFrameworkCoreSqliteAssemblyFileName)}
		""";

	// Release status, such as code signing progress, belongs in the linked docs rather than here:
	// this string ships frozen in each build and cannot be corrected after release.
	public static string GetApplicationControlBlockedMessage(Exception? ex = null)
	{
		var blockedFile = TryGetBlockedAssemblyPath(ex) ?? "(unknown)";

		return $"""
			Windows blocked Libation from loading a required file in its install folder. An Application Control policy, usually Smart App Control, is refusing to run it.

			Blocked file:
			{blockedFile}

			Install folder:
			{Configuration.ProcessDirectory}

			Smart App Control runs only code that Microsoft's reputation service recognises or that is signed with a trusted certificate. Libation's Windows builds are not signed yet. Windows offers no way to allow a single app through, so reinstalling to another folder and unblocking the files do not help.

			Your library database, accounts, and settings are stored separately and should be unaffected.

			{DescribeApplicationControlState(ApplicationControlPolicy.GetState())}

			More help:
			{TroubleshootApplicationControlUrl}
			""";
	}

	/// <summary>
	/// Says what Smart App Control is set to when we could read it, and how to look it up when we
	/// could not. Only enforcement is worth acting on, so the other states point elsewhere rather
	/// than inviting someone to turn off a setting that is not the cause.
	/// </summary>
	public static string DescribeApplicationControlState(ApplicationControlState state)
		=> state switch
		{
			ApplicationControlState.Enforcing =>
				"""
				Smart App Control is On for this PC, which is what blocked the file.

				Turning it off is one way out, but Windows cannot turn it back on again without a reset or reinstall. Check the page below for the current options before you change anything.
				""",

			ApplicationControlState.Evaluation =>
				"Smart App Control is in Evaluation mode for this PC, and that mode never blocks anything, so the block is coming from another Application Control policy, normally one set by whoever manages this PC.",

			ApplicationControlState.Off =>
				"Smart App Control is off for this PC, so the block is coming from another Application Control policy, normally one set by whoever manages this PC.",

			_ =>
				"""
				To check whether this is Smart App Control, open Settings -> Privacy & Security -> Windows Security -> App & browser control -> Smart App Control settings. Only the On setting blocks anything; Evaluation observes without blocking.

				If it is On, turning it off is one way out, but Windows cannot turn it back on again without a reset or reinstall. Check the page below for the current options before you change anything. If it is already off, the block comes from a policy set by whoever manages this PC.
				""",
		};

	public static bool IsApplicationControlBlockedAssembly(Exception ex)
	{
		for (var current = ex; current is not null; current = current.InnerException)
		{
			if (current is FileLoadException fileLoadException)
			{
				if (fileLoadException.HResult == ApplicationControlBlockedHResult)
					return true;

				if (fileLoadException.Message.Contains("Application Control policy", StringComparison.OrdinalIgnoreCase))
					return true;
			}

			if (current.Message.Contains("Application Control policy", StringComparison.OrdinalIgnoreCase))
				return true;
		}

		return false;
	}

	public static bool IsIncompleteUpgradeAssemblyFailure(Exception ex)
	{
		for (var current = ex; current is not null; current = current.InnerException)
		{
			if (current is TypeLoadException typeLoadException)
			{
				if (ContainsLibationUiBaseReference(typeLoadException.TypeName)
					|| ContainsLibationUiBaseReference(typeLoadException.Message))
					return true;
			}

			if (current is ReflectionTypeLoadException reflectionTypeLoadException)
			{
				if (ContainsLibationUiBaseReference(reflectionTypeLoadException.Message))
					return true;

				if (reflectionTypeLoadException.LoaderExceptions?.Any(e =>
					e is not null && (ContainsLibationUiBaseReference(e.Message) || ContainsLibationUiBaseReference((e as TypeLoadException)?.TypeName))) == true)
					return true;
			}

			if (current is FileLoadException { FileName: { Length: > 0 } fileName }
				&& fileName.Contains("LibationUiBase", StringComparison.OrdinalIgnoreCase))
				return true;
		}

		return false;
	}

	public static bool IsInstallFolderAssemblyLoadFailure(Exception ex) =>
		IsApplicationControlBlockedAssembly(ex)
		|| IsMissingDependencyAssembly(ex)
		|| IsIncompleteUpgradeAssemblyFailure(ex)
		|| TryGetInstallAssemblyFailure(ex, out _);

	/// <summary>
	/// Finds an assembly the runtime could not bind to a usable file in the install folder, whatever the
	/// assembly is called.
	/// <para/>
	/// The name-based checks above only recognise the handful of assemblies that had already caused a bug
	/// report, so a missing <c>Serilog.dll</c> fell through all of them to a generic "fatal error" dialog
	/// with no rollback attempted. See issue #2001.
	/// <para/>
	/// A stale file reports identically to an absent one: the loader says "the system cannot find the file
	/// specified" either way, because it rejects a file whose version is below the reference and then has
	/// nothing left to bind. The on-disk version is therefore worth reading and telling the user about.
	/// </summary>
	public static bool TryGetInstallAssemblyFailure(Exception? ex, out InstallAssemblyFailure? failure)
	{
		failure = null;
		for (var current = ex; current is not null; current = current.InnerException)
		{
			if (current is AggregateException aggregate)
			{
				foreach (var inner in aggregate.InnerExceptions)
				{
					if (TryGetInstallAssemblyFailure(inner, out failure))
						return true;
				}
			}

			if (current is not FileNotFoundException and not FileLoadException)
				continue;

			var fileName = (current as FileNotFoundException)?.FileName ?? (current as FileLoadException)?.FileName;
			if (!TryParseAssemblyReference(fileName, out var assemblyName, out var requestedVersion)
				|| assemblyName is null
				|| requestedVersion is null)
				continue;

			var path = FindInstallAssemblyPath(assemblyName);
			var installedVersion = path is null ? null : TryReadAssemblyVersion(path);

			// Present, readable and no older than the reference: this bind failed for some other reason,
			// so leave it to a caller that knows more rather than blaming the install folder.
			if (installedVersion is not null && installedVersion >= requestedVersion)
				continue;

			failure = new InstallAssemblyFailure(
				assemblyName,
				requestedVersion,
				installedVersion,
				path ?? Path.Combine(Configuration.ProcessDirectory, $"{assemblyName}.dll"));
			return true;
		}

		return false;
	}

	public static string DescribeInstallAssemblyFailure(InstallAssemblyFailure failure)
		=> failure.InstalledVersion is null
		? $"{failure.FileName} is missing from the install folder. This build of Libation needs version {failure.RequestedVersion}."
		: $"{failure.FileName} in the install folder is version {failure.InstalledVersion}, but this build of Libation needs version {failure.RequestedVersion}. The upgrade did not replace this file.";

	/// <summary>
	/// True only for a genuine assembly reference, which always carries a version. This keeps the check off
	/// the <see cref="FileNotFoundException"/>s that carry a plain file path, such as the one
	/// <see cref="ValidateEntityFrameworkCoreSqlitePresent"/> raises.
	/// </summary>
	private static bool TryParseAssemblyReference(string? fileName, out string? assemblyName, out Version? version)
	{
		assemblyName = null;
		version = null;

		if (string.IsNullOrWhiteSpace(fileName))
			return false;

		try
		{
			var parsed = new AssemblyName(fileName);
			if (string.IsNullOrWhiteSpace(parsed.Name) || parsed.Version is null)
				return false;

			assemblyName = parsed.Name;
			version = parsed.Version;
			return true;
		}
		catch (Exception)
		{
			return false;
		}
	}

	private static string? FindInstallAssemblyPath(string assemblyName)
	{
		foreach (var extension in new[] { ".dll", ".exe" })
		{
			var path = Path.Combine(Configuration.ProcessDirectory, assemblyName + extension);
			if (File.Exists(path))
				return path;
		}

		return null;
	}

	private static Version? TryReadAssemblyVersion(string path)
	{
		try
		{
			return AssemblyName.GetAssemblyName(path).Version;
		}
		catch (Exception)
		{
			// Not a managed assembly, or unreadable. Either way we cannot name a version.
			return null;
		}
	}

	public static FatalStartupMessage? GetStartupFailureMessage(Exception ex)
	{
		if (TryFindInvalidConfigurationValue(ex, out var configEx) && configEx is not null)
		{
			return new FatalStartupMessage(
				"Invalid Settings.json",
				configEx.Message
					+ Environment.NewLine
					+ Environment.NewLine
					+ "Edit Settings.json to use a valid value, then restart Libation.");
		}

		if (IsApplicationControlBlockedAssembly(ex))
		{
			return new FatalStartupMessage(
				"Libation blocked by Windows security",
				GetApplicationControlBlockedMessage(ex));
		}

		if (IsIncompleteUpgradeAssemblyFailure(ex))
		{
			return new FatalStartupMessage(
				"In-app upgrade failed",
				GetIncompleteUpgradeFailureMessage(ex));
		}

		if (IsMissingDependencyAssembly(ex))
		{
			return new FatalStartupMessage(
				"Library load failed",
				GetLibraryLoadFailureMessage());
		}

		// Last, so the checks above keep naming the specific cause they recognise.
		if (TryGetInstallAssemblyFailure(ex, out _))
		{
			return new FatalStartupMessage(
				"Libation could not load a required file",
				GetIncompleteUpgradeFailureMessage(ex));
		}

		return null;
	}

	public static bool TryFindInvalidConfigurationValue(Exception? ex, out InvalidConfigurationValueException? configEx)
	{
		configEx = null;
		for (var current = ex; current is not null; current = current.InnerException)
		{
			if (current is InvalidConfigurationValueException found)
			{
				configEx = found;
				return true;
			}

			if (current is AggregateException aggregate)
			{
				foreach (var inner in aggregate.InnerExceptions)
				{
					if (TryFindInvalidConfigurationValue(inner, out configEx))
						return true;
				}
			}
		}

		return false;
	}

	/// <summary>
	/// Resolves a user-facing title and body for a fatal startup or crash, including emergency rollback when needed.
	/// </summary>
	public static FatalStartupMessage GetFatalStartupMessage(Exception ex, FatalStartupMessage genericFallback)
	{
		// TryEmergencyRollback is a no-op without a backup folder, so it is safe to offer it to any
		// assembly failure that points at the install folder rather than only the named ones.
		if (IsIncompleteUpgradeAssemblyFailure(ex) || TryGetInstallAssemblyFailure(ex, out _))
		{
			var recovery = InstallUpgradeManager.TryEmergencyRollback(Configuration.ProcessDirectory);
			if (recovery.RolledBack)
			{
				return new FatalStartupMessage(
					recovery.Title,
					recovery.Message + Environment.NewLine + Environment.NewLine + "Please restart Libation.");
			}
		}

		return GetStartupFailureMessage(ex) ?? genericFallback;
	}

	public static string GetIncompleteUpgradeFailureMessage(Exception? ex = null)
	{
		// Naming the file and both versions turns an opaque loader message into something the user, and
		// anyone reading their bug report, can act on without guessing.
		var detail = TryGetInstallAssemblyFailure(ex, out var assemblyFailure) && assemblyFailure is not null
			? DescribeInstallAssemblyFailure(assemblyFailure)
			: ex?.Message;

		if (string.IsNullOrWhiteSpace(detail))
			detail = "(no additional detail)";

		return $"""
			Libation could not load a required file from its install folder. This usually means an in-app upgrade, or a zip extracted over an existing install, did not replace every file.

			Technical detail:
			{detail}

			Install folder:
			{Configuration.ProcessDirectory}
			{DescribeCloudSyncedInstall(CloudSyncedFolders.GetSyncStatus(Configuration.ProcessDirectory))}
			Your library database, accounts, and settings are stored separately and should be unaffected.

			To recover:
			1. Quit Libation completely.
			2. Download the latest release from GitHub. The setup.exe installer is the easiest option.
			3. If you use the zip instead, extract it to a new folder (do not copy files on top of the old install).
			4. Run Libation from the new install.

			More help:
			{TroubleshootIncompleteUpgradeUrl}
			""";
	}

	/// <summary>
	/// Blank unless the install sits in a cloud sync folder, where sync can undo part of an overlay
	/// upgrade on its own. Carries its own blank lines so the surrounding message reads the same either way.
	/// </summary>
	public static string DescribeCloudSyncedInstall(CloudSyncStatus syncStatus)
	{
		if (!syncStatus.IsSynced)
			return string.Empty;

		return $"{Environment.NewLine}This install is inside {syncStatus.Description}. Sync clients replace and restore files underneath Libation, which can undo part of an upgrade by itself. Install to an ordinary local folder instead.{Environment.NewLine}";
	}

	private static bool ContainsLibationUiBaseReference(string? text)
		=> !string.IsNullOrWhiteSpace(text)
		&& text.Contains("LibationUiBase", StringComparison.OrdinalIgnoreCase);

	public static bool IsMissingDependencyAssembly(Exception ex)
	{
		for (var current = ex; current is not null; current = current.InnerException)
		{
			if (current is not FileNotFoundException and not FileLoadException)
				continue;

			var name = (current as FileNotFoundException)?.FileName ?? current.Message;
			if (name.Contains("EntityFrameworkCore", StringComparison.OrdinalIgnoreCase)
				|| name.Contains("Microsoft.Data.Sqlite", StringComparison.OrdinalIgnoreCase))
				return true;
		}

		return false;
	}

	private static string? TryGetBlockedAssemblyPath(Exception? ex)
	{
		for (var current = ex; current is not null; current = current.InnerException)
		{
			if (current is FileLoadException { FileName: { Length: > 0 } blockedPath })
				return blockedPath;

			if (current is FileNotFoundException { FileName: { Length: > 0 } missingPath })
				return missingPath;
		}

		return null;
	}

	private static void ValidateEntityFrameworkCoreSqlitePresent()
	{
		var path = Path.Combine(Configuration.ProcessDirectory, EntityFrameworkCoreSqliteAssemblyFileName);
		if (File.Exists(path))
			return;

		throw new FileNotFoundException(
			$"Required file '{EntityFrameworkCoreSqliteAssemblyFileName}' was not found in the Libation install folder.{Environment.NewLine}{Environment.NewLine}{GetLibraryLoadFailureMessage()}",
			path);
	}
}
