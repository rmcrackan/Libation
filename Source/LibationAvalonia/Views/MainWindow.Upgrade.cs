using Avalonia.Threading;
using LibationAvalonia.Dialogs;
using LibationUiBase;
using System.Threading.Tasks;

namespace LibationAvalonia.Views
{
	public partial class MainWindow
	{
		private void Configure_Upgrade()
		{
			setProgressVisible(false);
#if !DEBUG
			async Task upgradeAvailable(UpgradeEventArgs e)
			{
				var notificationResult = await new UpgradeNotificationDialog(e.UpgradeProperties, e.CapUpgrade).ShowDialogAsync(this);

				e.Ignore = notificationResult == DialogResult.Ignore;
				e.InstallUpgrade = notificationResult == DialogResult.OK;
			}

			var upgrader = new Upgrader();
			upgrader.DownloadProgress += async (_, e) => await Dispatcher.UIThread.InvokeAsync(() => _viewModel.DownloadProgress = e.ProgressPercentage);
			upgrader.DownloadBegin += async (_, _) => await Dispatcher.UIThread.InvokeAsync(() => setProgressVisible(true));
			upgrader.DownloadCompleted += async (_, _) => await Dispatcher.UIThread.InvokeAsync(() => setProgressVisible(false));

			Opened += async (_, _) => await upgrader.CheckForUpgradeAsync(upgradeAvailable);
#endif
		}

		private void setProgressVisible(bool visible) => _viewModel.DownloadProgress = visible ? 0 : null;

	}
}
