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

			if (!Directory.Exists(newBooks) && booksSelectControl.SelectedDirectoryIsCustom)
			{
				validationError($"Not saving change to Books location. This folder does not exist:\r\n{newBooks}", "Folder does not exist");
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

			if (!Directory.Exists(newBooks) && booksSelectControl.SelectedDirectoryIsKnown)
				Directory.CreateDirectory(newBooks);

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
		}
	}
}
