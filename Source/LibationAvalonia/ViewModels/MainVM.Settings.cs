using Avalonia.Controls;
using LibationFileManager;
using ReactiveUI;
using System;
using System.Threading.Tasks;

namespace LibationAvalonia.ViewModels
{
	partial class MainVM
	{
		private bool _menuBarVisible = !Configuration.IsMacOs;
		public bool MenuBarVisible { get => _menuBarVisible; set => this.RaiseAndSetIfChanged(ref _menuBarVisible, value); }
		private void Configure_Settings()
		{
			((NativeMenuItem)NativeMenu.GetMenu(App.Current).Items[0]).Command = ReactiveCommand.Create(ShowAboutAsync);
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
