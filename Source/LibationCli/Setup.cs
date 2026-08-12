using AppScaffolding;
using CommandLine;
using LibationFileManager;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace LibationCli;

public static class Setup
{
	public static void Initialize()
	{
		//***********************************************//
		//                                               //
		//   do not use Configuration before this line   //
		//                                               //
		//***********************************************//
		var config = LibationScaffolding.RunPreConfigMigrations();

		if (!Directory.Exists(config.LibationFiles.Location))
		{
			Console.Error.WriteLine($"Cannot find LibationFiles at {config.LibationFiles.Location}");
			PrintLibationFilestipAndExit();
		}

		if (!File.Exists(config.LibationFiles.SettingsFilePath))
		{
			Console.Error.WriteLine($"Cannot find settings files at {config.LibationFiles.SettingsFilePath}");
			PrintLibationFilestipAndExit();
		}

		LibationScaffolding.RunPostConfigMigrations(config, ephemeralSettings: true);

#if classic
		LibationScaffolding.RunPostMigrationScaffolding(Variety.Classic, config);
#else
		LibationScaffolding.RunPostMigrationScaffolding(Variety.Chardonnay, config);
#endif

		// The CLI remains intentionally unrestricted for scripting and parallel use. However, if a
		// GUI owns this LibationFiles folder's single-instance lock, record an advisory warning so
		// concurrent access is visible when diagnosing database/search-index issues.
		using var guiInstanceCheck = SingleInstance.TryAcquire(config.LibationFiles.Location);
		if (!guiInstanceCheck.IsFirstInstance)
		{
			Serilog.Log.Logger.Warning(
				"Another Libation GUI instance is running against this LibationFiles folder. "
				+ "The CLI command will continue, but concurrent writes may conflict.");
		}
	}

	static void PrintLibationFilestipAndExit()
	{
		Console.Error.WriteLine($"Override LibationFiles directory location with '--libationFiles' option or '{LibationFiles.LIBATION_FILES_DIR}' environment variable.");
		Environment.Exit(-1);
	}

	public static Type[] LoadVerbs() => Assembly.GetExecutingAssembly()
		.GetTypes()
		.Where(t => t.GetCustomAttribute<VerbAttribute>() is not null)
		.ToArray();
}
