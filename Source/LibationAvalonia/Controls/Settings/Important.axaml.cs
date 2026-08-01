using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using AudibleUtilities;
using LibationAvalonia.Dialogs;
using LibationAvalonia.ViewModels.Settings;
using LibationFileManager;
using LibationUiBase;
using System.Linq;

namespace LibationAvalonia.Controls.Settings;

public partial class Important : UserControl
{
	private ImportantSettingsVM? ViewModel => DataContext as ImportantSettingsVM;
	public Important()
	{
		InitializeComponent();
		if (Design.IsDesignMode)
		{
			DataContext = new ImportantSettingsVM(Configuration.CreateMockInstance());
		}

		ThemeComboBox.SelectionChanged += ThemeComboBox_SelectionChanged;
	}

	private async void ExportMasterKey_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
	{
		var owner = this.GetParentWindow();
		if (!await TokenStorageSettingsUi.ConfirmExportMasterKeyAsync(owner))
			return;

		var options = new FilePickerSaveOptions
		{
			Title = TokenStorageSettingsUi.ExportConfirmCaption,
			SuggestedFileName = IdentityTokenStorageWiring.DefaultMasterKeyFileName,
			DefaultExtension = "key",
			ShowOverwritePrompt = true,
			FileTypeChoices =
			[
				new("Master key (*.key)") { Patterns = ["*.key"] },
				new("All files (*.*)") { Patterns = ["*"] }
			]
		};

		var selectedFile = (await owner.StorageProvider.SaveFilePickerAsync(options))?.TryGetLocalPath();
		if (selectedFile is null)
			return;

		await TokenStorageSettingsUi.ExportMasterKeyToFileAsync(owner, selectedFile);
	}

	private void EditThemeColors_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
	{
		if (App.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime lifetime)
		{
			//Only allow a single instance of the theme picker
			//Show it as a window, not a dialog, so users can preview
			//their changes throughout the entire app.
			if (lifetime.Windows.OfType<ThemePickerDialog>().FirstOrDefault() is ThemePickerDialog dialog)
			{
				dialog.BringIntoView();
			}
			else
			{
				var themePicker = new ThemePickerDialog();
				themePicker.Show();
			}
		}
	}

	private void ThemeComboBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
	{
		//Remove the combo box before changing the theme, then re-add it.
		//This is a workaround to a crash that will happen if the theme
		//is changed while the combo box is open
		ThemeComboBox.SelectionChanged -= ThemeComboBox_SelectionChanged;
		var parent = ThemeComboBox.Parent as Panel;
		if (parent?.Children.Remove(ThemeComboBox) ?? false)
		{

			Configuration.Instance.ThemeVariant = ViewModel?.ThemeVariant.Value ?? Configuration.Theme.System;
			parent.Children.Add(ThemeComboBox);
		}
		ThemeComboBox.SelectionChanged += ThemeComboBox_SelectionChanged;
	}
}
