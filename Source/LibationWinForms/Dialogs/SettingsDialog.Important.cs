using AudibleApi.Authorization;
using AudibleUtilities;
using Dinah.Core;
using FileManager;
using LibationFileManager;
using LibationUiBase;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LibationWinForms.Dialogs;

public partial class SettingsDialog
{
	private void logsBtn_Click(object sender, EventArgs e)
	{
		if (File.Exists(LogFileFilter.LogFilePath))
			Go.To.File(LogFileFilter.LogFilePath);
		else
			Go.To.Folder(Configuration.Instance.LibationFiles.Location.ShortPathName);
	}
	private Configuration.Theme themeVariant;
	private Configuration.Theme initialThemeVariant;
	private TokenStorageMethod initialTokenStorageMethod;
	private bool osSecretStoreAvailable;

	private void Load_Important(Configuration config)
	{
		{
			loggingLevelCb.Items.Clear();
			foreach (var level in Enum<Serilog.Events.LogEventLevel>.GetValues())
				loggingLevelCb.Items.Add(level);
			loggingLevelCb.SelectedItem = config.LogLevel;
		}

		booksLocationDescLbl.Text = desc(nameof(config.Books));
		saveEpisodesToSeriesFolderCbox.Text = desc(nameof(config.SavePodcastsToParentFolder));
		overwriteExistingCbox.Text = desc(nameof(config.OverwriteExisting));
		creationTimeLbl.Text = desc(nameof(config.CreationTime));
		lastWriteTimeLbl.Text = desc(nameof(config.LastWriteTime));
		gridScaleFactorLbl.Text = desc(nameof(config.GridScaleFactor));
		gridFontScaleFactorLbl.Text = desc(nameof(config.GridFontScaleFactor));

		var dateTimeSources = Enum.GetValues<Configuration.DateTimeSource>().Select(v => new EnumDisplay<Configuration.DateTimeSource>(v)).ToArray();
		creationTimeCb.Items.AddRange(dateTimeSources);
		lastWriteTimeCb.Items.AddRange(dateTimeSources);

		creationTimeCb.SelectedItem = dateTimeSources.SingleOrDefault(v => v.Value == config.CreationTime) ?? dateTimeSources[0];
		lastWriteTimeCb.SelectedItem = dateTimeSources.SingleOrDefault(v => v.Value == config.LastWriteTime) ?? dateTimeSources[0];

		themeVariant = initialThemeVariant = config.ThemeVariant;
		var themes = Enum.GetValues<Configuration.Theme>().Select(v => new EnumDisplay<Configuration.Theme>(v)).ToArray();
		themeCb.Items.AddRange(themes);
		themeCb.SelectedItem = themes.SingleOrDefault(v => v.Value == themeVariant) ?? themes[0];

		booksSelectControl.SetSearchTitle("books location");
		booksSelectControl.SetDirectoryItems(
			new()
			{
				Configuration.KnownDirectories.UserProfile,
				Configuration.KnownDirectories.AppDir,
				Configuration.KnownDirectories.MyDocs,
				Configuration.KnownDirectories.MyMusic,
			},
			Configuration.KnownDirectories.UserProfile,
			"Books");
		booksSelectControl.SelectDirectory(config.Books?.PathWithoutPrefix ?? "");

		saveEpisodesToSeriesFolderCbox.Checked = config.SavePodcastsToParentFolder;
		overwriteExistingCbox.Checked = config.OverwriteExisting;
		gridScaleFactorTbar.Value = scaleFactorToLinearRange(config.GridScaleFactor);
		gridFontScaleFactorTbar.Value = scaleFactorToLinearRange(config.GridFontScaleFactor);

		Load_TokenStorage(config);
	}

	private void Load_TokenStorage(Configuration config)
	{
		initialTokenStorageMethod = config.TokenStorageMethod;
		osSecretStoreAvailable = IdentityTokenStorageWiring.IsOsSecretStoreAvailable(out var unavailableReason);

		encryptTokensCbox.Text = TokenStorageSettingsUi.CheckboxText;
		plaintextTokenWarningLbl.Text = TokenStorageSettingsUi.PlaintextWarningText;
		exportMasterKeyBtn.Text = TokenStorageSettingsUi.ExportButtonText;
		updateExistingTokensBtn.Text = TokenStorageSettingsUi.ConvertButtonText;
		toolTip.SetToolTip(exportMasterKeyBtn, TokenStorageSettingsUi.ExportButtonToolTip);
		toolTip.SetToolTip(updateExistingTokensBtn, TokenStorageSettingsUi.ConvertButtonToolTip);

		if (!osSecretStoreAvailable)
		{
			osSecretStoreUnavailableLbl.Text = TokenStorageSettingsUi.OsStoreUnavailableMessage(unavailableReason);
			osSecretStoreUnavailableLbl.Visible = true;
			encryptTokensCbox.Checked = false;
			encryptTokensCbox.Enabled = false;
			exportMasterKeyBtn.Enabled = false;
		}
		else
		{
			osSecretStoreUnavailableLbl.Visible = false;
			encryptTokensCbox.Enabled = true;
			exportMasterKeyBtn.Enabled = true;
			encryptTokensCbox.Checked = TokenStorageSettingsUi.CheckboxFromMethod(config.TokenStorageMethod);
		}

		RefreshTokenStorageUiState();
	}

	private TokenStorageMethod SelectedTokenStorageMethod
		=> TokenStorageSettingsUi.MethodFromCheckbox(encryptTokensCbox.Checked);

	private void RefreshTokenStorageUiState()
	{
		var method = SelectedTokenStorageMethod;
		plaintextTokenWarningLbl.Visible = osSecretStoreAvailable && method == TokenStorageMethod.Plaintext;

		var alignment = AccountTokenStorage.GetAccountsAlignment(method);
		updateExistingTokensBtn.Enabled = TokenStorageSettingsUi.IsConvertButtonEnabled(alignment);
	}

	private void encryptTokensCbox_CheckedChanged(object? sender, EventArgs e)
		=> RefreshTokenStorageUiState();

	private async void exportMasterKeyBtn_Click(object? sender, EventArgs e)
	{
		if (!await TokenStorageSettingsUi.ConfirmExportMasterKeyAsync(this))
			return;

		using var dialog = new SaveFileDialog
		{
			Title = TokenStorageSettingsUi.ExportConfirmCaption,
			FileName = IdentityTokenStorageWiring.DefaultMasterKeyFileName,
			Filter = "Master key (*.key)|*.key|All files (*.*)|*.*",
			OverwritePrompt = true
		};

		if (dialog.ShowDialog(this) != DialogResult.OK)
			return;

		await TokenStorageSettingsUi.ExportMasterKeyToFileAsync(this, dialog.FileName);
	}

	private async void updateExistingTokensBtn_Click(object? sender, EventArgs e)
	{
		var converted = await TokenStorageSettingsUi.ConfirmAndConvertAsync(this, SelectedTokenStorageMethod);
		if (converted)
			RefreshTokenStorageUiState();
	}

	private async Task<bool> Save_Important(Configuration config)
	{
		var newBooks = booksSelectControl.SelectedDirectory;

		#region validation
		static void validationError(string text, string caption)
			=> MessageBox.Show(text, caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
		if (string.IsNullOrWhiteSpace(newBooks))
		{
			validationError("Cannot set Books Location to blank", "Location is blank");
			return false;
		}
		LongPath lonNewBooks = newBooks;
		if (!Directory.Exists(lonNewBooks))
		{
			try
			{
				Directory.CreateDirectory(lonNewBooks);
			}
			catch (Exception ex)
			{
				validationError($"Error creating Books Location:\r\n{ex.Message}", "Error creating directory");
				return false;
			}
		}
		#endregion

		var selectedTokenMethod = SelectedTokenStorageMethod;
		var tokenDecision = await TokenStorageSettingsUi.PromptOnPreferenceSaveAsync(this, initialTokenStorageMethod, selectedTokenMethod);
		if (tokenDecision == TokenStorageSaveDecision.Abort)
			return false;

		config.Books = newBooks;

		{
			var logLevelOld = config.LogLevel;
			var logLevelNew = (loggingLevelCb.SelectedItem as Serilog.Events.LogEventLevel?) ?? Serilog.Events.LogEventLevel.Information;

			config.LogLevel = logLevelNew;

			// only warn if changed during this time. don't want to warn every time user happens to change settings while level is verbose
			if (logLevelOld != logLevelNew)
				MessageBoxLib.VerboseLoggingWarning_ShowIfTrue();
		}

		config.SavePodcastsToParentFolder = saveEpisodesToSeriesFolderCbox.Checked;
		config.OverwriteExisting = overwriteExistingCbox.Checked;

		config.CreationTime = (creationTimeCb.SelectedItem as EnumDisplay<Configuration.DateTimeSource>)?.Value ?? Configuration.DateTimeSource.File;
		config.LastWriteTime = (lastWriteTimeCb.SelectedItem as EnumDisplay<Configuration.DateTimeSource>)?.Value ?? Configuration.DateTimeSource.File;
		config.ThemeVariant = (themeCb.SelectedItem as EnumDisplay<Configuration.Theme>)?.Value ?? Configuration.Theme.System;
		config.TokenStorageMethod = selectedTokenMethod;

		if (tokenDecision == TokenStorageSaveDecision.SavePreferenceAndConvert)
		{
			var converted = await TokenStorageSettingsUi.RunConversionAsync(this, selectedTokenMethod);
			if (!converted)
				return false;
		}

		initialTokenStorageMethod = selectedTokenMethod;
		RefreshTokenStorageUiState();
		return true;
	}

	private static int scaleFactorToLinearRange(float scaleFactor)
		=> (int)float.Round(100 * MathF.Log2(scaleFactor));
	private static float linearRangeToScaleFactor(int value)
		=> MathF.Pow(2, value / 100f);

	private void applyDisplaySettingsBtn_Click(object sender, EventArgs e)
	{
		config.GridFontScaleFactor = linearRangeToScaleFactor(gridFontScaleFactorTbar.Value);
		config.GridScaleFactor = linearRangeToScaleFactor(gridScaleFactorTbar.Value);
	}

	private void themeCb_SelectedIndexChanged(object? sender, EventArgs e)
	{
		var selected = themeCb.SelectedItem as EnumDisplay<Configuration.Theme>;
		if (selected != null)
		{
			themeVariant = selected.Value;
			themeLbl.Visible = themeVariant != initialThemeVariant;
		}
	}
}
