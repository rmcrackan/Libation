using LibationFileManager;

namespace LibationWinForms.Dialogs;

public partial class SettingsDialog
{
	private void Load_ImportLibrary(Configuration config)
	{
		this.autoScanCb.Text = desc(nameof(config.AutoScan));
		this.showImportedStatsCb.Text = desc(nameof(config.ShowImportedStats));
		this.importEpisodesCb.Text = desc(nameof(config.ImportEpisodes));
		this.importPlusTitlesCb.Text = desc(nameof(config.ImportPlusTitles));
		this.downloadEpisodesCb.Text = desc(nameof(config.DownloadEpisodes));
		this.autoDownloadEpisodesCb.Text = desc(nameof(config.AutoDownloadEpisodes));

		autoScanCb.Checked = config.AutoScan;
		showImportedStatsCb.Checked = config.ShowImportedStats;
		importEpisodesCb.Checked = config.ImportEpisodes;
		importPlusTitlesCb.Checked = config.ImportPlusTitles;
		downloadEpisodesCb.Checked = config.DownloadEpisodes;
		autoDownloadEpisodesCb.Checked = config.AutoDownloadEpisodes;
	}
	private void Save_ImportLibrary(Configuration config)
	{
		config.AutoScan = autoScanCb.Checked;
		config.ShowImportedStats = showImportedStatsCb.Checked;
		config.ImportEpisodes = importEpisodesCb.Checked;
		config.ImportPlusTitles = importPlusTitlesCb.Checked;
		config.DownloadEpisodes = downloadEpisodesCb.Checked;
		config.AutoDownloadEpisodes = autoDownloadEpisodesCb.Checked;
	}
}
