using Avalonia.Controls;
using LibationFileManager;
using LibationUiBase.Forms;
using ReactiveUI;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace LibationAvalonia.ViewModels;

partial class MainVM
{
	public string FindBetterQualityBooksTip => Configuration.GetHelpText("FindBetterQualityBooks");
	public bool MenuBarVisible { get => field; set => this.RaiseAndSetIfChanged(ref field, value); } = !Configuration.IsMacOs;
	public ICommand LaunchHangover { get; private set; } = null!;

	private void Configure_Settings()
	{
		LaunchHangover = ReactiveCommand.CreateFromTask(LaunchHangoverAsync);

		if (App.Current is Avalonia.Application app && NativeMenu.GetMenu(app) is NativeMenu appMenu)
		{
			SetAppMenuCommand(appMenu, "About Libation", ShowAboutAsync);
			SetAppMenuCommand(appMenu, "Settings…", ShowSettingsAsync);
			SetAppMenuCommand(appMenu, "Accounts…", ShowAccountsAsync);
		}
	}

	private static void SetAppMenuCommand(NativeMenu menu, string header, Func<Task> action)
	{
		if (menu.Items.OfType<NativeMenuItem>().FirstOrDefault(i => i.Header == header) is NativeMenuItem item)
			item.Command = ReactiveCommand.CreateFromTask(action);
	}

	public Task ShowAboutAsync() => new LibationAvalonia.Dialogs.AboutDialog().ShowDialog(MainWindow);
	public Task ShowAccountsAsync() => new LibationAvalonia.Dialogs.AccountsDialog().ShowDialog(MainWindow);
	public Task ShowSettingsAsync() => new LibationAvalonia.Dialogs.SettingsDialog().ShowDialog(MainWindow);
	public async Task ShowTrashBinAsync()
	{
		await new LibationAvalonia.Dialogs.TrashBinDialog().ShowDialog(MainWindow);
		//Restoring or permanently deleting inside the dialog changes the count behind it.
		await RefreshBooksInTrashAsync();
	}
	public Task ShowFindBetterQualityBooksAsync() => new LibationAvalonia.Dialogs.FindBetterQualityBooksDialog().ShowDialog(MainWindow);

	private async Task LaunchHangoverAsync()
	{
		try
		{
			HangoverLauncher.Launch();
		}
		catch (Exception ex)
		{
			Serilog.Log.Logger.Error(ex, "Failed to launch Hangover");
			await MessageBox.Show(
				MainWindow,
				HangoverLauncher.GetLaunchFailureMessage(ex),
				"Launch Hangover",
				MessageBoxButtons.OK,
				MessageBoxIcon.Warning);
		}
	}

	public async Task StartWalkthroughAsync()
	{
		MenuBarVisible = true;
		await new Walkthrough(MainWindow).RunAsync();
		MenuBarVisible = !Configuration.IsMacOs;
	}
}
