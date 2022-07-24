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
			//Start as much work in parallel as possible.
			var runDbMigrationsTask = Task.Run(() => RunDbMigrations());
			var classicLifetimeTask = Task.Run(() => new ClassicDesktopStyleApplicationLifetime());
			var appBuilderTask = Task.Run(BuildAvaloniaApp);

			if (!await runDbMigrationsTask)
				return;

			var dbLibraryTask = Task.Run(() => DbContexts.GetLibrary_Flat_NoTracking(includeParents: true));

			(await appBuilderTask).SetupWithLifetime(await classicLifetimeTask);

			var form1 = (Views.MainWindow)classicLifetimeTask.Result.MainWindow;

			form1.OnLibraryLoaded(await dbLibraryTask);

			var assets = AvaloniaLocator.Current.GetService<IAssetLoader>();

			classicLifetimeTask.Result.Start(null);
		}

		public static AppBuilder BuildAvaloniaApp()
			=> AppBuilder.Configure<App>()
			.UsePlatformDetect()
			.LogToTrace()
			.UseReactiveUI();

		private static bool RunDbMigrations()
		{
			try
			{
				//***********************************************//
				//                                               //
				//   do not use Configuration before this line   //
				//                                               //
				//***********************************************//
				// Migrations which must occur before configuration is loaded for the first time. Usually ones which alter the Configuration
				var config = AppScaffolding.LibationScaffolding.RunPreConfigMigrations();
				AudibleUtilities.AudibleApiStorage.EnsureAccountsSettingsFileExists();

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
