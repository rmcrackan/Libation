using AppScaffolding;
using Avalonia.Controls;
using Dinah.Core;
using LibationFileManager;
using LibationUiBase.Forms;

namespace LibationAvalonia.Dialogs;

public partial class UpgradeNotificationDialog : DialogWindow
{
	private const string UpdateMessage = "There is a new version available. Would you like to update?\r\n\r\nAfter you close Libation, the upgrade will start automatically.";
	public string? TopMessage { get; }
	public string? DownloadLinkText { get; }
	public string? ReleaseNotes { get; }
	public string? OkText { get; }
	private string? PackageUrl { get; }
	private bool CanUpgrade { get; } = true;
	public UpgradeNotificationDialog()
	{
		if (Design.IsDesignMode)
		{
			TopMessage = UpdateMessage;
			Title = "Libation version 8.7.0 is now available.";
			DownloadLinkText = "\r\nLibation.12.7.0-macOS-chardonnay-arm64.tgz ";
			ReleaseNotes = "New features:\r\n\r\n* 'Remove' now removes forever. Removed books won't be re-added on next scan\r\n* #406 : Right Click Menu for Stop-Light Icon\r\n* #398 : Grid, right-click, copy\r\n* Add option for user to choose custom temp folder\r\n* Build Docker image\r\n\r\nEnhancements\r\n\r\n* Illegal Char Replace dialog in Chardonnay\r\n* Filename character replacement allows replacing any char, not just illegal\r\n* #352 : Better error messages for license denial\r\n* Improve 'cancel download'\r\n\r\nThanks to @Mbucari (u/MSWMan), @pixil98 (u/pixil)\r\n\r\nLibation is a free, open source audible library manager for Windows. Decrypt, backup, organize, and search your audible library\r\n\r\nI intend to keep Libation free and open source, but if you want to leave a tip, who am I to argue?";
			OkText = "Yes";
			DataContext = this;
		}

		InitializeComponent();
		ControlToFocusOnShow = btnYes;
	}

	public UpgradeNotificationDialog(UpgradeProperties upgradeProperties, bool canUpgrade, string? upgradeUnavailableReason = null) : this()
	{
		Title = $"Libation version {upgradeProperties.LatestRelease.ToVersionString()} is now available.";
		CanUpgrade = canUpgrade;
		PackageUrl = upgradeProperties.ZipUrl;
		DownloadLinkText = upgradeProperties.ZipName;
		ReleaseNotes = upgradeProperties.Notes;
		TopMessage = canUpgrade ? UpdateMessage : upgradeUnavailableReason ?? "";
		OkText = canUpgrade ? "Yes" : "OK";
		DataContext = this;
	}

	// When Libation cannot install the upgrade itself, this button reads "OK" and the dialog is a
	// notice with a download link. Reporting OK there would be read as "yes, install it", starting a
	// download and an install that was never on offer, so acknowledging a notice closes and no more.
	public void OK_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
		=> Close(CanUpgrade ? DialogResult.OK : DialogResult.Cancel);
	public void DontRemind_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e) => Close(DialogResult.Ignore);
	public void Download_Tapped(object sender, Avalonia.Input.TappedEventArgs e)
		=> Go.To.Url(PackageUrl);
	public void Website_Tapped(object sender, Avalonia.Input.TappedEventArgs e)
		=> Go.To.Url(LibationScaffolding.WebsiteUrl);
	public void Github_Tapped(object sender, Avalonia.Input.TappedEventArgs e)
		=> Go.To.Url(LibationScaffolding.RepositoryUrl);
}
