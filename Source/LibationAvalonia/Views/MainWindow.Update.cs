using AppScaffolding;
using LibationAvalonia.Dialogs;
using LibationFileManager;
using System;
using System.IO;
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

				var zipFile = Path.GetTempFileName();
				try
				{
					System.Net.Http.HttpClient cli = new();
					using var fs = File.OpenWrite(zipFile);
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
				var thisDir = Path.GetDirectoryName(thisExe);

				var zipExtractor = Path.Combine(Path.GetTempPath(), "ZipExtractor.exe");

				File.Copy("ZipExtractor.exe", zipExtractor, overwrite: true);

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
			}

			try
			{
				var upgradeProperties = await Task.Run(LibationScaffolding.GetLatestRelease);
				if (upgradeProperties is null) return;

				const string ignoreUpdate = "IgnoreUpdate";
				var config = Configuration.Instance;

				if (config.GetString(propertyName: ignoreUpdate) == upgradeProperties.LatestRelease.ToString())
					return;

				var notificationResult = await new UpgradeNotificationDialog(upgradeProperties, Configuration.IsWindows).ShowDialog<DialogResult>(this);

				if (notificationResult == DialogResult.Ignore)
					config.SetString(upgradeProperties.LatestRelease.ToString(), ignoreUpdate);

				if (notificationResult != DialogResult.OK || !Configuration.IsWindows) return;

				//Download the update file in the background,
				//then wire up installaion on window close.

				string zipFile = await downloadUpdate(upgradeProperties);

				if (string.IsNullOrEmpty(zipFile) || !File.Exists(zipFile))
					return;

				Closed += (_, _) =>
				{
					if (File.Exists(zipFile))
						runWindowsUpgrader(zipFile);
				};
			}
			catch (Exception ex)
			{
				Serilog.Log.Logger.Error(ex, "An error occured while checking for app updates.");
			}
		}
    }
}
