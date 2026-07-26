using ApplicationServices;
using AudibleApi.Authorization;
using AudibleUtilities;
using LibationFileManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LibationWinForms.Dialogs;

public partial class SettingsDialog
{
	private List<AudiobookshelfApiService.Library> _absLibraries = [];

	private void Load_Audiobookshelf(Configuration config)
	{
		absEnabledCb.Text = desc(nameof(config.AudiobookshelfEnabled));
		absUrlLbl.Text = desc(nameof(config.AudiobookshelfServerUrl));
		absTokenLbl.Text = desc(nameof(config.AudiobookshelfApiToken));
		absLibraryLbl.Text = desc(nameof(config.AudiobookshelfLibraryId));
		absFolderLbl.Text = desc(nameof(config.AudiobookshelfFolderId));
		absConnectBtn.Text = "Connect / Refresh";
		absStatusLbl.Text = "";

		absEnabledCb.Checked = config.AudiobookshelfEnabled;
		absUrlTb.Text = config.AudiobookshelfServerUrl ?? "";
		absTokenTb.Text = AudiobookshelfTokenStorage.DecryptToken(config.AudiobookshelfApiToken) ?? "";
		absTokenTb.PasswordChar = '*';

		var osSecretStoreAvailable = IdentityTokenStorageWiring.IsOsSecretStoreAvailable(out _);
		absPlaintextWarningLbl.Visible = osSecretStoreAvailable && config.TokenStorageMethod == TokenStorageMethod.Plaintext;

		ToggleAudiobookshelfControls(absEnabledCb.Checked);

		// Try to restore saved selections if we have them
		if (!string.IsNullOrWhiteSpace(config.AudiobookshelfLibraryId))
			_ = RestoreLibraryAndFolderAsync(config);
	}

	private void ToggleAudiobookshelfControls(bool enabled)
	{
		absUrlTb.Enabled = enabled;
		absTokenTb.Enabled = enabled;
		absConnectBtn.Enabled = enabled;
		absLibraryCb.Enabled = enabled && _absLibraries.Count > 0;
		absFolderCb.Enabled = enabled && absLibraryCb.SelectedIndex >= 0;
	}

	private void absEnabledCb_CheckedChanged(object? sender, EventArgs e)
		=> ToggleAudiobookshelfControls(absEnabledCb.Checked);

	private async void absConnectBtn_Click(object? sender, EventArgs e)
	{
		absConnectBtn.Enabled = false;
		absStatusLbl.Text = "Connecting...";
		absStatusLbl.ForeColor = System.Drawing.SystemColors.ControlText;

		try
		{
			var url = absUrlTb.Text.Trim();
			var token = absTokenTb.Text.Trim();

			if (string.IsNullOrWhiteSpace(url) || string.IsNullOrWhiteSpace(token))
			{
				absStatusLbl.Text = "Please enter both server URL and API token.";
				absStatusLbl.ForeColor = System.Drawing.Color.DarkRed;
				return;
			}

			var libraries = await AudiobookshelfApiService.GetLibrariesAsync(url, token);

			if (libraries.Count == 0)
			{
				absStatusLbl.Text = "No book libraries found. Check your settings.";
				absStatusLbl.ForeColor = System.Drawing.Color.DarkRed;
				return;
			}

			_absLibraries = libraries;
			absLibraryCb.Items.Clear();
			absLibraryCb.Items.AddRange(libraries.Select(l => $"{l.Name} ({l.Id})").ToArray());
			absFolderCb.Items.Clear();
			absFolderCb.SelectedIndex = -1;

			absStatusLbl.Text = $"Connected. Found {libraries.Count} library(ies).";
			absStatusLbl.ForeColor = System.Drawing.Color.DarkGreen;
			ToggleAudiobookshelfControls(absEnabledCb.Checked);
		}
		catch (Exception ex)
		{
			absStatusLbl.Text = $"Connection failed: {ex.Message}";
			absStatusLbl.ForeColor = System.Drawing.Color.DarkRed;
		}
		finally
		{
			absConnectBtn.Enabled = true;
		}
	}

	private void absLibraryCb_SelectedIndexChanged(object? sender, EventArgs e)
	{
		absFolderCb.Items.Clear();
		absFolderCb.SelectedIndex = -1;

		if (absLibraryCb.SelectedIndex < 0 || _absLibraries.Count == 0)
			return;

		var library = _absLibraries[absLibraryCb.SelectedIndex];
		foreach (var folder in library.Folders)
		{
			absFolderCb.Items.Add($"{folder.FullPath} ({folder.Id})");
		}

		if (library.Folders.Count > 0)
			absFolderCb.SelectedIndex = 0;
	}

	private async Task RestoreLibraryAndFolderAsync(Configuration config)
	{
		try
		{
			if (string.IsNullOrWhiteSpace(config.AudiobookshelfServerUrl) || string.IsNullOrWhiteSpace(config.AudiobookshelfApiToken))
				return;

			var libraries = await AudiobookshelfApiService.GetLibrariesAsync(config.AudiobookshelfServerUrl, config.AudiobookshelfApiToken);
			_absLibraries = libraries;

			absLibraryCb.Items.Clear();
			absLibraryCb.Items.AddRange(libraries.Select(l => $"{l.Name} ({l.Id})").ToArray());

			var savedLibIndex = libraries.FindIndex(l => l.Id == config.AudiobookshelfLibraryId);
			if (savedLibIndex >= 0)
			{
				absLibraryCb.SelectedIndex = savedLibIndex;
				var library = libraries[savedLibIndex];
				var savedFolderIndex = library.Folders.FindIndex(f => f.Id == config.AudiobookshelfFolderId);
				if (savedFolderIndex >= 0)
					absFolderCb.SelectedIndex = savedFolderIndex;
			}
			ToggleAudiobookshelfControls(absEnabledCb.Checked);
		}
		catch (Exception ex)
		{
			Serilog.Log.Logger.Warning(ex, "Failed to restore Audiobookshelf library and folder selections");
		}
	}

	private void Save_Audiobookshelf(Configuration config)
	{
		config.AudiobookshelfEnabled = absEnabledCb.Checked;
		config.AudiobookshelfServerUrl = absUrlTb.Text.Trim();
		config.AudiobookshelfApiToken = AudiobookshelfTokenStorage.EncryptToken(absTokenTb.Text.Trim());

		if (absLibraryCb.SelectedIndex >= 0 && _absLibraries.Count > absLibraryCb.SelectedIndex)
			config.AudiobookshelfLibraryId = _absLibraries[absLibraryCb.SelectedIndex].Id;
		else
			config.AudiobookshelfLibraryId = null;

		if (absFolderCb.SelectedIndex >= 0 && absLibraryCb.SelectedIndex >= 0)
		{
			var library = _absLibraries[absLibraryCb.SelectedIndex];
			if (library.Folders.Count > absFolderCb.SelectedIndex)
				config.AudiobookshelfFolderId = library.Folders[absFolderCb.SelectedIndex].Id;
			else
				config.AudiobookshelfFolderId = null;
		}
		else
			config.AudiobookshelfFolderId = null;
	}
}
