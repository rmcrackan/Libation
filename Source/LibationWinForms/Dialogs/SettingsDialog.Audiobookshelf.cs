using ApplicationServices;
using LibationFileManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LibationWinForms.Dialogs;

public partial class SettingsDialog
{
	private TabPage? tab5Audiobookshelf;
	private CheckBox? absEnabledCb;
	private TextBox? absUrlTb;
	private TextBox? absTokenTb;
	private Button? absConnectBtn;
	private ComboBox? absLibraryCb;
	private ComboBox? absFolderCb;
	private Label? absUrlLbl;
	private Label? absTokenLbl;
	private Label? absLibraryLbl;
	private Label? absFolderLbl;
	private Label? absStatusLbl;

	private List<AudiobookshelfApiService.Library> _absLibraries = [];

	private void Load_Audiobookshelf(Configuration config)
	{
		CreateAudiobookshelfTab();

		absEnabledCb!.Text = desc(nameof(config.AudiobookshelfEnabled));
		absUrlLbl!.Text = "Server URL";
		absTokenLbl!.Text = "API Token";
		absLibraryLbl!.Text = "Library";
		absFolderLbl!.Text = "Folder";
		absConnectBtn!.Text = "Connect / Refresh";
		absStatusLbl!.Text = "";

		absEnabledCb.Checked = config.AudiobookshelfEnabled;
		absUrlTb!.Text = config.AudiobookshelfServerUrl ?? "";
		absTokenTb!.Text = config.AudiobookshelfApiToken ?? "";
		absTokenTb.PasswordChar = '*';

		ToggleAudiobookshelfControls(absEnabledCb.Checked);

		// Try to restore saved selections if we have them
		if (!string.IsNullOrWhiteSpace(config.AudiobookshelfLibraryId))
			_ = RestoreLibraryAndFolderAsync(config);
	}

	private void CreateAudiobookshelfTab()
	{
		tab5Audiobookshelf = new TabPage
		{
			AutoScroll = true,
			BackColor = System.Drawing.SystemColors.Window,
			Location = new System.Drawing.Point(4, 24),
			Name = "tab5Audiobookshelf",
			Padding = new Padding(3),
			Size = new System.Drawing.Size(856, 457),
			TabIndex = 4,
			Text = "Audiobookshelf"
		};

		absEnabledCb = new CheckBox
		{
			AutoSize = true,
			Location = new System.Drawing.Point(6, 6),
			Name = "absEnabledCb",
			Size = new System.Drawing.Size(280, 19),
			TabIndex = 0,
			Text = "[AudiobookshelfEnabled desc]",
			UseVisualStyleBackColor = true
		};
		absEnabledCb.CheckedChanged += absEnabledCb_CheckedChanged;

		absUrlLbl = new Label
		{
			AutoSize = true,
			Location = new System.Drawing.Point(6, 35),
			Size = new System.Drawing.Size(70, 15),
			TabIndex = 1,
			Text = "Server URL"
		};

		absUrlTb = new TextBox
		{
			Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
			Location = new System.Drawing.Point(90, 32),
			Size = new System.Drawing.Size(550, 23),
			TabIndex = 2
		};

		absTokenLbl = new Label
		{
			AutoSize = true,
			Location = new System.Drawing.Point(6, 64),
			Size = new System.Drawing.Size(65, 15),
			TabIndex = 3,
			Text = "API Token"
		};

		absTokenTb = new TextBox
		{
			Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
			Location = new System.Drawing.Point(90, 61),
			Size = new System.Drawing.Size(550, 23),
			TabIndex = 4
		};

		absConnectBtn = new Button
		{
			Anchor = AnchorStyles.Top | AnchorStyles.Right,
			Location = new System.Drawing.Point(646, 32),
			Size = new System.Drawing.Size(130, 52),
			TabIndex = 5,
			Text = "Connect / Refresh",
			UseVisualStyleBackColor = true
		};
		absConnectBtn.Click += absConnectBtn_Click;

		absStatusLbl = new Label
		{
			AutoSize = true,
			Location = new System.Drawing.Point(6, 94),
			Size = new System.Drawing.Size(0, 15),
			TabIndex = 6,
			Text = ""
		};

		absLibraryLbl = new Label
		{
			AutoSize = true,
			Location = new System.Drawing.Point(6, 120),
			Size = new System.Drawing.Size(50, 15),
			TabIndex = 7,
			Text = "Library"
		};

		absLibraryCb = new ComboBox
		{
			Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
			DropDownStyle = ComboBoxStyle.DropDownList,
			FormattingEnabled = true,
			Location = new System.Drawing.Point(90, 117),
			Size = new System.Drawing.Size(686, 23),
			TabIndex = 8
		};
		absLibraryCb.SelectedIndexChanged += absLibraryCb_SelectedIndexChanged;

		absFolderLbl = new Label
		{
			AutoSize = true,
			Location = new System.Drawing.Point(6, 150),
			Size = new System.Drawing.Size(45, 15),
			TabIndex = 9,
			Text = "Folder"
		};

		absFolderCb = new ComboBox
		{
			Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
			DropDownStyle = ComboBoxStyle.DropDownList,
			FormattingEnabled = true,
			Location = new System.Drawing.Point(90, 147),
			Size = new System.Drawing.Size(686, 23),
			TabIndex = 10
		};

		tab5Audiobookshelf.Controls.Add(absEnabledCb);
		tab5Audiobookshelf.Controls.Add(absUrlLbl);
		tab5Audiobookshelf.Controls.Add(absUrlTb);
		tab5Audiobookshelf.Controls.Add(absTokenLbl);
		tab5Audiobookshelf.Controls.Add(absTokenTb);
		tab5Audiobookshelf.Controls.Add(absConnectBtn);
		tab5Audiobookshelf.Controls.Add(absStatusLbl);
		tab5Audiobookshelf.Controls.Add(absLibraryLbl);
		tab5Audiobookshelf.Controls.Add(absLibraryCb);
		tab5Audiobookshelf.Controls.Add(absFolderLbl);
		tab5Audiobookshelf.Controls.Add(absFolderCb);

		tabControl.Controls.Add(tab5Audiobookshelf);
	}

	private void ToggleAudiobookshelfControls(bool enabled)
	{
		if (absUrlTb is null) return;
		absUrlTb.Enabled = enabled;
		absTokenTb!.Enabled = enabled;
		absConnectBtn!.Enabled = enabled;
		absLibraryCb!.Enabled = enabled && _absLibraries.Count > 0;
		absFolderCb!.Enabled = enabled && absLibraryCb!.SelectedIndex >= 0;
	}

	private void absEnabledCb_CheckedChanged(object? sender, EventArgs e)
		=> ToggleAudiobookshelfControls(absEnabledCb!.Checked);

	private async void absConnectBtn_Click(object? sender, EventArgs e)
	{
		absConnectBtn!.Enabled = false;
		absStatusLbl!.Text = "Connecting...";
		absStatusLbl.ForeColor = System.Drawing.SystemColors.ControlText;

		try
		{
			var url = absUrlTb!.Text.Trim();
			var token = absTokenTb!.Text.Trim();

			if (string.IsNullOrWhiteSpace(url) || string.IsNullOrWhiteSpace(token))
			{
				absStatusLbl.Text = "Please enter both server URL and API token.";
				absStatusLbl.ForeColor = System.Drawing.Color.DarkRed;
				return;
			}

			var libraries = await AudiobookshelfApiService.GetLibrariesAsync(url, token);

			if (libraries.Count == 0)
			{
				absStatusLbl.Text = "Could not connect or no libraries found. Check your settings.";
				absStatusLbl.ForeColor = System.Drawing.Color.DarkRed;
				return;
			}

			_absLibraries = libraries;
			absLibraryCb!.Items.Clear();
			absLibraryCb.Items.AddRange(libraries.Select(l => $"{l.Name} ({l.Id})").ToArray());
			absFolderCb!.Items.Clear();
			absFolderCb.SelectedIndex = -1;

			absStatusLbl.Text = $"Connected. Found {libraries.Count} library(ies).";
			absStatusLbl.ForeColor = System.Drawing.Color.DarkGreen;
			ToggleAudiobookshelfControls(absEnabledCb!.Checked);
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
		absFolderCb!.Items.Clear();
		absFolderCb.SelectedIndex = -1;

		if (absLibraryCb!.SelectedIndex < 0 || _absLibraries.Count == 0)
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

			absLibraryCb!.Items.Clear();
			absLibraryCb.Items.AddRange(libraries.Select(l => $"{l.Name} ({l.Id})").ToArray());

			var savedLibIndex = libraries.FindIndex(l => l.Id == config.AudiobookshelfLibraryId);
			if (savedLibIndex >= 0)
			{
				absLibraryCb.SelectedIndex = savedLibIndex;
				var library = libraries[savedLibIndex];
				var savedFolderIndex = library.Folders.FindIndex(f => f.Id == config.AudiobookshelfFolderId);
				if (savedFolderIndex >= 0)
					absFolderCb!.SelectedIndex = savedFolderIndex;
			}
			ToggleAudiobookshelfControls(absEnabledCb!.Checked);
		}
		catch
		{
			// Silently fail; user can manually reconnect
		}
	}

	private void Save_Audiobookshelf(Configuration config)
	{
		config.AudiobookshelfEnabled = absEnabledCb!.Checked;
		config.AudiobookshelfServerUrl = absUrlTb!.Text.Trim();
		config.AudiobookshelfApiToken = absTokenTb!.Text.Trim();

		if (absLibraryCb!.SelectedIndex >= 0 && _absLibraries.Count > absLibraryCb.SelectedIndex)
			config.AudiobookshelfLibraryId = _absLibraries[absLibraryCb.SelectedIndex].Id;
		else
			config.AudiobookshelfLibraryId = null;

		if (absFolderCb!.SelectedIndex >= 0 && absLibraryCb.SelectedIndex >= 0)
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
