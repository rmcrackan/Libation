using Avalonia.Controls;
using LibationFileManager;
using ReactiveUI;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace LibationAvalonia.ViewModels;

partial class MainVM
{
	public string FindBetterQualityBooksTip => Configuration.GetHelpText("FindBetterQualityBooks");
	public bool MenuBarVisible { get => field; set => this.RaiseAndSetIfChanged(ref field, value); } = !Configuration.IsMacOs;
	private void Configure_Settings()
	{
		if (App.Current is not Avalonia.Application app || NativeMenu.GetMenu(app) is not NativeMenu nativeAppMenu)
			return;

		// Avalonia provides the first native app-menu item on macOS, but we replace its command so it opens
		// Libation's About dialog instead of a no-op/default implementation.
		if (nativeAppMenu.Items.Count > 0 && nativeAppMenu.Items[0] is NativeMenuItem aboutMenu)
			aboutMenu.Command = ReactiveCommand.Create(ShowAboutAsync);

		if (!Configuration.IsMacOs)
			return;

		var checkForUpdatesMenuItem = nativeAppMenu.Items
			.OfType<NativeMenuItem>()
			.FirstOrDefault(item => string.Equals(item.Header?.ToString(), "Check for Updates…", StringComparison.Ordinal));

		if (checkForUpdatesMenuItem is null)
		{
			checkForUpdatesMenuItem = new NativeMenuItem
			{
				Header = "Check for Updates…",
				Command = ReactiveCommand.CreateFromTask(CheckForUpdatesAsync)
			};
			// Keep the native-style update entry close to About, where macOS users expect to find it.
			nativeAppMenu.Items.Insert(1, checkForUpdatesMenuItem);
		}

		UpdateChecker.AttachNativeMenuItem(checkForUpdatesMenuItem);
	}

	/// <summary>
	/// Manual update checks use the main window both as the dialog owner and as the surface for the shared
	/// bottom status-bar progress indicator.
	/// </summary>
	public Task CheckForUpdatesAsync() => UpdateChecker.CheckForUpgradeAsync(MainWindow, MainWindow);
	public Task ShowAboutAsync() => new LibationAvalonia.Dialogs.AboutDialog(UpdateChecker).ShowDialog(MainWindow);
	public Task ShowAccountsAsync() => new LibationAvalonia.Dialogs.AccountsDialog().ShowDialog(MainWindow);
	public Task ShowSettingsAsync() => new LibationAvalonia.Dialogs.SettingsDialog().ShowDialog(MainWindow);
	public Task ShowTrashBinAsync() => new LibationAvalonia.Dialogs.TrashBinDialog().ShowDialog(MainWindow);
	public Task ShowFindBetterQualityBooksAsync() => new LibationAvalonia.Dialogs.FindBetterQualityBooksDialog().ShowDialog(MainWindow);

	public void LaunchHangover()
	{
		try
		{
			System.Diagnostics.Process.Start("Hangover" + (Configuration.IsWindows ? ".exe" : ""));
		}
		catch (Exception ex)
		{
			Serilog.Log.Logger.Error(ex, "Failed to launch Hangover");
		}
	}

	public async Task StartWalkthroughAsync()
	{
		MenuBarVisible = true;
		await new Walkthrough(MainWindow).RunAsync();
		MenuBarVisible = !Configuration.IsMacOs;
	}
}
