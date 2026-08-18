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
	private const string TroubleshootApplicationControlUrl = "https://getlibation.com/docs/advanced/troubleshoot#windows-smart-app-control-and-in-app-upgrades";
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
	public static void RecoverFromIncompleteUpgradeIfNeeded()
	{
		try
		{
			InstallUpgradeManager.RecoverPendingUpgradeIfNeeded(Configuration.ProcessDirectory);
		}
		catch (Exception ex)
		{
			Serilog.Log.Logger.Error(ex, "Failed while recovering from a pending in-app upgrade");
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
			Serilog.Log.Logger.Warning(ex, "Could not run install metadata sync at startup");
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

			To check whether this is Smart App Control, open Settings -> Privacy & Security -> Windows Security -> App & browser control -> Smart App Control settings. Only the On setting blocks anything; Evaluation observes without blocking.

			If it is On, you can turn it off, but be aware that Windows cannot turn it back on again without a reset or reinstall. If it is already off, the block comes from a policy set by whoever manages this PC.

			More help:
			{TroubleshootApplicationControlUrl}
			""";
	}

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
		|| IsIncompleteUpgradeAssemblyFailure(ex);

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
		if (IsIncompleteUpgradeAssemblyFailure(ex))
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
		var detail = ex?.Message;
		if (string.IsNullOrWhiteSpace(detail))
			detail = "(no additional detail)";

		return $"""
			Libation could not load a required component after an in-app upgrade. This usually means the upgrade overlay did not replace every install file.

			Technical detail:
			{detail}

			Install folder:
			{Configuration.ProcessDirectory}

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
