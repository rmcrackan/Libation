using AudibleApi;
using LibationFileManager;
using LibationUiBase;
using System.Linq;

namespace LibationWinForms.Dialogs;

public partial class SettingsDialog
{
	private void Load_ImportLibrary(Configuration config)
	{
		this.autoScanCb.Text = desc(nameof(config.AutoScan));
		this.showImportedStatsCb.Text = desc(nameof(config.ShowImportedStats));
		this.useWebViewCb.Text = desc(nameof(config.UseWebView));
		this.deviceRegistrationLbl.Text = DeviceRegistrationSettingsUi.SettingLabel;
		this.importEpisodesCb.Text = desc(nameof(config.ImportEpisodes));
		this.importPlusTitlesCb.Text = desc(nameof(config.ImportPlusTitles));
		toolTip.SetToolTip(importPlusTitlesCb, Configuration.ImportPlusTitlesToolTip);
		this.downloadEpisodesCb.Text = desc(nameof(config.DownloadEpisodes));
		this.autoDownloadEpisodesCb.Text = desc(nameof(config.AutoDownloadEpisodes));

		autoScanCb.Checked = config.AutoScan;
		showImportedStatsCb.Checked = config.ShowImportedStats;
		useWebViewCb.Checked = config.UseWebView;
		importEpisodesCb.Checked = config.ImportEpisodes;
		importPlusTitlesCb.Checked = config.ImportPlusTitles;
		downloadEpisodesCb.Checked = config.DownloadEpisodes;
		autoDownloadEpisodesCb.Checked = config.AutoDownloadEpisodes;

		deviceRegistrationCb.Items.Clear();
		deviceRegistrationCb.Items.AddRange(DeviceRegistrationSettingsUi.Options.Cast<object>().ToArray());
		deviceRegistrationCb.SelectedItem = DeviceRegistrationSettingsUi.Display(config.DeviceRegistrationKind);
		toolTip.SetToolTip(deviceRegistrationLbl, Configuration.GetHelpText(nameof(config.DeviceRegistrationKind)));
		toolTip.SetToolTip(deviceRegistrationCb, DeviceRegistrationSettingsUi.ReLoginNote);
	}
	private void Save_ImportLibrary(Configuration config)
	{
		config.AutoScan = autoScanCb.Checked;
		config.ShowImportedStats = showImportedStatsCb.Checked;
		config.ImportEpisodes = importEpisodesCb.Checked;
		config.ImportPlusTitles = importPlusTitlesCb.Checked;
		config.DownloadEpisodes = downloadEpisodesCb.Checked;
		config.AutoDownloadEpisodes = autoDownloadEpisodesCb.Checked;
		config.UseWebView = useWebViewCb.Checked;
		config.DeviceRegistrationKind = (deviceRegistrationCb.SelectedItem as EnumDisplay<DeviceRegistrationKind>)?.Value
			?? DeviceRegistrationKind.CurrentAndroid;
	}
}
