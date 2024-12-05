using System;
using System.Diagnostics;
using System.IO;
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
		[STAThread]
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
			AppDomain.CurrentDomain.UnhandledException += (o, e) => LogError(e.ExceptionObject);

			bool loggingEnabled = false;
			//***********************************************//
			//                                               //
			//   do not use Configuration before this line   //
			//                                               //
			//***********************************************//
			// Migrations which must occur before configuration is loaded for the first time. Usually ones which alter the Configuration
			try
			{
				var config = LibationScaffolding.RunPreConfigMigrations();
				if (config.LibationSettingsAreValid)
				{
					// most migrations go in here
					LibationScaffolding.RunPostConfigMigrations(config);
					LibationScaffolding.RunPostMigrationScaffolding(Variety.Chardonnay, config);
					loggingEnabled = true;

					//Start loading the library before loading the main form
					App.LibraryTask = Task.Run(() => DbContexts.GetLibrary_Flat_NoTracking(includeParents: true));
				}

				BuildAvaloniaApp().StartWithClassicDesktopLifetime(null);
			}
			catch (Exception ex)
			{
				if (loggingEnabled)
					Serilog.Log.Logger.Error(ex, "CRASH");
				else
					LogError(ex);
			}
		}

		public static AppBuilder BuildAvaloniaApp()
			=> AppBuilder.Configure<App>()
			.UsePlatformDetect()
			.LogToTrace()
			.UseReactiveUI();

		private static void LogError(object exceptionObject)
		{
			var logError = $"""
				{DateTime.Now} - Libation Crash
				 OS                    {Configuration.OS}
				 Version               {LibationScaffolding.BuildVersion}
				 ReleaseIdentifier     {LibationScaffolding.ReleaseIdentifier}
				 InteropFunctionsType  {InteropFactory.InteropFunctionsType}
				 LibationFiles         {getConfigValue(c => c.LibationFiles)}
				 Books Folder          {getConfigValue(c => c.Books)}
				 === EXCEPTION ===
				 {exceptionObject}
				""";

			var crashLog = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "LibationCrash.log");

			using var sw = new StreamWriter(crashLog, true);
			sw.WriteLine(logError);

			static string getConfigValue(Func<Configuration, string> selector)
			{
				try
				{
					return selector(Configuration.Instance);
				}
				catch (Exception ex)
				{
					return ex.ToString();
				}
			}
		}
	}
}
