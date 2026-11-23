using Avalonia.Controls;
using FileManager;
using LibationAvalonia.ViewModels.Settings;
using LibationFileManager;
using LibationUiBase;
using LibationUiBase.Forms;
using System;
using System.Threading.Tasks;

namespace LibationAvalonia.Dialogs;

public partial class SettingsDialog : DialogWindow
{
	private readonly SettingsVM settingsDisp;

	private readonly Configuration config = Design.IsDesignMode ? Configuration.CreateMockInstance() : Configuration.Instance;
	public SettingsDialog()
	{
		InitializeComponent();

		DataContext = settingsDisp = new(config);
		sectionList.SelectedIndex = 0;
	}

	protected override async Task SaveAndCloseAsync()
	{
		#region validation

		if (string.IsNullOrWhiteSpace(settingsDisp.ImportantSettings.BooksDirectory))
		{
			await MessageBox.Show(this.GetParentWindow(), "Cannot set Books Location to blank", "Location is blank", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

	private void SectionList_SelectionChanged(object? sender, SelectionChangedEventArgs e)
	{
		if (sectionList.SelectedItem is not ListBoxItem item)
			return;

		sectionTitle.Text = item.Content as string;
		sectionDescription.Text = item.Tag as string;

		Control[] sections = [generalSection, librarySection, downloadsSection, namingSection, audioSection, audiobookshelfSection];
		for (var i = 0; i < sections.Length; i++)
			sections[i].IsVisible = i == sectionList.SelectedIndex;
	}

	public void CancelButton_Clicked(object sender, Avalonia.Interactivity.RoutedEventArgs e)
		=> CancelAndClose();

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
				await MessageBox.Show(this, $"Error creating Books Location:\n\n{ex.Message}", "Error creating directory", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}
		}
		await SaveAndCloseAsync();
	}
}
