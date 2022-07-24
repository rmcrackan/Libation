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
		static void Main()
		{
			var sw = System.Diagnostics.Stopwatch.StartNew();
			var config = LoadLibationConfig();

			if (config is null) return;

			//Start as much work in parallel as possible.
			var runDbMigrationsTask = Task.Run(() => RunDbMigrations(config));
			var classicLifetimeTask = Task.Run(() => new ClassicDesktopStyleApplicationLifetime());
			var appBuilderTask = Task.Run(BuildAvaloniaApp);

			if (!runDbMigrationsTask.GetAwaiter().GetResult())
				return;

			var dbLibraryTask = Task.Run(() => DbContexts.GetLibrary_Flat_NoTracking(includeParents: true));

			appBuilderTask.GetAwaiter().GetResult().SetupWithLifetime(classicLifetimeTask.GetAwaiter().GetResult());

			var form1 = (AvaloniaUI.Views.MainWindow)classicLifetimeTask.Result.MainWindow;

			form1.OnLibraryLoaded(dbLibraryTask.GetAwaiter().GetResult());

			var assets = AvaloniaLocator.Current.GetService<IAssetLoader>();

			classicLifetimeTask.Result.Start(null);
		}

		public static AppBuilder BuildAvaloniaApp()
			=> AppBuilder.Configure<AvaloniaUI.App>()
			.UsePlatformDetect()
			.LogToTrace()
			.UseReactiveUI();

		private static Configuration LoadLibationConfig()
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
				return config;
			}
			catch (Exception ex)
			{

				return null;
			}
		}

		private static bool RunDbMigrations(Configuration config)
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
