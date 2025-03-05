using LibationFileManager;

#nullable enable
namespace LibationAvalonia.ViewModels.Settings
{
	public class ImportSettingsVM
	{
		public ImportSettingsVM(Configuration config)
		{
			AutoScan = config.AutoScan;
			ShowImportedStats = config.ShowImportedStats;
			ImportEpisodes = config.ImportEpisodes;
			DownloadEpisodes = config.DownloadEpisodes;
			AutoDownloadEpisodes = config.AutoDownloadEpisodes;
		}

		public void SaveSettings(Configuration config)
		{
			config.AutoScan = AutoScan;
			config.ShowImportedStats = ShowImportedStats;
			config.ImportEpisodes = ImportEpisodes;
			config.DownloadEpisodes = DownloadEpisodes;
			config.AutoDownloadEpisodes = AutoDownloadEpisodes;
		}

		public string AutoScanText { get; } = Configuration.GetDescription(nameof(Configuration.AutoScan));
		public string ShowImportedStatsText { get; } = Configuration.GetDescription(nameof(Configuration.ShowImportedStats));
		public string ImportEpisodesText { get; } = Configuration.GetDescription(nameof(Configuration.ImportEpisodes));
		public string DownloadEpisodesText { get; } = Configuration.GetDescription(nameof(Configuration.DownloadEpisodes));
		public string AutoDownloadEpisodesText { get; } = Configuration.GetDescription(nameof(Configuration.AutoDownloadEpisodes));

		public bool AutoScan { get; set; }
		public bool ShowImportedStats { get; set; }
		public bool ImportEpisodes { get; set; }
		public bool DownloadEpisodes { get; set; }
		public bool AutoDownloadEpisodes { get; set; }
	}
}
