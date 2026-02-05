using System;
using System.Windows.Forms;
using LibationWinForms.Dialogs;

namespace LibationWinForms;

public partial class Form1
{
	private void Configure_Settings()
	{
		Shown += FormShown_Settings;
	}

	private void FormShown_Settings(object? sender, EventArgs e)
	{
		if (LibationFileManager.AudibleFileStorage.BooksDirectory is null)
		{
			var result = MessageBox.Show(
				this,
				"Please set a valid Books location in the settings dialog.",
				"Books Directory Not Set",
				MessageBoxButtons.OKCancel,
				MessageBoxIcon.Warning,
				MessageBoxDefaultButton.Button1);

			if (result is DialogResult.OK)
				new SettingsDialog().ShowDialog(this);
		}
	}

	private void accountsToolStripMenuItem_Click(object sender, EventArgs e) => new AccountsDialog().ShowDialog();

	private void basicSettingsToolStripMenuItem_Click(object sender, EventArgs e) => new SettingsDialog().ShowDialog();

	private void aboutToolStripMenuItem_Click(object sender, EventArgs e) => new AboutDialog().ShowDialog(this);
	private async void tourToolStripMenuItem_Click(object sender, EventArgs e)
		=> await new Walkthrough(this).RunAsync();
	private void scanForHigherQualityBooksStripMenuItem_Click(object sender, EventArgs e)
		=> new FindBetterQualityBooksDialog().ShowDialog(this);

	private void launchHangoverToolStripMenuItem_Click(object sender, EventArgs e)
	{
		try
		{
			System.Diagnostics.Process.Start("Hangover.exe");
		}
		catch (Exception ex)
		{
			Serilog.Log.Logger.Error(ex, "Failed to launch Hangover");
		}
	}
}
