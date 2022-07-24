using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ApplicationServices;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.ReactiveUI;
using LibationFileManager;

namespace LibationAvalonia
{
	static class Program
	{
		private static string EXE_DIR = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

		static async Task Main()
		{
			//***********************************************//
			//                                               //
			//   do not use Configuration before this line   //
			//                                               //
			//***********************************************//
			// Migrations which must occur before configuration is loaded for the first time. Usually ones which alter the Configuration
			var config = AppScaffolding.LibationScaffolding.RunPreConfigMigrations();

			App.SetupRequired = !config.LibationSettingsAreValid;

			//Start as much work in parallel as possible.
			var classicLifetimeTask = Task.Run(() => new ClassicDesktopStyleApplicationLifetime());
			var appBuilderTask = Task.Run(BuildAvaloniaApp);


			if (!App.SetupRequired)
			{
				if (!RunDbMigrations(config))
					return;

				App.LibraryTask = Task.Run(() => DbContexts.GetLibrary_Flat_NoTracking(includeParents: true));
			}



			(await appBuilderTask).SetupWithLifetime(await classicLifetimeTask);

			classicLifetimeTask.Result.Start(null);
		}

		public static AppBuilder BuildAvaloniaApp()
			=> AppBuilder.Configure<App>()
			.UsePlatformDetect()
			.LogToTrace()
			.UseReactiveUI();
		public static AppBuilder BuildAvaloniaAppBasic()
			=> AppBuilder.Configure<AppBasic>()
			.UsePlatformDetect()
			.LogToTrace();

		public static bool RunDbMigrations(Configuration config)
		{
			try
			{
				// most migrations go in here
				AppScaffolding.LibationScaffolding.RunPostConfigMigrations(config);
				AppScaffolding.LibationScaffolding.RunPostMigrationScaffolding(config);

				return true;
			}
			catch (Exception ex)
			{
				return false;
			}
		}
	}
}
