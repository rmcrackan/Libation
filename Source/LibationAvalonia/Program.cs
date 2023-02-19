using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ApplicationServices;
using AppScaffolding;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.ReactiveUI;
using LibationFileManager;

namespace LibationAvalonia
{
	static class Program
	{
		static void Main(string[] args)
		{

			if (Configuration.IsMacOs && args?.Length > 0 && args[0] == "hangover")
			{
				//Launch the Hangover app within the sandbox
				//We can do this because we're already executing inside the sandbox.
				//Any process created in the sandbox executes in the same sandbox.
				//Unfortunately, all sandbox files are read/execute, so no writing!
				Process.Start("Hangover");
				return;
			}
			if (Configuration.IsMacOs && args?.Length > 0 && args[0] == "cli")
			{
				//Open a new Terminal in the sandbox
				Process.Start(
					"/System/Applications/Utilities/Terminal.app/Contents/MacOS/Terminal",
					$"\"{Configuration.ProcessDirectory}\"");
				return;
			}

			//***********************************************//
			//                                               //
			//   do not use Configuration before this line   //
			//                                               //
			//***********************************************//
			// Migrations which must occur before configuration is loaded for the first time. Usually ones which alter the Configuration
			var config = LibationScaffolding.RunPreConfigMigrations();

			App.SetupRequired = !config.LibationSettingsAreValid;

			//Start as much work in parallel as possible.
			var classicLifetimeTask = Task.Run(() => new ClassicDesktopStyleApplicationLifetime());
			var appBuilderTask = Task.Run(BuildAvaloniaApp);

			LibationScaffolding.SetReleaseIdentifier(Variety.Chardonnay);

			if (LibationScaffolding.ReleaseIdentifier is ReleaseIdentifier.None)
				return;


			if (!App.SetupRequired)
			{
				if (!RunDbMigrations(config))
					return;

				App.LibraryTask = Task.Run(() => DbContexts.GetLibrary_Flat_NoTracking(includeParents: true));
			}

			(appBuilderTask.GetAwaiter().GetResult()).SetupWithLifetime(classicLifetimeTask.GetAwaiter().GetResult());

			classicLifetimeTask.Result.Start(null);
		}

		public static AppBuilder BuildAvaloniaApp()
			=> AppBuilder.Configure<App>()
			.UsePlatformDetect()
			.LogToTrace()
			.UseReactiveUI();

		public static bool RunDbMigrations(Configuration config)
		{
			try
			{
				// most migrations go in here
				LibationScaffolding.RunPostConfigMigrations(config);
				LibationScaffolding.RunPostMigrationScaffolding(config);

				return true;
			}
			catch (Exception exDebug)
			{
				Serilog.Log.Logger.Debug(exDebug, "Silent failure");
				return false;
			}
		}
	}
}
