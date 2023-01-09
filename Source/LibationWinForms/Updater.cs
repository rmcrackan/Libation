using System;
using System.Windows.Forms;
using AppScaffolding;
using AutoUpdaterDotNET;
using LibationFileManager;
using LibationWinForms.Dialogs;

namespace LibationWinForms
{
    public static class Updater
	{
		public static void Run(UpgradeProperties upgradeProperties)
		{
			string latestVersionOnServer = upgradeProperties.LatestRelease.ToString();
			string downloadZipUrl = upgradeProperties.ZipUrl;
			AutoUpdater.ParseUpdateInfoEvent +=
				args => args.UpdateInfo = new()
				{
					CurrentVersion = latestVersionOnServer,
					DownloadURL = downloadZipUrl,
					ChangelogURL = LibationScaffolding.RepositoryLatestUrl
				};

			void AutoUpdaterOnCheckForUpdateEvent(UpdateInfoEventArgs args)
			{
				if (args is null || !args.IsUpdateAvailable)
					return;

				const string ignoreUpdate = "IgnoreUpdate";
				var config = Configuration.Instance;

				if (config.GetString(ignoreUpdate) == args.CurrentVersion)
					return;

				var notificationResult = new UpgradeNotificationDialog(upgradeProperties).ShowDialog();

				if (notificationResult == DialogResult.Ignore)
					config.SetString(upgradeProperties.LatestRelease.ToString(), ignoreUpdate);

				if (notificationResult != DialogResult.Yes) return;

				try
				{
					Serilog.Log.Logger.Information("Start upgrade. {@DebugInfo}", new { CurrentlyInstalled = args.InstalledVersion, TargetVersion = args.CurrentVersion });
					AutoUpdater.DownloadUpdate(args);
				}
				catch (Exception ex)
				{
					MessageBoxLib.ShowAdminAlert(null, "Error downloading update", "Error downloading update", ex);
				}
			}

			AutoUpdater.CheckForUpdateEvent += AutoUpdaterOnCheckForUpdateEvent;
			AutoUpdater.Start(LibationScaffolding.RepositoryLatestUrl);
		}
	}
}
