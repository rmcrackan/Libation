using ApplicationServices;
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
using LibationUiBase.Forms;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

#nullable enable
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
			// Chardonnay uses the OnLastWindowClose shutdown mode. As long as the application lifetime
			// has one active window, the application will stay alive. Setup windows must be daisy chained,
			// each closing windows opens the next window before closing itself to prevent the app from exiting.

			MessageBoxBase.ShowAsyncImpl = (owner, message, caption, buttons, icon, defaultButton, saveAndRestorePosition) =>
				MessageBox.Show(owner as Window, message, caption, buttons, icon, defaultButton, saveAndRestorePosition);

			// Avoid duplicate validations from both Avalonia and the CommunityToolkit. 
			// More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
			DisableAvaloniaDataAnnotationValidation();

			Configuration config = Configuration.Instance;

			if (!config.LibationSettingsAreValid)
			{
				string defaultLibationFilesDir = Configuration.DefaultLibationFilesDirectory;

				// check for existing settings in default location
				string defaultSettingsFile = Path.Combine(defaultLibationFilesDir, "Settings.json");
				if (Configuration.SettingsFileIsValid(defaultSettingsFile))
					Configuration.SetLibationFiles(defaultLibationFilesDir);

				if (config.LibationSettingsAreValid)
				{
					LibraryTask = Task.Run(() => DbContexts.GetLibrary_Flat_NoTracking(includeParents: true));
					ShowMainWindow(desktop);
				}
				else
				{
					SetupDialog setupDialog = new() { Config = config };
					setupDialog.Closing += (_, e) => SetupClosing(setupDialog, desktop, e);
					desktop.MainWindow = setupDialog;
				}
			}
			else
			{
				ShowMainWindow(desktop);
			}
		}

		base.OnFrameworkInitializationCompleted();
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

	private async void SetupClosing(SetupDialog setupDialog, IClassicDesktopStyleApplicationLifetime desktop, System.ComponentModel.CancelEventArgs e)
	{
		try
		{
			if (setupDialog.IsNewUser)
			{
				Configuration.SetLibationFiles(Configuration.DefaultLibationFilesDirectory);
				setupDialog.Config.Books = Configuration.DefaultBooksDirectory;

				if (setupDialog.Config.LibationSettingsAreValid)
				{
					string? theme = setupDialog.SelectedTheme.Content as string;
					setupDialog.Config.SetString(theme, nameof(ThemeVariant));

					await RunMigrationsAsync(setupDialog.Config);
					LibraryTask = Task.Run(() => DbContexts.GetLibrary_Flat_NoTracking(includeParents: true));
					AudibleUtilities.AudibleApiStorage.EnsureAccountsSettingsFileExists();
					ShowMainWindow(desktop);
				}
				else
				{
					e.Cancel = true;
					await CancelInstallation(setupDialog);
				}
			}
			else if (setupDialog.IsReturningUser)
			{
				ShowLibationFilesDialog(desktop, setupDialog.Config);
			}
			else
			{
				e.Cancel = true;
				await CancelInstallation(setupDialog);
			}
		}
		catch (Exception ex)
		{
			string title = "Fatal error, pre-logging";
			string body = "An unrecoverable error occurred. Since this error happened before logging could be initialized, this error can not be written to the log file.";

			MessageBoxAlertAdminDialog alert = new(body, title, ex);
			desktop.MainWindow = alert;
			alert.Show();
		}
	}

	private void ShowLibationFilesDialog(IClassicDesktopStyleApplicationLifetime desktop, Configuration config)
	{
		LibationFilesDialog libationFilesDialog = new();
		desktop.MainWindow = libationFilesDialog;
		libationFilesDialog.Show();

		async void WindowClosing(object? sender, System.ComponentModel.CancelEventArgs e)
		{
			libationFilesDialog.Closing -= WindowClosing;
			e.Cancel = true;
			if (libationFilesDialog.DialogResult == DialogResult.OK)
				OnLibationFilesCompleted(desktop, libationFilesDialog, config);
			else
				await CancelInstallation(libationFilesDialog);
		}
		libationFilesDialog.Closing += WindowClosing;
	}

	private async void OnLibationFilesCompleted(IClassicDesktopStyleApplicationLifetime desktop, LibationFilesDialog libationFilesDialog, Configuration config)
	{
		Configuration.SetLibationFiles(libationFilesDialog.SelectedDirectory);
		if (config.LibationSettingsAreValid)
		{
			await RunMigrationsAsync(config);

			LibraryTask = Task.Run(() => DbContexts.GetLibrary_Flat_NoTracking(includeParents: true));
			AudibleUtilities.AudibleApiStorage.EnsureAccountsSettingsFileExists();
			ShowMainWindow(desktop);
		}
		else
		{
			// path did not result in valid settings
			DialogResult continueResult = await MessageBox.Show(
				libationFilesDialog,
				$"No valid settings were found at this location.\r\nWould you like to create a new install settings in this folder?\r\n\r\n{libationFilesDialog.SelectedDirectory}",
				"New install?",
				MessageBoxButtons.YesNo,
				MessageBoxIcon.Question);

			if (continueResult == DialogResult.Yes)
			{
				config.Books = Path.Combine(libationFilesDialog.SelectedDirectory, nameof(Configuration.Books));

				if (config.LibationSettingsAreValid)
				{
					await RunMigrationsAsync(config);
					LibraryTask = Task.Run(() => DbContexts.GetLibrary_Flat_NoTracking(includeParents: true));
					AudibleUtilities.AudibleApiStorage.EnsureAccountsSettingsFileExists();
					ShowMainWindow(desktop);
				}
				else
				{
					await CancelInstallation(libationFilesDialog);
				}
			}
			else
			{
				await CancelInstallation(libationFilesDialog);
			}
		}

		libationFilesDialog.Close();
	}

	private static async Task CancelInstallation(Window window)
	{
		await MessageBox.Show(window, "Initial set up cancelled.", "Cancelled", MessageBoxButtons.OK, MessageBoxIcon.Warning);
		Environment.Exit(-1);
	}

	private async Task RunMigrationsAsync(Configuration config)
	{
		// most migrations go in here
		AppScaffolding.LibationScaffolding.RunPostConfigMigrations(config);

		await MessageBox.VerboseLoggingWarning_ShowIfTrue();

		// logging is init'd here
		AppScaffolding.LibationScaffolding.RunPostMigrationScaffolding(AppScaffolding.Variety.Chardonnay, config);
	}

	private static void ShowMainWindow(IClassicDesktopStyleApplicationLifetime desktop)
	{
		Configuration.Instance.PropertyChanged += ThemeVariant_PropertyChanged;
		Current.ActualThemeVariantChanged += OnActualThemeVariantChanged;
		OnActualThemeVariantChanged(Current, EventArgs.Empty);

		MainWindow mainWindow = new();
		desktop.MainWindow = MainWindow = mainWindow;
		mainWindow.Loaded += MainWindow_Loaded;
		mainWindow.RestoreSizeAndLocation(Configuration.Instance);
		mainWindow.Show();
	}

	[PropertyChangeFilter(nameof(ThemeVariant))]
	private static void ThemeVariant_PropertyChanged(object sender, PropertyChangedEventArgsEx e)
		=> OpenAndApplyTheme(e.NewValue as string);

	private static void OnActualThemeVariantChanged(object? sender, EventArgs e)
		=> OpenAndApplyTheme(Configuration.Instance.GetString(propertyName: nameof(ThemeVariant)));

	private static void OpenAndApplyTheme(string? themeVariant)
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
