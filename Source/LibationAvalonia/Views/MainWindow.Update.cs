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
					Serilog.Log.Logger.Warning("Download link for new version not found");
					return null;
				}

				//Silently download the update in the background, save it to a temp file.

				var zipFile = Path.Combine(Path.GetTempPath(), Path.GetFileName(upgradeProperties.ZipUrl));

				Serilog.Log.Logger.Information($"Downloading {zipFile}");

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

			try
			{
				var upgradeProperties = await Task.Run(LibationScaffolding.GetLatestRelease);
				if (upgradeProperties is null) return;

				const string ignoreUpdate = "IgnoreUpdate";
				var config = Configuration.Instance;

				if (config.GetString(propertyName: ignoreUpdate) == upgradeProperties.LatestRelease.ToString())
					return;

				var interop = InteropFactory.Create();

				if (!interop.CanUpdate)
					Serilog.Log.Logger.Information("Can't perform update automatically");

				var notificationResult = await new UpgradeNotificationDialog(upgradeProperties, interop.CanUpdate).ShowDialog<DialogResult>(this);

				if (notificationResult == DialogResult.Ignore)
					config.SetString(upgradeProperties.LatestRelease.ToString(), ignoreUpdate);

				if (notificationResult != DialogResult.OK) return;

				//Download the update file in the background,
				string updateBundle = await downloadUpdate(upgradeProperties);

				if (string.IsNullOrEmpty(updateBundle) || !File.Exists(updateBundle)) return;

				//Install the update
				Serilog.Log.Logger.Information($"Begin running auto-updater");
				interop.InstallUpdate(updateBundle);
				Serilog.Log.Logger.Information($"Completed running auto-updater");
			}
			catch (Exception ex)
			{
				Serilog.Log.Logger.Error(ex, "An error occured while checking for app updates.");
			}
		}
    }
}
