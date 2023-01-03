using AppScaffolding;
using Avalonia.Controls;
using Dinah.Core;
using System;

namespace LibationAvalonia.Dialogs
{
	public partial class UpgradeNotification : Window
	{
		public string VersionText { get; }
		public string DownloadLinkText { get; }
		private string PackageUrl { get; }
		public UpgradeNotification()
		{
			if (Design.IsDesignMode)
			{
				VersionText = "Libation version 8.7.0 is now available.";
				DownloadLinkText = "Download Libation.8.7.0-macos-chardonnay.tar.gz";
				DataContext = this;
			}

			InitializeComponent();
		}

		public UpgradeNotification(UpgradeProperties upgradeProperties) : this()
		{
			VersionText = $"Libation version {upgradeProperties.LatestRelease.ToString(3)} is now available.";
			PackageUrl = upgradeProperties.ZipUrl;
			DownloadLinkText = $"Download {upgradeProperties.ZipName}";
			DataContext = this;
		}

		public void OK_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e) => Close(DialogResult.OK);
		public void DontRemind_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e) => Close(DialogResult.Ignore);
		public void Download_Tapped(object sender, Avalonia.Input.TappedEventArgs e)
			=> Go.To.Url(PackageUrl);
		public void Website_Tapped(object sender, Avalonia.Input.TappedEventArgs e)
			=> Go.To.Url("ht" + "tps://getlibation.com");
		public void Github_Tapped(object sender, Avalonia.Input.TappedEventArgs e)
			=> Go.To.Url("ht" + "tps://github.com/rmcrackan/Libation");
	}
}
