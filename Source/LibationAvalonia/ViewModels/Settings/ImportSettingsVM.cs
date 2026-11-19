using LibationFileManager;
using LibationUiBase;

namespace LibationAvalonia.ViewModels.Settings;

public class ImportSettingsVM
{
	public ImportSettingsVM(Configuration config)
	{
		AutoScan = config.AutoScan;
		ShowImportedStats = config.ShowImportedStats;
		ImportEpisodes = config.ImportEpisodes;
		ImportPlusTitles = config.ImportPlusTitles;
		DownloadEpisodes = config.DownloadEpisodes;
		AutoDownloadEpisodes = config.AutoDownloadEpisodes;
	}

	public void SaveSettings(Configuration config)
	{
		config.AutoScan = AutoScan;
		config.ShowImportedStats = ShowImportedStats;
		config.ImportEpisodes = ImportEpisodes;
		config.ImportPlusTitles = ImportPlusTitles;
		config.DownloadEpisodes = DownloadEpisodes;
		config.AutoDownloadEpisodes = AutoDownloadEpisodes;
	}

	public string AutoScanText { get; } = SettingsText.Label(nameof(Configuration.AutoScan));
	public string ShowImportedStatsText { get; } = SettingsText.Label(nameof(Configuration.ShowImportedStats));
	public string ImportEpisodesText { get; } = SettingsText.Label(nameof(Configuration.ImportEpisodes));
	public string ImportPlusTitlesText { get; } = SettingsText.Label(nameof(Configuration.ImportPlusTitles));
	public string ImportPlusTitlesTip => Configuration.ImportPlusTitlesToolTip;
	public string DownloadEpisodesText { get; } = SettingsText.Label(nameof(Configuration.DownloadEpisodes));
	public string AutoDownloadEpisodesText { get; } = SettingsText.Label(nameof(Configuration.AutoDownloadEpisodes));

	public bool AutoScan { get; set; }
	public bool ShowImportedStats { get; set; }
	public bool ImportEpisodes { get; set; }
	public bool ImportPlusTitles { get; set; }
	public bool DownloadEpisodes { get; set; }
	public bool AutoDownloadEpisodes { get; set; }
}
