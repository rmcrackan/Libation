using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using ApplicationServices;
using AppScaffolding;
using Avalonia;
using Avalonia.Controls;
using ReactiveUI.Avalonia;
using LibationFileManager;
using LibationAvalonia.Dialogs;
using Avalonia.Threading;
using FileManager;
using System.Linq;

namespace LibationAvalonia
{
	static class Program
	{
		private static System.Threading.Lock SetupLock { get; } = new();
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
			AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

			//***********************************************//
			//                                               //
			//   do not use Configuration before this line   //
			//                                               //
			//***********************************************//
			// Migrations which must occur before configuration is loaded for the first time. Usually ones which alter the Configuration
			try
			{
				var config = LibationScaffolding.RunPreConfigMigrations();
				if (config.LibationFiles.SettingsAreValid)
				{
					// most migrations go in here
					LibationScaffolding.RunPostConfigMigrations(config);
					LibationScaffolding.RunPostMigrationScaffolding(Variety.Chardonnay, config);

					//Start loading the library before loading the main form
					App.LibraryTask = Task.Run(() => DbContexts.GetLibrary_Flat_NoTracking(includeParents: true));
				}
				BuildAvaloniaApp()?.StartWithClassicDesktopLifetime([], ShutdownMode.OnExplicitShutdown);
			}
			catch (Exception ex)
			{
				if (new StackTrace(ex).GetFrames().Any(f => f.GetMethod()?.DeclaringType?.Assembly == typeof(NativeWebDialog).Assembly))
				{
					//Many of the NativeWebDialog exceptions cannot be handled by user code,
					//so a webview failure is a fatal error. Disable webview usage and rely
					//on the external browser login method instead.
					Configuration.Instance.UseWebView = false;
				}
				LogAndShowCrashMessage(ex);
			}
		}

		public static AppBuilder? BuildAvaloniaApp()
		{
			//Ensure that setup is only run once
			SetupLock.Enter();
			if (Application.Current is not null)
			{
				SetupLock.Exit();
				return null;
			}
			else
			{
				return AppBuilder.Configure<App>()
					.UsePlatformDetect()
					.LogToTrace()
					.UseReactiveUI()
					.AfterSetup(_ => SetupLock.Exit());
			}
		}

		private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			var ex = e.ExceptionObject as Exception ?? new Exception(e.ExceptionObject.ToString());
			LogAndShowCrashMessage(ex);
		}

		private static void LogAndShowCrashMessage(Exception exception)
		{
			try
			{
				//Try to log the error message before displaying the crash dialog
				if (Configuration.Instance.SerilogInitialized)
					Serilog.Log.Logger.Error(exception, "CRASH");
				else
					LogErrorWithoutSerilog(exception);
			}
			catch { /* continue to show the crash dialog even if logging fails */ }

			//Run setup if needed so that we can show the crash dialog
			BuildAvaloniaApp()?.SetupWithoutStarting();

			try
			{
				Dispatcher.UIThread.Invoke(() => DisplayErrorMessage(exception));
			}
			catch (Exception ex)
			{
				Environment.FailFast("Fatal error displaying crash message", new AggregateException(ex, exception));
			}
		}

		private static void DisplayErrorMessage(Exception exception)
		{
			var dispatcher = new DispatcherFrame();

			var mbAlert = new MessageBoxAlertAdminDialog("""
				Libation encountered a fatal error and must close.

				Please consider reporting this issue on GitHub, including the contents of the LibationCrash.log file created in your user folder.
				""",
				"Libation Crash",
				exception);
			mbAlert.Closed += (_, _) => dispatcher.Continue = false;
			mbAlert.Show();
			Dispatcher.UIThread.PushFrame(dispatcher);
		}

		private static void LogErrorWithoutSerilog(object exceptionObject)
		{
			var logError = $"""
			{DateTime.Now} - Libation Crash
			 OS                    {Configuration.OS}
			 Version               {LibationScaffolding.BuildVersion}
			 ReleaseIdentifier     {LibationScaffolding.ReleaseIdentifier}
			 InteropFunctionsType  {InteropFactory.InteropFunctionsType}
			 LibationFiles         {getConfigValue(c => c.LibationFiles.Location)}
			 Books Folder          {getConfigValue(c => c.Books)}
			 === EXCEPTION ===
			 {exceptionObject}
			""";

			LongPath logFile;
			try
			{
				//Try to add crash message to the newest existing Libation log file
				//then to LibationFiles/LibationCrash.log
				//then to %UserProfile%/LibationCrash.log
				string logDir = Configuration.Instance.LibationFiles.Location;
				var existingLogFiles = Directory.GetFiles(logDir, "Log*.log");

				logFile = existingLogFiles.Length == 0 ? getFallbackLogFile()
					: existingLogFiles.Select(f => new FileInfo(f)).OrderByDescending(f => f.CreationTimeUtc).First().FullName;
			}
			catch
			{
				logFile = getFallbackLogFile();
			}


			using var sw = new StreamWriter(logFile, true);
			sw.WriteLine(logError);

			static string getConfigValue(Func<Configuration, string?> selector)
			{
				try
				{
					return selector(Configuration.Instance) ?? "[null]";
				}
				catch (Exception ex)
				{
					return ex.ToString();
				}
			}

			static string getFallbackLogFile()
			{
				try
				{

					string logDir = Configuration.Instance.LibationFiles.Location;
					if (!Directory.Exists(logDir))
						logDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

					return Path.Combine(logDir, "LibationCrash.log");
				}
				catch
				{
					return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "LibationCrash.log");
				}
			}
		}
	}
}
