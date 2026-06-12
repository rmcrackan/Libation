using Avalonia.Controls;
using LibationFileManager;
using LibationUiBase.Forms;
using ReactiveUI;
using System;
using System.Reactive;
using System.Threading.Tasks;

namespace LibationAvalonia.ViewModels;

partial class MainVM
{
	public string FindBetterQualityBooksTip => Configuration.GetHelpText("FindBetterQualityBooks");
	public bool MenuBarVisible { get => field; set => this.RaiseAndSetIfChanged(ref field, value); } = !Configuration.IsMacOs;
	public ReactiveCommand<Unit, Unit> LaunchHangover { get; private set; } = null!;

	private void Configure_Settings()
	{
		LaunchHangover = ReactiveCommand.CreateFromTask(LaunchHangoverAsync);

		if (App.Current is Avalonia.Application app &&
			NativeMenu.GetMenu(app)?.Items[0] is NativeMenuItem aboutMenu)
			aboutMenu.Command = ReactiveCommand.Create(ShowAboutAsync);
	}

	public Task ShowAboutAsync() => new LibationAvalonia.Dialogs.AboutDialog().ShowDialog(MainWindow);
	public Task ShowAccountsAsync() => new LibationAvalonia.Dialogs.AccountsDialog().ShowDialog(MainWindow);
	public Task ShowSettingsAsync() => new LibationAvalonia.Dialogs.SettingsDialog().ShowDialog(MainWindow);
	public Task ShowTrashBinAsync() => new LibationAvalonia.Dialogs.TrashBinDialog().ShowDialog(MainWindow);
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
