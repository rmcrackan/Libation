using LibationWinForms.Dialogs;
using System;
using System.Linq;

namespace LibationWinForms.AvaloniaUI.Views
{
	//DONE
	public partial class MainWindow
	{
		private void Configure_Settings() { }

		public void accountsToolStripMenuItem_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e) => new AccountsDialog().ShowDialog();

		public void basicSettingsToolStripMenuItem_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e) => new SettingsDialog().ShowDialog();

		public void aboutToolStripMenuItem_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
			=> System.Windows.Forms.MessageBox.Show($"Running Libation version {AppScaffolding.LibationScaffolding.BuildVersion}", $"Libation v{AppScaffolding.LibationScaffolding.BuildVersion}");
	}
}
