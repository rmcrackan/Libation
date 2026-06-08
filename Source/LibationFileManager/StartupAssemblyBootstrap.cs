using System;
using System.IO;

namespace LibationFileManager;

/// <summary>
/// Ensures OS interop assembly resolution and required dependency files are ready before background library load.
/// </summary>
public static class StartupAssemblyBootstrap
{
	public const string EntityFrameworkCoreSqliteAssemblyFileName = "Microsoft.EntityFrameworkCore.Sqlite.dll";

	/// <summary>
	/// Registers <see cref="InteropFactory"/> assembly resolution and verifies required install-folder assemblies exist.
	/// Call before <c>Task.Run</c> loads the library or opens the database on a thread-pool thread.
	/// </summary>
	public static void PrepareForBackgroundDataAccess()
	{
		_ = InteropFactory.InteropFunctionsType;
		ValidateEntityFrameworkCoreSqlitePresent();
		TrySyncWindowsInstallMetadata();
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

		Install folder:
		{Configuration.ProcessDirectory}

		Expected file:
		{Path.Combine(Configuration.ProcessDirectory, EntityFrameworkCoreSqliteAssemblyFileName)}
		""";

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
