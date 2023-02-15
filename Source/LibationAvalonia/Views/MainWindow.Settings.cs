using LibationFileManager;
using System;

namespace LibationAvalonia.Views
{
	//DONE
	public partial class MainWindow
	{
		private void Configure_Settings() { }

		public async void accountsToolStripMenuItem_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
			=> await new Dialogs.AccountsDialog().ShowDialog(this);

		public async void basicSettingsToolStripMenuItem_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
			=> await new Dialogs.SettingsDialog().ShowDialog(this);

		public async void aboutToolStripMenuItem_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
			=> await MessageBox.Show($"Libation {AppScaffolding.LibationScaffolding.Variety}{Environment.NewLine}Version {AppScaffolding.LibationScaffolding.BuildVersion}", $"Libation v{AppScaffolding.LibationScaffolding.BuildVersion}");

		public void launchHangoverToolStripMenuItem_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
		{
			try
			{
				System.Diagnostics.Process.Start("Hangover" + (Configuration.IsWindows ? ".exe" : ""));
			}
			catch(Exception ex)
			{
				Serilog.Log.Logger.Error(ex, "Failed to launch Hangover");
			}
		}
	}
}
