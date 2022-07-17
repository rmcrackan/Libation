using Dinah.Core;
using FileManager;
using LibationFileManager;
using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace LibationWinForms.Dialogs
{
	public partial class SettingsDialog
	{
		private void logsBtn_Click(object sender, EventArgs e) => Go.To.Folder(((LongPath)Configuration.Instance.LibationFiles).ShortPathName);

		private void Load_Important(Configuration config)
		{
			{
				loggingLevelCb.Items.Clear();
				foreach (var level in Enum<Serilog.Events.LogEventLevel>.GetValues())
					loggingLevelCb.Items.Add(level);
				loggingLevelCb.SelectedItem = config.LogLevel;
			}

			booksLocationDescLbl.Text = desc(nameof(config.Books));
			betaOptInCbox.Text = desc(nameof(config.BetaOptIn));
			this.saveEpisodesToSeriesFolderCbox.Text = desc(nameof(config.SavePodcastsToParentFolder));

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

			saveEpisodesToSeriesFolderCbox.Checked = config.SavePodcastsToParentFolder;
			betaOptInCbox.Checked = config.BetaOptIn;

			if (!betaOptInCbox.Checked)
				betaOptInCbox.CheckedChanged += betaOptInCbox_CheckedChanged;
		}

		private void Save_Important(Configuration config)
		{
			var newBooks = booksSelectControl.SelectedDirectory;

			#region validation
			static void validationError(string text, string caption)
				=> MessageBox.Show(text, caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
			if (string.IsNullOrWhiteSpace(newBooks))
			{
				validationError("Cannot set Books Location to blank", "Location is blank");
				return;
			}

			// these 3 should do nothing. Configuration will only init these with a valid value. EditTemplateDialog ensures valid before returning
			if (!Templates.Folder.IsValid(folderTemplateTb.Text))
			{
				validationError($"Not saving change to folder naming template. Invalid format.", "Invalid folder template");
				return;
			}
			if (!Templates.File.IsValid(fileTemplateTb.Text))
			{
				validationError($"Not saving change to file naming template. Invalid format.", "Invalid file template");
				return;
			}
			if (!Templates.ChapterFile.IsValid(chapterFileTemplateTb.Text))
			{
				validationError($"Not saving change to chapter file naming template. Invalid format.", "Invalid chapter file template");
				return;
			}
			#endregion

			LongPath lonNewBooks = newBooks;
			if (!Directory.Exists(lonNewBooks))
				Directory.CreateDirectory(lonNewBooks);

			config.Books = newBooks;

			{
				var logLevelOld = config.LogLevel;
				var logLevelNew = (Serilog.Events.LogEventLevel)loggingLevelCb.SelectedItem;

				config.LogLevel = logLevelNew;

				// only warn if changed during this time. don't want to warn every time user happens to change settings while level is verbose
				if (logLevelOld != logLevelNew)
					MessageBoxLib.VerboseLoggingWarning_ShowIfTrue();
			}

			config.SavePodcastsToParentFolder = saveEpisodesToSeriesFolderCbox.Checked;

			config.BetaOptIn = betaOptInCbox.Checked;
		}


		private void betaOptInCbox_CheckedChanged(object sender, EventArgs e)
		{
			if (!betaOptInCbox.Checked)
				return;

			var result = MessageBox.Show(this, @"


You've chosen to opt-in to Libation's beta releases. Thank you! We need all the testers we can get.

These features are works in progress and potentially very buggy. Libation may crash unexpectedly, and your library database may even be corruted.  We suggest you back up your LibationContext.db file before proceding.

If bad/weird things happen, please report them at getlibation.com.

".Trim(), "A word of warning...", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);

			if (result == DialogResult.Yes)
			{
				betaOptInCbox.CheckedChanged -= betaOptInCbox_CheckedChanged;
			}
			else
			{
				betaOptInCbox.Checked = false;
			}
		}
	}
}
