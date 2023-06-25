using LibationFileManager;
using LibationUiBase;
using System;
using System.Linq;

namespace LibationWinForms.Dialogs
{
	public partial class SettingsDialog
	{
		private void Load_ImportLibrary(Configuration config)
		{
			this.autoScanCb.Text = desc(nameof(config.AutoScan));
			this.showImportedStatsCb.Text = desc(nameof(config.ShowImportedStats));
			this.importEpisodesCb.Text = desc(nameof(config.ImportEpisodes));
			this.downloadEpisodesCb.Text = desc(nameof(config.DownloadEpisodes));
			this.autoDownloadEpisodesCb.Text = desc(nameof(config.AutoDownloadEpisodes));
			creationTimeLbl.Text = desc(nameof(config.CreationTime));
			lastWriteTimeLbl.Text = desc(nameof(config.LastWriteTime));

			var dateTimeSources = Enum.GetValues<Configuration.DateTimeSource>().Select(v => new EnumDiaplay<Configuration.DateTimeSource>(v)).ToArray();
			creationTimeCb.Items.AddRange(dateTimeSources);
			lastWriteTimeCb.Items.AddRange(dateTimeSources);

			creationTimeCb.SelectedItem = dateTimeSources.SingleOrDefault(v => v.Value == config.CreationTime) ?? dateTimeSources[0];
			lastWriteTimeCb.SelectedItem = dateTimeSources.SingleOrDefault(v => v.Value == config.LastWriteTime) ?? dateTimeSources[0];

			autoScanCb.Checked = config.AutoScan;
			showImportedStatsCb.Checked = config.ShowImportedStats;
			importEpisodesCb.Checked = config.ImportEpisodes;
			downloadEpisodesCb.Checked = config.DownloadEpisodes;
			autoDownloadEpisodesCb.Checked = config.AutoDownloadEpisodes;
		}
		private void Save_ImportLibrary(Configuration config)
		{
			config.CreationTime = ((EnumDiaplay<Configuration.DateTimeSource>)creationTimeCb.SelectedItem).Value;
			config.LastWriteTime = ((EnumDiaplay<Configuration.DateTimeSource>)lastWriteTimeCb.SelectedItem).Value;

			config.AutoScan = autoScanCb.Checked;
			config.ShowImportedStats = showImportedStatsCb.Checked;
			config.ImportEpisodes = importEpisodesCb.Checked;
			config.DownloadEpisodes = downloadEpisodesCb.Checked;
			config.AutoDownloadEpisodes = autoDownloadEpisodesCb.Checked;
		}
	}
}
