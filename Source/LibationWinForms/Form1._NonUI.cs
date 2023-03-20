using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ApplicationServices;
using Dinah.Core.WindowsDesktop.Drawing;
using LibationFileManager;
using LibationUiBase;

namespace LibationWinForms
{
    public partial class Form1
    {
        private void Configure_NonUI()
        {
            // init default/placeholder cover art
            var format = System.Drawing.Imaging.ImageFormat.Jpeg;
            PictureStorage.SetDefaultImage(PictureSize._80x80, Properties.Resources.default_cover_80x80.ToBytes(format));
            PictureStorage.SetDefaultImage(PictureSize._300x300, Properties.Resources.default_cover_300x300.ToBytes(format));
            PictureStorage.SetDefaultImage(PictureSize._500x500, Properties.Resources.default_cover_500x500.ToBytes(format));
            PictureStorage.SetDefaultImage(PictureSize.Native, Properties.Resources.default_cover_500x500.ToBytes(format));

            BaseUtil.SetLoadImageDelegate(WinFormsUtil.TryLoadImageOrDefault);

            // wire-up event to automatically download after scan.
            // winforms only. this should NOT be allowed in cli
            updateCountsBw.RunWorkerCompleted += (object sender, System.ComponentModel.RunWorkerCompletedEventArgs e) =>
            {
                if (!Configuration.Instance.AutoDownloadEpisodes)
                    return;

                var libraryStats = e.Result as LibraryCommands.LibraryStats;

                if ((libraryStats.booksNoProgress + libraryStats.pdfsNotDownloaded) > 0)
                    beginBookBackupsToolStripMenuItem_Click();
            };
        }
    }
}
