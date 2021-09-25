using Dinah.Core;
using FileManager;
using System;
using System.Linq;
using System.IO;
using System.Windows.Forms;

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
		}

		private void allowLibationFixupCbox_CheckedChanged(object sender, EventArgs e)
		{
			convertLosslessRb.Enabled = allowLibationFixupCbox.Checked;
			convertLossyRb.Enabled = allowLibationFixupCbox.Checked;

			if (!allowLibationFixupCbox.Checked)
			{
				convertLosslessRb.Checked = true;
			}
		}

		private void logsBtn_Click(object sender, EventArgs e) => Go.To.Folder(Configuration.Instance.LibationFiles);

		private void saveBtn_Click(object sender, EventArgs e)
		{
			var newBooks = booksSelectControl.SelectedDirectory;

			if (string.IsNullOrWhiteSpace(newBooks))
			{
				MessageBox.Show("Cannot set Books Location to blank", "Location is blank", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			if (!Directory.Exists(newBooks))
			{
				if (booksSelectControl.SelectedDirectoryIsCustom)
				{
					MessageBox.Show($"Not saving change to Books location. This folder does not exist:\r\n{newBooks}", "Folder does not exist", MessageBoxButtons.OK, MessageBoxIcon.Error);
					return;
				}

				if (booksSelectControl.SelectedDirectoryIsKnown)
					Directory.CreateDirectory(newBooks);
			}

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

			config.InProgress = inProgressSelectControl.SelectedDirectory;

			config.BadBook
				= badBookAskRb.Checked ? Configuration.BadBookAction.Ask
				: badBookAbortRb.Checked ? Configuration.BadBookAction.Abort
				: badBookRetryRb.Checked ? Configuration.BadBookAction.Retry
				: badBookIgnoreRb.Checked ? Configuration.BadBookAction.Ignore
				: Configuration.BadBookAction.Ask;

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
