using Avalonia.Threading;
using LibationAvalonia.Dialogs;
using LibationUiBase;
using System.Threading.Tasks;

namespace LibationAvalonia.Views
{
    public partial class MainWindow
    {
		private void Configure_Update()
		{
			setProgressVisible(false);
#if !DEBUG
			var upgrader = new Upgrader();
			upgrader.DownloadProgress += async (_, e) => await Dispatcher.UIThread.InvokeAsync(() => _viewModel.DownloadProgress = e.ProgressPercentage);
			upgrader.DownloadBegin += async (_, _) => await Dispatcher.UIThread.InvokeAsync(() => setProgressVisible(false));
			upgrader.DownloadCompleted += async (_, _) => await Dispatcher.UIThread.InvokeAsync(() => setProgressVisible(true));

			Opened += async (_, _) => await upgrader.CheForUpgradeAsync(UpgradeAvailable);
#endif
		}

		private void setProgressVisible(bool visible) => _viewModel.DownloadProgress = visible ? 0 : null;

		private async Task UpgradeAvailable(UpgradeEventArgs e)
		{
			var notificationResult = await new UpgradeNotificationDialog(e.UpgradeProperties, e.CapUpgrade).ShowDialog<DialogResult>(this);

			e.Ignore = notificationResult == DialogResult.Ignore;
			e.InstallUpdate = notificationResult == DialogResult.OK;
		}
	}
}
