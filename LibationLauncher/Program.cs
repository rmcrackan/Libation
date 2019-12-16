using System;
using System.IO;
using System.Windows.Forms;
using Dinah.Core.Logging;
using FileManager;
using LibationWinForms;
using LibationWinForms.Dialogs;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace LibationLauncher
{
	static class Program
	{
		[STAThread]
		static void Main()
		{
			Application.SetHighDpiMode(HighDpiMode.SystemAware);
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

			createSettings();
			initLogging();

			Application.Run(new Form1());
		}

		private static void createSettings()
		{
			var config = Configuration.Instance;
			if (config.IsComplete)
				return;

			var isAdvanced = false;

			var setupDialog = new SetupDialog();
			setupDialog.NoQuestionsBtn_Click += (_, __) =>
			{
				config.DecryptKey ??= "";
				config.LocaleCountryCode ??= "us";
				config.DownloadsInProgressEnum ??= "WinTemp";
				config.DecryptInProgressEnum ??= "WinTemp";
				config.Books ??= Configuration.AppDir;
			};
			// setupDialog.BasicBtn_Click += (_, __) => // no action needed
			setupDialog.AdvancedBtn_Click += (_, __) => isAdvanced = true;
			setupDialog.ShowDialog();

			if (isAdvanced)
			{
				var dialog = new LibationFilesDialog();
				if (dialog.ShowDialog() != DialogResult.OK)
					MessageBox.Show("Libation Files location not changed");
			}

			if (config.IsComplete)
				return;

			if (new SettingsDialog().ShowDialog() == DialogResult.OK)
				return;

			MessageBox.Show("Initial set up cancelled.", "Cancelled", MessageBoxButtons.OK, MessageBoxIcon.Warning);
			Application.Exit();
			Environment.Exit(0);
		}

		private static void initLogging()
		{
			// default. for reference. output example:
			// 2019-11-26 08:48:40.224 -05:00 [DBG] Begin Libation
			var default_outputTemplate = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}";
			// with class and method info. output example:
			// 2019-11-26 08:48:40.224 -05:00 [DBG] (at LibationWinForms.Program.init()) Begin Libation
			var code_outputTemplate = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] (at {Caller}) {Message:lj}{NewLine}{Exception}";


			var logPath = Path.Combine(Configuration.Instance.LibationFiles, "Log.log");

//var configuration = new ConfigurationBuilder()
//	.AddJsonFile("appsettings.json")
//	.Build();
//Log.Logger = new LoggerConfiguration()
//  .ReadFrom.Configuration(configuration)
//  .CreateLogger();
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
