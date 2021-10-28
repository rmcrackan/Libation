using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Windows.Forms;
using Dinah.Core;
using LibationFileManager;

namespace LibationWinForms.Dialogs
{
	public partial class SettingsDialog : Form
	{
		private Configuration config { get; } = Configuration.Instance;
		private Func<string, string> desc { get; } = Configuration.GetDescription;

		public SettingsDialog() => InitializeComponent();

		private void SettingsDialog_Load(object sender, EventArgs e)
		{
			if (this.DesignMode)
				return;

			{
				loggingLevelCb.Items.Clear();
				foreach (var level in Enum<Serilog.Events.LogEventLevel>.GetValues())
					loggingLevelCb.Items.Add(level);
				loggingLevelCb.SelectedItem = config.LogLevel;
			}

			this.importEpisodesCb.Text = desc(nameof(config.ImportEpisodes));
			this.downloadEpisodesCb.Text = desc(nameof(config.DownloadEpisodes));
			this.booksLocationDescLbl.Text = desc(nameof(config.Books));
			this.inProgressDescLbl.Text = desc(nameof(config.InProgress));
			this.allowLibationFixupCbox.Text = desc(nameof(config.AllowLibationFixup));
			this.splitFilesByChapterCbox.Text = desc(nameof(config.SplitFilesByChapter));

			booksSelectControl.SetSearchTitle("books location");
			booksSelectControl.SetDirectoryItems(
				new()
				{
					Configuration.KnownDirectories.UserProfile,
					Configuration.KnownDirectories.AppDir,
					Configuration.KnownDirectories.MyDocs
				},
				Configuration.KnownDirectories.UserProfile,
				"Books");
			booksSelectControl.SelectDirectory(config.Books);

			importEpisodesCb.Checked = config.ImportEpisodes;
			downloadEpisodesCb.Checked = config.DownloadEpisodes;
			allowLibationFixupCbox.Checked = config.AllowLibationFixup;
			convertLosslessRb.Checked = !config.DecryptToLossy;
			convertLossyRb.Checked = config.DecryptToLossy;
			splitFilesByChapterCbox.Checked = config.SplitFilesByChapter;

			allowLibationFixupCbox_CheckedChanged(this, e);

			inProgressSelectControl.SetDirectoryItems(new()
			{
				Configuration.KnownDirectories.WinTemp,
				Configuration.KnownDirectories.UserProfile,
				Configuration.KnownDirectories.AppDir,
				Configuration.KnownDirectories.MyDocs,
				Configuration.KnownDirectories.LibationFiles
			}, Configuration.KnownDirectories.WinTemp);
			inProgressSelectControl.SelectDirectory(config.InProgress);

			badBookGb.Text = desc(nameof(config.BadBook));
			badBookAskRb.Text = Configuration.BadBookAction.Ask.GetDescription();
			badBookAbortRb.Text = Configuration.BadBookAction.Abort.GetDescription();
			badBookRetryRb.Text = Configuration.BadBookAction.Retry.GetDescription();
			badBookIgnoreRb.Text = Configuration.BadBookAction.Ignore.GetDescription();
			var rb = config.BadBook switch
			{
				Configuration.BadBookAction.Ask => this.badBookAskRb,
				Configuration.BadBookAction.Abort => this.badBookAbortRb,
				Configuration.BadBookAction.Retry => this.badBookRetryRb,
				Configuration.BadBookAction.Ignore => this.badBookIgnoreRb,
				_ => this.badBookAskRb
			};
			rb.Checked = true;

			folderTemplateLbl.Text = desc(nameof(config.FolderTemplate));
			fileTemplateLbl.Text = desc(nameof(config.FileTemplate));
			chapterFileTemplateLbl.Text = desc(nameof(config.ChapterFileTemplate));
			folderTemplateTb.Text = config.FolderTemplate;
			fileTemplateTb.Text = config.FileTemplate;
			chapterFileTemplateTb.Text = config.ChapterFileTemplate;
		}

		private void allowLibationFixupCbox_CheckedChanged(object sender, EventArgs e)
		{
			convertLosslessRb.Enabled = allowLibationFixupCbox.Checked;
			convertLossyRb.Enabled = allowLibationFixupCbox.Checked;
			splitFilesByChapterCbox.Enabled = allowLibationFixupCbox.Checked;

			if (!allowLibationFixupCbox.Checked)
			{
				convertLosslessRb.Checked = true;
				splitFilesByChapterCbox.Checked = false;
			}
		}

		private void logsBtn_Click(object sender, EventArgs e) => Go.To.Folder(Configuration.Instance.LibationFiles);

		private void folderTemplateBtn_Click(object sender, EventArgs e)
		{
			TEMP_TEMP_TEMP();

		}

		private void fileTemplateBtn_Click(object sender, EventArgs e)
		{
			TEMP_TEMP_TEMP();

		}

		private void chapterFileTemplateBtn_Click(object sender, EventArgs e)
		{
			TEMP_TEMP_TEMP();

		}

		private void TEMP_TEMP_TEMP()
			=> MessageBox.Show("Sorry, not yet. Coming soon :)");

		private void saveBtn_Click(object sender, EventArgs e)
		{
			var newBooks = booksSelectControl.SelectedDirectory;

			#region validation
			if (string.IsNullOrWhiteSpace(newBooks))
			{
				MessageBox.Show("Cannot set Books Location to blank", "Location is blank", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			if (!Directory.Exists(newBooks) && booksSelectControl.SelectedDirectoryIsCustom)
			{
				MessageBox.Show($"Not saving change to Books location. This folder does not exist:\r\n{newBooks}", "Folder does not exist", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			if (Templates.Folder.IsValid(folderTemplateTb.Text))
			{
				MessageBox.Show($"Not saving change to folder naming template. Invalid format.", "Invalid folder template", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}
			if (Templates.File.IsValid(fileTemplateTb.Text))
			{
				MessageBox.Show($"Not saving change to file naming template. Invalid format.", "Invalid file template", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}
			if (Templates.ChapterFile.IsValid(chapterFileTemplateTb.Text))
			{
				MessageBox.Show($"Not saving change to chapter file naming template. Invalid format.", "Invalid chapter file template", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}
			#endregion

			if (!Directory.Exists(newBooks) && booksSelectControl.SelectedDirectoryIsKnown)
				Directory.CreateDirectory(newBooks);

			config.Books = newBooks;

			{
				var logLevelOld = config.LogLevel;
				var logLevelNew = (Serilog.Events.LogEventLevel)loggingLevelCb.SelectedItem;

				config.LogLevel = logLevelNew;

				// only warn if changed during this time. don't want to warn every time user happens to change settings while level is verbose
				if (logLevelOld != logLevelNew)
					MessageBoxVerboseLoggingWarning.ShowIfTrue();
			}

			config.ImportEpisodes = importEpisodesCb.Checked;
			config.DownloadEpisodes = downloadEpisodesCb.Checked;
			config.AllowLibationFixup = allowLibationFixupCbox.Checked;
			config.DecryptToLossy = convertLossyRb.Checked;
			config.SplitFilesByChapter = splitFilesByChapterCbox.Checked;

			config.InProgress = inProgressSelectControl.SelectedDirectory;

			config.BadBook
				= badBookAskRb.Checked ? Configuration.BadBookAction.Ask
				: badBookAbortRb.Checked ? Configuration.BadBookAction.Abort
				: badBookRetryRb.Checked ? Configuration.BadBookAction.Retry
				: badBookIgnoreRb.Checked ? Configuration.BadBookAction.Ignore
				: Configuration.BadBookAction.Ask;

			config.FolderTemplate = folderTemplateTb.Text;
			config.FileTemplate = fileTemplateTb.Text;
			config.ChapterFileTemplate = chapterFileTemplateTb.Text;

			this.DialogResult = DialogResult.OK;
			this.Close();
		}

		private void cancelBtn_Click(object sender, EventArgs e)
		{
			this.DialogResult = DialogResult.Cancel;
			this.Close();
		}
	}
}
