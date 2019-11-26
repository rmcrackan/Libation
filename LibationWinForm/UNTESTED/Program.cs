using System;
using System.Windows.Forms;
using Dinah.Core.Logging;
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
			// default. for reference. output example:
			// 2019-11-26 08:48:40.224 -05:00 [DBG] Begin Libation
			var default_outputTemplate = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}";
			// with class and method info. output example:
			// 2019-11-26 08:48:40.224 -05:00 [DBG] (at LibationWinForm.Program.init()) Begin Libation
			var code_outputTemplate = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] (at {Caller}) {Message:lj}{NewLine}{Exception}";


			var logPath = System.IO.Path.Combine(FileManager.Configuration.Instance.LibationFiles, "Log.log");

			Log.Logger = new LoggerConfiguration()
				.Enrich.WithCaller()
				.MinimumLevel.Debug()
				.WriteTo.File(logPath,
					rollingInterval: RollingInterval.Month,
					outputTemplate: code_outputTemplate)
				.CreateLogger();

			Log.Logger.Debug("Begin Libation");

			// .Here() captures debug info via System.Runtime.CompilerServices attributes. Warning: expensive
			//var withLineNumbers_outputTemplate = "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message}{NewLine}in method {MemberName} at {FilePath}:{LineNumber}{NewLine}{Exception}{NewLine}";
			//Log.Logger.Here().Debug("Begin Libation. Debug with line numbers");
		}
	}
}
