using ApplicationServices;
using AppScaffolding;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Avalonia.Platform;
using Avalonia.Styling;
using Avalonia.Threading;
using Dinah.Core;
using LibationAvalonia.Dialogs;
using LibationAvalonia.Themes;
using LibationAvalonia.Views;
using LibationFileManager;
using LibationUiBase;
using LibationUiBase.Forms;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace LibationAvalonia;

public class App : Application
{
	public static Task<List<DataLayer.LibraryBook>>? LibraryTask { get; set; }
	public static ChardonnayTheme? DefaultThemeColors { get; private set; }
	public static MainWindow? MainWindow { get; private set; }
	public static Uri AssetUriBase { get; } = new("avares://Libation/Assets/");
	public static new Application Current => Application.Current ?? throw new InvalidOperationException("The Avalonia app hasn't started yet.");

	public static Stream OpenAsset(string assetRelativePath)
		=> AssetLoader.Open(new Uri(AssetUriBase, assetRelativePath));

	public override void Initialize() => AvaloniaXamlLoader.Load(this);

	public override void OnFrameworkInitializationCompleted()
	{
		DefaultThemeColors = ChardonnayTheme.GetLiveTheme();

		if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
		{
			// Chardonnay uses the OnExplicitShutdown shutdown mode. The application will stay alive until
			// Shutdown() is called on App.Current.ApplicationLifetime.
			MessageBoxBase.ShowAsyncImpl = (owner, message, caption, buttons, icon, defaultButton, saveAndRestorePosition) =>
				MessageBox.Show(owner as Window, message, caption, buttons, icon, defaultButton, saveAndRestorePosition);

			if (InstallUpgradeManager.TakeStartupRecoveryAlert() is { } recovery)
				_ = MessageBox.Show(null, recovery.Body, recovery.Title, MessageBoxButtons.OK, MessageBoxIcon.Warning);

			BadBookActionDialogBase.ShowAsyncImpl = (owner, message, caption) =>
				Dialogs.BadBookActionDialog.ShowAsync(owner as Window, message, caption);

			if (LibraryTask is null)
			{
				RunSetupIfNeededAsync(desktop, Configuration.Instance);
			}
			else
			{
				//LibraryTask was already started early in Program.Main(),
				//which means config is valid and migrations have already run.
				ShowMainWindow(desktop);
			}
		}

		base.OnFrameworkInitializationCompleted();
	}

	private static async void RunSetupIfNeededAsync(IClassicDesktopStyleApplicationLifetime desktop, Configuration config)
	{
		var setup = new LibationSetup(config.LibationFiles)
		{
			SetupPromptAsync = () => ShowSetupAsync(desktop),
			SelectFolderPromptAsync = () => SelectInstallLocation(desktop, config.LibationFiles)
		};
		if (await setup.RunSetupIfNeededAsync())
		{
			// setup succeeded or wasn't needed and LibationFiles are valid
			RunMigrations(config);
			StartupAssemblyBootstrap.PrepareForBackgroundDataAccess();
			LibraryTask = Task.Run(() => DbContexts.GetLibrary_Flat_NoTracking(includeParents: true));
			ShowMainWindow(desktop);
		}
		else
		{
			await MessageBox.Show("Initial set up cancelled.", "Cancelled", MessageBoxButtons.OK, MessageBoxIcon.Warning);
			desktop.Shutdown(-1);
		}
	}

	static async Task<ILibationSetup> ShowSetupAsync(IClassicDesktopStyleApplicationLifetime desktop)
	{
		var tcs = new TaskCompletionSource<ILibationSetup>();
		var setupDialog = new SetupDialog();
		desktop.MainWindow = setupDialog;
		setupDialog.Closed += (_, _) => tcs.SetResult(setupDialog);
		setupDialog.Show();
		return await tcs.Task;
	}

	static async Task<ILibationInstallLocation?> SelectInstallLocation(IClassicDesktopStyleApplicationLifetime desktop, LibationFiles libationFiles)
	{
		var tcs = new TaskCompletionSource<ILibationInstallLocation>();
		var libationFilesDialog = new LibationFilesDialog(libationFiles.Location.PathWithoutPrefix);
		desktop.MainWindow = libationFilesDialog;
		libationFilesDialog.Closed += (_, _) => tcs.SetResult(libationFilesDialog);
		libationFilesDialog.Show();
		return await tcs.Task;
	}

	public static void RunMigrations(Configuration config)
	{
		// most migrations go in here
		LibationScaffolding.RunPostConfigMigrations(config);
		// logging is init'd here
		LibationScaffolding.RunPostMigrationScaffolding(Variety.Chardonnay, config);
	}

	private static void ShowMainWindow(IClassicDesktopStyleApplicationLifetime desktop)
	{
		Configuration.Instance.PropertyChanged += ThemeVariant_PropertyChanged;
		Current.ActualThemeVariantChanged += OnActualThemeVariantChanged;
		OnActualThemeVariantChanged(Current, EventArgs.Empty);

		MainWindow mainWindow = new();
		desktop.MainWindow = MainWindow = mainWindow;
		mainWindow.Loaded += MainWindow_Loaded;
		mainWindow.Closed += (_, _) => desktop.Shutdown();
		mainWindow.RestoreSizeAndLocation(Configuration.Instance);
		mainWindow.Show();
	}

	[PropertyChangeFilter(nameof(ThemeVariant))]
	private static void ThemeVariant_PropertyChanged(object sender, PropertyChangedEventArgsEx e)
		=> OpenAndApplyTheme(e.NewValue as Configuration.Theme? ?? Configuration.Theme.System);

	private static void OnActualThemeVariantChanged(object? sender, EventArgs e)
		=> OpenAndApplyTheme(Configuration.Instance.ThemeVariant);

	private static void OpenAndApplyTheme(Configuration.Theme themeVariant)
	{
		using ChardonnayThemePersister? themePersister = ChardonnayThemePersister.Create();
		themePersister?.Target.ApplyTheme(themeVariant);
	}

	private static async void MainWindow_Loaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
	{
		// This is an async void handler: any exception that escapes it becomes an unhandled
		// exception and crashes Libation. Every path below is caught so that a failure to load
		// or bind the library degrades to an empty, still-usable grid instead. See issue #1931.
		if (LibraryTask is null || MainWindow is not { } mainWindow)
			return;

		try
		{
			List<DataLayer.LibraryBook> library = await LibraryTask;
			await Dispatcher.UIThread.InvokeAsync(() => mainWindow.OnLibraryLoadedAsync(library));
		}
		catch (Exception ex) when (StartupAssemblyBootstrap.IsInstallFolderAssemblyLoadFailure(ex))
		{
			Serilog.Log.Logger.Error(ex, "Failed to load library at startup");
			FatalStartupMessage failure = StartupAssemblyBootstrap.GetStartupFailureMessage(ex)!;
			await MessageBox.Show(
				mainWindow,
				failure.Body,
				failure.Title,
				MessageBoxButtons.OK,
				MessageBoxIcon.Error);
			await showEmptyLibraryAsync();
		}
		catch (Exception ex)
		{
			// Any other failure loading or binding the library must not take down the whole app.
			// Log it, tell the user, and continue with an empty grid so Settings, re-scan, etc.
			// remain reachable.
			Serilog.Log.Logger.Error(ex, "Failed to load library at startup");
			await MessageBox.Show(
				mainWindow,
				"Libation could not load your library, so the library view will be empty for this session. "
					+ "Restarting Libation may resolve it. If this keeps happening, your library database or your "
					+ "computer's memory or disk may be corrupted.",
				"Error loading library",
				MessageBoxButtons.OK,
				MessageBoxIcon.Error);
			await showEmptyLibraryAsync();
		}

		async Task showEmptyLibraryAsync()
		{
			try
			{
				await Dispatcher.UIThread.InvokeAsync(() => mainWindow.OnLibraryLoadedAsync([]));
			}
			catch (Exception ex)
			{
				Serilog.Log.Logger.Error(ex, "Failed to show empty library after a library load failure");
			}
		}
	}
}
