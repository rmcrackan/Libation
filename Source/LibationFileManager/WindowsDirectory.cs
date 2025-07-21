using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LibationFileManager
{
    public static class WindowsDirectory
    {

        public static void SetCoverAsFolderIcon(string pictureId, string directory, CancellationToken cancellationToken)
        {
            try
            {
                //Currently only works for Windows and macOS
                if (!Configuration.Instance.UseCoverAsFolderIcon)
                    return;

                // get path of cover art in Images dir. Download first if not exists
                var coverArtPath = PictureStorage.GetPicturePathSynchronously(new(pictureId, PictureSize._300x300), cancellationToken);
				InteropFactory.Create().SetFolderIcon(image: coverArtPath, directory: directory);
            }
            catch (Exception ex)
            {
                // Failure to 'set cover as folder icon' should not be considered a failure to download the book
                Serilog.Log.Logger.Error(ex, "Error setting cover art as folder icon. {@DebugInfo}", new { directory });

                try
                {
                    InteropFactory.Create().DeleteFolderIcon(directory);
                }
                catch
                {
                    Serilog.Log.Logger.Error(ex, "Error rolling back: setting cover art as folder icon. {@DebugInfo}", new { directory });
                }
            }
        }
    }
}
