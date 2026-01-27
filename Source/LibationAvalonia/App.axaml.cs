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

			// Avoid duplicate validations from both Avalonia and the CommunityToolkit. 
			// More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
			DisableAvaloniaDataAnnotationValidation();
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
			SetupPromptAsync =() => ShowSetupAsync(desktop),
			SelectFolderPromptAsync = () => SelectInstallLocation(desktop, config.LibationFiles)
		};
		if (await setup.RunSetupIfNeededAsync())
		{
			// setup succeeded or wasn't needed and LibationFiles are valid
			RunMigrations(config);
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

	private void DisableAvaloniaDataAnnotationValidation()
	{
		// Get an array of plugins to remove
		DataAnnotationsValidationPlugin[] dataValidationPluginsToRemove =
			BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

		// remove each entry found
		foreach (DataAnnotationsValidationPlugin? plugin in dataValidationPluginsToRemove)
		{
			BindingPlugins.DataValidators.Remove(plugin);
		}
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
		if (LibraryTask is not null && MainWindow is not null)
		{
			List<DataLayer.LibraryBook> library = await LibraryTask;
			await Dispatcher.UIThread.InvokeAsync(() => MainWindow.OnLibraryLoadedAsync(library));
		}
	}
}
