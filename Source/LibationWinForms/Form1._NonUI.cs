using System.Windows.Forms;
using ApplicationServices;
using AudibleUtilities;
using Dinah.Core.WindowsDesktop.Drawing;
using FileManager;
using LibationFileManager;
using LibationUiBase;

namespace LibationWinForms
{
    public partial class Form1
    {
        private void Configure_NonUI()
		{
			AudibleApiStorage.LoadError += AudibleApiStorage_LoadError;

			// init default/placeholder cover art
			var format = System.Drawing.Imaging.ImageFormat.Jpeg;
            PictureStorage.SetDefaultImage(PictureSize._80x80, Properties.Resources.default_cover_80x80.ToBytes(format));
            PictureStorage.SetDefaultImage(PictureSize._300x300, Properties.Resources.default_cover_300x300.ToBytes(format));
            PictureStorage.SetDefaultImage(PictureSize._500x500, Properties.Resources.default_cover_500x500.ToBytes(format));
            PictureStorage.SetDefaultImage(PictureSize.Native, Properties.Resources.default_cover_500x500.ToBytes(format));

            BaseUtil.SetLoadImageDelegate(WinFormsUtil.TryLoadImageOrDefault);
            BaseUtil.SetLoadResourceImageDelegate(Properties.Resources.ResourceManager.GetObject);

            // wire-up event to automatically download after scan.
            // winforms only. this should NOT be allowed in cli
            updateCountsBw.RunWorkerCompleted += (object sender, System.ComponentModel.RunWorkerCompletedEventArgs e) =>
            {
                if (!Configuration.Instance.AutoDownloadEpisodes || e.Result is not LibraryCommands.LibraryStats libraryStats)
                    return;

                if ((libraryStats.PendingBooks + libraryStats.pdfsNotDownloaded) > 0)
					BackupAllBooks(libraryStats.LibraryBooks);
            };
		}

		private void AudibleApiStorage_LoadError(object sender, AccountSettingsLoadErrorEventArgs e)
		{
			try
			{
				//Backup AccountSettings.json and create a new, empty file.
				var backupFile = 
					FileUtility.SaferMoveToValidPath(
						e.SettingsFilePath,
						e.SettingsFilePath,
						Configuration.Instance.ReplacementCharacters,
						"bak");

				AudibleApiStorage.EnsureAccountsSettingsFileExists();
				e.Handled = true;

				showAccountSettingsRecoveredMessage(backupFile);
			}
			catch
			{
				showAccountSettingsUnrecoveredMessage();
			}

			void showAccountSettingsRecoveredMessage(LongPath backupFile)
			=> MessageBox.Show(this, $"""
				Libation could not load your account settings, so it had created a new, empty account settings file.

				You will need to re-add you Audible account(s) before scanning or downloading.

				The old account settings file has been archived at '{backupFile.PathWithoutPrefix}'

				{e.GetException().ToString()}
				""",
				"Error Loading Account Settings",
				MessageBoxButtons.OK,
				MessageBoxIcon.Warning);

			void showAccountSettingsUnrecoveredMessage()
			=> MessageBox.Show(this, $"""
				Libation could not load your account settings. The file may be corrupted, but Libation is unable to delete it.

				Please move or delete the account settings file '{e.SettingsFilePath}'

				{e.GetException().ToString()}
				""",
				"Error Loading Account Settings",
				MessageBoxButtons.OK,
				MessageBoxIcon.Error);
		}
	}
}
