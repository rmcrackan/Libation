using AppScaffolding;
using LibationAvalonia.Dialogs;
using LibationFileManager;
using System;
using System.Threading.Tasks;

namespace LibationAvalonia.Views
{
    public partial class MainWindow
    {
		private void Configure_Update()
		{
			Opened += async (_, _) => await checkForUpdates();
		}

        private async Task checkForUpdates()
		{
			async Task<string> downloadUpdate(UpgradeProperties upgradeProperties)
			{
				if (upgradeProperties.ZipUrl is null)
				{
					Serilog.Log.Logger.Information("Download link for new version not found");
					return null;
				}

				//Silently download the update in the background, save it to a temp file.

				var zipFile = System.IO.Path.GetTempFileName();
				try
				{
					System.Net.Http.HttpClient cli = new();
					using var fs = System.IO.File.OpenWrite(zipFile);
					using var dlStream = await cli.GetStreamAsync(new Uri(upgradeProperties.ZipUrl));
					await dlStream.CopyToAsync(fs);
				}
				catch (Exception ex)
				{
					Serilog.Log.Logger.Error(ex, "Failed to download the update: {pdate}", upgradeProperties.ZipUrl);
					return null;
				}
				return zipFile;
			}

			void runWindowsUpgrader(string zipFile)
			{
				var thisExe = Environment.ProcessPath;
				var thisDir = System.IO.Path.GetDirectoryName(thisExe);

				var zipExtractor = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "ZipExtractor.exe");

				System.IO.File.Copy("ZipExtractor.exe", zipExtractor, overwrite: true);

				var psi = new System.Diagnostics.ProcessStartInfo()
				{
					FileName = zipExtractor,
					UseShellExecute = true,
					Verb = "runas",
					WindowStyle = System.Diagnostics.ProcessWindowStyle.Normal,
					CreateNoWindow = true,
					ArgumentList =
					{
						"--input",
						zipFile,
						"--output",
						thisDir,
						"--executable",
						thisExe
					}
				};

				System.Diagnostics.Process.Start(psi);
				Environment.Exit(0);
			}

			try
			{
				var upgradeProperties = await Task.Run(LibationScaffolding.GetLatestRelease);
				if (upgradeProperties is null) return;

				if (Configuration.IsWindows)
				{
					string zipFile = await downloadUpdate(upgradeProperties);

					if (string.IsNullOrEmpty(zipFile) || !System.IO.File.Exists(zipFile))
						return;

					var result = await MessageBox.Show(this, $"{upgradeProperties.HtmlUrl}\r\n\r\nWould you like to upgrade now?", "New version available", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);

					if (result == DialogResult.Yes)
						runWindowsUpgrader(zipFile);
				}
				else
				{
					//We're not going to have a solution for in-place upgrade on
					//linux/mac, so just notify that an update is available.

					const string ignoreUpdate = "IgnoreUpdate";
					var config = Configuration.Instance;

					if (config.GetObject(ignoreUpdate)?.ToString() == upgradeProperties.LatestRelease.ToString())
						return;

					var notificationResult = await new UpgradeNotification(upgradeProperties).ShowDialog<DialogResult>(this);

					if (notificationResult == DialogResult.Ignore)
						config.SetObject(ignoreUpdate, upgradeProperties.LatestRelease.ToString());
				}
			}
			catch (Exception ex)
			{
				Serilog.Log.Logger.Error(ex, "An error occured while checking for app updates.");
			}
		}
    }
}
