using System;
using System.Windows.Forms;
using AutoUpdaterDotNET;

namespace LibationWinForms
{
    public static class Updater
	{
		private const string REPO_URL = "https://github.com/rmcrackan/Libation/releases/latest";

		public static void Run(Version latestVersionOnServer, string downloadZipUrl)
			=> Run(latestVersionOnServer.ToString(), downloadZipUrl);
		public static void Run(string latestVersionOnServer, string downloadZipUrl)
		{
			AutoUpdater.ParseUpdateInfoEvent +=
				args => args.UpdateInfo = new()
				{
					CurrentVersion = latestVersionOnServer,
					DownloadURL = downloadZipUrl,
					ChangelogURL = REPO_URL
				};
			AutoUpdater.CheckForUpdateEvent += AutoUpdaterOnCheckForUpdateEvent;
			AutoUpdater.Start(REPO_URL);
		}

		private static void AutoUpdaterOnCheckForUpdateEvent(UpdateInfoEventArgs args)
		{
			if (args is null || !args.IsUpdateAvailable)
				return;

			var dialogResult = MessageBox.Show(string.Format(
				$"There is a new version available. Would you like to update?\r\n\r\nAfter you close Libation, the upgrade will start automatically."),
				"Update Available",
				MessageBoxButtons.YesNo,
				MessageBoxIcon.Information);
			if (dialogResult != DialogResult.Yes)
				return;

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
	}
}
