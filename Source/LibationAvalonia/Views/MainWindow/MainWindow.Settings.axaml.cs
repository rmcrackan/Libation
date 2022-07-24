using System;
using System.Linq;

namespace LibationAvalonia.Views
{
	//DONE
	public partial class MainWindow
	{
		private void Configure_Settings() { }

		public async void accountsToolStripMenuItem_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e) => await new Dialogs.AccountsDialog().ShowDialog(this);

		public async void basicSettingsToolStripMenuItem_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e) => await new Dialogs.SettingsDialog().ShowDialog(this);

		public void aboutToolStripMenuItem_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
			=> MessageBox.Show($"Running Libation version {AppScaffolding.LibationScaffolding.BuildVersion}", $"Libation v{AppScaffolding.LibationScaffolding.BuildVersion}");
	}
}
