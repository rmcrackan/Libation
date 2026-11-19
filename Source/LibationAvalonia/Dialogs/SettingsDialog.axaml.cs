using Avalonia.Controls;
using FileManager;
using LibationAvalonia.ViewModels.Settings;
using LibationFileManager;
using LibationUiBase;
using LibationUiBase.Forms;
using System;
using System.Threading.Tasks;
using LibationUiBase.Resources;

namespace LibationAvalonia.Dialogs;

public partial class SettingsDialog : DialogWindow
{
	private readonly SettingsVM settingsDisp;

	private readonly Configuration config = Design.IsDesignMode ? Configuration.CreateMockInstance() : Configuration.Instance;
	public SettingsDialog()
	{
		InitializeComponent();

		DataContext = settingsDisp = new(config);
	}

	protected override async Task SaveAndCloseAsync()
	{
		#region validation

		if (string.IsNullOrWhiteSpace(settingsDisp.ImportantSettings.BooksDirectory))
		{
			await MessageBox.Show(this.GetParentWindow(), Strings.SettingsDialog_CannotSetBooksLocationToBlank, Strings.SettingsDialog_LocationIsBlank, MessageBoxButtons.OK, MessageBoxIcon.Error);
			return;
		}

		#endregion

		var tokenDecision = await settingsDisp.ImportantSettings.PromptOnSaveAsync(this);
		if (tokenDecision == TokenStorageSaveDecision.Abort)
			return;

		settingsDisp.SaveSettings(config);

		if (tokenDecision == TokenStorageSaveDecision.SavePreferenceAndConvert)
		{
			var converted = await settingsDisp.ImportantSettings.ConvertAfterSaveAsync(this);
			if (!converted)
				return;
		}

		await MessageBox.VerboseLoggingWarning_ShowIfTrue();
		await base.SaveAndCloseAsync();
	}

	public async void SaveButton_Clicked(object sender, Avalonia.Interactivity.RoutedEventArgs e)
	{
		LongPath lonNewBooks = settingsDisp.ImportantSettings.GetBooksDirectory();
		if (!System.IO.Directory.Exists(lonNewBooks))
		{
			try
			{
				System.IO.Directory.CreateDirectory(lonNewBooks);
			}
			catch (Exception ex)
			{
				await MessageBox.Show(this, string.Format(Strings.SettingsDialog_ErrorCreatingBooksLocation, ex.Message), Strings.SettingsDialog_ErrorCreatingDirectory, MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}
		}
		await SaveAndCloseAsync();
	}
}
