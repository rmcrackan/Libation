using Dinah.Core;
using FileManager;
using LibationFileManager;
using LibationUiBase;
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
			saveEpisodesToSeriesFolderCbox.Text = desc(nameof(config.SavePodcastsToParentFolder));
			overwriteExistingCbox.Text = desc(nameof(config.OverwriteExisting));
			creationTimeLbl.Text = desc(nameof(config.CreationTime));
			lastWriteTimeLbl.Text = desc(nameof(config.LastWriteTime));
			gridScaleFactorLbl.Text = desc(nameof(config.GridScaleFactor));
			gridFontScaleFactorLbl.Text = desc(nameof(config.GridFontScaleFactor));

			var dateTimeSources = Enum.GetValues<Configuration.DateTimeSource>().Select(v => new EnumDiaplay<Configuration.DateTimeSource>(v)).ToArray();
			creationTimeCb.Items.AddRange(dateTimeSources);
			lastWriteTimeCb.Items.AddRange(dateTimeSources);

			creationTimeCb.SelectedItem = dateTimeSources.SingleOrDefault(v => v.Value == config.CreationTime) ?? dateTimeSources[0];
			lastWriteTimeCb.SelectedItem = dateTimeSources.SingleOrDefault(v => v.Value == config.LastWriteTime) ?? dateTimeSources[0];


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
			booksSelectControl.SelectDirectory(config.Books.PathWithoutPrefix);

			saveEpisodesToSeriesFolderCbox.Checked = config.SavePodcastsToParentFolder;
			overwriteExistingCbox.Checked = config.OverwriteExisting;
			gridScaleFactorTbar.Value = scaleFactorToLinearRange(config.GridScaleFactor);
			gridFontScaleFactorTbar.Value = scaleFactorToLinearRange(config.GridFontScaleFactor);
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
			config.OverwriteExisting = overwriteExistingCbox.Checked;


			config.CreationTime = ((EnumDiaplay<Configuration.DateTimeSource>)creationTimeCb.SelectedItem).Value;
			config.LastWriteTime = ((EnumDiaplay<Configuration.DateTimeSource>)lastWriteTimeCb.SelectedItem).Value;
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
	}
}
