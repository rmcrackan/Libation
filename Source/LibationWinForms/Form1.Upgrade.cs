using LibationUiBase;
using LibationWinForms.Dialogs;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LibationWinForms;

public partial class Form1
{
	private void Configure_Upgrade()
	{
		setProgressVisible(false);
#pragma warning disable CS8321 // Local function is declared but never used
		async Task upgradeAvailable(UpgradeEventArgs e)
		{
			var notificationResult = await new UpgradeNotificationDialog(e.UpgradeProperties).ShowDialogAsync(this);

			e.Ignore = notificationResult == DialogResult.Ignore;
			e.InstallUpgrade = notificationResult == DialogResult.Yes;
		}
#pragma warning restore CS8321 // Local function is declared but never used

		var upgrader = new Upgrader();
		upgrader.DownloadProgress += (_, e) => Invoke(() => upgradePb.Value = int.Max(0, int.Min(100, (int)(e.ProgressPercentage ?? 0))));
		upgrader.DownloadBegin += (_, _) => Invoke(() => setProgressVisible(true));
		upgrader.DownloadCompleted += (_, _) => Invoke(() => setProgressVisible(false));
		upgrader.UpgradeFailed += (_, message) => Invoke(() => { setProgressVisible(false); MessageBox.Show(this, message, "Upgrade Failed", MessageBoxButtons.OK, MessageBoxIcon.Error); });

#if !DEBUG
		Shown += async (_, _) => await upgrader.CheckForUpgradeAsync(upgradeAvailable);
#endif
	}

	private void setProgressVisible(bool visible) => upgradeLbl.Visible = upgradePb.Visible = visible;

}
