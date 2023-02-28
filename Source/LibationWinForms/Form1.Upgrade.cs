using LibationUiBase;
using LibationWinForms.Dialogs;
using System.Threading.Tasks;

namespace LibationWinForms
{
	public partial class Form1
	{
		private void Configure_Upgrade()
		{
			setProgressVisible(false);
#if !DEBUG
			Task upgradeAvailable(UpgradeEventArgs e)
			{
				var notificationResult = new UpgradeNotificationDialog(e.UpgradeProperties).ShowDialog(this);

				e.Ignore = notificationResult == System.Windows.Forms.DialogResult.Ignore;
				e.InstallUpgrade = notificationResult == System.Windows.Forms.DialogResult.Yes;

				return Task.CompletedTask;
			}

			var upgrader = new Upgrader();
			upgrader.DownloadProgress += (_, e) => Invoke(() => upgradePb.Value = int.Max(0, int.Min(100, (int)(e.ProgressPercentage ?? 0))));
			upgrader.DownloadBegin += (_, _) => Invoke(() => setProgressVisible(true));
			upgrader.DownloadCompleted += (_, _) => Invoke(() => setProgressVisible(false));

			Shown += async (_, _) => await upgrader.CheckForUpgradeAsync(upgradeAvailable);
#endif
		}

		private void setProgressVisible(bool visible) => upgradeLbl.Visible = upgradePb.Visible = visible;

	}
}
