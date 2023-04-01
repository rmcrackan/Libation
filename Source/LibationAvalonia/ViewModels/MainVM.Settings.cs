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
		private void Configure_Settings() { }

		public Task ShowAboutAsync() => MessageBox.Show(MainWindow, $"Libation {AppScaffolding.LibationScaffolding.Variety}{Environment.NewLine}Version {AppScaffolding.LibationScaffolding.BuildVersion}", $"Libation v{AppScaffolding.LibationScaffolding.BuildVersion}");
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
