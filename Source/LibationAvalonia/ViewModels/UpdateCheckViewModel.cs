using AppScaffolding;
using Avalonia.Controls;
using Avalonia.Threading;
using LibationAvalonia.Dialogs;
using LibationAvalonia.Views;
using LibationUiBase;
using LibationUiBase.Forms;
using System.Threading.Tasks;

namespace LibationAvalonia.ViewModels;

/// <summary>
/// Coordinates the Avalonia-only "check for updates" UX so the About dialog and the macOS app menu
/// share the same in-flight state, status text, and upgrade prompt behavior.
/// </summary>
public class UpdateCheckViewModel : ViewModelBase
{
	private NativeMenuItem? nativeMenuItem;

	/// <summary>
	/// Prevents overlapping release checks from the About dialog button and the native macOS menu item.
	/// </summary>
	public bool CanCheckForUpgrade
	{
		get => field;
		private set
		{
			this.RaiseAndSetIfChanged(ref field, value);
			// Native menu items are not data-bound, so keep the macOS app-menu item in sync manually.
			if (nativeMenuItem is not null)
				nativeMenuItem.IsEnabled = value;
		}
	} = true;

	/// <summary>
	/// Mirrors the existing About dialog button copy so manual checks can report the most recent outcome.
	/// </summary>
	public string UpgradeButtonText { get => field; private set => this.RaiseAndSetIfChanged(ref field, value); } = "Check for Upgrade";

	/// <summary>
	/// Registers the native macOS app-menu item so its enabled state tracks the shared update-check state.
	/// </summary>
	public void AttachNativeMenuItem(NativeMenuItem menuItem)
	{
		nativeMenuItem = menuItem;
		nativeMenuItem.IsEnabled = CanCheckForUpgrade;
	}

	/// <summary>
	/// Runs a user-initiated update check and, when a new release is available, presents the existing
	/// upgrade dialog owned by <paramref name="owner"/>. <paramref name="mainWindow"/> is optional so
	/// callers can still forward progress to the main window's status bar when available.
	/// </summary>
	public async Task CheckForUpgradeAsync(Window owner, MainWindow? mainWindow = null)
	{
		if (!CanCheckForUpgrade)
			return;

		CanCheckForUpgrade = false;

		var upgrader = new Upgrader();
		upgrader.DownloadProgress += async (_, e) => await Dispatcher.UIThread.InvokeAsync(() => mainWindow?.ViewModel?.DownloadProgress = e.ProgressPercentage);
		upgrader.DownloadCompleted += async (_, _) => await Dispatcher.UIThread.InvokeAsync(() => mainWindow?.ViewModel?.DownloadProgress = null);
		upgrader.UpgradeFailed += async (_, _) => await Dispatcher.UIThread.InvokeAsync(() => mainWindow?.ViewModel?.DownloadProgress = null);

		try
		{
			var result = await upgrader.CheckForUpgradeAsync(OnUpgradeAvailable);
			UpgradeButtonText = result.Outcome switch
			{
				VersionCheckOutcome.UpToDate => "Libation is up to date. Check Again.",
				VersionCheckOutcome.UnableToDetermine => "Unable to check for updates. Try again later.",
				VersionCheckOutcome.UpdateAvailable when result.UpgradeProperties is { } p => $"Version {p.LatestRelease:3} is available",
				_ => "Check for Upgrade"
			};
		}
		finally
		{
			CanCheckForUpgrade = true;
		}

		async Task OnUpgradeAvailable(UpgradeEventArgs e)
		{
			var notificationResult = await new UpgradeNotificationDialog(e.UpgradeProperties, e.CapUpgrade).ShowDialogAsync(owner);

			e.Ignore = notificationResult == DialogResult.Ignore;
			e.InstallUpgrade = notificationResult == DialogResult.OK;
		}
	}
}
