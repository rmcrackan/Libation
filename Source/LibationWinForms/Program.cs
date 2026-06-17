using ApplicationServices;
using AppScaffolding;
using DataLayer;
using LibationFileManager;
using LibationUiBase;
using LibationWinForms.Dialogs;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LibationWinForms;

static class Program
{
	[System.Runtime.InteropServices.DllImport("kernel32.dll", SetLastError = true)]
	[return: System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.Bool)]
	static extern bool AllocConsole();

	private static Form1? form1;

	[STAThread]
	static void Main()
	{
		Task<List<LibraryBook>> libraryLoadTask;

		try
		{
			//// Uncomment to see Console. Must be called before anything writes to Console.
			//// Only use while debugging. Acts erratically in the wild
			//AllocConsole();

			// run as early as possible. see notes in postLoggingGlobalExceptionHandling
			Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);

			ApplicationConfiguration.Initialize();

			// When essential file validation fails and the error cannot be written to the log, show the user
			EssentialFileValidator.ShowUserWhenLogUnavailable = msg => MessageBox.Show(msg, "Libation - Essential File Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);

			//***********************************************//
			//                                               //
			//   do not use Configuration before this line   //
			//                                               //
			//***********************************************//
			// Migrations which must occur before configuration is loaded for the first time. Usually ones which alter the Configuration
			var config = AppScaffolding.LibationScaffolding.RunPreConfigMigrations();
			StartupAssemblyBootstrap.RecoverFromIncompleteUpgradeIfNeeded();
			LibationUiBase.Forms.MessageBoxBase.ShowAsyncImpl = ShowMessageBox;
			BadBookActionDialogBase.ShowAsyncImpl = ShowBadBookActionDialog;

			// do this as soon as possible (post-config)
			RunSetupIfNeededAsync(config);

			// most migrations go in here
			LibationScaffolding.RunPostConfigMigrations(config);
			SetThemeColor(config);

			// migrations which require Forms or are long-running
			RunWindowsOnlyMigrations(config);

			MessageBoxLib.VerboseLoggingWarning_ShowIfTrue();

			// logging is init'd here (also initializes InteropFactory via logStartupState)
			LibationScaffolding.RunPostMigrationScaffolding(Variety.Classic, config);

			//*******************************************************************//
			//                                                                   //
			//  Start loading the library as soon as possible after logging    //
			//  and InteropFactory assembly resolution are ready.                //
			//                                                                   //
			//*******************************************************************//
			StartupAssemblyBootstrap.PrepareForBackgroundDataAccess();
			libraryLoadTask = Task.Run(() => DbContexts.GetLibrary_Flat_NoTracking(includeParents: true));
		}
		catch (Exception ex)
		{
			if (Configuration.Instance.SerilogInitialized)
				Log.Error(ex, "Fatal error during startup");

			var fatalMessage = StartupAssemblyBootstrap.GetFatalStartupMessage(
				ex,
				new FatalStartupMessage(
					"Fatal error, pre-logging",
					"An unrecoverable error occurred. Since this error happened before logging could be initialized, this error can not be written to the log file."));

			try
			{
				MessageBoxLib.ShowAdminAlert(null, fatalMessage.Body, fatalMessage.Title, ex);
			}
			catch
			{
				MessageBox.Show($"{fatalMessage.Body}\r\n\r\n{ex.Message}\r\n\r\n{ex.StackTrace}", fatalMessage.Title, MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			return;
		}

		// global exception handling (ShowAdminAlert) attempts to use logging. only call it after logging has been init'd
		postLoggingGlobalExceptionHandling();

		form1 = new Form1();
		form1.Load += async (_, _) => await LoadLibraryIntoFormAsync(form1, libraryLoadTask);
		Application.Run(form1);
	}

	#region Message Box Handler for LibationUiBase
	static Task<LibationUiBase.Forms.DialogResult> ShowMessageBox(
		object? owner,
		string message,
		string caption,
		LibationUiBase.Forms.MessageBoxButtons buttons,
		LibationUiBase.Forms.MessageBoxIcon icon,
		LibationUiBase.Forms.MessageBoxDefaultButton defaultButton,
		bool _)
	{
		Func<DialogResult> showMessageBox = () => MessageBox.Show(
				owner as IWin32Window ?? form1,
				message,
				caption,
				(MessageBoxButtons)buttons,
				(MessageBoxIcon)icon,
				(MessageBoxDefaultButton)defaultButton);


		var result = form1 is null ? showMessageBox() : form1.Invoke(showMessageBox);
		return Task.FromResult((LibationUiBase.Forms.DialogResult)result);
	}
	#endregion;

	#region Bad Book Action Dialog Handler for LibationUiBase
	static Task<BadBookDialogResult> ShowBadBookActionDialog(object? owner, string message, string caption)
	{
		Func<BadBookDialogResult> showDialog = () =>
		{
			BadBookDialogResult result = new(LibationUiBase.Forms.DialogResult.Retry, false, false);
			using var dialog = new BadBookActionDialog(message, caption);
			dialog.ShowDialog(owner as IWin32Window ?? form1);
			return dialog.Result;
		};

		var result = form1 is null ? showDialog() : form1.Invoke(showDialog);
		return Task.FromResult(result);
	}
	#endregion;

	private static void SetThemeColor(Configuration config)
	{
		var theme = config.ThemeVariant switch
		{
			Configuration.Theme.Light => SystemColorMode.Classic,
			Configuration.Theme.Dark => SystemColorMode.Dark,
			_ => SystemColorMode.System,
		};

		Application.SetColorMode(theme);
	}

	private static void RunSetupIfNeededAsync(Configuration config)
	{
		var setup = new LibationSetup(config.LibationFiles)
		{
			SetupPrompt = ShowSetup,
			SelectFolderPrompt = SelectInstallLocation
		};

		if (!setup.RunSetupIfNeededAsync().GetAwaiter().GetResult())
		{
			MessageBox.Show("Initial set up cancelled.", "Cancelled", MessageBoxButtons.OK, MessageBoxIcon.Warning);
			Application.Exit();
			Environment.Exit(-1);
		}

		static ILibationSetup ShowSetup()
		{
			var setupDialog = new SetupDialog();
			setupDialog.ShowDialog();
			return setupDialog;
		}

		static ILibationInstallLocation? SelectInstallLocation()
		{
			var libationFilesDialog = new LibationFilesDialog();
			return libationFilesDialog.ShowDialog() is DialogResult.OK ? libationFilesDialog : null;
		}
	}

	/// <summary>migrations which require Forms or are long-running</summary>
	private static void RunWindowsOnlyMigrations(Configuration config)
	{
		// examples:
		// - only supported in winforms. don't move to app scaffolding
		// - long running. won't get a chance to finish in cli. don't move to app scaffolding

		const string hasMigratedKey = "hasMigratedToHighDPI";
		if (!config.GetNonString(defaultValue: false, hasMigratedKey))
		{
			config.RemoveProperty(nameof(config.GridColumnsWidths));

			foreach (var form in typeof(Program).Assembly.GetTypes().Where(t => t.IsSubclassOf(typeof(Form))))
				config.RemoveProperty(form.Name);

			config.SetNonString(true, hasMigratedKey);
		}
	}

	private static async Task LoadLibraryIntoFormAsync(Form1 form, Task<List<LibraryBook>> libraryLoadTask)
	{
		try
		{
			await form.InitLibraryAsync(await libraryLoadTask);
		}
		catch (Exception ex) when (StartupAssemblyBootstrap.IsInstallFolderAssemblyLoadFailure(ex))
		{
			Log.Error(ex, "Failed to load library at startup");
			var failure = StartupAssemblyBootstrap.GetStartupFailureMessage(ex)!;
			MessageBoxLib.ShowAdminAlert(form, failure.Body, failure.Title, ex);
			await form.InitLibraryAsync([]);
		}
	}

	private static void postLoggingGlobalExceptionHandling()
	{
		// this line is all that's needed for strict handling
		AppDomain.CurrentDomain.UnhandledException += (_, e) => MessageBoxLib.ShowAdminAlert(null, "Libation has crashed due to an unhandled error.", "Application crash!", (Exception)e.ExceptionObject);

		// these 2 lines makes it graceful. sync (eg in main form's ctor) and thread exceptions will still crash us, but event (sync, void async, Task async) will not
		Application.ThreadException += (_, e) => MessageBoxLib.ShowAdminAlert(null, "Libation has encountered an unexpected error.", "Unexpected error", e.Exception);
		// move to beginning of execution. crashes app if this is called post-RunInstaller: System.InvalidOperationException: 'Thread exception mode cannot be changed once any Controls are created on the thread.'
		//// I never found a case where including made a difference. I think this enum is default and including it will override app user config file
		//Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
	}
}