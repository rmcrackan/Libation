using Avalonia.Controls;
using LibationFileManager;
using ReactiveUI;
using System;
using System.Threading.Tasks;

#nullable enable
namespace LibationAvalonia.ViewModels
{
	partial class MainVM
	{
		public bool MenuBarVisible { get => field; set => this.RaiseAndSetIfChanged(ref field, value); } = !Configuration.IsMacOs;
		private void Configure_Settings()
		{
			if (App.Current is Avalonia.Application app &&
				NativeMenu.GetMenu(app)?.Items[0] is NativeMenuItem aboutMenu)
				aboutMenu.Command = ReactiveCommand.Create(ShowAboutAsync);
		}

		public Task ShowAboutAsync() => new LibationAvalonia.Dialogs.AboutDialog().ShowDialog(MainWindow);
		public Task ShowAccountsAsync() => new LibationAvalonia.Dialogs.AccountsDialog().ShowDialog(MainWindow);
		public Task ShowSettingsAsync() => new LibationAvalonia.Dialogs.SettingsDialog().ShowDialog(MainWindow);
		public Task ShowTrashBinAsync() => new LibationAvalonia.Dialogs.TrashBinDialog().ShowDialog(MainWindow);

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
}
