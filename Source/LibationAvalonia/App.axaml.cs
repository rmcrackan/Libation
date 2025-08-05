using ApplicationServices;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Platform;
using Avalonia.Styling;
using LibationAvalonia.Dialogs;
using LibationAvalonia.Views;
using LibationFileManager;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Threading;
using Dinah.Core;
using LibationAvalonia.Themes;
using Avalonia.Data.Core.Plugins;
using System.Linq;
using LibationUiBase.Forms;
using Avalonia.Controls;

#nullable enable
namespace LibationAvalonia
{
	public class App : Application
	{
		public static Task<List<DataLayer.LibraryBook>>? LibraryTask { get; set; }
		public static ChardonnayTheme? DefaultThemeColors { get; private set; }
		public static MainWindow? MainWindow { get; private set; }
		public static Uri AssetUriBase { get; } = new("avares://Libation/Assets/");
		public static new Application Current => Application.Current ?? throw new InvalidOperationException("The Avalonia app hasn't started yet.");

		public static Stream OpenAsset(string assetRelativePath)
			=> AssetLoader.Open(new Uri(AssetUriBase, assetRelativePath));

		public override void Initialize()
		{
			AvaloniaXamlLoader.Load(this);
		}

		public override void OnFrameworkInitializationCompleted()
		{
			DefaultThemeColors = ChardonnayTheme.GetLiveTheme();

			if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
			{
				MessageBoxBase.ShowAsyncImpl = (owner, message, caption, buttons, icon, defaultButton, saveAndRestorePosition) =>
					MessageBox.Show(owner as Window, message, caption, buttons, icon, defaultButton, saveAndRestorePosition);

				// Avoid duplicate validations from both Avalonia and the CommunityToolkit. 
				// More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
				DisableAvaloniaDataAnnotationValidation();

				var config = Configuration.Instance;

				if (!config.LibationSettingsAreValid)
				{
					var defaultLibationFilesDir = Configuration.DefaultLibationFilesDirectory;

					// check for existing settings in default location
					var defaultSettingsFile = Path.Combine(defaultLibationFilesDir, "Settings.json");
					if (Configuration.SettingsFileIsValid(defaultSettingsFile))
						Configuration.SetLibationFiles(defaultLibationFilesDir);

					if (config.LibationSettingsAreValid)
					{
						LibraryTask = Task.Run(() => DbContexts.GetLibrary_Flat_NoTracking(includeParents: true));
						ShowMainWindow(desktop);
					}
					else
					{
						var setupDialog = new SetupDialog { Config = config };
						setupDialog.Closing += Setup_Closing;
						desktop.MainWindow = setupDialog;
					}
				}
				else
					ShowMainWindow(desktop);
			}

			base.OnFrameworkInitializationCompleted();
		}
		private void DisableAvaloniaDataAnnotationValidation()
		{
			// Get an array of plugins to remove
			var dataValidationPluginsToRemove =
				BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

			// remove each entry found
			foreach (var plugin in dataValidationPluginsToRemove)
			{
				BindingPlugins.DataValidators.Remove(plugin);
			}
		}

		private async void Setup_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
		{
			if (sender is not SetupDialog setupDialog || ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop)
				return;

			try
			{
				// all returns should be preceded by either:
				// - if config.LibationSettingsAreValid
				// - error message, Exit()				
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
					ShowLibationFilesDialog(desktop, setupDialog.Config, OnLibationFilesCompleted);
				}
				else
				{
					e.Cancel = true;
					await CancelInstallation(setupDialog);
					return;
				}

			}
			catch (Exception ex)
			{
				var title = "Fatal error, pre-logging";
				var body = "An unrecoverable error occurred. Since this error happened before logging could be initialized, this error can not be written to the log file.";
				try
				{
					await MessageBox.ShowAdminAlert(setupDialog, body, title, ex);
				}
				catch
				{
					await MessageBox.Show(setupDialog, $"{body}\r\n\r\n{ex.Message}\r\n\r\n{ex.StackTrace}", title, MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
				return;
			}
		}

		private async Task RunMigrationsAsync(Configuration config)
		{
			// most migrations go in here
			AppScaffolding.LibationScaffolding.RunPostConfigMigrations(config);

			await MessageBox.VerboseLoggingWarning_ShowIfTrue();

			// logging is init'd here
			AppScaffolding.LibationScaffolding.RunPostMigrationScaffolding(AppScaffolding.Variety.Chardonnay, config);
		}

		private void ShowLibationFilesDialog(IClassicDesktopStyleApplicationLifetime desktop, Configuration config, Action<IClassicDesktopStyleApplicationLifetime, LibationFilesDialog, Configuration> OnClose)
		{
			var libationFilesDialog = new LibationFilesDialog();
			desktop.MainWindow = libationFilesDialog;
			libationFilesDialog.Show();

			void WindowClosing(object? sender, System.ComponentModel.CancelEventArgs e)
			{
				libationFilesDialog.Closing -= WindowClosing;
				e.Cancel = true;
				OnClose?.Invoke(desktop, libationFilesDialog, config);
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
				var continueResult = await MessageBox.Show(
					libationFilesDialog,
					$"No valid settings were found at this location.\r\nWould you like to create a new install settings in this folder?\r\n\r\n{libationFilesDialog.SelectedDirectory}",
					"New install?",
					MessageBoxButtons.YesNo,
					MessageBoxIcon.Question);

				if (continueResult == DialogResult.Yes)
				{
					config.Books = Configuration.DefaultBooksDirectory;

					if (config.LibationSettingsAreValid)
					{
						await RunMigrationsAsync(config);
						LibraryTask = Task.Run(() => DbContexts.GetLibrary_Flat_NoTracking(includeParents: true));
						AudibleUtilities.AudibleApiStorage.EnsureAccountsSettingsFileExists();
						ShowMainWindow(desktop);
					}
					else
						await CancelInstallation(libationFilesDialog);
				}
				else
					await CancelInstallation(libationFilesDialog);
			}

			libationFilesDialog.Close();
		}

		static async Task CancelInstallation(Window window)
		{
			await MessageBox.Show(window, "Initial set up cancelled.", "Cancelled", MessageBoxButtons.OK, MessageBoxIcon.Warning);
			Environment.Exit(0);
		}

		private static void ShowMainWindow(IClassicDesktopStyleApplicationLifetime desktop)
		{
			Configuration.Instance.PropertyChanged += ThemeVariant_PropertyChanged;
			OpenAndApplyTheme(Configuration.Instance.GetString(propertyName: nameof(ThemeVariant)));

			var mainWindow = new MainWindow();
			desktop.MainWindow = MainWindow = mainWindow;
			mainWindow.Loaded += MainWindow_Loaded;
			mainWindow.RestoreSizeAndLocation(Configuration.Instance);
			mainWindow.Show();
		}

		[PropertyChangeFilter(nameof(ThemeVariant))]
		private static void ThemeVariant_PropertyChanged(object sender, PropertyChangedEventArgsEx e)
			=> OpenAndApplyTheme(e.NewValue as string);

		private static void OpenAndApplyTheme(string? themeVariant)
		{
			using var themePersister = ChardonnayThemePersister.Create();
			themePersister?.Target.ApplyTheme(themeVariant);
		}

		private static async void MainWindow_Loaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
		{
			if (LibraryTask is not null && MainWindow is not null)
			{
				var library = await LibraryTask;
				await Dispatcher.UIThread.InvokeAsync(() => MainWindow.OnLibraryLoadedAsync(library));
			}
		}
	}
}
