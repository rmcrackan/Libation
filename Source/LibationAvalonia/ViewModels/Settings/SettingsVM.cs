using LibationFileManager;

namespace LibationAvalonia.ViewModels.Settings;

public class SettingsVM
{
	public SettingsVM(Configuration config)
	{
		ImportantSettings = new ImportantSettingsVM(config);
		ImportSettings = new ImportSettingsVM(config);
		DownloadDecryptSettings = new DownloadDecryptSettingsVM(config);
		AudioSettings = new AudioSettingsVM(config);
	}

	public ImportantSettingsVM ImportantSettings { get; private set; }
	public ImportSettingsVM ImportSettings { get; private set; }
	public DownloadDecryptSettingsVM DownloadDecryptSettings { get; private set; }
	public AudioSettingsVM AudioSettings { get; private set; }

	public void SaveSettings(Configuration config)
	{
		ImportantSettings.SaveSettings(config);
		ImportSettings.SaveSettings(config);
		DownloadDecryptSettings.SaveSettings(config);
		AudioSettings.SaveSettings(config);
	}
}
