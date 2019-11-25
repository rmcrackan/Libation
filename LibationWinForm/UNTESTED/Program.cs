using System;
using System.Windows.Forms;
using Serilog;

namespace LibationWinForm
{
    static class Program
    {
        [STAThread]
        static void Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

			if (!createSettings())
				return;

			init();

			Application.Run(new Form1());
		}

		private static bool createSettings()
		{
			if (!string.IsNullOrWhiteSpace(FileManager.Configuration.Instance.Books))
				return true;

			var welcomeText = @"
This appears to be your first time using Libation. Welcome.
Please fill in a few settings on the following page. You can also change these settings later.

After you make your selections, get started by importing your library.
Go to Import > Scan Library
".Trim();
			MessageBox.Show(welcomeText, "Welcome to Libation", MessageBoxButtons.OK);
			var dialogResult = new SettingsDialog().ShowDialog();
			if (dialogResult != DialogResult.OK)
			{
				MessageBox.Show("Initial set up cancelled.", "Cancelled", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return false;
			}

			return true;
		}

		private static void init()
		{
			var logPath = System.IO.Path.Combine(FileManager.Configuration.Instance.LibationFiles, "Log.log");
			Log.Logger = new LoggerConfiguration()
				.MinimumLevel.Debug()
				.WriteTo.File(logPath, rollingInterval: RollingInterval.Month)
				.CreateLogger();
		}
	}
}
