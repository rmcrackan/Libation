using ApplicationServices;
using AppScaffolding;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using LibationAvalonia.Dialogs;
using LibationFileManager;
using LibationUiBase.Forms;
using ReactiveUI.Avalonia;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace LibationAvalonia;

static class Program
{
	private static System.Threading.Lock SetupLock { get; } = new();

	/// <summary>Held for the lifetime of the process to enforce a single instance per LibationFiles folder.</summary>
	private static SingleInstance? SingleInstanceLock { get; set; }
	[STAThread]
	static void Main(string[] args)
	{
		if (Configuration.IsMacOs && args?.Length > 0 && args[0] == "hangover")
		{
			//Launch the Hangover app within the sandbox
			//We can do this because we're already executing inside the sandbox.
			//Any process created in the sandbox executes in the same sandbox.
			//Unfortunately, all sandbox files are read/execute, so no writing!
			HangoverLauncher.Launch();
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

		// When essential file validation fails and the error cannot be written to the log, show the user
		EssentialFileValidator.ShowUserWhenLogUnavailable = msg => Dispatcher.UIThread.Post(() => _ = MessageBoxBase.Show(null, msg, "Libation - Essential File Error", MessageBoxButtons.OK, MessageBoxIcon.Warning));

		//***********************************************//
		//                                               //
		//   do not use Configuration before this line   //
		//                                               //
		//***********************************************//
		// Migrations which must occur before configuration is loaded for the first time. Usually ones which alter the Configuration
		try
		{
			var config = LibationScaffolding.RunPreConfigMigrations();

			// A rollback swaps install files out from under assemblies this process has already loaded, so
			// it must not go on to touch the database or open a window. App reports it and shuts down
			// instead. See issue #2001.
			App.StartupRecoveryNotice = StartupAssemblyBootstrap.RecoverFromIncompleteUpgradeIfNeeded();

			if (App.StartupRecoveryNotice is null)
			{
				// Prevent a second instance from racing on the same database, search index, and log file.
				// Hold the lock for the whole process; skip all database access when we are not the first
				// instance so the running copy's state is never touched. See issue #1931.
				SingleInstanceLock = SingleInstance.TryAcquire(config.LibationFiles.Location);
				App.IsAnotherInstanceRunning = !SingleInstanceLock.IsFirstInstance;

				if (SingleInstanceLock.IsFirstInstance && config.LibationFiles.SettingsAreValid)
				{
					App.RunMigrations(config);
					StartupAssemblyBootstrap.PrepareForBackgroundDataAccess();
					App.LibraryTask = Task.Run(() => DbContexts.GetLibrary_Flat_NoTracking(includeParents: true));
				}
			}

			BuildAvaloniaApp()?.StartWithClassicDesktopLifetime([], ShutdownMode.OnExplicitShutdown);

			// After the lifetime ends, so the new process starts into a folder this one has finished with
			// and does not race it for the single-instance lock.
			if (App.RestartRequested)
				InstallRelauncher.TryRelaunch();
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
				.UseReactiveUI(_ => { })
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
		string? crashLogFile = null;
		try
		{
			//Try to log the error message before displaying the crash dialog
			if (Configuration.Instance.SerilogInitialized)
				Serilog.Log.Logger.Error(exception, "CRASH");
			else
				crashLogFile = PreLoggingCrashLog.TryWrite(exception, [("ReleaseIdentifier", LibationScaffolding.ReleaseIdentifier.ToString())]);
		}
		catch { /* continue to show the crash dialog even if logging fails */ }

		//Run setup if needed so that we can show the crash dialog
		BuildAvaloniaApp()?.SetupWithoutStarting();

		try
		{
			Dispatcher.UIThread.Invoke(() => DisplayErrorMessage(exception, crashLogFile));
		}
		catch (Exception ex)
		{
			Environment.FailFast("Fatal error displaying crash message", new AggregateException(ex, exception));
		}
	}

	private static void DisplayErrorMessage(Exception exception, string? crashLogFile)
	{
		var dispatcher = new DispatcherFrame();

		var fatalMessage = StartupAssemblyBootstrap.GetFatalStartupMessage(
			exception,
			new FatalStartupMessage(
				"Libation Crash",
				$"""
				Libation encountered a fatal error and must close.

				{DescribeCrashLog(crashLogFile)}
				"""));

		var mbAlert = new MessageBoxAlertAdminDialog(fatalMessage.Body, fatalMessage.Title, exception);
		mbAlert.Closed += (_, _) => dispatcher.Continue = false;
		mbAlert.Show();
		Dispatcher.UIThread.PushFrame(dispatcher);
	}

	/// <summary>
	/// Names the file the crash was actually written to. This used to name LibationCrash.log
	/// unconditionally, which is not where the record goes when a Log*.log already exists, so reporters
	/// went looking for a file that was not there and attached nothing. See issue #2001.
	/// </summary>
	private static string DescribeCrashLog(string? crashLogFile)
		=> crashLogFile is null
		? "Please consider reporting this issue on GitHub. Libation could not write this error to a log file, so please include the text below."
		: $"""
			Please consider reporting this issue on GitHub, including the contents of this file:
			{crashLogFile}
			""";

}
